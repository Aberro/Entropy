// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using UnityEngine;
using UnityEngine.Rendering;
using JetBrains.Annotations;
using Entropy.Scripts.Utilities;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Sonic Ether/SEGI (Cascaded)")]
public class SEGICascaded : SEGI
{
	#region Internal variables
	///<summary>The main GI data clipmaps that hold GI data referenced during GI tracing</summary>
	Clipmap[]? clipmaps;
	///<summary>The secondary clipmaps that hold irradiance data for infinite bounces</summary>
	Clipmap[]? irradianceClipmaps;

	int clipmapCounter = 0;
	int currentClipmapIndex = 0;
	#endregion

	#region SupportingObjectsAndProperties

	/// <summary>
	/// Estimates the VRAM usage of all the render textures used to render GI.
	/// </summary>
	public override long VRAMUsage  //TODO: Update vram usage calculation
	{
		get
		{
			var v = base.VRAMUsage;

			if (this.sunDepthTexture != null)
				v += this.sunDepthTexture.width * this.sunDepthTexture.height * 32;

			if (this.clipmaps != null)
				for (var i = 0; i < NumClipMaps; i++)
					if (this.clipmaps[i].initialized)
						v += this.clipmaps[i].volumeTexture0.width * this.clipmaps[i].volumeTexture0.height * this.clipmaps[i].volumeTexture0.volumeDepth * 16 * 4;

			if (this.irradianceClipmaps != null)
				for (var i = 0; i < NumClipMaps; i++)
					if (this.irradianceClipmaps[i].initialized)
						v += this.irradianceClipmaps[i].volumeTexture0.width * this.irradianceClipmaps[i].volumeTexture0.height * this.irradianceClipmaps[i].volumeTexture0.volumeDepth * 16 * 4;

			return v;
		}
	}

	struct Clipmap
	{
		public bool initialized = false;
		public Vector3 origin;
		public Vector3 originDelta;
		public Vector3 previousOrigin;
		public float localScale;

		public int resolution;

		public RenderTexture volumeTexture0 = null!;

		public FilterMode filterMode = FilterMode.Bilinear;
		public RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGBHalf;

		public Clipmap() { }

		public void UpdateTextures()
		{
			DestroyTexture();
			this.volumeTexture0 = new RenderTexture(this.resolution, this.resolution, 0, this.renderTextureFormat, RenderTextureReadWrite.Linear)
			{
				wrapMode = TextureWrapMode.Clamp,
				dimension = TextureDimension.Tex3D,
				volumeDepth = this.resolution,
				enableRandomWrite = true,
				filterMode = this.filterMode,
				autoGenerateMips = false,
				useMipMap = false,
				hideFlags = HideFlags.HideAndDontSave
			};
			this.volumeTexture0.Create();
		}

		public void DestroyTexture()
		{
			if (this.volumeTexture0 != null && this.volumeTexture0)
			{
				this.volumeTexture0.DiscardContents();
				this.volumeTexture0.Release();
				DestroyImmediate(this.volumeTexture0);
			}
		}
	}

	protected override int DummyVoxelResolution => (int) this.voxelResolution * (this.voxelAA ? 4 : 1);
	protected override int GlobalVoxelAA => this.voxelAA ? 3 : 0;
	protected override bool DisableShadowRendering => true;
	#endregion

	#region Unity messages
	[UsedImplicitly]
	private void OnDrawGizmosSelected()
	{
		if(!enabled || this.clipmaps == null)
			return;

		var prevColor = Gizmos.color;
		Gizmos.color = new Color(1.0f, 0.25f, 0.0f, 0.5f);
		var scale = this.clipmaps[NumClipMaps - 1].localScale;
		Gizmos.DrawCube(this.clipmaps[0].origin, new Vector3(this.voxelSpaceSize * scale, this.voxelSpaceSize * scale, this.voxelSpaceSize * scale));
		Gizmos.color = prevColor;
	}

	protected override void Update()
	{
		this.doReflections = false;   //Locked to keep reflections disabled since they're in a broken state with cascades at the moment
		this.gaussianMipFilter = false;
		base.Update();

		if(this.clipmaps != null && this.clipmaps[0].resolution != (int)this.voxelResolution)
		{
			this.clipmaps[0].resolution = (int)this.voxelResolution;
			this.clipmaps[0].UpdateTextures();
		}
	}

	protected override void OnPreRender()
	{
		//Force reinitialization to make sure that everything is working properly if one of the cameras was unexpectedly destroyed
		if(this.notReadyToRender || !this.followTransform.IsValid() || !this.voxelCamera.IsValid()
			|| !this.voxelCameraGO.IsValid() || !this.leftViewPoint.IsValid() || !this.topViewPoint.IsValid() || !this.attachedCamera.IsValid()
			|| !this.shadowCamTransform.IsValid() || !this.shadowCam.IsValid() || !this.sunDepthTexture.IsValid() || !this.clearCompute.IsValid()
			|| !this.transferIntsCompute.IsValid() || !this.mipFilterCompute.IsValid())
			this.initialized = false;

		base.OnPreRender();
	}
	#endregion

	public override void ApplyPreset(SEGIPreset preset)
	{
		base.ApplyPreset(preset);
		this.doReflections = false;   //Locked to keep reflections disabled since they're in a broken state with cascades at the moment
		this.gaussianMipFilter = false;

	}

	protected override void LoadResources()
	{
		//Setup shaders and materials
		this.sunDepthShader = LoadAsset<Shader>("SEGIRenderSunDepth_C");
		this.clearCompute = LoadAsset<ComputeShader>("SEGIClear_C");
		this.transferIntsCompute = LoadAsset<ComputeShader>("SEGITransferInts_C");
		this.mipFilterCompute = LoadAsset<ComputeShader>("SEGIMipFilter_C");
		this.voxelizationShader = LoadAsset<Shader>("SEGIVoxelizeScene_C");
		this.voxelTracingShader = LoadAsset<Shader>("SEGITraceScene_C");

		if(!this.material.IsValid())
		{
			var segi = LoadAsset<Shader>("SEGI_C");
			this.material = new Material(segi) { hideFlags = HideFlags.HideAndDontSave };
		}
	}

	protected override void Init()
	{
		LoadResources();
		base.Init();
		this.attachedCamera!.depthTextureMode |= DepthTextureMode.DepthNormals;

		// Build clipmaps
		if(this.clipmaps != null)
			for(var i = 0; i < NumClipMaps; i++)
				if(this.clipmaps[i].initialized)
					this.clipmaps[i].DestroyTexture();

		this.clipmaps = new Clipmap[NumClipMaps];

		for(var i = 0; i < NumClipMaps; i++)
		{
			this.clipmaps[i] = new Clipmap
			{
				localScale = Mathf.Pow(2.0f, (float)i),
				resolution = (int)this.voxelResolution,
				filterMode = FilterMode.Bilinear,
				renderTextureFormat = RenderTextureFormat.ARGBHalf
			};
			this.clipmaps[i].UpdateTextures();
		}

		if(this.irradianceClipmaps != null)
			for(var i = 0; i < NumClipMaps; i++)
				if(this.irradianceClipmaps[i].initialized)
					this.irradianceClipmaps[i].DestroyTexture();

		this.irradianceClipmaps = new Clipmap[NumClipMaps];

		for(var i = 0; i < NumClipMaps; i++)
		{
			this.irradianceClipmaps[i] = new Clipmap
			{
				localScale = Mathf.Pow(2.0f, i),
				resolution = (int)this.voxelResolution,
				filterMode = FilterMode.Point,
				renderTextureFormat = RenderTextureFormat.ARGBHalf
			};
			this.irradianceClipmaps[i].UpdateTextures();
		}

		this.initialized = true;
	}

	protected override void CleanupTextures()
	{
		base.CleanupTextures();
		if (this.clipmaps != null)
			for (var i = 0; i < NumClipMaps; i++)
				if (this.clipmaps[i].initialized)
					this.clipmaps[i].DestroyTexture();

		if (this.irradianceClipmaps != null)
			for (var i = 0; i < NumClipMaps; i++)
				if (this.irradianceClipmaps[i].initialized)
					this.irradianceClipmaps[i].DestroyTexture();
	}

	int SelectCascadeBinary(int c)
	{
		var counter = c + 0.01f;

		var result = 0;
		for (var i = 1; i < NumClipMaps; i++)
		{
			var level = Mathf.Pow(2.0f, i);
			result += Mathf.CeilToInt((counter / level % 1.0f) - ((level - 1.0f) / level));
		}

		return result;
	}

	protected override void PreRenderVoxelize()
	{
		var prevClipmapRelativeOrigin = Vector3.zero;
		var prevClipmapOccupance = 0.0f;
		this.currentClipmapIndex = SelectCascadeBinary(this.clipmapCounter);      //Determine which clipmap to update during this frame

		ref var activeClipmap = ref this.clipmaps![this.currentClipmapIndex];          //Set the active clipmap based on which one is determined to render this frame
		var clipmapSize = this.voxelSpaceSize * activeClipmap.localScale;  //Determine the current clipmap's size in world units based on its scale

		//If we're not updating the base level 0 clipmap, get the previous clipmap
		if(this.currentClipmapIndex != 0)
		{
			//Calculate the relative origin and overlap/size of the previous cascade as compared to the active cascade.
			//This is used to avoid voxelizing areas that have already been voxelized by previous (smaller) cascades
			prevClipmapRelativeOrigin = (this.clipmaps[this.currentClipmapIndex - 1].origin - activeClipmap.origin) / clipmapSize;
			prevClipmapOccupance = this.clipmaps[this.currentClipmapIndex - 1].localScale / activeClipmap.localScale;
		}

		var clipmapShadowSize = this.shadowSpaceSize * activeClipmap.localScale;
		//float voxelTexel = (1.0f * clipmapSize) / activeClipmap.resolution * 0.5f;	//Calculate the size of a voxel texel in world-space units

		//Setup the voxel volume origin position
		GetVoxelVolumeOriginAndInterval(clipmapSize, out var origin, out var interval);

		//Lock the voxel volume origin based on the interval
		activeClipmap.previousOrigin = activeClipmap.origin;
		activeClipmap.origin = new Vector3(Mathf.Round(origin.x / interval) * interval, Mathf.Round(origin.y / interval) * interval, Mathf.Round(origin.z / interval) * interval);

		//Clipmap delta movement for scrolling secondary bounce irradiance volume when this clipmap has changed origin
		activeClipmap.originDelta = activeClipmap.origin - activeClipmap.previousOrigin;
		Shader.SetGlobalVector(SEGIVoxelSpaceOriginDelta, activeClipmap.originDelta / (this.voxelSpaceSize * activeClipmap.localScale));

		Shader.SetGlobalVector(SEGIClipmapOverlap, new Vector4(prevClipmapRelativeOrigin.x, prevClipmapRelativeOrigin.y, prevClipmapRelativeOrigin.z, prevClipmapOccupance));

		//Calculate the relative origin and scale of this cascade as compared to the first (level 0) cascade. This is used during GI tracing/data lookup to ensure tracing is done in the correct space
		for(var i = 1; i < NumClipMaps; i++)
		{
			var clipPosFromMaster = (this.clipmaps[i].origin - this.clipmaps[0].origin) / (this.voxelSpaceSize * this.clipmaps[i].localScale);
			var clipScaleFromMaster = this.clipmaps[0].localScale / this.clipmaps[i].localScale;

			Shader.SetGlobalVector(SEGIClipTransform[i], new Vector4(clipPosFromMaster.x, clipPosFromMaster.y, clipPosFromMaster.z, clipScaleFromMaster));
		}

		//Set the voxel camera (proxy camera used to render the scene for voxelization) parameters
		SetVoxelCamera(activeClipmap.origin, clipmapSize * 0.5f);

		//Set matrices needed for voxelization
		//Shader.SetGlobalMatrix("WorldToGI", shadowCam.worldToCameraMatrix);
		//Shader.SetGlobalMatrix("GIToWorld", shadowCam.cameraToWorldMatrix);
		//Shader.SetGlobalMatrix("GIProjection", shadowCam.projectionMatrix);
		//Shader.SetGlobalMatrix("GIProjectionInverse", shadowCam.projectionMatrix.inverse);
		Shader.SetGlobalMatrix(WorldToCamera, this.attachedCamera!.worldToCameraMatrix);
		Shader.SetGlobalFloat(GIDepthRatio, this.shadowSpaceDepthRatio);

		var frontViewMatrix = TransformViewMatrix(this.voxelCamera!.transform.worldToLocalMatrix);
		var leftViewMatrix = TransformViewMatrix(this.leftViewPoint!.transform.worldToLocalMatrix);
		var topViewMatrix = TransformViewMatrix(this.topViewPoint!.transform.worldToLocalMatrix);

		Shader.SetGlobalMatrix(SEGIVoxelViewFront, frontViewMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelViewLeft, leftViewMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelViewTop, topViewMatrix);
		Shader.SetGlobalMatrix(SEGIWorldToVoxel, this.voxelCamera.worldToCameraMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelProjection, this.voxelCamera.projectionMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelProjectionInverse, this.voxelCamera.projectionMatrix.inverse);

		Shader.SetGlobalMatrix(SEGIVoxelVPFront, GL.GetGPUProjectionMatrix(this.voxelCamera.projectionMatrix, true) * frontViewMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelVPLeft, GL.GetGPUProjectionMatrix(this.voxelCamera.projectionMatrix, true) * leftViewMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelVPTop, GL.GetGPUProjectionMatrix(this.voxelCamera.projectionMatrix, true) * topViewMatrix);

		Shader.SetGlobalMatrix(SEGIWorldToVoxelCascaded[this.currentClipmapIndex], this.voxelCamera.worldToCameraMatrix);
		Shader.SetGlobalMatrix(SEGIVoxelProjectionCascaded[this.currentClipmapIndex], this.voxelCamera.projectionMatrix);

		var voxelToGIProjection = this.shadowCam!.projectionMatrix * this.shadowCam.worldToCameraMatrix * this.voxelCamera.cameraToWorldMatrix;
		Shader.SetGlobalMatrix(SEGIVoxelToGIProjection, voxelToGIProjection);
		Shader.SetGlobalVector(SEGISunlightVector, this.sun.IsValid() ? Vector3.Normalize(this.sun.transform.forward) : Vector3.up);

		//Set paramteters
		Shader.SetGlobalInt(SEGIVoxelResolution, (int)this.voxelResolution);

		Shader.SetGlobalColor(GISunColor, this.sun == null ? Color.black : new Color(Mathf.Pow(this.sun.color.r, 2.2f), Mathf.Pow(this.sun.color.g, 2.2f), Mathf.Pow(this.sun.color.b, 2.2f), Mathf.Pow(this.sun.intensity, 2.2f)));
		Shader.SetGlobalColor(SEGISkyColor, new Color(Mathf.Pow(this.skyColor.r * this.skyIntensity * 0.5f, 2.2f), Mathf.Pow(this.skyColor.g * this.skyIntensity * 0.5f, 2.2f), Mathf.Pow(this.skyColor.b * this.skyIntensity * 0.5f, 2.2f), Mathf.Pow(this.skyColor.a, 2.2f)));
		Shader.SetGlobalFloat(GIGain, this.giGain);
		Shader.SetGlobalFloat(SEGISecondaryBounceGain, this.infiniteBounces ? this.secondaryBounceGain : 0.0f);
		Shader.SetGlobalFloat(SEGISoftSunlight, this.softSunlight);
		Shader.SetGlobalInt(SEGISphericalSkylight, this.sphericalSkylight ? 1 : 0);
		Shader.SetGlobalInt(SEGIInnerOcclusionLayers, this.innerOcclusionLayers);

		//Render the depth texture from the sun's perspective in order to inject sunlight with shadows during voxelization
		if(this.sun.IsValid())
		{
			this.shadowCam.cullingMask = this.giCullingMask;

			var shadowCamPosition = activeClipmap.origin + (Vector3.Normalize(-this.sun.transform.forward) * (clipmapShadowSize * 0.5f * this.shadowSpaceDepthRatio));

			this.shadowCamTransform!.position = shadowCamPosition;
			this.shadowCamTransform.LookAt(activeClipmap.origin, Vector3.up);

			this.shadowCam.renderingPath = RenderingPath.Forward;
			this.shadowCam.depthTextureMode |= DepthTextureMode.None;

			this.shadowCam.orthographicSize = clipmapShadowSize;
			this.shadowCam.farClipPlane = clipmapShadowSize * 2.0f * this.shadowSpaceDepthRatio;

			//Shader.SetGlobalMatrix("WorldToGI", shadowCam.worldToCameraMatrix);
			//Shader.SetGlobalMatrix("GIToWorld", shadowCam.cameraToWorldMatrix);
			//Shader.SetGlobalMatrix("GIProjection", shadowCam.projectionMatrix);
			//Shader.SetGlobalMatrix("GIProjectionInverse", shadowCam.projectionMatrix.inverse);
			voxelToGIProjection = this.shadowCam.projectionMatrix * this.shadowCam.worldToCameraMatrix * this.voxelCamera.cameraToWorldMatrix;
			Shader.SetGlobalMatrix(SEGIVoxelToGIProjection, voxelToGIProjection);

			Graphics.SetRenderTarget(this.sunDepthTexture);
			this.shadowCam.SetTargetBuffers(this.sunDepthTexture!.colorBuffer, this.sunDepthTexture.depthBuffer);

			this.shadowCam.RenderWithShader(this.sunDepthShader, "");

			Shader.SetGlobalTexture(SEGISunDepth, this.sunDepthTexture);
		}

		//Clear the volume texture that is immediately written to in the voxelization scene shader
		this.clearCompute!.SetTexture(0, RG0, this.integerVolume);
		this.clearCompute.SetInt(Res, activeClipmap.resolution);
		this.clearCompute.Dispatch(0, Math.Max(1, activeClipmap.resolution / 16), Math.Max(1, activeClipmap.resolution / 16), 1);

		//Set irradiance "secondary bounce" texture
		Shader.SetGlobalTexture(SEGICurrentIrradianceVolume, this.irradianceClipmaps![this.currentClipmapIndex].volumeTexture0);

		Graphics.SetRandomWriteTarget(1, this.integerVolume);
		this.voxelCamera.targetTexture = this.dummyVoxelTextureAAScaled;
		this.voxelCamera.RenderWithShader(this.voxelizationShader, "");
		Graphics.ClearRandomWriteTargets();

		//Transfer the data from the volume integer texture to the main volume texture used for GI tracing.
		this.transferIntsCompute!.SetTexture(0, Result, activeClipmap.volumeTexture0);
		this.transferIntsCompute.SetTexture(0, RG0, this.integerVolume);
		this.transferIntsCompute.SetInt(VoxelAA, this.voxelAA ? 3 : 0);
		this.transferIntsCompute.SetInt(Resolution, activeClipmap.resolution);
		this.transferIntsCompute.Dispatch(0, Math.Max(1, activeClipmap.resolution / 16), Math.Max(1, activeClipmap.resolution / 16), 1);

		//Push current voxelization result to higher levels
		for(var i = 1; i < NumClipMaps; i++)
		{
			var sourceClipmap = this.clipmaps[i - 1];
			var targetClipmap = this.clipmaps[i];

			var sourceRelativeOrigin = (sourceClipmap.origin - targetClipmap.origin) / (targetClipmap.localScale * this.voxelSpaceSize);
			var sourceOccupance = sourceClipmap.localScale / targetClipmap.localScale;

			this.mipFilterCompute!.SetTexture(0, Source, sourceClipmap.volumeTexture0);
			this.mipFilterCompute.SetTexture(0, Destination, targetClipmap.volumeTexture0);
			this.mipFilterCompute.SetInt(destinationRes, targetClipmap.resolution);
			this.mipFilterCompute.SetVector(ClipmapOverlap, new Vector4(sourceRelativeOrigin.x, sourceRelativeOrigin.y, sourceRelativeOrigin.z, sourceOccupance));
			this.mipFilterCompute.Dispatch(0, Math.Max(1, targetClipmap.resolution / 16), Math.Max(1, targetClipmap.resolution / 16), 1);
		}

		for(var i = 0; i < NumClipMaps; i++)
			Shader.SetGlobalTexture(SEGIVolumeLevel[i], this.clipmaps[i].volumeTexture0);

		if(!this.infiniteBounces)
		{
			//Increment clipmap counter
			this.clipmapCounter++;
			if(this.clipmapCounter >= (int)Mathf.Pow(2.0f, NumClipMaps))
				this.clipmapCounter = 0;
		}
	}

	protected override void PreRenderBounce()
	{
		if(!this.clearCompute.IsValid() || !this.voxelCamera.IsValid() || !this.transferIntsCompute.IsValid())
			return;
		//Calculate the relative position and scale of the current clipmap as compared to the first (level 0) clipmap. Used to ensure tracing is performed in the correct space
		var translateToZero = (this.clipmaps![this.currentClipmapIndex].origin - this.clipmaps[0].origin) / (this.voxelSpaceSize * this.clipmaps[this.currentClipmapIndex].localScale);
		var scaleToZero = 1.0f / this.clipmaps[this.currentClipmapIndex].localScale;
		Shader.SetGlobalVector(SEGICurrentClipTransform, new Vector4(translateToZero.x, translateToZero.y, translateToZero.z, scaleToZero));

		//Clear the volume texture that is immediately written to in the voxelization scene shader
		this.clearCompute.SetTexture(0, RG0, this.integerVolume);
		this.clearCompute.SetInt(Res, this.clipmaps[this.currentClipmapIndex].resolution);
		this.clearCompute.Dispatch(0, Math.Max(1, (int)this.voxelResolution / 16), Math.Max(1, (int)this.voxelResolution / 16), 1);

		//Only render infinite bounces for clipmaps 0, 1, and 2
		if(this.currentClipmapIndex <= 2)
		{
			Shader.SetGlobalInt(SEGISecondaryCones, this.secondaryCones);
			Shader.SetGlobalFloat(SEGISecondaryOcclusionStrength, this.secondaryOcclusionStrength);

			Graphics.SetRandomWriteTarget(1, this.integerVolume);
			this.voxelCamera.targetTexture = this.dummyVoxelTextureFixed;
			this.voxelCamera.RenderWithShader(this.voxelTracingShader, "");
			Graphics.ClearRandomWriteTargets();

			this.transferIntsCompute.SetTexture(1, Result, this.irradianceClipmaps![this.currentClipmapIndex].volumeTexture0);
			this.transferIntsCompute.SetTexture(1, RG0, this.integerVolume);
			this.transferIntsCompute.SetInt(Resolution, (int)this.voxelResolution);
			this.transferIntsCompute.Dispatch(1, Math.Max(1, (int)this.voxelResolution / 16), Math.Max(1, (int)this.voxelResolution / 16), 1);
		}

		//Increment clipmap counter
		this.clipmapCounter++;
		if(this.clipmapCounter >= (int)Mathf.Pow(2.0f, NumClipMaps))
			this.clipmapCounter = 0;
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

			//Perform a bilateral blur to be applied in newly revealed areas that are still noisy due to not having previous data blended with it
			var blur0 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
			var blur1 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
			this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));
			Graphics.Blit(gi3, blur1, this.material, Pass.BilateralBlur);

			this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
			Graphics.Blit(blur1, blur0, this.material, Pass.BilateralBlur);

			this.material.SetVector(Kernel, new Vector2(0.0f, 2.0f));
			Graphics.Blit(blur0, blur1, this.material, Pass.BilateralBlur);

			this.material.SetVector(Kernel, new Vector2(2.0f, 0.0f));
			Graphics.Blit(blur1, blur0, this.material, Pass.BilateralBlur);

			this.material.SetTexture(BlurredGI, blur0);

			//Perform temporal reprojection and blending
			if(this.temporalBlendWeight < 1.0f)
			{
				Graphics.Blit(gi3, gi4);
				Graphics.Blit(gi4, gi3, this.material, Pass.TemporalBlend);
				Graphics.Blit(gi3, this.previousGIResult);
				Graphics.Blit(source, this.previousCameraDepth, this.material, Pass.GetCameraDepthTexture);

				//Perform bilateral filtering on temporally blended result
				if(this.useBilateralFiltering)
				{
					this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));
					Graphics.Blit(gi3, gi4, this.material, Pass.BilateralBlur);

					this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
					Graphics.Blit(gi4, gi3, this.material, Pass.BilateralBlur);

					this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));
					Graphics.Blit(gi3, gi4, this.material, Pass.BilateralBlur);

					this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
					Graphics.Blit(gi4, gi3, this.material, Pass.BilateralBlur);
				}
			}

			//Set the result to be accessed in the shader
			this.material.SetTexture(GITexture, gi3);

			//Actually apply the GI to the scene using gbuffer data
			Graphics.Blit(source, destination, this.material, this.visualizeGI ? Pass.VisualizeGI : Pass.BlendWithScene);

			//Release temporary textures
			RenderTexture.ReleaseTemporary(blur0);
			RenderTexture.ReleaseTemporary(blur1);
			RenderTexture.ReleaseTemporary(gi3);
			RenderTexture.ReleaseTemporary(gi4);
		}
		else    //If Half Resolution tracing is disabled
		{
			if(this.temporalBlendWeight < 1.0f)
			{
				//Perform a bilateral blur to be applied in newly revealed areas that are still noisy due to not having previous data blended with it
				var blur0 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
				var blur1 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
				this.material!.SetVector(Kernel, new Vector2(0.0f, 1.0f));
				Graphics.Blit(gi2, blur1, this.material, Pass.BilateralBlur);

				this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
				Graphics.Blit(blur1, blur0, this.material, Pass.BilateralBlur);

				this.material.SetVector(Kernel, new Vector2(0.0f, 2.0f));
				Graphics.Blit(blur0, blur1, this.material, Pass.BilateralBlur);

				this.material.SetVector(Kernel, new Vector2(2.0f, 0.0f));
				Graphics.Blit(blur1, blur0, this.material, Pass.BilateralBlur);

				this.material.SetTexture(BlurredGI, blur0);

				//Perform temporal reprojection and blending
				Graphics.Blit(gi2, gi1, this.material, Pass.TemporalBlend);
				Graphics.Blit(gi1, this.previousGIResult);
				Graphics.Blit(source, this.previousCameraDepth, this.material, Pass.GetCameraDepthTexture);

				//Perform bilateral filtering on temporally blended result
				if(this.useBilateralFiltering)
				{
					this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));
					Graphics.Blit(gi1, gi2, this.material, Pass.BilateralBlur);

					this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
					Graphics.Blit(gi2, gi1, this.material, Pass.BilateralBlur);

					this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));
					Graphics.Blit(gi1, gi2, this.material, Pass.BilateralBlur);

					this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
					Graphics.Blit(gi2, gi1, this.material, Pass.BilateralBlur);
				}

				RenderTexture.ReleaseTemporary(blur0);
				RenderTexture.ReleaseTemporary(blur1);
			}

			//Actually apply the GI to the scene using gbuffer data
			this.material!.SetTexture(GITexture, this.temporalBlendWeight < 1.0f ? gi1 : gi2);
			Graphics.Blit(source, destination, this.material, this.visualizeGI ? Pass.VisualizeGI : Pass.BlendWithScene);
		}
	}
}