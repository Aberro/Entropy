// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using UnityEngine;
using UnityEngine.Rendering;
using JetBrains.Annotations;
using Entropy.Scripts.Utilities;

// ReSharper disable InconsistentNaming

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Sonic Ether/SEGI")]
public class SEGIBehavior : SEGI
{

	#region Internal variables
	///<summary>An array of volume textures where each element is a mip/LOD level. Each volume is half the resolution of the previous volume. Separate textures for each mip level are required for manual mip-mapping of the main GI volume texture.</summary>
	private RenderTexture?[]? volumeTextures;

	///<summary>The secondary volume texture that holds irradiance calculated during the in-volume GI tracing that occurs when Infinite Bounces is enabled. </summary>
	private RenderTexture? secondaryIrradianceVolume;

	///<summary>The alternate mip level 0 main volume texture needed to avoid simultaneous read/write errors while performing temporal stabilization on the main voxel volume.</summary>
	private RenderTexture? volumeTextureB;

	///<summary>The current active volume texture that holds GI information to be read during GI tracing.</summary>
	private RenderTexture? activeVolume;

	///<summary>The volume texture that holds GI information to be read during GI tracing that was used in the previous frame.</summary>
	private RenderTexture? previousActiveVolume;

	private Vector3 voxelSpaceOrigin;
	private Vector3 previousVoxelSpaceOrigin;
	private Vector3 voxelSpaceOriginDelta;

	private int voxelFlipFlop;
	#endregion

	#region SupportingObjectsAndProperties

	/// <summary>
	/// Estimates the VRAM usage of all the render textures used to render GI.
	/// </summary>
	public override long VRAMUsage
	{
		get
		{
			long v = base.VRAMUsage;

			if (this.sunDepthTexture.IsValid())
				v += this.sunDepthTexture.width * this.sunDepthTexture.height * 16;

			if (this.volumeTextures != null)
				foreach (var texture in this.volumeTextures)
					if (texture.IsValid())
						v += texture.width * texture.height * texture.volumeDepth * 16 * 4;

			if (this.secondaryIrradianceVolume.IsValid())
				v += this.secondaryIrradianceVolume.width * this.secondaryIrradianceVolume.height * this.secondaryIrradianceVolume.volumeDepth * 16 * 4;

			if (this.volumeTextureB.IsValid())
				v += this.volumeTextureB.width * this.volumeTextureB.height * this.volumeTextureB.volumeDepth * 16 * 4;

			return v;
		}
	}

	protected override int DummyVoxelResolution => (int) this.voxelResolution * (this.voxelAA ? 2 : 1);
	protected override int GlobalVoxelAA => this.voxelAA ? 1 : 0;
	protected override bool DisableShadowRendering => false;
	#endregion

	#region UnityEvents

	[UsedImplicitly]
	private void OnDrawGizmosSelected()
	{
		if(!enabled)
			return;

		var prevColor = Gizmos.color;
		Gizmos.color = new Color(1.0f, 0.25f, 0.0f, 0.5f);
		Gizmos.DrawCube(this.voxelSpaceOrigin, new Vector3(this.voxelSpaceSize, this.voxelSpaceSize, this.voxelSpaceSize));
		Gizmos.color = prevColor;
	}

	[UsedImplicitly]
	protected override void Update()
	{
		base.Update();

		if(this.volumeTextures is not { Length: NumMipLevels }
			|| !this.volumeTextures[0].IsValid()
			|| this.volumeTextures[0]!.width != (int)this.voxelResolution)
			CreateVolumeTextures();

	}

	protected override void OnPreRender()
	{
		//Force reinitialization to make sure that everything is working properly if one of the cameras was unexpectedly destroyed
		if(this.notReadyToRender || this.volumeTextures == null || !this.followTransform.IsValid() || !this.voxelCamera.IsValid()
			|| !this.voxelCameraGO.IsValid() || !this.leftViewPoint.IsValid() || !this.topViewPoint.IsValid() || !this.attachedCamera.IsValid()
			|| !this.shadowCamTransform.IsValid() || !this.shadowCam.IsValid() || !this.sunDepthTexture.IsValid() || !this.clearCompute.IsValid()
			|| !this.transferIntsCompute.IsValid() || !this.mipFilterCompute.IsValid())
			this.initialized = false;

		base.OnPreRender();
	}
	#endregion

	protected override void PreRenderVoxelize()
	{
		if(this.volumeTextures == null)
			return;
		this.activeVolume = this.voxelFlipFlop == 0 ? this.volumeTextures[0] : this.volumeTextureB;             //Flip-flopping volume textures to avoid simultaneous read and write errors in shaders //-V3125
		this.previousActiveVolume = this.voxelFlipFlop == 0 ? this.volumeTextureB : this.volumeTextures[0];

		//float voxelTexel = (1.0f * voxelSpaceSize) / (int)voxelResolution * 0.5f;			//Calculate the size of a voxel texel in world-space units

		//Setup the voxel volume origin position
		//The interval at which the voxel volume will be "locked" in world-space
		GetVoxelVolumeOriginAndInterval(this.voxelSpaceSize, out var origin, out var interval);

		//Lock the voxel volume origin based on the interval
		this.voxelSpaceOrigin = new Vector3(Mathf.Round(origin.x / interval) * interval, Mathf.Round(origin.y / interval) * interval, Mathf.Round(origin.z / interval) * interval);

		//Calculate how much the voxel origin has moved since last voxelization pass. Used for scrolling voxel data in shaders to avoid ghosting when the voxel volume moves in the world
		this.voxelSpaceOriginDelta = this.voxelSpaceOrigin - this.previousVoxelSpaceOrigin;
		Shader.SetGlobalVector(SEGIVoxelSpaceOriginDelta, this.voxelSpaceOriginDelta / this.voxelSpaceSize);

		this.previousVoxelSpaceOrigin = this.voxelSpaceOrigin;

		//Set the voxel camera (proxy camera used to render the scene for voxelization) parameters
		SetVoxelCamera(this.voxelSpaceOrigin, this.voxelSpaceSize * 0.5f);

		//Set matrices needed for voxelization
		Shader.SetGlobalMatrix(WorldToCamera, this.attachedCamera!.worldToCameraMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelViewFront, TransformViewMatrix(this.voxelCamera!.transform.worldToLocalMatrix));
		Shader.SetGlobalMatrix(SEGIVoxelViewLeft, TransformViewMatrix(this.leftViewPoint!.transform.worldToLocalMatrix));
		Shader.SetGlobalMatrix(SEGIVoxelViewTop, TransformViewMatrix(this.topViewPoint!.transform.worldToLocalMatrix));
		Shader.SetGlobalMatrix(SEGIWorldToVoxel, this.voxelCamera.worldToCameraMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelProjection, this.voxelCamera.projectionMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelProjectionInverse, this.voxelCamera.projectionMatrix.inverse);

		Shader.SetGlobalInt(SEGIVoxelResolution, (int)this.voxelResolution);

		//Set paramteters
		var sunColor = this.sun == null ? Color.black : this.sun.color;
		var sunIntensity = this.sun == null ? 0 : this.sun.intensity;
		Shader.SetGlobalColor(GISunColor, new Color(sunColor.r, sunColor.g, sunColor.b, sunIntensity).linear);
		Shader.SetGlobalColor(SEGISkyColor, new Color(this.skyColor.r * this.skyIntensity, this.skyColor.g * this.skyIntensity, this.skyColor.b * this.skyIntensity, this.skyColor.a).linear);
		//Shader.SetGlobalColor(GISunColor, sun == null ? Color.black : new Color(Mathf.Pow(sun.color.r, 2.2f), Mathf.Pow(sun.color.g, 2.2f), Mathf.Pow(sun.color.b, 2.2f), Mathf.Pow(sun.intensity, 2.2f)));
		//Shader.SetGlobalColor(SEGISkyColor, new Color(Mathf.Pow(skyColor.r * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.g * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.b * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.a, 2.2f)));
		Shader.SetGlobalFloat(GIGain, this.giGain);
		Shader.SetGlobalFloat(SEGISecondaryBounceGain, this.infiniteBounces ? this.secondaryBounceGain : 0.0f);
		Shader.SetGlobalFloat(SEGISoftSunlight, this.softSunlight);
		Shader.SetGlobalInt(SEGISphericalSkylight, this.sphericalSkylight ? 1 : 0);
		Shader.SetGlobalInt(SEGIInnerOcclusionLayers, this.innerOcclusionLayers);
		//Render the depth texture from the sun's perspective in order to inject sunlight with shadows during voxelization
		if(this.sun.IsValid())
		{
			this.shadowCam!.cullingMask = this.giCullingMask;

			var shadowCamPosition = this.voxelSpaceOrigin + (Vector3.Normalize(-this.sun.transform.forward) * (this.shadowSpaceSize * 0.5f * this.shadowSpaceDepthRatio));

			this.shadowCamTransform!.position = shadowCamPosition;
			this.shadowCamTransform.LookAt(this.voxelSpaceOrigin, Vector3.up);

			this.shadowCam.renderingPath = RenderingPath.Forward;
			this.shadowCam.depthTextureMode = DepthTextureMode.None;

			this.shadowCam.orthographicSize = this.shadowSpaceSize;
			this.shadowCam.farClipPlane = this.shadowSpaceSize * 2.0f * this.shadowSpaceDepthRatio;

			Graphics.SetRenderTarget(this.sunDepthTexture);
			this.shadowCam.SetTargetBuffers(this.sunDepthTexture!.colorBuffer, this.sunDepthTexture.depthBuffer);

			this.shadowCam.RenderWithShader(this.sunDepthShader, "");

			Shader.SetGlobalTexture(SEGISunDepth, this.sunDepthTexture);
		}

		var voxelToGIProjection = this.shadowCam!.projectionMatrix * this.shadowCam.worldToCameraMatrix * this.voxelCamera.cameraToWorldMatrix;
		Shader.SetGlobalMatrix(SEGIVoxelToGIProjection, voxelToGIProjection);
		Shader.SetGlobalVector(SEGISunlightVector, this.sun.IsValid() ? Vector3.Normalize(this.sun.transform.forward) : Vector3.up);

		//Clear the volume texture that is immediately written to in the voxelization scene shader
		this.clearCompute!.SetTexture(0, RG0, this.integerVolume);
		this.clearCompute.SetInt(Res, (int)this.voxelResolution);
		this.clearCompute.Dispatch(0, Math.Max(1, (int)this.voxelResolution / 16), Math.Max(1, (int)this.voxelResolution / 16), 1);

		//Render the scene with the voxel proxy camera object with the voxelization shader to voxelize the scene to the volume integer texture
		Graphics.SetRandomWriteTarget(1, this.integerVolume);
		this.voxelCamera.targetTexture = this.dummyVoxelTextureAAScaled;
		this.voxelCamera.RenderWithShader(this.voxelizationShader, "");
		Graphics.ClearRandomWriteTargets();

		//Transfer the data from the volume integer texture to the main volume texture used for GI tracing.
		this.transferIntsCompute!.SetTexture(0, Result, this.activeVolume);
		this.transferIntsCompute.SetTexture(0, PrevResult, this.previousActiveVolume);
		this.transferIntsCompute.SetTexture(0, RG0, this.integerVolume);
		this.transferIntsCompute.SetInt(VoxelAA, this.voxelAA ? 1 : 0);
		this.transferIntsCompute.SetInt(Resolution, (int)this.voxelResolution);
		this.transferIntsCompute.SetVector(VoxelOriginDelta, this.voxelSpaceOriginDelta / (this.voxelSpaceSize * (int)this.voxelResolution));
		this.transferIntsCompute.Dispatch(0, Math.Max(1, (int)this.voxelResolution / 16), Math.Max(1, (int)this.voxelResolution / 16), 1);

		Shader.SetGlobalTexture(SEGIVolumeLevel[0], this.activeVolume);

		//Manually filter/render mip maps
		for(var i = 0; i < NumMipLevels - 1; i++)
		{
			var source = this.volumeTextures[i];

			if(i == 0)
				source = this.activeVolume;

			var destinationRes = (int)this.voxelResolution >> (i + 1);
			this.mipFilterCompute!.SetTexture(MipFilterKernel, Source, source);
			this.mipFilterCompute.SetTexture(MipFilterKernel, Destination, this.volumeTextures[i + 1]);
			this.mipFilterCompute.SetInt(destinationRes, destinationRes);
			this.mipFilterCompute.Dispatch(MipFilterKernel, Math.Max(1, destinationRes / 8), Math.Max(1, destinationRes / 8), 1);
			Shader.SetGlobalTexture(SEGIVolumeLevel[i + 1], this.volumeTextures[i + 1]);
		}

		//Advance the voxel flip flop counter
		this.voxelFlipFlop += 1;
		this.voxelFlipFlop %= 2;
	}
	protected override void PreRenderBounce()
	{
		if(!this.clearCompute.IsValid() || !this.voxelCamera.IsValid() || !this.transferIntsCompute.IsValid())
			return;
		//Clear the volume texture that is immediately written to in the voxelization scene shader
		this.clearCompute.SetTexture(0, RG0, this.integerVolume);
		this.clearCompute.Dispatch(0, Math.Max(1, (int)this.voxelResolution / 16), Math.Max(1, (int)this.voxelResolution / 16), 1);

		//Set secondary tracing parameters
		Shader.SetGlobalInt(SEGISecondaryCones, this.secondaryCones);
		Shader.SetGlobalFloat(SEGISecondaryOcclusionStrength, this.secondaryOcclusionStrength);

		//Render the scene from the voxel camera object with the voxel tracing shader to render a bounce of GI into the irradiance volume
		Graphics.SetRandomWriteTarget(1, this.integerVolume);
		this.voxelCamera.targetTexture = this.dummyVoxelTextureFixed;
		this.voxelCamera.RenderWithShader(this.voxelTracingShader, "");
		Graphics.ClearRandomWriteTargets();

		//Transfer the data from the volume integer texture to the irradiance volume texture. This result is added to the next main voxelization pass to create a feedback loop for infinite bounces
		this.transferIntsCompute.SetTexture(1, Result, this.secondaryIrradianceVolume);
		this.transferIntsCompute.SetTexture(1, RG0, this.integerVolume);
		this.transferIntsCompute.SetInt(Resolution, (int)this.voxelResolution);
		this.transferIntsCompute.Dispatch(1, Math.Max(1, (int)this.voxelResolution / 16), Math.Max(1, (int)this.voxelResolution / 16), 1);

		Shader.SetGlobalTexture(SEGIVolumeTexture1, this.secondaryIrradianceVolume);
	}

	protected override void RenderImageInternal(RenderTexture source, RenderTexture destination, RenderTexture gi1, RenderTexture gi2, RenderTexture currentDepth, RenderTexture currentNormal, RenderTexture? reflections)
	{
		//If Half Resolution tracing is enabled
		if(GIRenderRes == 2)
		{
			RenderTexture.ReleaseTemporary(gi1);

			//Setup temporary textures
			var gi3 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
			var gi4 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);

			//Prepare the half-resolution diffuse GI result to be bilaterally upsampled
			gi2.filterMode = FilterMode.Point;
			Graphics.Blit(gi2, gi4);

			RenderTexture.ReleaseTemporary(gi2);

			gi4.filterMode = FilterMode.Point;
			gi3.filterMode = FilterMode.Point;

			//Perform bilateral upsampling on half-resolution diffuse GI result
			this.material!.SetVector(Kernel, new Vector2(1.0f, 0.0f));
			Graphics.Blit(gi4, gi3, this.material, Pass.BilateralUpsample);
			this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));

			//Perform temporal reprojection and blending
			if(this.temporalBlendWeight < 1.0f)
			{
				Graphics.Blit(gi3, gi4);
				Graphics.Blit(gi4, gi3, this.material, Pass.TemporalBlend);
				Graphics.Blit(gi3, this.previousGIResult);
				Graphics.Blit(source, this.previousCameraDepth, this.material, Pass.GetCameraDepthTexture);
			}

			//Set the result to be accessed in the shader
			this.material.SetTexture(GITexture, gi3);

			//Actually apply the GI to the scene using gbuffer data
			Graphics.Blit(source, destination, this.material, this.visualizeGI ? Pass.VisualizeGI : Pass.BlendWithScene);

			//Release temporary textures
			RenderTexture.ReleaseTemporary(gi3);
			RenderTexture.ReleaseTemporary(gi4);
		}
		else    //If Half Resolution tracing is disabled
		{
			//Perform temporal reprojection and blending
			if(this.temporalBlendWeight < 1.0f)
			{
				Graphics.Blit(gi2, gi1, this.material, Pass.TemporalBlend);
				Graphics.Blit(gi1, this.previousGIResult);
				Graphics.Blit(source, this.previousCameraDepth, this.material, Pass.GetCameraDepthTexture);
			}

			//Actually apply the GI to the scene using gbuffer data
			this.material!.SetTexture(GITexture, this.temporalBlendWeight < 1.0f ? gi1 : gi2);
			Graphics.Blit(source, destination, this.material, this.visualizeGI ? Pass.VisualizeGI : Pass.BlendWithScene);
		}
	}

	protected override void CreateVolumeTextures()
	{
		foreach(var texture in this.volumeTextures ?? [] )
			DestroyTexture(texture);
		DestroyTexture(this.volumeTextureB);
		DestroyTexture(this.secondaryIrradianceVolume);
		DestroyTexture(this.integerVolume);

		this.volumeTextures = new RenderTexture[NumMipLevels];

		// ReSharper disable once LocalVariableHidesMember
		var voxelResolution = (int)this.voxelResolution;
		for (var i = 0; i < NumMipLevels; i++)
		{
			var resolution = voxelResolution >> i;
			var texture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
			{
				dimension = TextureDimension.Tex3D,
				volumeDepth = resolution,
				enableRandomWrite = true,
				filterMode = FilterMode.Bilinear,
				autoGenerateMips = false,
				useMipMap = false,
				hideFlags = HideFlags.HideAndDontSave,
			};
			texture.Create();
			this.volumeTextures[i] = texture;
		}
		this.volumeTextureB = new RenderTexture(voxelResolution, voxelResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
		{
			dimension = TextureDimension.Tex3D,
			volumeDepth = voxelResolution,
			enableRandomWrite = true,
			filterMode = FilterMode.Bilinear,
			autoGenerateMips = false,
			useMipMap = false,
			hideFlags = HideFlags.HideAndDontSave,
		};
		this.volumeTextureB.Create();

		this.secondaryIrradianceVolume = new RenderTexture(voxelResolution, voxelResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
		{
			dimension = TextureDimension.Tex3D, 
			volumeDepth = voxelResolution,
			enableRandomWrite = true,
			filterMode = FilterMode.Point,
			autoGenerateMips = false,
			useMipMap = false,
			antiAliasing = 1,
			hideFlags = HideFlags.HideAndDontSave,
		};
		this.secondaryIrradianceVolume.Create();

		base.CreateVolumeTextures();
	}

	protected override void LoadResources()
	{
		//Setup shaders and materials
		this.sunDepthShader = LoadAsset<Shader>("SEGIRenderSunDepth");
		this.clearCompute = LoadAsset<ComputeShader>("SEGIClear");
		this.transferIntsCompute = LoadAsset<ComputeShader>("SEGITransferInts");
		this.mipFilterCompute = LoadAsset<ComputeShader>("SEGIMipFilter");
		this.voxelizationShader = LoadAsset<Shader>("SEGIVoxelizeScene");
		this.voxelTracingShader = LoadAsset<Shader>("SEGITraceScene");

		if(!this.material.IsValid())
		{
			var segi = LoadAsset<Shader>("SEGI");
			this.material = new Material(segi) { hideFlags = HideFlags.HideAndDontSave };
		}
	}

	protected override void Init()
	{
		LoadResources();
		base.Init();
		this.voxelCamera!.aspect = 1;

		this.initialized = true;
	}

	protected override void CleanupTextures()
	{
		base.CleanupTextures();
		foreach(var texture in this.volumeTextures ?? [])
			DestroyTexture(texture);
		this.volumeTextures = [];
		DestroyTexture(this.secondaryIrradianceVolume);
		DestroyTexture(this.volumeTextureB);
	}
}
