// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using Entropy.Scripts.SEGI;
using Entropy.Scripts.Utilities;
using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public abstract class SEGI : MonoBehaviour
{
	#region Shader properties
	protected static readonly int SEGIVoxelAA = Shader.PropertyToID("SEGIVoxelAA");
	protected static readonly int SEGIVoxelSpaceOriginDelta = Shader.PropertyToID("SEGIVoxelSpaceOriginDelta");
	protected static readonly int WorldToCamera = Shader.PropertyToID("WorldToCamera");
	protected static readonly int SEGIVoxelViewFront = Shader.PropertyToID("SEGIVoxelViewFront");
	protected static readonly int SEGIVoxelViewLeft = Shader.PropertyToID("SEGIVoxelViewLeft");
	protected static readonly int SEGIVoxelViewTop = Shader.PropertyToID("SEGIVoxelViewTop");
	protected static readonly int SEGIWorldToVoxel = Shader.PropertyToID("SEGIWorldToVoxel");
	protected static readonly int[] SEGIWorldToVoxelCascaded =
	[
		Shader.PropertyToID("SEGIWorldToVoxel0"),
		Shader.PropertyToID("SEGIWorldToVoxel1"),
		Shader.PropertyToID("SEGIWorldToVoxel2"),
		Shader.PropertyToID("SEGIWorldToVoxel3"),
		Shader.PropertyToID("SEGIWorldToVoxel4"),
		Shader.PropertyToID("SEGIWorldToVoxel5"),
	];
	protected static readonly int SEGIVoxelProjection = Shader.PropertyToID("SEGIVoxelProjection");
	protected static readonly int[] SEGIVoxelProjectionCascaded =
	[
		Shader.PropertyToID("SEGIVoxelProjection0"),
		Shader.PropertyToID("SEGIVoxelProjection1"),
		Shader.PropertyToID("SEGIVoxelProjection2"),
		Shader.PropertyToID("SEGIVoxelProjection3"),
		Shader.PropertyToID("SEGIVoxelProjection4"),
		Shader.PropertyToID("SEGIVoxelProjection5"),
	];
	protected static readonly int SEGIVoxelProjectionInverse = Shader.PropertyToID("SEGIVoxelProjectionInverse");
	protected static readonly int SEGIVoxelResolution = Shader.PropertyToID("SEGIVoxelResolution");
	protected static readonly int SEGIVoxelToGIProjection = Shader.PropertyToID("SEGIVoxelToGIProjection");
	protected static readonly int SEGISunlightVector = Shader.PropertyToID("SEGISunlightVector");
	protected static readonly int GISunColor = Shader.PropertyToID("GISunColor");
	protected static readonly int SEGISkyColor = Shader.PropertyToID("SEGISkyColor");
	protected static readonly int GIGain = Shader.PropertyToID("GIGain");
	protected static readonly int SEGISecondaryBounceGain = Shader.PropertyToID("SEGISecondaryBounceGain");
	protected static readonly int SEGISoftSunlight = Shader.PropertyToID("SEGISoftSunlight");
	protected static readonly int SEGISphericalSkylight = Shader.PropertyToID("SEGISphericalSkylight");
	protected static readonly int SEGIInnerOcclusionLayers = Shader.PropertyToID("SEGIInnerOcclusionLayers");
	protected static readonly int SEGISunDepth = Shader.PropertyToID("SEGISunDepth");

	protected const int NumMipLevels = 6;
	protected static readonly int[] SEGIVolumeLevel =
	[
		Shader.PropertyToID("SEGIVolumeLevel0"),
		Shader.PropertyToID("SEGIVolumeLevel1"),
		Shader.PropertyToID("SEGIVolumeLevel2"),
		Shader.PropertyToID("SEGIVolumeLevel3"),
		Shader.PropertyToID("SEGIVolumeLevel4"),
		Shader.PropertyToID("SEGIVolumeLevel5"),
	];

	protected static readonly int RG0 = Shader.PropertyToID("RG0");
	protected static readonly int Res = Shader.PropertyToID("Res");
	protected static readonly int Result = Shader.PropertyToID("Result");
	protected static readonly int PrevResult = Shader.PropertyToID("PrevResult");
	protected static readonly int VoxelAA = Shader.PropertyToID("VoxelAA");
	protected static readonly int Resolution = Shader.PropertyToID("Resolution");
	protected static readonly int VoxelOriginDelta = Shader.PropertyToID("VoxelOriginDelta");
	protected static readonly int destinationRes = Shader.PropertyToID("destinationRes");
	protected static readonly int Source = Shader.PropertyToID("Source");
	protected static readonly int Destination = Shader.PropertyToID("Destination");
	protected static readonly int SEGISecondaryCones = Shader.PropertyToID("SEGISecondaryCones");
	protected static readonly int SEGISecondaryOcclusionStrength = Shader.PropertyToID("SEGISecondaryOcclusionStrength");
	protected static readonly int SEGIVolumeTexture1 = Shader.PropertyToID("SEGIVolumeTexture1");

	protected static readonly int SEGIVoxelScaleFactor = Shader.PropertyToID("SEGIVoxelScaleFactor");
	protected static readonly int CameraToWorld = Shader.PropertyToID("CameraToWorld");
	protected static readonly int ProjectionMatrixInverse = Shader.PropertyToID("ProjectionMatrixInverse");
	protected static readonly int ProjectionMatrix = Shader.PropertyToID("ProjectionMatrix");
	protected static readonly int FrameSwitch = Shader.PropertyToID("FrameSwitch");
	protected static readonly int SEGIFrameSwitch = Shader.PropertyToID("SEGIFrameSwitch");
	protected static readonly int CameraPosition = Shader.PropertyToID("CameraPosition");
	protected static readonly int DeltaTime = Shader.PropertyToID("DeltaTime");
	protected static readonly int StochasticSampling = Shader.PropertyToID("StochasticSampling");
	protected static readonly int TraceDirections = Shader.PropertyToID("TraceDirections");
	protected static readonly int TraceSteps = Shader.PropertyToID("TraceSteps");
	protected static readonly int TraceLength = Shader.PropertyToID("TraceLength");
	protected static readonly int ConeSize = Shader.PropertyToID("ConeSize");
	protected static readonly int OcclusionStrength = Shader.PropertyToID("OcclusionStrength");
	protected static readonly int OcclusionPower = Shader.PropertyToID("OcclusionPower");
	protected static readonly int ConeTraceBias = Shader.PropertyToID("ConeTraceBias");
	protected static readonly int NearLightGain = Shader.PropertyToID("NearLightGain");
	protected static readonly int NearOcclusionStrength = Shader.PropertyToID("NearOcclusionStrength");
	protected static readonly int DoReflections = Shader.PropertyToID("DoReflections");
	protected static readonly int HalfResolution = Shader.PropertyToID("HalfResolution");
	protected static readonly int ReflectionSteps = Shader.PropertyToID("ReflectionSteps");
	protected static readonly int ReflectionOcclusionPower = Shader.PropertyToID("ReflectionOcclusionPower");
	protected static readonly int SkyReflectionIntensity = Shader.PropertyToID("SkyReflectionIntensity");
	protected static readonly int FarOcclusionStrength = Shader.PropertyToID("FarOcclusionStrength");
	protected static readonly int FarthestOcclusionStrength = Shader.PropertyToID("FarthestOcclusionStrength");
	protected static readonly int NoiseTexture = Shader.PropertyToID("NoiseTexture");
	protected static readonly int BlendWeight = Shader.PropertyToID("BlendWeight");

	protected static readonly int CurrentDepth = Shader.PropertyToID("CurrentDepth");
	protected static readonly int CurrentNormal = Shader.PropertyToID("CurrentNormal");
	protected static readonly int PreviousGITexture = Shader.PropertyToID("PreviousGITexture");
	protected static readonly int PreviousDepth = Shader.PropertyToID("PreviousDepth");
	protected static readonly int Reflections = Shader.PropertyToID("Reflections");
	protected static readonly int Kernel = Shader.PropertyToID("Kernel");
	protected static readonly int GITexture = Shader.PropertyToID("GITexture");
	protected static readonly int ProjectionPrev = Shader.PropertyToID("ProjectionPrev");
	protected static readonly int ProjectionPrevInverse = Shader.PropertyToID("ProjectionPrevInverse");
	protected static readonly int WorldToCameraPrev = Shader.PropertyToID("WorldToCameraPrev");
	protected static readonly int CameraToWorldPrev = Shader.PropertyToID("CameraToWorldPrev");
	protected static readonly int CameraPositionPrev = Shader.PropertyToID("CameraPositionPrev");

	protected static readonly int SEGIClipmapOverlap = Shader.PropertyToID("SEGIClipmapOverlap");

	protected const int NumClipMaps = 6;
	protected static readonly int[] SEGIClipTransform =
	[
		Shader.PropertyToID("SEGIClipTransform0"),
		Shader.PropertyToID("SEGIClipTransform1"),
		Shader.PropertyToID("SEGIClipTransform2"),
		Shader.PropertyToID("SEGIClipTransform3"),
		Shader.PropertyToID("SEGIClipTransform4"),
		Shader.PropertyToID("SEGIClipTransform5"),
	];
	protected static readonly int GIDepthRatio = Shader.PropertyToID("GIDepthRatio");
	protected static readonly int SEGIVoxelVPFront = Shader.PropertyToID("SEGIVoxelVPFront");
	protected static readonly int SEGIVoxelVPLeft = Shader.PropertyToID("SEGIVoxelVPLeft");
	protected static readonly int SEGIVoxelVPTop = Shader.PropertyToID("SEGIVoxelVPTop");
	protected static readonly int SEGICurrentIrradianceVolume = Shader.PropertyToID("SEGICurrentIrradianceVolume");
	protected static readonly int ClipmapOverlap = Shader.PropertyToID("ClipmapOverlap");
	protected static readonly int SEGICurrentClipTransform = Shader.PropertyToID("SEGICurrentClipTransform");
	protected static readonly int GIToVoxelProjection = Shader.PropertyToID("GIToVoxelProjection");
	protected static readonly int BlurredGI = Shader.PropertyToID("BlurredGI");
	#endregion

	#region Parameters
	public bool updateGI = true;
	public LayerMask giCullingMask = 2147483647;
	public VoxelResolution voxelResolution = VoxelResolution.high;
	public float shadowSpaceSize = 50.0f;
	public Color skyColor;
	public float voxelSpaceSize = 25.0f;
	public bool useBilateralFiltering;
	public bool halfResolution = true;
	public bool stochasticSampling = true;
	public bool infiniteBounces;
	public bool doReflections = true;
	public bool sphericalSkylight = false;

	public bool visualizeSunDepthTexture = false;
	public bool visualizeGI = false;
	public bool visualizeVoxels = false;

	[Range(0, 2)]
	public int innerOcclusionLayers = 1;
	[Range(0.01f, 1.0f)]
	public float temporalBlendWeight = 0.1f;
	[Range(1, 128)]
	public int cones = 4;
	[Range(1, 32)]
	public int coneTraceSteps = 10;
	[Range(0.1f, 2.0f)]
	public float coneLength = 1.0f;
	[Range(0.5f, 6.0f)]
	public float coneWidth = 3.9f;
	[Range(0.0f, 2.0f)]
	public float occlusionStrength = 1.0f;
	[Range(0.0f, 4.0f)]
	public float nearOcclusionStrength = 0.5f;
	[Range(0.001f, 4.0f)]
	public float occlusionPower = 0.65f;
	[Range(0.0f, 4.0f)]
	public float coneTraceBias = 1f;
	[Range(0.0f, 4.0f)]
	public float nearLightGain = 1f;
	[Range(0.0f, 4.0f)]
	public float giGain = 1.0f;
	[Range(0.0f, 4.0f)]
	public float secondaryBounceGain = 0.9f;
	[Range(0.0f, 16.0f)]
	public float softSunlight = 0.0f;
	[Range(0.0f, 8.0f)]
	public float skyIntensity = 1.0f;
	[Range(12, 128)]
	public int reflectionSteps = 64;
	[Range(0.001f, 4.0f)]
	public float reflectionOcclusionPower = 1.0f;
	[Range(0.0f, 1.0f)]
	public float skyReflectionIntensity = 1.0f;
	[Range(0.1f, 4.0f)]
	public float farOcclusionStrength = 1.0f;
	[Range(0.1f, 4.0f)]
	public float farthestOcclusionStrength = 1.0f;
	[Range(3, 16)]
	public int secondaryCones = 6;
	[Range(0.1f, 2.0f)]
	public float secondaryOcclusionStrength = 1.0f;
	public ShadowMapResolution sunShadowResolution = ShadowMapResolution.medium;
	[Range(0f, 20f)]
	public float shadowSpaceDepthRatio = 10.0f;
	public bool voxelAA;
	public bool gaussianMipFilter;

	public Light? sun;
	public Transform? followTransform;
	#endregion

	#region Internal variables
	protected int frameCounter;
	protected bool initialized;
	protected bool notReadyToRender;
	protected Material? material;
	protected Camera? attachedCamera;
	protected Transform? shadowCamTransform;
	protected Camera? shadowCam;
	protected GameObject? shadowCamGameObject;
	protected Texture2D?[]? blueNoise;
	protected ShadowMapResolution prevSunShadowResolution = (ShadowMapResolution)0;
	protected RenderTexture? previousGIResult;
	protected RenderTexture? previousCameraDepth;
	protected RenderTexture? sunDepthTexture;
	protected Camera? voxelCamera;
	protected GameObject? voxelCameraGO;
	protected GameObject? leftViewPoint;
	protected GameObject? topViewPoint;
	protected Shader? voxelizationShader;
	protected Shader? voxelTracingShader;
	protected Shader? sunDepthShader;
	protected ComputeShader? clearCompute;
	protected ComputeShader? transferIntsCompute;
	protected ComputeShader? mipFilterCompute;

	///<summary>This is a volume texture that is immediately written to in the voxelization shader. The RInt format enables atomic writes to avoid issues where multiple fragments are trying to write to the same voxel in the volume.</summary>
	protected RenderTexture? integerVolume;

	///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture scales depending on whether Voxel AA is enabled to ensure correct voxelization.</summary>
	protected RenderTexture? dummyVoxelTextureAAScaled;

	///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture is always the same size whether Voxel AA is enabled or not.</summary>
	protected RenderTexture? dummyVoxelTextureFixed;
	#endregion

	#region Supporting objects and properties
	protected readonly struct Pass
	{
		public static readonly int DiffuseTrace = 0;
		public static readonly int BilateralBlur = 1;
		public static readonly int BlendWithScene = 2;
		public static readonly int TemporalBlend = 3;
		public static readonly int SpecularTrace = 4;
		public static readonly int GetCameraDepthTexture = 5;
		public static readonly int GetWorldNormals = 6;
		public static readonly int VisualizeGI = 7;
		[PublicAPI]
		public static readonly int WriteBlack = 8;
		public static readonly int VisualizeVoxels = 10;
		public static readonly int BilateralUpsample = 11;
	}

	public struct SystemSupported
	{
		public bool hdrTextures;
		public bool rIntTextures;
		public bool dx11;
		public bool volumeTextures;
		public bool postShader;
		public bool sunDepthShader;
		public bool voxelizationShader;
		public bool tracingShader;

		public readonly bool FullFunctionality => this.hdrTextures && this.rIntTextures && this.dx11 && this.volumeTextures && this.postShader && this.sunDepthShader && this.voxelizationShader && this.tracingShader;
	}

	protected enum RenderState
	{
		Voxelize,
		Bounce
	}

	/// <summary>
	/// Contains info on system compatibility of required hardware functionality
	/// </summary>
	public SystemSupported systemSupported;
	protected RenderState renderState = RenderState.Voxelize;

	protected Quaternion rotationFront = new(0.0f, 0.0f, 0.0f, 1.0f);
	protected Quaternion rotationLeft = new(0.0f, 0.7f, 0.0f, 0.7f);
	protected Quaternion rotationTop = new(0.7f, 0.0f, 0.0f, 0.7f);
	protected int GIRenderRes => this.halfResolution ? 2 : 1;
	protected float VoxelScaleFactor => (float)this.voxelResolution / 256.0f;

	/// <summary>
	/// Estimates the VRAM usage of all the render textures used to render GI.
	/// </summary>
	public virtual long VRAMUsage
	{
		get
		{
			long v = 0;

			if(this.previousGIResult.IsValid())
				v += this.previousGIResult.width * this.previousGIResult.height * 16 * 4;

			if(this.previousCameraDepth.IsValid())
				v += this.previousCameraDepth.width * this.previousCameraDepth.height * 32;

			if(this.integerVolume.IsValid())
				v += this.integerVolume.width * this.integerVolume.height * this.integerVolume.volumeDepth * 32;

			if(this.dummyVoxelTextureAAScaled.IsValid())
				v += this.dummyVoxelTextureAAScaled.width * this.dummyVoxelTextureAAScaled.height * 8;

			if(this.dummyVoxelTextureFixed.IsValid())
				v += this.dummyVoxelTextureFixed.width * this.dummyVoxelTextureFixed.height * 8;

			return v;
		}
	}

	protected int MipFilterKernel => this.gaussianMipFilter ? 1 : 0;

	protected abstract int DummyVoxelResolution { get; }

	protected abstract int GlobalVoxelAA { get; }
	protected abstract bool DisableShadowRendering { get; }
	#endregion

	#region Unity messages
	[UsedImplicitly]
	protected virtual void OnEnable()
	{
		InitCheck();
		ResizeRenderTextures();

		CheckSupport();
	}

	[UsedImplicitly]
	protected void OnDisable() => Cleanup();
	[UsedImplicitly]
	protected void Start() => InitCheck();
	[UsedImplicitly]
	protected virtual void Update()
	{
		if(this.notReadyToRender || !this.attachedCamera.IsValid())
			return;

		if(!this.previousGIResult.IsValid()
			|| this.previousGIResult.width != this.attachedCamera.pixelWidth
			|| this.previousGIResult.height != this.attachedCamera.pixelHeight)
			ResizeRenderTextures();

		if(this.sunShadowResolution != this.prevSunShadowResolution)
		{
			ResizeSunShadowBuffer();
			this.prevSunShadowResolution = this.sunShadowResolution;
		}
		if(!this.dummyVoxelTextureAAScaled.IsValid() || this.dummyVoxelTextureAAScaled.width != DummyVoxelResolution)
			ResizeDummyTexture();
	}

	[UsedImplicitly]
	protected virtual void OnPreRender()
	{
		if(!this.initialized || !this.updateGI || !this.voxelCamera.IsValid() || !this.shadowCam.IsValid())
		{
			return;
		}
		//Cache the previous active render texture to avoid issues with other Unity rendering going on
		var previousActive = RenderTexture.active;
		Shader.SetGlobalInt(SEGIVoxelAA, GlobalVoxelAA);

		var prevSunShadowSetting = LightShadows.None;
		if(DisableShadowRendering && this.sun.IsValid())
		{
			//Temporarily disable rendering of shadows on the directional light during voxelization pass. Cache the result to set it back to what it was after voxelization is done
			prevSunShadowSetting = this.sun.shadows;
			this.sun.shadows = LightShadows.None;
		}

		if(this.renderState == RenderState.Voxelize)
		{
			PreRenderVoxelize();
			if(this.infiniteBounces)
			{
				this.renderState = RenderState.Bounce;
			}
		}
		else if(this.renderState == RenderState.Bounce)
		{
			PreRenderBounce();
			this.renderState = RenderState.Voxelize;
		}

		if(this is SEGICascaded)
		{
			var giToVoxelProjection = this.voxelCamera.projectionMatrix * this.voxelCamera.worldToCameraMatrix * this.shadowCam.cameraToWorldMatrix;
			Shader.SetGlobalMatrix(GIToVoxelProjection, giToVoxelProjection);
		}

		RenderTexture.active = previousActive;

		//Set the sun's shadow setting back to what it was before voxelization
		if(DisableShadowRendering && this.sun.IsValid())
			this.sun.shadows = prevSunShadowSetting;
	}

	[ImageEffectOpaque]
	protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if(this.notReadyToRender || !this.material.IsValid() || !this.attachedCamera.IsValid() || this.blueNoise == null)
		{
			Graphics.Blit(source, destination);
			return;
		}

		//Set parameters
		Shader.SetGlobalFloat(SEGIVoxelScaleFactor, VoxelScaleFactor);

		this.material.SetMatrix(CameraToWorld, this.attachedCamera.cameraToWorldMatrix);
		this.material.SetMatrix(WorldToCamera, this.attachedCamera.worldToCameraMatrix);
		this.material.SetMatrix(ProjectionMatrixInverse, this.attachedCamera.projectionMatrix.inverse);
		this.material.SetMatrix(ProjectionMatrix, this.attachedCamera.projectionMatrix);
		this.material.SetInt(FrameSwitch, this.frameCounter);
		Shader.SetGlobalInt(SEGIFrameSwitch, this.frameCounter);
		this.material.SetVector(CameraPosition, transform.position);
		this.material.SetFloat(DeltaTime, Time.deltaTime);

		this.material.SetInt(StochasticSampling, this.stochasticSampling ? 1 : 0);
		this.material.SetInt(TraceDirections, this.cones);
		this.material.SetInt(TraceSteps, this.coneTraceSteps);
		this.material.SetFloat(TraceLength, this.coneLength);
		this.material.SetFloat(ConeSize, this.coneWidth);
		this.material.SetFloat(OcclusionStrength, this.occlusionStrength);
		this.material.SetFloat(OcclusionPower, this.occlusionPower);
		this.material.SetFloat(ConeTraceBias, this.coneTraceBias);
		this.material.SetFloat(GIGain, this.giGain);
		this.material.SetFloat(NearLightGain, this.nearLightGain);
		this.material.SetFloat(NearOcclusionStrength, this.nearOcclusionStrength);
		this.material.SetInt(DoReflections, this.doReflections ? 1 : 0);
		this.material.SetInt(HalfResolution, this.halfResolution ? 1 : 0);
		this.material.SetInt(ReflectionSteps, this.reflectionSteps);
		this.material.SetFloat(ReflectionOcclusionPower, this.reflectionOcclusionPower);
		this.material.SetFloat(SkyReflectionIntensity, this.skyReflectionIntensity);
		this.material.SetFloat(FarOcclusionStrength, this.farOcclusionStrength);
		this.material.SetFloat(FarthestOcclusionStrength, this.farthestOcclusionStrength);
		this.material.SetTexture(NoiseTexture, this.blueNoise[this.frameCounter % 64]);
		this.material.SetFloat(BlendWeight, this.temporalBlendWeight);

		//If Visualize Voxels is enabled, just render the voxel visualization shader pass and return
		if(this.visualizeVoxels)
		{
			Graphics.Blit(source, destination, this.material, Pass.VisualizeVoxels);
			return;
		}

		//Setup temporary textures
		var gi1 = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.ARGBHalf);
		var gi2 = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.ARGBHalf);
		RenderTexture? reflections = null;

		//If reflections are enabled, create a temporary render buffer to hold them
		if(this.doReflections)
			reflections = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);

		//Get the camera depth and normals
		var currentDepth = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
		currentDepth.filterMode = FilterMode.Point;
		var currentNormal = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		currentNormal.filterMode = FilterMode.Point;

		//Get the camera depth and normals
		Graphics.Blit(source, currentDepth, this.material, Pass.GetCameraDepthTexture);
		this.material.SetTexture(CurrentDepth, currentDepth);
		Graphics.Blit(source, currentNormal, this.material, Pass.GetWorldNormals);
		this.material.SetTexture(CurrentNormal, currentNormal);

		//Set the previous GI result and camera depth textures to access them in the shader
		this.material.SetTexture(PreviousGITexture, this.previousGIResult);
		Shader.SetGlobalTexture(PreviousGITexture, this.previousGIResult);
		this.material.SetTexture(PreviousDepth, this.previousCameraDepth);

		//Render diffuse GI tracing result
		Graphics.Blit(source, gi2, this.material, Pass.DiffuseTrace);
		if(this.doReflections)
		{
			//Render GI reflections result
			Graphics.Blit(source, reflections, this.material, Pass.SpecularTrace);
			this.material.SetTexture(Reflections, reflections);
		}

		//Perform bilateral filtering
		if(this.useBilateralFiltering && this.temporalBlendWeight >= 0.99999f)
		{
			this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));
			Graphics.Blit(gi2, gi1, this.material, Pass.BilateralBlur);

			this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
			Graphics.Blit(gi1, gi2, this.material, Pass.BilateralBlur);

			this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));
			Graphics.Blit(gi2, gi1, this.material, Pass.BilateralBlur);

			this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
			Graphics.Blit(gi1, gi2, this.material, Pass.BilateralBlur);
		}

		RenderImageInternal(source, destination, gi1, gi2, currentDepth, currentNormal, reflections);

		//Release temporary textures
		RenderTexture.ReleaseTemporary(gi1);
		RenderTexture.ReleaseTemporary(gi2);
		RenderTexture.ReleaseTemporary(currentDepth);
		RenderTexture.ReleaseTemporary(currentNormal);

		//Visualize the sun depth texture
		if(this.visualizeSunDepthTexture)
			Graphics.Blit(this.sunDepthTexture, destination);

		//Release the temporary reflections result texture
		if(this.doReflections)
			RenderTexture.ReleaseTemporary(reflections);

		//Set matrices/vectors for use during temporal reprojection
		this.material.SetMatrix(ProjectionPrev, this.attachedCamera.projectionMatrix);
		this.material.SetMatrix(ProjectionPrevInverse, this.attachedCamera.projectionMatrix.inverse);
		this.material.SetMatrix(WorldToCameraPrev, this.attachedCamera.worldToCameraMatrix);
		this.material.SetMatrix(CameraToWorldPrev, this.attachedCamera.cameraToWorldMatrix);
		this.material.SetVector(CameraPositionPrev, transform.position);

		//Advance the frame counter
		this.frameCounter = (this.frameCounter + 1) % 64;
	}
	#endregion

	///<summary>Applies an SEGIPreset to this instance of SEGI.</summary>
	public virtual void ApplyPreset(SEGIPreset preset)
	{
		this.voxelResolution = preset.voxelResolution;
		this.voxelAA = preset.voxelAA;
		this.innerOcclusionLayers = preset.innerOcclusionLayers;
		this.infiniteBounces = preset.infiniteBounces;

		this.temporalBlendWeight = preset.temporalBlendWeight;
		this.useBilateralFiltering = preset.useBilateralFiltering;
		this.halfResolution = preset.halfResolution;
		this.stochasticSampling = preset.stochasticSampling;
		this.doReflections = preset.doReflections;

		this.cones = preset.cones;
		this.coneTraceSteps = preset.coneTraceSteps;
		this.coneLength = preset.coneLength;
		this.coneWidth = preset.coneWidth;
		this.coneTraceBias = preset.coneTraceBias;
		this.occlusionStrength = preset.occlusionStrength;
		this.nearOcclusionStrength = preset.nearOcclusionStrength;
		this.occlusionPower = preset.occlusionPower;
		this.nearLightGain = preset.nearLightGain;
		this.giGain = preset.giGain;
		this.secondaryBounceGain = preset.secondaryBounceGain;

		this.reflectionSteps = preset.reflectionSteps;
		this.reflectionOcclusionPower = preset.reflectionOcclusionPower;
		this.skyReflectionIntensity = preset.skyReflectionIntensity;
		this.gaussianMipFilter = preset.gaussianMipFilter;

		this.farOcclusionStrength = preset.farOcclusionStrength;
		this.farthestOcclusionStrength = preset.farthestOcclusionStrength;
		this.secondaryCones = preset.secondaryCones;
		this.secondaryOcclusionStrength = preset.secondaryOcclusionStrength;
		this.sunShadowResolution = preset.sunShadowResolution;
		this.shadowSpaceDepthRatio = preset.shadowSpaceDepthRatio;
	}
	protected abstract void PreRenderVoxelize();
	protected abstract void PreRenderBounce();

	protected abstract void RenderImageInternal(RenderTexture source, RenderTexture destination, RenderTexture gi1, RenderTexture gi2, RenderTexture currentDepth, RenderTexture currentNormal, RenderTexture? reflections);

	protected virtual void CreateVolumeTextures()
	{
		DestroyTexture(this.integerVolume);
		this.integerVolume = new RenderTexture((int)this.voxelResolution, (int)this.voxelResolution, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear)
		{
			dimension = TextureDimension.Tex3D,
			volumeDepth = (int)this.voxelResolution,
			enableRandomWrite = true,
			filterMode = FilterMode.Point,
			hideFlags = HideFlags.HideAndDontSave
		};
		this.integerVolume.Create();

		ResizeDummyTexture();
	}

	protected static Matrix4x4 TransformViewMatrix(Matrix4x4 mat)
	{
		if(!SystemInfo.usesReversedZBuffer)
			return mat;
		mat[2, 0] = -mat[2, 0];
		mat[2, 1] = -mat[2, 1];
		mat[2, 2] = -mat[2, 2];
		mat[2, 3] = -mat[2, 3];
		// mat[3, 2] += 0.0f;
		return mat;
	}

	protected T LoadAsset<T>(string assetName) where T : Object
	{
		var asset = AssetsManager.LoadAsset<T>(assetName);
		if(!asset.IsValid())
			throw new ApplicationException($"Could not find asset \"Hidden/{assetName}\"");
		return asset;
	}

	protected void DestroyTexture(RenderTexture? texture)
	{
		if(texture.IsValid())
		{
			texture.DiscardContents();
			texture.Release();
			DestroyImmediate(texture);
		}
	}

	protected void InitCheck()
	{
		if(this.initialized)
			return;
		try
		{
			Init();
		}
		catch(Exception e)
		{
			Debug.LogError("SEGI initialization failed: " + e.Message);
			this.notReadyToRender = true;
			enabled = false;
		}
	}

	protected virtual void Init()
	{
		LoadResources();
		//Get the camera attached to this game object
		this.attachedCamera = GetComponent<Camera>();
		if(!this.attachedCamera.IsValid())
			throw new Exception("SEGI needs to be attached to an object with the main camera!");
		this.attachedCamera.depthTextureMode |= DepthTextureMode.Depth;
		this.attachedCamera.depthTextureMode |= DepthTextureMode.MotionVectors;

		//Find the proxy shadow rendering camera if it exists
		var segiShadowCam = GameObject.Find("SEGI_SHADOWCAM");
		if(!segiShadowCam.IsValid())
		{
			//If not, create it
			this.shadowCamGameObject = new GameObject("SEGI_SHADOWCAM");
			this.shadowCam = this.shadowCamGameObject.AddComponent<Camera>();
			this.shadowCamGameObject.hideFlags = HideFlags.HideAndDontSave;

			this.shadowCam.enabled = false;
			this.shadowCam.depth = this.attachedCamera.depth - 1;
			this.shadowCam.orthographic = true;
			this.shadowCam.orthographicSize = this.shadowSpaceSize;
			this.shadowCam.clearFlags = CameraClearFlags.SolidColor;
			this.shadowCam.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			this.shadowCam.farClipPlane = this.shadowSpaceSize * 2.0f * this.shadowSpaceDepthRatio;
			this.shadowCam.cullingMask = this.giCullingMask;
			this.shadowCam.useOcclusionCulling = false;

			this.shadowCamTransform = this.shadowCamGameObject.transform;
		}
		else
		{
			//Otherwise, it already exists, just get it
			this.shadowCamGameObject = segiShadowCam;
			this.shadowCam = segiShadowCam.GetComponent<Camera>();
			this.shadowCamTransform = this.shadowCamGameObject.transform;
		}

		//Create the proxy camera objects responsible for rendering the scene to voxelize the scene. If they already exist, destroy them
		var segiVoxelCamera = GameObject.Find("SEGI_VOXEL_CAMERA");

		if(!segiVoxelCamera.IsValid())
		{
			this.voxelCameraGO = new GameObject("SEGI_VOXEL_CAMERA")
			{
				hideFlags = HideFlags.HideAndDontSave
			};

			this.voxelCamera = this.voxelCameraGO.AddComponent<Camera>();
			this.voxelCamera.enabled = false;
			this.voxelCamera.orthographic = true;
			this.voxelCamera.orthographicSize = this.voxelSpaceSize * 0.5f;
			this.voxelCamera.nearClipPlane = 0.0f;
			this.voxelCamera.farClipPlane = this.voxelSpaceSize;
			this.voxelCamera.depth = -2;
			this.voxelCamera.renderingPath = RenderingPath.Forward;
			this.voxelCamera.clearFlags = CameraClearFlags.Color;
			this.voxelCamera.backgroundColor = Color.black;
			this.voxelCamera.useOcclusionCulling = false;
		}
		else
		{
			this.voxelCameraGO = segiVoxelCamera;
			this.voxelCamera = segiVoxelCamera.GetComponent<Camera>();
		}

		var lvp = GameObject.Find("SEGI_LEFT_VOXEL_VIEW");
		if(!lvp.IsValid())
			this.leftViewPoint = new GameObject("SEGI_LEFT_VOXEL_VIEW")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		else
			this.leftViewPoint = lvp;

		var tvp = GameObject.Find("SEGI_TOP_VOXEL_VIEW");
		if(!tvp.IsValid())
			this.topViewPoint = new GameObject("SEGI_TOP_VOXEL_VIEW")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		else
			this.topViewPoint = tvp;

		//Get blue noise textures
		this.blueNoise = new Texture2D[64];
		for(var i = 0; i < 64; i++)
			this.blueNoise[i] = LoadAsset<Texture2D>($"LDR_RGBA_{i}");

		//Setup sun depth texture
		ResizeSunShadowBuffer();

		//Create the volume textures
		CreateVolumeTextures();
	}

	protected abstract void LoadResources();

	protected void SetVoxelCamera(Vector3 spaceOrigin, float halfVoxelSpaceSize)
	{
		this.voxelCamera!.enabled = false;
		this.voxelCamera.orthographic = true;
		this.voxelCamera.orthographicSize = halfVoxelSpaceSize;
		this.voxelCamera.nearClipPlane = 0.0f;
		this.voxelCamera.farClipPlane = halfVoxelSpaceSize * 2;
		this.voxelCamera.depth = -2;
		this.voxelCamera.renderingPath = RenderingPath.Forward;
		this.voxelCamera.clearFlags = CameraClearFlags.Color;
		this.voxelCamera.backgroundColor = Color.black;
		this.voxelCamera.cullingMask = this.giCullingMask;

		//Move the voxel camera game object and other related objects to the above calculated voxel space origin
		this.voxelCameraGO!.transform.SetPositionAndRotation(spaceOrigin - (Vector3.forward * halfVoxelSpaceSize), this.rotationFront);
		this.leftViewPoint!.transform.SetPositionAndRotation(spaceOrigin + (Vector3.left * halfVoxelSpaceSize), this.rotationLeft);
		this.topViewPoint!.transform.SetPositionAndRotation(spaceOrigin + (Vector3.up * halfVoxelSpaceSize), this.rotationTop);
	}

	protected void GetVoxelVolumeOriginAndInterval(float size, out Vector3 origin, out float interval)
	{
		//The interval at which the voxel volume will be "locked" in world-space
		interval = size / 8.0f;
		if(this.followTransform)
			origin = this.followTransform!.position;
		else
			//GI is still flickering a bit when the scene view and the game view are opened at the same time
			origin = transform.position + (transform.forward * (this.voxelSpaceSize / 4.0f));
	}

	protected virtual void CleanupTextures()
	{
		DestroyTexture(this.sunDepthTexture);
		DestroyTexture(this.previousGIResult);
		DestroyTexture(this.previousCameraDepth);
		DestroyTexture(this.integerVolume);
		DestroyTexture(this.dummyVoxelTextureAAScaled);
		DestroyTexture(this.dummyVoxelTextureFixed);
	}

	private void Cleanup()
	{
		DestroyImmediate(this.material);
		DestroyImmediate(this.voxelCameraGO);
		DestroyImmediate(this.leftViewPoint);
		DestroyImmediate(this.topViewPoint);
		DestroyImmediate(this.shadowCamGameObject);
		this.initialized = false;

		CleanupTextures();
	}

	private void ResizeRenderTextures()
	{
		if(!this.attachedCamera.IsValid())
			return;
		DestroyTexture(this.previousGIResult);
		DestroyTexture(this.previousCameraDepth);

		var width = this.attachedCamera.pixelWidth == 0 ? 2 : this.attachedCamera.pixelWidth;
		var height = this.attachedCamera.pixelHeight == 0 ? 2 : this.attachedCamera.pixelHeight;

		this.previousGIResult = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Bilinear,
			hideFlags = HideFlags.HideAndDontSave
		};
		if(this is SEGIBehavior)
		{
			this.previousGIResult.useMipMap = true;
			this.previousGIResult.autoGenerateMips = false;
		}
		this.previousGIResult.Create();

		this.previousCameraDepth = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Bilinear,
			hideFlags = HideFlags.HideAndDontSave
		};
		this.previousCameraDepth.Create();
	}

	private void ResizeSunShadowBuffer()
	{
		DestroyTexture(this.sunDepthTexture);
		this.sunDepthTexture = new RenderTexture((int)this.sunShadowResolution, (int)this.sunShadowResolution, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point,
			hideFlags = HideFlags.HideAndDontSave
		};
		this.sunDepthTexture.Create();
	}

	private void ResizeDummyTexture()
	{
		DestroyTexture(this.dummyVoxelTextureAAScaled);
		DestroyTexture(this.dummyVoxelTextureFixed);

		this.dummyVoxelTextureAAScaled = new RenderTexture(DummyVoxelResolution, DummyVoxelResolution, 0, RenderTextureFormat.R8)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		this.dummyVoxelTextureAAScaled.Create();

		this.dummyVoxelTextureFixed = new RenderTexture((int)this.voxelResolution, (int)this.voxelResolution, 0, RenderTextureFormat.R8)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		this.dummyVoxelTextureFixed.Create();
	}

	private void CheckSupport()
	{
		this.systemSupported.hdrTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
		this.systemSupported.rIntTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RInt);
		this.systemSupported.dx11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
		this.systemSupported.volumeTextures = SystemInfo.supports3DTextures;

		if(this.material.IsValid() && this.sunDepthShader.IsValid() && this.voxelizationShader.IsValid() && this.voxelTracingShader.IsValid())
		{
			this.systemSupported.postShader = this.material.shader.isSupported;
			this.systemSupported.sunDepthShader = this.sunDepthShader.isSupported;
			this.systemSupported.voxelizationShader = this.voxelizationShader.isSupported;
			this.systemSupported.tracingShader = this.voxelTracingShader.isSupported;

			if(this.systemSupported.FullFunctionality)
				return;

			Debug.LogWarning("SEGI is not supported on the current platform. Check for shader compile errors in SEGI/Resources");
			enabled = false;
			return;
		}

		Debug.LogWarning("SEGI is not configured correctly.");
		enabled = false;
	}
}