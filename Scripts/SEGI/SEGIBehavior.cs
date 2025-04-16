// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using JetBrains.Annotations;
using Entropy.Scripts.Utilities;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Entropy.Scripts.SEGI;

// ReSharper disable InconsistentNaming

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Sonic Ether/SEGI")]
public class SEGIBehavior : MonoBehaviour
{

	#region Shader properties

	private static readonly int SEGIVoxelAA = Shader.PropertyToID("SEGIVoxelAA");
	private static readonly int SEGIVoxelSpaceOriginDelta = Shader.PropertyToID("SEGIVoxelSpaceOriginDelta");
	private static readonly int WorldToCamera = Shader.PropertyToID("WorldToCamera");
	private static readonly int SEGIVoxelViewFront = Shader.PropertyToID("SEGIVoxelViewFront");
	private static readonly int SEGIVoxelViewLeft = Shader.PropertyToID("SEGIVoxelViewLeft");
	private static readonly int SEGIVoxelViewTop = Shader.PropertyToID("SEGIVoxelViewTop");
	private static readonly int SEGIWorldToVoxel = Shader.PropertyToID("SEGIWorldToVoxel");
	private static readonly int SEGIVoxelProjection = Shader.PropertyToID("SEGIVoxelProjection");
	private static readonly int SEGIVoxelProjectionInverse = Shader.PropertyToID("SEGIVoxelProjectionInverse");
	private static readonly int SEGIVoxelResolution = Shader.PropertyToID("SEGIVoxelResolution");
	private static readonly int SEGIVoxelToGIProjection = Shader.PropertyToID("SEGIVoxelToGIProjection");
	private static readonly int SEGISunlightVector = Shader.PropertyToID("SEGISunlightVector");
	private static readonly int GISunColor = Shader.PropertyToID("GISunColor");
	private static readonly int SEGISkyColor = Shader.PropertyToID("SEGISkyColor");
	private static readonly int GIGain = Shader.PropertyToID("GIGain");
	private static readonly int SEGISecondaryBounceGain = Shader.PropertyToID("SEGISecondaryBounceGain");
	private static readonly int SEGISoftSunlight = Shader.PropertyToID("SEGISoftSunlight");
	private static readonly int SEGISphericalSkylight = Shader.PropertyToID("SEGISphericalSkylight");
	private static readonly int SEGIInnerOcclusionLayers = Shader.PropertyToID("SEGIInnerOcclusionLayers");
	private static readonly int SEGISunDepth = Shader.PropertyToID("SEGISunDepth");
	private static readonly int SEGIVolumeLevel0 = Shader.PropertyToID("SEGIVolumeLevel0");
	private static readonly int[] SEGIVolumeLevel =
	[
		Shader.PropertyToID("SEGIVolumeLevel0"),
		Shader.PropertyToID("SEGIVolumeLevel1"),
		Shader.PropertyToID("SEGIVolumeLevel2"),
		Shader.PropertyToID("SEGIVolumeLevel3"),
		Shader.PropertyToID("SEGIVolumeLevel4"),
		Shader.PropertyToID("SEGIVolumeLevel5"),
		Shader.PropertyToID("SEGIVolumeLevel6"),
	];

	private static readonly int RG0 = Shader.PropertyToID("RG0");
	private static readonly int Res = Shader.PropertyToID("Res");
	private static readonly int Result = Shader.PropertyToID("Result");
	private static readonly int PrevResult = Shader.PropertyToID("PrevResult");
	private static readonly int VoxelAA = Shader.PropertyToID("VoxelAA");
	private static readonly int Resolution = Shader.PropertyToID("Resolution");
	private static readonly int VoxelOriginDelta = Shader.PropertyToID("VoxelOriginDelta");
	private static readonly int destinationRes = Shader.PropertyToID("destinationRes");
	private static readonly int Source = Shader.PropertyToID("Source");
	private static readonly int Destination = Shader.PropertyToID("Destination");
	private static readonly int SEGISecondaryCones = Shader.PropertyToID("SEGISecondaryCones");
	private static readonly int SEGISecondaryOcclusionStrength = Shader.PropertyToID("SEGISecondaryOcclusionStrength");
	private static readonly int SEGIVolumeTexture1 = Shader.PropertyToID("SEGIVolumeTexture1");

	private static readonly int SEGIVoxelScaleFactor = Shader.PropertyToID("SEGIVoxelScaleFactor");
	private static readonly int CameraToWorld = Shader.PropertyToID("CameraToWorld");
	private static readonly int ProjectionMatrixInverse = Shader.PropertyToID("ProjectionMatrixInverse");
	private static readonly int ProjectionMatrix = Shader.PropertyToID("ProjectionMatrix");
	private static readonly int FrameSwitch = Shader.PropertyToID("FrameSwitch");
	private static readonly int SEGIFrameSwitch = Shader.PropertyToID("SEGIFrameSwitch");
	private static readonly int CameraPosition = Shader.PropertyToID("CameraPosition");
	private static readonly int DeltaTime = Shader.PropertyToID("DeltaTime");
	private static readonly int StochasticSampling = Shader.PropertyToID("StochasticSampling");
	private static readonly int TraceDirections = Shader.PropertyToID("TraceDirections");
	private static readonly int TraceSteps = Shader.PropertyToID("TraceSteps");
	private static readonly int TraceLength = Shader.PropertyToID("TraceLength");
	private static readonly int ConeSize = Shader.PropertyToID("ConeSize");
	private static readonly int OcclusionStrength = Shader.PropertyToID("OcclusionStrength");
	private static readonly int OcclusionPower = Shader.PropertyToID("OcclusionPower");
	private static readonly int ConeTraceBias = Shader.PropertyToID("ConeTraceBias");
	private static readonly int NearLightGain = Shader.PropertyToID("NearLightGain");
	private static readonly int NearOcclusionStrength = Shader.PropertyToID("NearOcclusionStrength");
	private static readonly int DoReflections = Shader.PropertyToID("DoReflections");
	private static readonly int HalfResolution = Shader.PropertyToID("HalfResolution");
	private static readonly int ReflectionSteps = Shader.PropertyToID("ReflectionSteps");
	private static readonly int ReflectionOcclusionPower = Shader.PropertyToID("ReflectionOcclusionPower");
	private static readonly int SkyReflectionIntensity = Shader.PropertyToID("SkyReflectionIntensity");
	private static readonly int FarOcclusionStrength = Shader.PropertyToID("FarOcclusionStrength");
	private static readonly int FarthestOcclusionStrength = Shader.PropertyToID("FarthestOcclusionStrength");
	private static readonly int NoiseTexture = Shader.PropertyToID("NoiseTexture");
	private static readonly int BlendWeight = Shader.PropertyToID("BlendWeight");

	private static readonly int CurrentDepth = Shader.PropertyToID("CurrentDepth");
	private static readonly int CurrentNormal = Shader.PropertyToID("CurrentNormal");
	private static readonly int PreviousGITexture = Shader.PropertyToID("PreviousGITexture");
	private static readonly int PreviousDepth = Shader.PropertyToID("PreviousDepth");
	private static readonly int Reflections = Shader.PropertyToID("Reflections");
	private static readonly int Kernel = Shader.PropertyToID("Kernel");
	private static readonly int GITexture = Shader.PropertyToID("GITexture");
	private static readonly int ProjectionPrev = Shader.PropertyToID("ProjectionPrev");
	private static readonly int ProjectionPrevInverse = Shader.PropertyToID("ProjectionPrevInverse");
	private static readonly int WorldToCameraPrev = Shader.PropertyToID("WorldToCameraPrev");
	private static readonly int CameraToWorldPrev = Shader.PropertyToID("CameraToWorldPrev");
	private static readonly int CameraPositionPrev = Shader.PropertyToID("CameraPositionPrev");
	#endregion

	#region Parameters

	public bool updateGI = true;
	public LayerMask giCullingMask = 2147483647;
	public float shadowSpaceSize = 50.0f;
	public Light? sun;

	public Color skyColor;

	public float voxelSpaceSize = 25.0f;

	public bool useBilateralFiltering;

	[Range(0, 2)]
	public int innerOcclusionLayers = 1;

	[Range(0.01f, 1.0f)]
	public float temporalBlendWeight = 0.1f;

	public VoxelResolution voxelResolution = VoxelResolution.high;

	public bool visualizeSunDepthTexture = false;
	public bool visualizeGI = false;
	public bool visualizeVoxels = false;

	public bool halfResolution = true;
	public bool stochasticSampling = true;
	public bool infiniteBounces;
	public Transform? followTransform;
	[Range(1, 128)]
	public int cones = 6;
	[Range(1, 32)]
	public int coneTraceSteps = 14;
	[Range(0.1f, 2.0f)]
	public float coneLength = 1.0f;
	[Range(0.5f, 6.0f)]
	public float coneWidth = 5.5f;
	[Range(0.0f, 4.0f)]
	public float occlusionStrength = 1.0f;
	[Range(0.0f, 4.0f)]
	public float nearOcclusionStrength = 0.5f;
	[Range(0.001f, 4.0f)]
	public float occlusionPower = 1.5f;
	[Range(0.0f, 4.0f)]
	public float coneTraceBias = 1.0f;
	[Range(0.0f, 4.0f)]
	public float nearLightGain = 1.0f;
	[Range(0.0f, 4.0f)]
	public float giGain = 1.0f;
	[Range(0.0f, 4.0f)]
	public float secondaryBounceGain = 1.0f;
	[Range(0.0f, 16.0f)]
	public float softSunlight = 0.0f;

	[Range(0.0f, 8.0f)]
	public float skyIntensity = 1.0f;

	public bool doReflections = true;
	[Range(12, 128)]
	public int reflectionSteps = 64;
	[Range(0.001f, 4.0f)]
	public float reflectionOcclusionPower = 1.0f;
	[Range(0.0f, 1.0f)]
	public float skyReflectionIntensity = 1.0f;

	public bool voxelAA;

	public bool gaussianMipFilter;

	[Range(0.1f, 4.0f)]
	public float farOcclusionStrength = 1.0f;
	[Range(0.1f, 4.0f)]
	public float farthestOcclusionStrength = 1.0f;

	[Range(3, 16)]
	public int secondaryCones = 6;
	[Range(0.1f, 4.0f)]
	public float secondaryOcclusionStrength = 1.0f;

	public bool sphericalSkylight = false;

	#endregion

	#region InternalVariables
	private bool initialized;
	private Material? material;
	private Camera? attachedCamera;
	private Transform? shadowCamTransform;
	private Camera? shadowCam;
	private GameObject? shadowCamGameObject;
	private Texture2D?[]? blueNoise;

	private int sunShadowResolution = 256;
	private int prevSunShadowResolution;

	private Shader? sunDepthShader;

	private float shadowSpaceDepthRatio = 10.0f;

	private int frameCounter;

	private RenderTexture? sunDepthTexture;
	private RenderTexture? previousGIResult;
	private RenderTexture? previousCameraDepth;

	///<summary>This is a volume texture that is immediately written to in the voxelization shader. The RInt format enables atomic writes to avoid issues where multiple fragments are trying to write to the same voxel in the volume.</summary>
	private RenderTexture? integerVolume;

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

	///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture scales depending on whether Voxel AA is enabled to ensure correct voxelization.</summary>
	private RenderTexture? dummyVoxelTextureAAScaled;

	///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture is always the same size whether Voxel AA is enabled or not.</summary>
	private RenderTexture? dummyVoxelTextureFixed;

	private bool notReadyToRender;

	private Shader? voxelizationShader;
	private Shader? voxelTracingShader;

	private ComputeShader? clearCompute;
	private ComputeShader? transferIntsCompute;
	private ComputeShader? mipFilterCompute;

	private const int numMipLevels = 6;

	private Camera? voxelCamera;
	private GameObject? voxelCameraGO;
	private GameObject? leftViewPoint;
	private GameObject? topViewPoint;

	private float VoxelScaleFactor => (float)this.voxelResolution / 256.0f;

	private Vector3 voxelSpaceOrigin;
	private Vector3 previousVoxelSpaceOrigin;
	private Vector3 voxelSpaceOriginDelta;

	private Quaternion rotationFront = new(0.0f, 0.0f, 0.0f, 1.0f);
	private Quaternion rotationLeft = new(0.0f, 0.7f, 0.0f, 0.7f);
	private Quaternion rotationTop = new(0.7f, 0.0f, 0.0f, 0.7f);

	private int voxelFlipFlop;

	private enum RenderState
	{
		Voxelize,
		Bounce
	}

	private RenderState renderState = RenderState.Voxelize;
	#endregion

	#region SupportingObjectsAndProperties
	private readonly struct Pass
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

	/// <summary>
	/// Contains info on system compatibility of required hardware functionality
	/// </summary>
	public SystemSupported systemSupported;

	/// <summary>
	/// Estimates the VRAM usage of all the render textures used to render GI.
	/// </summary>
	public float VRAMUsage
	{
		get
		{
			long v = 0;

			if (IsValid(this.sunDepthTexture))
				v += this.sunDepthTexture.width * this.sunDepthTexture.height * 16;

			if (IsValid(this.previousGIResult))
				v += this.previousGIResult.width * this.previousGIResult.height * 16 * 4;

			if (IsValid(this.previousCameraDepth))
				v += this.previousCameraDepth.width * this.previousCameraDepth.height * 32;

			if (IsValid(this.integerVolume))
				v += this.integerVolume.width * this.integerVolume.height * this.integerVolume.volumeDepth * 32;

			if (this.volumeTextures != null)
				foreach (var texture in this.volumeTextures)
					if (IsValid(texture))
						v += texture.width * texture.height * texture.volumeDepth * 16 * 4;

			if (IsValid(this.secondaryIrradianceVolume))
				v += this.secondaryIrradianceVolume.width * this.secondaryIrradianceVolume.height * this.secondaryIrradianceVolume.volumeDepth * 16 * 4;

			if (IsValid(this.volumeTextureB))
				v += this.volumeTextureB.width * this.volumeTextureB.height * this.volumeTextureB.volumeDepth * 16 * 4;

			if (IsValid(this.dummyVoxelTextureAAScaled))
				v += this.dummyVoxelTextureAAScaled.width * this.dummyVoxelTextureAAScaled.height * 8;

			if (IsValid(this.dummyVoxelTextureFixed))
				v += this.dummyVoxelTextureFixed.width * this.dummyVoxelTextureFixed.height * 8;

			var vram = v / 8388608.0f;

			return vram;
		}
	}

	private int MipFilterKernel => this.gaussianMipFilter ? 1 : 0;

	private int DummyVoxelResolution => (int) this.voxelResolution * (this.voxelAA ? 2 : 1);

	private int GIRenderRes => this.halfResolution ? 2 : 1;

	#endregion

	///<summary>Applies an SEGIPreset to this instance of SEGI.</summary>
	public void ApplyPreset(SEGIPreset preset)
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
		this.sunShadowResolution = (int)preset.sunShadowResolution;
		this.shadowSpaceDepthRatio = preset.shadowSpaceDepthRatio;
	}

	[UsedImplicitly]
	private void Start() => InitCheck();

	private void InitCheck()
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

	private void CreateVolumeTextures()
	{
		foreach(var texture in this.volumeTextures ?? [] )
			DestroyTexture(texture);
		DestroyTexture(this.volumeTextureB);
		DestroyTexture(this.secondaryIrradianceVolume);
		DestroyTexture(this.integerVolume);

		this.volumeTextures = new RenderTexture[numMipLevels];

		// ReSharper disable once LocalVariableHidesMember
		var voxelResolution = (int)this.voxelResolution;
		for (var i = 0; i < numMipLevels; i++)
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

		this.integerVolume = new RenderTexture(voxelResolution, voxelResolution, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear)
		{
			dimension = TextureDimension.Tex3D,
			volumeDepth = voxelResolution,
			enableRandomWrite = true,
			filterMode = FilterMode.Point,
			hideFlags = HideFlags.HideAndDontSave,
		};
		this.integerVolume.Create();

		ResizeDummyTexture();
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

		this.dummyVoxelTextureFixed = new RenderTexture((int) this.voxelResolution, (int) this.voxelResolution, 0, RenderTextureFormat.R8)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		this.dummyVoxelTextureFixed.Create();
	}

	private void Init()
	{
		//Setup shaders and materials
		this.sunDepthShader = LoadAsset<Shader>("SEGIRenderSunDepth");
		this.clearCompute = LoadAsset<ComputeShader>("SEGIClear");
		this.transferIntsCompute = LoadAsset<ComputeShader>("SEGITransferInts");
		this.mipFilterCompute = LoadAsset<ComputeShader>("SEGIMipFilter");
		this.voxelizationShader = LoadAsset<Shader>("SEGIVoxelizeScene");
		this.voxelTracingShader = LoadAsset<Shader>("SEGITraceScene");

		if (!IsValid(this.material))
		{
			var segi = LoadAsset<Shader>("SEGI");
			this.material = new Material(segi) { hideFlags = HideFlags.HideAndDontSave };
		}

		//Get the camera attached to this game object
		this.attachedCamera = GetComponent<Camera>();
		if(!IsValid(this.attachedCamera))
			throw new Exception("SEGI needs to be attached to an object with the main camera!");
		this.attachedCamera.depthTextureMode |= DepthTextureMode.Depth;
		this.attachedCamera.depthTextureMode |= DepthTextureMode.MotionVectors;

		//Find the proxy shadow rendering camera if it exists
		var segiShadowCam = GameObject.Find("SEGI_SHADOWCAM");
		if (!IsValid(segiShadowCam))
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

		if (!IsValid(segiVoxelCamera))
		{
			this.voxelCameraGO = new GameObject("SEGI_VOXEL_CAMERA")
			{
				hideFlags = HideFlags.HideAndDontSave
			};

			this.voxelCamera = this.voxelCameraGO.AddComponent<Camera>();
			this.voxelCamera.enabled = false;
			this.voxelCamera.aspect = 1;
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
		if (!IsValid(lvp))
		{
			this.leftViewPoint = new GameObject("SEGI_LEFT_VOXEL_VIEW")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		}
		else
			this.leftViewPoint = lvp;

		var tvp = GameObject.Find("SEGI_TOP_VOXEL_VIEW");
		if (!IsValid(tvp))
		{
			this.topViewPoint = new GameObject("SEGI_TOP_VOXEL_VIEW")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		}
		else
			this.topViewPoint = tvp;

		//Get blue noise textures
		this.blueNoise = new Texture2D[64];
		for (var i = 0; i < 64; i++)
			this.blueNoise[i] = LoadAsset<Texture2D>($"LDR_RGBA_{i}");

		//Setup sun depth texture
		DestroyTexture(this.sunDepthTexture);
		this.sunDepthTexture = new RenderTexture(this.sunShadowResolution, this.sunShadowResolution, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point,
			hideFlags = HideFlags.HideAndDontSave
		};
		this.sunDepthTexture.Create();

		//Create the volume textures
		CreateVolumeTextures();

		this.initialized = true;
		T LoadAsset<T>(string assetName) where T : Object
		{
			var asset = AssetsManager.LoadAsset<T>(assetName);
			if(!IsValid(asset))
				throw new ApplicationException($"Could not find asset \"Hidden/{assetName}\"");
			return asset;
		}
	}
	private void DestroyTexture(RenderTexture? texture)
	{
		if(IsValid(texture))
		{
			texture.DiscardContents();
			texture.Release();
			DestroyImmediate(texture);
		}
	}

	private void CheckSupport()
	{
		this.systemSupported.hdrTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
		this.systemSupported.rIntTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RInt);
		this.systemSupported.dx11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
		this.systemSupported.volumeTextures = SystemInfo.supports3DTextures;

		if(IsValid(this.material) && IsValid(this.sunDepthShader) && IsValid(this.voxelizationShader) && IsValid(this.voxelTracingShader))
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

	[UsedImplicitly]
	private void OnDrawGizmosSelected()
	{
		if (!enabled)
			return;

		var prevColor = Gizmos.color;
		Gizmos.color = new Color(1.0f, 0.25f, 0.0f, 0.5f);
		Gizmos.DrawCube(this.voxelSpaceOrigin, new Vector3(this.voxelSpaceSize, this.voxelSpaceSize, this.voxelSpaceSize));
		Gizmos.color = prevColor;
	}

	private void CleanupTextures()
	{
		DestroyTexture(this.sunDepthTexture);
		DestroyTexture(this.previousGIResult);
		DestroyTexture(this.previousCameraDepth);
		DestroyTexture(this.integerVolume);
		foreach(var texture in this.volumeTextures ?? [])
			DestroyTexture(texture);
		this.volumeTextures = [];
		DestroyTexture(this.secondaryIrradianceVolume);
		DestroyTexture(this.volumeTextureB);
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

	[UsedImplicitly]
	private void OnEnable()
	{
		InitCheck();
		ResizeRenderTextures();

		CheckSupport();
	}

	[UsedImplicitly]
	private void OnDisable() => Cleanup();

	private void ResizeRenderTextures()
	{
		if(!IsValid(this.attachedCamera))
			return;
		DestroyTexture(this.previousGIResult);
		DestroyTexture(this.previousCameraDepth);

		var width = this.attachedCamera.pixelWidth == 0 ? 2 : this.attachedCamera.pixelWidth;
		var height = this.attachedCamera.pixelHeight == 0 ? 2 : this.attachedCamera.pixelHeight;

		this.previousGIResult = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Bilinear,
			useMipMap = true,
			autoGenerateMips = false,
			hideFlags = HideFlags.HideAndDontSave
		};
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
		this.sunDepthTexture = new RenderTexture(this.sunShadowResolution, this.sunShadowResolution, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
		{
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point,
			hideFlags = HideFlags.HideAndDontSave
		};
		this.sunDepthTexture.Create();
	}

	[UsedImplicitly]
	private void Update()
	{
		if(this.notReadyToRender || !IsValid(this.attachedCamera))
			return;

		if(!IsValid(this.previousGIResult)
			|| this.previousGIResult.width != this.attachedCamera.pixelWidth
			|| this.previousGIResult.height != this.attachedCamera.pixelHeight)
			ResizeRenderTextures();

		if(this.sunShadowResolution != this.prevSunShadowResolution)
			ResizeSunShadowBuffer();

		this.prevSunShadowResolution = this.sunShadowResolution;

		if(this.volumeTextures is not { Length: numMipLevels } 
			|| !IsValid(this.volumeTextures[0])
			|| this.volumeTextures[0]!.width != (int)this.voxelResolution)
			CreateVolumeTextures();

		if(!IsValid(this.dummyVoxelTextureAAScaled) || this.dummyVoxelTextureAAScaled.width != DummyVoxelResolution)
			ResizeDummyTexture();
	}

	[MemberNotNull(
		nameof(volumeTextures),
		nameof(followTransform),
		nameof(voxelCamera),
		nameof(voxelCameraGO),
		nameof(leftViewPoint),
		nameof(topViewPoint),
		nameof(attachedCamera),
		nameof(shadowCamTransform),
		nameof(shadowCam),
		nameof(sunDepthTexture),
		nameof(clearCompute),
		nameof(transferIntsCompute),
		nameof(mipFilterCompute))]
	private void PreRenderContract()
	{
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
	}
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

	[UsedImplicitly]
	private void OnPreRender()
	{
		//Force reinitialization to make sure that everything is working properly if one of the cameras was unexpectedly destroyed
		if(this.notReadyToRender || this.volumeTextures == null || !IsValid(this.followTransform) || !IsValid(this.voxelCamera)
			|| !IsValid(this.voxelCameraGO) || !IsValid(this.leftViewPoint) || !IsValid(this.topViewPoint) || !IsValid(this.attachedCamera)
			|| !IsValid(this.shadowCamTransform) || !IsValid(this.shadowCam) || !IsValid(this.sunDepthTexture) || !IsValid(this.clearCompute)
			|| !IsValid(this.transferIntsCompute) || !IsValid(this.mipFilterCompute))
			this.initialized = false;

		if(!this.initialized || !this.updateGI)
			return;
		PreRenderContract();

		//Cache the previous active render texture to avoid issues with other Unity rendering going on
		var previousActive = RenderTexture.active;

		Shader.SetGlobalInt(SEGIVoxelAA, this.voxelAA ? 1 : 0);

		//Main voxelization work
		if (this.renderState == RenderState.Voxelize)
		{
			this.activeVolume = this.voxelFlipFlop == 0 ? this.volumeTextures[0] : this.volumeTextureB;             //Flip-flopping volume textures to avoid simultaneous read and write errors in shaders //-V3125
			this.previousActiveVolume = this.voxelFlipFlop == 0 ? this.volumeTextureB : this.volumeTextures[0];

			//float voxelTexel = (1.0f * voxelSpaceSize) / (int)voxelResolution * 0.5f;			//Calculate the size of a voxel texel in world-space units

			//Setup the voxel volume origin position
			var interval = this.voxelSpaceSize / 8.0f;                                             //The interval at which the voxel volume will be "locked" in world-space
			Vector3 origin;
			if (this.followTransform)
				origin = this.followTransform.position;
			else

				//GI is still flickering a bit when the scene view and the game view are opened at the same time
				origin = transform.position + (transform.forward * (this.voxelSpaceSize / 4.0f));

			//Lock the voxel volume origin based on the interval
			this.voxelSpaceOrigin = new Vector3(Mathf.Round(origin.x / interval) * interval, Mathf.Round(origin.y / interval) * interval, Mathf.Round(origin.z / interval) * interval);

			//Calculate how much the voxel origin has moved since last voxelization pass. Used for scrolling voxel data in shaders to avoid ghosting when the voxel volume moves in the world
			this.voxelSpaceOriginDelta = this.voxelSpaceOrigin - this.previousVoxelSpaceOrigin;
			Shader.SetGlobalVector(SEGIVoxelSpaceOriginDelta, this.voxelSpaceOriginDelta / this.voxelSpaceSize);

			this.previousVoxelSpaceOrigin = this.voxelSpaceOrigin;

			//Set the voxel camera (proxy camera used to render the scene for voxelization) parameters
			this.voxelCamera.enabled = false;
			this.voxelCamera.orthographic = true;
			this.voxelCamera.orthographicSize = this.voxelSpaceSize * 0.5f;
			this.voxelCamera.nearClipPlane = 0.0f;
			this.voxelCamera.farClipPlane = this.voxelSpaceSize;
			this.voxelCamera.depth = -2;
			this.voxelCamera.renderingPath = RenderingPath.Forward;
			this.voxelCamera.clearFlags = CameraClearFlags.Color;
			this.voxelCamera.backgroundColor = Color.black;
			this.voxelCamera.cullingMask = this.giCullingMask;

			//Move the voxel camera game object and other related objects to the above calculated voxel space origin
			this.voxelCameraGO.transform.SetPositionAndRotation(this.voxelSpaceOrigin - (Vector3.forward * (this.voxelSpaceSize * 0.5f)), this.rotationFront);
			this.leftViewPoint.transform.SetPositionAndRotation(this.voxelSpaceOrigin + (Vector3.left * (this.voxelSpaceSize * 0.5f)), this.rotationLeft);
			this.topViewPoint.transform.SetPositionAndRotation(this.voxelSpaceOrigin + (Vector3.up * (this.voxelSpaceSize * 0.5f)), this.rotationTop);

			//Set matrices needed for voxelization
			Shader.SetGlobalMatrix(WorldToCamera, this.attachedCamera.worldToCameraMatrix);
			Shader.SetGlobalMatrix(SEGIVoxelViewFront, TransformViewMatrix(this.voxelCamera.transform.worldToLocalMatrix));
			Shader.SetGlobalMatrix(SEGIVoxelViewLeft, TransformViewMatrix(this.leftViewPoint.transform.worldToLocalMatrix));
			Shader.SetGlobalMatrix(SEGIVoxelViewTop, TransformViewMatrix(this.topViewPoint.transform.worldToLocalMatrix));
			Shader.SetGlobalMatrix(SEGIWorldToVoxel, this.voxelCamera.worldToCameraMatrix);
			Shader.SetGlobalMatrix(SEGIVoxelProjection, this.voxelCamera.projectionMatrix);
			Shader.SetGlobalMatrix(SEGIVoxelProjectionInverse, this.voxelCamera.projectionMatrix.inverse);

			Shader.SetGlobalInt(SEGIVoxelResolution, (int) this.voxelResolution);

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
			if (this.sun != null)
			{
				this.shadowCam.cullingMask = this.giCullingMask;

				var shadowCamPosition = this.voxelSpaceOrigin + (Vector3.Normalize(-this.sun.transform.forward) * (this.shadowSpaceSize * 0.5f * this.shadowSpaceDepthRatio));

				this.shadowCamTransform.position = shadowCamPosition;
				this.shadowCamTransform.LookAt(this.voxelSpaceOrigin, Vector3.up);

				this.shadowCam.renderingPath = RenderingPath.Forward;
				this.shadowCam.depthTextureMode = DepthTextureMode.None;

				this.shadowCam.orthographicSize = this.shadowSpaceSize;
				this.shadowCam.farClipPlane = this.shadowSpaceSize * 2.0f * this.shadowSpaceDepthRatio;

				Graphics.SetRenderTarget(this.sunDepthTexture);
				this.shadowCam.SetTargetBuffers(this.sunDepthTexture.colorBuffer, this.sunDepthTexture.depthBuffer);

				this.shadowCam.RenderWithShader(this.sunDepthShader, "");

				Shader.SetGlobalTexture(SEGISunDepth, this.sunDepthTexture);
			}

			var voxelToGIProjection = this.shadowCam.projectionMatrix * this.shadowCam.worldToCameraMatrix * this.voxelCamera.cameraToWorldMatrix;
			Shader.SetGlobalMatrix(SEGIVoxelToGIProjection, voxelToGIProjection);
			Shader.SetGlobalVector(SEGISunlightVector, IsValid(this.sun) ? Vector3.Normalize(this.sun.transform.forward) : Vector3.up);

			//Clear the volume texture that is immediately written to in the voxelization scene shader
			this.clearCompute.SetTexture(0, RG0, this.integerVolume);
			this.clearCompute.SetInt(Res, (int) this.voxelResolution);
			this.clearCompute.Dispatch(0, Math.Max(1, (int) this.voxelResolution / 16), Math.Max(1, (int) this.voxelResolution / 16), 1);

			//Render the scene with the voxel proxy camera object with the voxelization shader to voxelize the scene to the volume integer texture
			Graphics.SetRandomWriteTarget(1, this.integerVolume);
			this.voxelCamera.targetTexture = this.dummyVoxelTextureAAScaled;
			this.voxelCamera.RenderWithShader(this.voxelizationShader, "");
			Graphics.ClearRandomWriteTargets();

			//Transfer the data from the volume integer texture to the main volume texture used for GI tracing.
			this.transferIntsCompute.SetTexture(0, Result, this.activeVolume);
			this.transferIntsCompute.SetTexture(0, PrevResult, this.previousActiveVolume);
			this.transferIntsCompute.SetTexture(0, RG0, this.integerVolume);
			this.transferIntsCompute.SetInt(VoxelAA, this.voxelAA ? 1 : 0);
			this.transferIntsCompute.SetInt(Resolution, (int) this.voxelResolution);
			this.transferIntsCompute.SetVector(VoxelOriginDelta, this.voxelSpaceOriginDelta / (this.voxelSpaceSize * (int) this.voxelResolution));
			this.transferIntsCompute.Dispatch(0, Math.Max(1, (int) this.voxelResolution / 16), Math.Max(1, (int) this.voxelResolution / 16), 1);

			Shader.SetGlobalTexture(SEGIVolumeLevel0, this.activeVolume);

			//Manually filter/render mip maps
			for (var i = 0; i < numMipLevels - 1; i++)
			{
				var source = this.volumeTextures[i];

				if (i == 0)
					source = this.activeVolume;

				var destinationRes = (int) this.voxelResolution >> (i + 1);
				this.mipFilterCompute.SetInt(SEGIBehavior.destinationRes, destinationRes);
				this.mipFilterCompute.SetTexture(MipFilterKernel, Source, source);
				this.mipFilterCompute.SetTexture(MipFilterKernel, Destination, this.volumeTextures[i + 1]);
				this.mipFilterCompute.Dispatch(MipFilterKernel, Math.Max(1, destinationRes / 8), Math.Max(1, destinationRes / 8), 1);
				Shader.SetGlobalTexture(SEGIVolumeLevel[i + 1], this.volumeTextures[i + 1]);
			}

			//Advance the voxel flip flop counter
			this.voxelFlipFlop += 1;
			this.voxelFlipFlop %= 2;

			if (this.infiniteBounces)
				this.renderState = RenderState.Bounce;
		}
		else if (this.renderState == RenderState.Bounce)
		{

			//Clear the volume texture that is immediately written to in the voxelization scene shader
			this.clearCompute.SetTexture(0, RG0, this.integerVolume);
			this.clearCompute.Dispatch(0, Math.Max(1, (int) this.voxelResolution / 16), Math.Max(1, (int) this.voxelResolution / 16), 1);

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
			this.transferIntsCompute.SetInt(Resolution, (int) this.voxelResolution);
			this.transferIntsCompute.Dispatch(1, Math.Max(1, (int) this.voxelResolution / 16), Math.Max(1, (int) this.voxelResolution / 16), 1);

			Shader.SetGlobalTexture(SEGIVolumeTexture1, this.secondaryIrradianceVolume);

			this.renderState = RenderState.Voxelize;
		}

		RenderTexture.active = previousActive;

		static Matrix4x4 TransformViewMatrix(Matrix4x4 mat)
		{
			//Since the third column of the view matrix needs to be reversed if using reversed z-buffer, do so here
			if(!SystemInfo.usesReversedZBuffer)
				return mat;
			mat[2, 0] = -mat[2, 0];
			mat[2, 1] = -mat[2, 1];
			mat[2, 2] = -mat[2, 2];
			mat[2, 3] = -mat[2, 3];

			return mat;
		}
}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (this.notReadyToRender || !IsValid(this.material) || !IsValid(this.attachedCamera) || this.blueNoise == null)
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
		if (this.visualizeVoxels)
		{
			Graphics.Blit(source, destination, this.material, Pass.VisualizeVoxels);
			return;
		}

		//Setup temporary textures
		var gi1 = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.ARGBHalf);
		var gi2 = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.ARGBHalf);
		RenderTexture? reflections = null;

		//If reflections are enabled, create a temporary render buffer to hold them
		if (this.doReflections)
			reflections = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);

		//Setup textures to hold the current camera depth and normal
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
		if (this.doReflections)
		{
			//Render GI reflections result
			Graphics.Blit(source, reflections, this.material, Pass.SpecularTrace);
			this.material.SetTexture(Reflections, reflections);
		}

		//Perform bilateral filtering
		if (this.useBilateralFiltering)
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

		//If Half Resolution tracing is enabled
		if (GIRenderRes == 2)
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
			this.material.SetVector(Kernel, new Vector2(1.0f, 0.0f));
			Graphics.Blit(gi4, gi3, this.material, Pass.BilateralUpsample);
			this.material.SetVector(Kernel, new Vector2(0.0f, 1.0f));

			//Perform temporal reprojection and blending
			if (this.temporalBlendWeight < 1.0f)
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
			if (this.temporalBlendWeight < 1.0f)
			{
				Graphics.Blit(gi2, gi1, this.material, Pass.TemporalBlend);
				Graphics.Blit(gi1, this.previousGIResult);
				Graphics.Blit(source, this.previousCameraDepth, this.material, Pass.GetCameraDepthTexture);
			}

			//Actually apply the GI to the scene using gbuffer data
			this.material.SetTexture(GITexture, this.temporalBlendWeight < 1.0f ? gi1 : gi2);
			Graphics.Blit(source, destination, this.material, this.visualizeGI ? Pass.VisualizeGI : Pass.BlendWithScene);

			//Release temporary textures
			RenderTexture.ReleaseTemporary(gi1);
			RenderTexture.ReleaseTemporary(gi2);
		}

		//Release temporary textures
		RenderTexture.ReleaseTemporary(currentDepth);
		RenderTexture.ReleaseTemporary(currentNormal);

		//Visualize the sun depth texture
		if (this.visualizeSunDepthTexture)
			Graphics.Blit(this.sunDepthTexture, destination);

		//Release the temporary reflections result texture
		if (this.doReflections)
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsValid([NotNullWhen(true)] Object? obj)
	{
		if(obj == null)
			return false;
		return obj switch
		{
			GameObject go => go,
			Component comp => comp,
			_ => true
		};
	}
}
