using UnityEngine;
using UnityEngine.Rendering;
using System;
using Entropy.Assets.Scripts.Assets.Scripts.Utilities;


[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Sonic Ether/SEGI")]
public class SEGIBehavior : MonoBehaviour
{

    #region Shader properties
    static readonly int SEGIVoxelAA = Shader.PropertyToID("SEGIVoxelAA");
    static readonly int SEGIVoxelSpaceOriginDelta = Shader.PropertyToID("SEGIVoxelSpaceOriginDelta");
    static readonly int WorldToCamera = Shader.PropertyToID("WorldToCamera");
    static readonly int SEGIVoxelViewFront = Shader.PropertyToID("SEGIVoxelViewFront");
    static readonly int SEGIVoxelViewLeft = Shader.PropertyToID("SEGIVoxelViewLeft");
    static readonly int SEGIVoxelViewTop = Shader.PropertyToID("SEGIVoxelViewTop");
    static readonly int SEGIWorldToVoxel = Shader.PropertyToID("SEGIWorldToVoxel");
    static readonly int SEGIVoxelProjection = Shader.PropertyToID("SEGIVoxelProjection");
    static readonly int SEGIVoxelProjectionInverse = Shader.PropertyToID("SEGIVoxelProjectionInverse");
    static readonly int SEGIVoxelResolution = Shader.PropertyToID("SEGIVoxelResolution");
    static readonly int SEGIVoxelToGIProjection = Shader.PropertyToID("SEGIVoxelToGIProjection");
    static readonly int SEGISunlightVector = Shader.PropertyToID("SEGISunlightVector");
    static readonly int GISunColor = Shader.PropertyToID("GISunColor");
    static readonly int SEGISkyColor = Shader.PropertyToID("SEGISkyColor");
    static readonly int GIGain = Shader.PropertyToID("GIGain");
    static readonly int SEGISecondaryBounceGain = Shader.PropertyToID("SEGISecondaryBounceGain");
    static readonly int SEGISoftSunlight = Shader.PropertyToID("SEGISoftSunlight");
    static readonly int SEGISphericalSkylight = Shader.PropertyToID("SEGISphericalSkylight");
    static readonly int SEGIInnerOcclusionLayers = Shader.PropertyToID("SEGIInnerOcclusionLayers");
    static readonly int SEGISunDepth = Shader.PropertyToID("SEGISunDepth");
    static readonly int SEGIVolumeLevel0 = Shader.PropertyToID("SEGIVolumeLevel0");
    private static readonly int[] SEGIVolumeLevel =
    {
        Shader.PropertyToID("SEGIVolumeLevel0"),
        Shader.PropertyToID("SEGIVolumeLevel1"),
        Shader.PropertyToID("SEGIVolumeLevel2"),
        Shader.PropertyToID("SEGIVolumeLevel3"),
        Shader.PropertyToID("SEGIVolumeLevel4"),
        Shader.PropertyToID("SEGIVolumeLevel5"),
        Shader.PropertyToID("SEGIVolumeLevel6"),
    };
    static readonly int RG0 = Shader.PropertyToID("RG0");
    static readonly int Res = Shader.PropertyToID("Res");
    static readonly int Result = Shader.PropertyToID("Result");
    static readonly int PrevResult = Shader.PropertyToID("PrevResult");
    static readonly int VoxelAA = Shader.PropertyToID("VoxelAA");
    static readonly int Resolution = Shader.PropertyToID("Resolution");
    static readonly int VoxelOriginDelta = Shader.PropertyToID("VoxelOriginDelta");
    static readonly int destinationRes = Shader.PropertyToID("destinationRes");
    static readonly int Source = Shader.PropertyToID("Source");
    static readonly int Destination = Shader.PropertyToID("Destination");
    static readonly int SEGISecondaryCones = Shader.PropertyToID("SEGISecondaryCones");
    static readonly int SEGISecondaryOcclusionStrength = Shader.PropertyToID("SEGISecondaryOcclusionStrength");
    static readonly int SEGIVolumeTexture1 = Shader.PropertyToID("SEGIVolumeTexture1");

    static readonly int SEGIVoxelScaleFactor = Shader.PropertyToID("SEGIVoxelScaleFactor");
    static readonly int CameraToWorld = Shader.PropertyToID("CameraToWorld");
    static readonly int ProjectionMatrixInverse = Shader.PropertyToID("ProjectionMatrixInverse");
    static readonly int ProjectionMatrix = Shader.PropertyToID("ProjectionMatrix");
    static readonly int FrameSwitch = Shader.PropertyToID("FrameSwitch");
    static readonly int SEGIFrameSwitch = Shader.PropertyToID("SEGIFrameSwitch");
    static readonly int CameraPosition = Shader.PropertyToID("CameraPosition");
    static readonly int DeltaTime = Shader.PropertyToID("DeltaTime");
    static readonly int StochasticSampling = Shader.PropertyToID("StochasticSampling");
    static readonly int TraceDirections = Shader.PropertyToID("TraceDirections");
    static readonly int TraceSteps = Shader.PropertyToID("TraceSteps");
    static readonly int TraceLength = Shader.PropertyToID("TraceLength");
    static readonly int ConeSize = Shader.PropertyToID("ConeSize");
    static readonly int OcclusionStrength = Shader.PropertyToID("OcclusionStrength");
    static readonly int OcclusionPower = Shader.PropertyToID("OcclusionPower");
    static readonly int ConeTraceBias = Shader.PropertyToID("ConeTraceBias");
    static readonly int NearLightGain = Shader.PropertyToID("NearLightGain");
    static readonly int NearOcclusionStrength = Shader.PropertyToID("NearOcclusionStrength");
    static readonly int DoReflections = Shader.PropertyToID("DoReflections");
    static readonly int HalfResolution = Shader.PropertyToID("HalfResolution");
    static readonly int ReflectionSteps = Shader.PropertyToID("ReflectionSteps");
    static readonly int ReflectionOcclusionPower = Shader.PropertyToID("ReflectionOcclusionPower");
    static readonly int SkyReflectionIntensity = Shader.PropertyToID("SkyReflectionIntensity");
    static readonly int FarOcclusionStrength = Shader.PropertyToID("FarOcclusionStrength");
    static readonly int FarthestOcclusionStrength = Shader.PropertyToID("FarthestOcclusionStrength");
    static readonly int NoiseTexture = Shader.PropertyToID("NoiseTexture");
    static readonly int BlendWeight = Shader.PropertyToID("BlendWeight");

    static readonly int CurrentDepth = Shader.PropertyToID("CurrentDepth");
    static readonly int CurrentNormal = Shader.PropertyToID("CurrentNormal");
    static readonly int PreviousGITexture = Shader.PropertyToID("PreviousGITexture");
    static readonly int PreviousDepth = Shader.PropertyToID("PreviousDepth");
    static readonly int Reflections = Shader.PropertyToID("Reflections");
    static readonly int Kernel = Shader.PropertyToID("Kernel");
    static readonly int GITexture = Shader.PropertyToID("GITexture");
    static readonly int ProjectionPrev = Shader.PropertyToID("ProjectionPrev");
    static readonly int ProjectionPrevInverse = Shader.PropertyToID("ProjectionPrevInverse");
    static readonly int WorldToCameraPrev = Shader.PropertyToID("WorldToCameraPrev");
    static readonly int CameraToWorldPrev = Shader.PropertyToID("CameraToWorldPrev");
    static readonly int CameraPositionPrev = Shader.PropertyToID("CameraPositionPrev");
    #endregion

    #region Parameters
    [Serializable]
    public enum VoxelResolution
    {
        low = 128,
        high = 256
    }

    public bool updateGI = true;
    public LayerMask giCullingMask = 2147483647;
    public float shadowSpaceSize = 50.0f;
    public Light sun;

    public Color skyColor;

    public float voxelSpaceSize = 25.0f;

    public bool useBilateralFiltering = false;

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
    public bool infiniteBounces = false;
    public Transform followTransform;
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

    public bool voxelAA = false;

    public bool gaussianMipFilter = false;


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
    object initChecker;
    Material material;
    Camera attachedCamera;
    Transform shadowCamTransform;
    Camera shadowCam;
    GameObject shadowCamGameObject;
    Texture2D[] blueNoise;

    int sunShadowResolution = 256;
    int prevSunShadowResolution;

    Shader sunDepthShader;

    float shadowSpaceDepthRatio = 10.0f;

    int frameCounter = 0;


    RenderTexture sunDepthTexture;
    RenderTexture previousGIResult;
    RenderTexture previousCameraDepth;

    ///<summary>This is a volume texture that is immediately written to in the voxelization shader. The RInt format enables atomic writes to avoid issues where multiple fragments are trying to write to the same voxel in the volume.</summary>
    RenderTexture integerVolume;

    ///<summary>An array of volume textures where each element is a mip/LOD level. Each volume is half the resolution of the previous volume. Separate textures for each mip level are required for manual mip-mapping of the main GI volume texture.</summary>
    RenderTexture[] volumeTextures;

    ///<summary>The secondary volume texture that holds irradiance calculated during the in-volume GI tracing that occurs when Infinite Bounces is enabled. </summary>
    RenderTexture secondaryIrradianceVolume;

    ///<summary>The alternate mip level 0 main volume texture needed to avoid simultaneous read/write errors while performing temporal stabilization on the main voxel volume.</summary>
    RenderTexture volumeTextureB;

    ///<summary>The current active volume texture that holds GI information to be read during GI tracing.</summary>
    RenderTexture activeVolume;

    ///<summary>The volume texture that holds GI information to be read during GI tracing that was used in the previous frame.</summary>
    RenderTexture previousActiveVolume;

    ///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture scales depending on whether Voxel AA is enabled to ensure correct voxelization.</summary>
    RenderTexture dummyVoxelTextureAAScaled;

    ///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture is always the same size whether Voxel AA is enabled or not.</summary>
    RenderTexture dummyVoxelTextureFixed;

    bool notReadyToRender = false;

    Shader voxelizationShader;
    Shader voxelTracingShader;

    ComputeShader clearCompute;
    ComputeShader transferIntsCompute;
    ComputeShader mipFilterCompute;

    const int numMipLevels = 6;

    Camera voxelCamera;
    GameObject voxelCameraGO;
    GameObject leftViewPoint;
    GameObject topViewPoint;

    float voxelScaleFactor
    {
        get
        {
            return (float) this.voxelResolution / 256.0f;
        }
    }

    Vector3 voxelSpaceOrigin;
    Vector3 previousVoxelSpaceOrigin;
    Vector3 voxelSpaceOriginDelta;


    Quaternion rotationFront = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
    Quaternion rotationLeft = new Quaternion(0.0f, 0.7f, 0.0f, 0.7f);
    Quaternion rotationTop = new Quaternion(0.7f, 0.0f, 0.0f, 0.7f);

    int voxelFlipFlop = 0;


    enum RenderState
    {
        Voxelize,
        Bounce
    }

    RenderState renderState = RenderState.Voxelize;
    #endregion

    #region SupportingObjectsAndProperties
    struct Pass
    {
        public static int DiffuseTrace = 0;
        public static int BilateralBlur = 1;
        public static int BlendWithScene = 2;
        public static int TemporalBlend = 3;
        public static int SpecularTrace = 4;
        public static int GetCameraDepthTexture = 5;
        public static int GetWorldNormals = 6;
        public static int VisualizeGI = 7;
        public static int WriteBlack = 8;
        public static int VisualizeVoxels = 10;
        public static int BilateralUpsample = 11;
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

        public bool fullFunctionality
        {
            get
            {
                return this.hdrTextures && this.rIntTextures && this.dx11 && this.volumeTextures && this.postShader && this.sunDepthShader && this.voxelizationShader && this.tracingShader;
            }
        }
    }

    /// <summary>
    /// Contains info on system compatibility of required hardware functionality
    /// </summary>
    public SystemSupported systemSupported;

    /// <summary>
    /// Estimates the VRAM usage of all the render textures used to render GI.
    /// </summary>
    public float vramUsage
    {
        get
        {
            long v = 0;

            if (this.sunDepthTexture != null)
                v += this.sunDepthTexture.width * this.sunDepthTexture.height * 16;

            if (this.previousGIResult != null)
                v += this.previousGIResult.width * this.previousGIResult.height * 16 * 4;

            if (this.previousCameraDepth != null)
                v += this.previousCameraDepth.width * this.previousCameraDepth.height * 32;

            if (this.integerVolume != null)
                v += this.integerVolume.width * this.integerVolume.height * this.integerVolume.volumeDepth * 32;

            if (this.volumeTextures != null)
	            for (int i = 0; i < this.volumeTextures.Length; i++)
		            if (this.volumeTextures[i] != null)
			            v += this.volumeTextures[i].width * this.volumeTextures[i].height * this.volumeTextures[i].volumeDepth * 16 * 4;

            if (this.secondaryIrradianceVolume != null)
                v += this.secondaryIrradianceVolume.width * this.secondaryIrradianceVolume.height * this.secondaryIrradianceVolume.volumeDepth * 16 * 4;

            if (this.volumeTextureB != null)
                v += this.volumeTextureB.width * this.volumeTextureB.height * this.volumeTextureB.volumeDepth * 16 * 4;

            if (this.dummyVoxelTextureAAScaled != null)
                v += this.dummyVoxelTextureAAScaled.width * this.dummyVoxelTextureAAScaled.height * 8;

            if (this.dummyVoxelTextureFixed != null)
                v += this.dummyVoxelTextureFixed.width * this.dummyVoxelTextureFixed.height * 8;

            float vram = (v / 8388608.0f);

            return vram;
        }
    }

    int mipFilterKernel
    {
        get
        {
            return this.gaussianMipFilter ? 1 : 0;
        }
    }

    int dummyVoxelResolution
    {
        get
        {
            return (int) this.voxelResolution * (this.voxelAA ? 2 : 1);
        }
    }

    int giRenderRes
    {
        get
        {
            return this.halfResolution ? 2 : 1;
        }
    }

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
    }

    void Start()
    {
        InitCheck();
    }

    void InitCheck()
    {
        if (this.initChecker == null) Init();
    }

    void CreateVolumeTextures()
    {
        if (this.volumeTextures != null)
	        for (int i = 0; i < numMipLevels; i++)
		        if (this.volumeTextures[i] != null)
		        {
			        this.volumeTextures[i].DiscardContents();
			        this.volumeTextures[i].Release();
			        DestroyImmediate(this.volumeTextures[i]);
		        }

        this.volumeTextures = new RenderTexture[numMipLevels];

        for (int i = 0; i < numMipLevels; i++)
        {
            int resolution = (int) this.voxelResolution >> i;
            this.volumeTextures[i] = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
	        this.volumeTextures[i].dimension = TextureDimension.Tex3D;
	        this.volumeTextures[i].volumeDepth = resolution;
	        this.volumeTextures[i].enableRandomWrite = true;
	        this.volumeTextures[i].filterMode = FilterMode.Bilinear;
	        this.volumeTextures[i].autoGenerateMips = false;
	        this.volumeTextures[i].useMipMap = false;
	        this.volumeTextures[i].Create();
	        this.volumeTextures[i].hideFlags = HideFlags.HideAndDontSave;
        }

        if (this.volumeTextureB)
        {
	        this.volumeTextureB.DiscardContents();
	        this.volumeTextureB.Release();
            DestroyImmediate(this.volumeTextureB);
        }
        this.volumeTextureB = new RenderTexture((int) this.voxelResolution, (int) this.voxelResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
	    this.volumeTextureB.dimension = TextureDimension.Tex3D;
	    this.volumeTextureB.volumeDepth = (int) this.voxelResolution;
	    this.volumeTextureB.enableRandomWrite = true;
	    this.volumeTextureB.filterMode = FilterMode.Bilinear;
	    this.volumeTextureB.autoGenerateMips = false;
	    this.volumeTextureB.useMipMap = false;
	    this.volumeTextureB.Create();
	    this.volumeTextureB.hideFlags = HideFlags.HideAndDontSave;

        if (this.secondaryIrradianceVolume)
        {
	        this.secondaryIrradianceVolume.DiscardContents();
	        this.secondaryIrradianceVolume.Release();
            DestroyImmediate(this.secondaryIrradianceVolume);
        }
        this.secondaryIrradianceVolume = new RenderTexture((int) this.voxelResolution, (int) this.voxelResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
	    this.secondaryIrradianceVolume.dimension = TextureDimension.Tex3D;
	    this.secondaryIrradianceVolume.volumeDepth = (int) this.voxelResolution;
	    this.secondaryIrradianceVolume.enableRandomWrite = true;
	    this.secondaryIrradianceVolume.filterMode = FilterMode.Point;
	    this.secondaryIrradianceVolume.autoGenerateMips = false;
	    this.secondaryIrradianceVolume.useMipMap = false;
	    this.secondaryIrradianceVolume.antiAliasing = 1;
	    this.secondaryIrradianceVolume.Create();
	    this.secondaryIrradianceVolume.hideFlags = HideFlags.HideAndDontSave;



        if (this.integerVolume)
        {
	        this.integerVolume.DiscardContents();
	        this.integerVolume.Release();
            DestroyImmediate(this.integerVolume);
        }
        this.integerVolume = new RenderTexture((int) this.voxelResolution, (int) this.voxelResolution, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear);
	    this.integerVolume.dimension = TextureDimension.Tex3D;
	    this.integerVolume.volumeDepth = (int) this.voxelResolution;
	    this.integerVolume.enableRandomWrite = true;
	    this.integerVolume.filterMode = FilterMode.Point;
	    this.integerVolume.Create();
	    this.integerVolume.hideFlags = HideFlags.HideAndDontSave;

        ResizeDummyTexture();

    }

    void ResizeDummyTexture()
    {
        if (this.dummyVoxelTextureAAScaled)
        {
	        this.dummyVoxelTextureAAScaled.DiscardContents();
	        this.dummyVoxelTextureAAScaled.Release();
            DestroyImmediate(this.dummyVoxelTextureAAScaled);
        }
        this.dummyVoxelTextureAAScaled = new RenderTexture(dummyVoxelResolution, dummyVoxelResolution, 0, RenderTextureFormat.R8);
        this.dummyVoxelTextureAAScaled.Create();
        this.dummyVoxelTextureAAScaled.hideFlags = HideFlags.HideAndDontSave;

        if (this.dummyVoxelTextureFixed)
        {
	        this.dummyVoxelTextureFixed.DiscardContents();
	        this.dummyVoxelTextureFixed.Release();
            DestroyImmediate(this.dummyVoxelTextureFixed);
        }
        this.dummyVoxelTextureFixed = new RenderTexture((int) this.voxelResolution, (int) this.voxelResolution, 0, RenderTextureFormat.R8);
        this.dummyVoxelTextureFixed.Create();
        this.dummyVoxelTextureFixed.hideFlags = HideFlags.HideAndDontSave;
    }

    void Init()
    {
        //Setup shaders and materials
        if ((this.sunDepthShader = AssetsManager.LoadAsset<Shader>("SEGIRenderSunDepth")) == null)
            throw new ApplicationException("Could not find asset \"Hidden/SEGIRenderSunDepth\"");
        if ((this.clearCompute = AssetsManager.LoadAsset<ComputeShader>("SEGIClear")) == null)
            throw new ApplicationException("Could not find asset \"SEGIClear\"");
        if ((this.transferIntsCompute = AssetsManager.LoadAsset<ComputeShader>("SEGITransferInts")) == null)
            throw new ApplicationException("Could not find asset \"SEGITransferInts\"");
        if ((this.mipFilterCompute = AssetsManager.LoadAsset<ComputeShader>("SEGIMipFilter")) == null)
            throw new ApplicationException("Could not find asset \"Hidden/SEGIMipFilter\"");

        if ((this.voxelizationShader = AssetsManager.LoadAsset<Shader>("SEGIVoxelizeScene")) == null)
            throw new ApplicationException("Could not find asset \"Hidden/SEGIVoxelizeScene\"");
        if ((this.voxelTracingShader = AssetsManager.LoadAsset<Shader>("SEGITraceScene")) == null)
            throw new ApplicationException("Could not find asset \"Hidden/SEGITraceScene\"");

        if (!this.material)
        {
            Shader segi;
            if ((segi = AssetsManager.LoadAsset<Shader>("SEGI")) == null)
                throw new ApplicationException("Could not find asset \"Hidden/SEGI\"");

            this.material = new Material(segi);
            this.material.hideFlags = HideFlags.HideAndDontSave;
        }

        //Get the camera attached to this game object
        this.attachedCamera = GetComponent<Camera>();
        this.attachedCamera.depthTextureMode |= DepthTextureMode.Depth;
	    this.attachedCamera.depthTextureMode |= DepthTextureMode.MotionVectors;


        //Find the proxy shadow rendering camera if it exists
        GameObject scgo = GameObject.Find("SEGI_SHADOWCAM");

        //If not, create it
        if (!scgo)
        {
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
        else    //Otherwise, it already exists, just get it
        {
	        this.shadowCamGameObject = scgo;
	        this.shadowCam = scgo.GetComponent<Camera>();
	        this.shadowCamTransform = this.shadowCamGameObject.transform;
        }



        //Create the proxy camera objects responsible for rendering the scene to voxelize the scene. If they already exist, destroy them
        GameObject vcgo = GameObject.Find("SEGI_VOXEL_CAMERA");

        if (!vcgo)
        {
	        this.voxelCameraGO = new GameObject("SEGI_VOXEL_CAMERA");
	        this.voxelCameraGO.hideFlags = HideFlags.HideAndDontSave;

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
	        this.voxelCameraGO = vcgo;
	        this.voxelCamera = vcgo.GetComponent<Camera>();
        }

        GameObject lvp = GameObject.Find("SEGI_LEFT_VOXEL_VIEW");

        if (!lvp)
        {
	        this.leftViewPoint = new GameObject("SEGI_LEFT_VOXEL_VIEW");
	        this.leftViewPoint.hideFlags = HideFlags.HideAndDontSave;
        }
        else
        {
	        this.leftViewPoint = lvp;
        }

        GameObject tvp = GameObject.Find("SEGI_TOP_VOXEL_VIEW");

        if (!tvp)
        {
	        this.topViewPoint = new GameObject("SEGI_TOP_VOXEL_VIEW");
	        this.topViewPoint.hideFlags = HideFlags.HideAndDontSave;
        }
        else
        {
	        this.topViewPoint = tvp;
        }

        //Get blue noise textures
        this.blueNoise = null;
        this.blueNoise = new Texture2D[64];
        for (int i = 0; i < 64; i++)
        {
            string fileName = "LDR_RGBA_" + i.ToString();
            Texture2D blueNoiseTexture;
            if ((blueNoiseTexture = AssetsManager.LoadAsset<Texture2D>(fileName)) == null)
                throw new ApplicationException("Unable to find noise texture \"" + fileName + "\" for SEGI!");

            this.blueNoise[i] = blueNoiseTexture;

        }

        //Setup sun depth texture
        if (this.sunDepthTexture)
        {
	        this.sunDepthTexture.DiscardContents();
	        this.sunDepthTexture.Release();
            DestroyImmediate(this.sunDepthTexture);
        }
        this.sunDepthTexture = new RenderTexture(this.sunShadowResolution, this.sunShadowResolution, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        this.sunDepthTexture.wrapMode = TextureWrapMode.Clamp;
        this.sunDepthTexture.filterMode = FilterMode.Point;
        this.sunDepthTexture.Create();
        this.sunDepthTexture.hideFlags = HideFlags.HideAndDontSave;


        //Create the volume textures
        CreateVolumeTextures();

        this.initChecker = new object();
    }

    void CheckSupport()
    {
	    this.systemSupported.hdrTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
	    this.systemSupported.rIntTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RInt);
	    this.systemSupported.dx11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
	    this.systemSupported.volumeTextures = SystemInfo.supports3DTextures;

	    this.systemSupported.postShader = this.material.shader.isSupported;
	    this.systemSupported.sunDepthShader = this.sunDepthShader.isSupported;
	    this.systemSupported.voxelizationShader = this.voxelizationShader.isSupported;
	    this.systemSupported.tracingShader = this.voxelTracingShader.isSupported;

        if (!this.systemSupported.fullFunctionality)
        {
            Debug.LogWarning("SEGI is not supported on the current platform. Check for shader compile errors in SEGI/Resources");
            enabled = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!enabled)
            return;

        Color prevColor = Gizmos.color;
        Gizmos.color = new Color(1.0f, 0.25f, 0.0f, 0.5f);

        Gizmos.DrawCube(this.voxelSpaceOrigin, new Vector3(this.voxelSpaceSize, this.voxelSpaceSize, this.voxelSpaceSize));

        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.1f);

        Gizmos.color = prevColor;
    }

    void CleanupTexture(ref RenderTexture texture)
    {
        if (texture)
        {
            texture.DiscardContents();
            texture.Release();
            DestroyImmediate(texture);
        }
    }

    void CleanupTextures()
    {
        CleanupTexture(ref this.sunDepthTexture);
        CleanupTexture(ref this.previousGIResult);
        CleanupTexture(ref this.previousCameraDepth);
        CleanupTexture(ref this.integerVolume);
        for (int i = 0; i < this.volumeTextures.Length; i++) CleanupTexture(ref this.volumeTextures[i]);
        CleanupTexture(ref this.secondaryIrradianceVolume);
        CleanupTexture(ref this.volumeTextureB);
        CleanupTexture(ref this.dummyVoxelTextureAAScaled);
        CleanupTexture(ref this.dummyVoxelTextureFixed);
    }

    void Cleanup()
    {
        DestroyImmediate(this.material);
        DestroyImmediate(this.voxelCameraGO);
        DestroyImmediate(this.leftViewPoint);
        DestroyImmediate(this.topViewPoint);
        DestroyImmediate(this.shadowCamGameObject);
        this.initChecker = null;

        CleanupTextures();
    }

    void OnEnable()
    {
        InitCheck();
        ResizeRenderTextures();

        CheckSupport();
    }

    void OnDisable()
    {
        Cleanup();
    }

    void ResizeRenderTextures()
    {
        if (this.previousGIResult)
        {
	        this.previousGIResult.DiscardContents();
	        this.previousGIResult.Release();
            DestroyImmediate(this.previousGIResult);
        }

        int width = this.attachedCamera.pixelWidth == 0 ? 2 : this.attachedCamera.pixelWidth;
        int height = this.attachedCamera.pixelHeight == 0 ? 2 : this.attachedCamera.pixelHeight;

        this.previousGIResult = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
        this.previousGIResult.wrapMode = TextureWrapMode.Clamp;
        this.previousGIResult.filterMode = FilterMode.Bilinear;
        this.previousGIResult.useMipMap = true;
	    this.previousGIResult.autoGenerateMips = false;
	    this.previousGIResult.Create();
	    this.previousGIResult.hideFlags = HideFlags.HideAndDontSave;

        if (this.previousCameraDepth)
        {
	        this.previousCameraDepth.DiscardContents();
	        this.previousCameraDepth.Release();
            DestroyImmediate(this.previousCameraDepth);
        }
        this.previousCameraDepth = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        this.previousCameraDepth.wrapMode = TextureWrapMode.Clamp;
        this.previousCameraDepth.filterMode = FilterMode.Bilinear;
        this.previousCameraDepth.Create();
        this.previousCameraDepth.hideFlags = HideFlags.HideAndDontSave;
    }

    void ResizeSunShadowBuffer()
    {

        if (this.sunDepthTexture)
        {
	        this.sunDepthTexture.DiscardContents();
	        this.sunDepthTexture.Release();
            DestroyImmediate(this.sunDepthTexture);
        }
        this.sunDepthTexture = new RenderTexture(this.sunShadowResolution, this.sunShadowResolution, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        this.sunDepthTexture.wrapMode = TextureWrapMode.Clamp;
        this.sunDepthTexture.filterMode = FilterMode.Point;
        this.sunDepthTexture.Create();
        this.sunDepthTexture.hideFlags = HideFlags.HideAndDontSave;
    }

    void Update()
    {
        if (this.notReadyToRender)
            return;

        if (this.previousGIResult == null) ResizeRenderTextures();

        if (this.previousGIResult.width != this.attachedCamera.pixelWidth || this.previousGIResult.height != this.attachedCamera.pixelHeight) ResizeRenderTextures();

        if ((int) this.sunShadowResolution != this.prevSunShadowResolution) ResizeSunShadowBuffer();

        this.prevSunShadowResolution = (int) this.sunShadowResolution;

        if (this.volumeTextures[0].width != (int) this.voxelResolution) CreateVolumeTextures();

        if (this.dummyVoxelTextureAAScaled.width != dummyVoxelResolution) ResizeDummyTexture();
    }

    Matrix4x4 TransformViewMatrix(Matrix4x4 mat)
    {
        //Since the third column of the view matrix needs to be reversed if using reversed z-buffer, do so here
#if UNITY_5_5_OR_NEWER
        if (SystemInfo.usesReversedZBuffer)
        {
            mat[2, 0] = -mat[2, 0];
            mat[2, 1] = -mat[2, 1];
            mat[2, 2] = -mat[2, 2];
            mat[2, 3] = -mat[2, 3];
        }
#endif
        return mat;
    }
    void OnPreRender()
    {
        //Force reinitialization to make sure that everything is working properly if one of the cameras was unexpectedly destroyed
        if (!this.voxelCamera || !this.shadowCam) this.initChecker = null;

        InitCheck();

        if (this.notReadyToRender)
            return;

        if (!this.updateGI) return;

        //Cache the previous active render texture to avoid issues with other Unity rendering going on
        RenderTexture previousActive = RenderTexture.active;

        Shader.SetGlobalInt(SEGIVoxelAA, this.voxelAA ? 1 : 0);



        //Main voxelization work
        if (this.renderState == RenderState.Voxelize)
        {
	        this.activeVolume = this.voxelFlipFlop == 0 ? this.volumeTextures[0] : this.volumeTextureB;             //Flip-flopping volume textures to avoid simultaneous read and write errors in shaders
	        this.previousActiveVolume = this.voxelFlipFlop == 0 ? this.volumeTextureB : this.volumeTextures[0];

            //float voxelTexel = (1.0f * voxelSpaceSize) / (int)voxelResolution * 0.5f;			//Calculate the size of a voxel texel in world-space units


            //Setup the voxel volume origin position
            float interval = this.voxelSpaceSize / 8.0f;                                             //The interval at which the voxel volume will be "locked" in world-space
            Vector3 origin;
            if (this.followTransform)
	            origin = this.followTransform.position;
            else

	            //GI is still flickering a bit when the scene view and the game view are opened at the same time
	            origin = transform.position + transform.forward * this.voxelSpaceSize / 4.0f;

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
            this.voxelCameraGO.transform.position = this.voxelSpaceOrigin - Vector3.forward * this.voxelSpaceSize * 0.5f;
            this.voxelCameraGO.transform.rotation = this.rotationFront;

            this.leftViewPoint.transform.position = this.voxelSpaceOrigin + Vector3.left * this.voxelSpaceSize * 0.5f;
            this.leftViewPoint.transform.rotation = this.rotationLeft;
            this.topViewPoint.transform.position = this.voxelSpaceOrigin + Vector3.up * this.voxelSpaceSize * 0.5f;
            this.topViewPoint.transform.rotation = this.rotationTop;

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

                Vector3 shadowCamPosition = this.voxelSpaceOrigin + Vector3.Normalize(-this.sun.transform.forward) * this.shadowSpaceSize * 0.5f * this.shadowSpaceDepthRatio;

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

            Matrix4x4 voxelToGIProjection = (this.shadowCam.projectionMatrix) * (this.shadowCam.worldToCameraMatrix) * (this.voxelCamera.cameraToWorldMatrix);
            Shader.SetGlobalMatrix(SEGIVoxelToGIProjection, voxelToGIProjection);
            Shader.SetGlobalVector(SEGISunlightVector, this.sun ? Vector3.Normalize(this.sun.transform.forward) : Vector3.up);

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
            this.transferIntsCompute.SetVector(VoxelOriginDelta, (this.voxelSpaceOriginDelta / this.voxelSpaceSize) * (int) this.voxelResolution);
            this.transferIntsCompute.Dispatch(0, Math.Max(1, (int) this.voxelResolution / 16), Math.Max(1, (int) this.voxelResolution / 16), 1);

            Shader.SetGlobalTexture(SEGIVolumeLevel0, this.activeVolume);

            //Manually filter/render mip maps
            for (int i = 0; i < numMipLevels - 1; i++)
            {
                RenderTexture source = this.volumeTextures[i];

                if (i == 0) source = this.activeVolume;

                int destinationRes = (int) this.voxelResolution >> (i + 1);
                this.mipFilterCompute.SetInt(SEGIBehavior.destinationRes, destinationRes);
                this.mipFilterCompute.SetTexture(mipFilterKernel, Source, source);
                this.mipFilterCompute.SetTexture(mipFilterKernel, Destination, this.volumeTextures[i + 1]);
                this.mipFilterCompute.Dispatch(mipFilterKernel, Math.Max(1, destinationRes / 8), Math.Max(1, destinationRes / 8), 1);
                Shader.SetGlobalTexture(SEGIVolumeLevel[i + 1], this.volumeTextures[i + 1]);
            }

            //Advance the voxel flip flop counter
            this.voxelFlipFlop += 1;
            this.voxelFlipFlop = this.voxelFlipFlop % 2;

            if (this.infiniteBounces) this.renderState = RenderState.Bounce;
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
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.notReadyToRender)
        {
            Graphics.Blit(source, destination);
            return;
        }

        //Set parameters
        Shader.SetGlobalFloat(SEGIVoxelScaleFactor, voxelScaleFactor);

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
        RenderTexture gi1 = RenderTexture.GetTemporary(source.width / giRenderRes, source.height / giRenderRes, 0, RenderTextureFormat.ARGBHalf);
        RenderTexture gi2 = RenderTexture.GetTemporary(source.width / giRenderRes, source.height / giRenderRes, 0, RenderTextureFormat.ARGBHalf);
        RenderTexture reflections = null;

        //If reflections are enabled, create a temporary render buffer to hold them
        if (this.doReflections) reflections = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);

        //Setup textures to hold the current camera depth and normal
        RenderTexture currentDepth = RenderTexture.GetTemporary(source.width / giRenderRes, source.height / giRenderRes, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        currentDepth.filterMode = FilterMode.Point;

        RenderTexture currentNormal = RenderTexture.GetTemporary(source.width / giRenderRes, source.height / giRenderRes, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
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
        if (giRenderRes == 2)
        {
            RenderTexture.ReleaseTemporary(gi1);

            //Setup temporary textures
            RenderTexture gi3 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
            RenderTexture gi4 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);


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
        if (this.doReflections) RenderTexture.ReleaseTemporary(reflections);

        //Set matrices/vectors for use during temporal reprojection
        this.material.SetMatrix(ProjectionPrev, this.attachedCamera.projectionMatrix);
        this.material.SetMatrix(ProjectionPrevInverse, this.attachedCamera.projectionMatrix.inverse);
        this.material.SetMatrix(WorldToCameraPrev, this.attachedCamera.worldToCameraMatrix);
        this.material.SetMatrix(CameraToWorldPrev, this.attachedCamera.cameraToWorldMatrix);
        this.material.SetVector(CameraPositionPrev, transform.position);

        //Advance the frame counter
        this.frameCounter = (this.frameCounter + 1) % (64);
    }
}
