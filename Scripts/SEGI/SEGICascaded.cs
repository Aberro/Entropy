﻿using UnityEngine;
using UnityEngine.Rendering;
using System;
using Entropy.Scripts.Utilities;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Sonic Ether/SEGI (Cascaded)")]
public class SEGICascaded : MonoBehaviour
{

    #region Parameters

    public SEGIBehavior.VoxelResolution voxelResolution = SEGIBehavior.VoxelResolution.high;

    public bool visualizeSunDepthTexture = false;
    public bool visualizeGI = false;

    public Light sun;
    public LayerMask giCullingMask = 2147483647;

    public float shadowSpaceSize = 50.0f;

    [Range(0.01f, 1.0f)]
    public float temporalBlendWeight = 0.1f;

    public bool visualizeVoxels = false;

    public bool updateGI = true;


    public Color skyColor;

    public float voxelSpaceSize = 25.0f;

    public bool useBilateralFiltering = false;

    [Range(0, 2)]
    public int innerOcclusionLayers = 1;

    public bool halfResolution = false;
    public bool stochasticSampling = true;
    public bool infiniteBounces = false;
    public Transform followTransform;
    [Range(1, 128)]
    public int cones = 4;
    [Range(1, 32)]
    public int coneTraceSteps = 10;
    [Range(0.1f, 2.0f)]
    public float coneLength = 1.0f;
    [Range(0.5f, 6.0f)]
    public float coneWidth = 3.9f;
    [Range(0.0f, 2.0f)]
    public float occlusionStrength = 0.15f;
    [Range(0.0f, 4.0f)]
    public float nearOcclusionStrength = 0.5f;
    [Range(0.001f, 4.0f)]
    public float occlusionPower = 0.65f;
    [Range(0.0f, 4.0f)]
    public float coneTraceBias = 2.8f;
    [Range(0.0f, 4.0f)]
    public float nearLightGain = 0.36f;
    [Range(0.0f, 4.0f)]
    public float giGain = 1.0f;
    [Range(0.0f, 4.0f)]
    public float secondaryBounceGain = 0.9f;
    [Range(0.0f, 16.0f)]
    public float softSunlight = 0.0f;

    [Range(0.0f, 8.0f)]
    public float skyIntensity = 1.0f;

    [HideInInspector]
    public bool doReflections
    {
        get
        {
            return false;   //Locked to keep reflections disabled since they're in a broken state with cascades at the moment
        }
        set
        {
            value = false;
        }
    }

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
    public float secondaryOcclusionStrength = 0.27f;

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

    int sunShadowResolution = 128;
    int prevSunShadowResolution;

    Shader sunDepthShader;

    float shadowSpaceDepthRatio = 10.0f;

    int frameCounter = 0;



    RenderTexture sunDepthTexture;
    RenderTexture previousGIResult;
    RenderTexture previousDepth;

    ///<summary>This is a volume texture that is immediately written to in the voxelization shader. The RInt format enables atomic writes to avoid issues where multiple fragments are trying to write to the same voxel in the volume.</summary>
    RenderTexture integerVolume;

    ///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture scales depending on whether Voxel AA is enabled to ensure correct voxelization.</summary>
    RenderTexture dummyVoxelTextureAAScaled;

    ///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture is always the same size whether Voxel AA is enabled or not.</summary>
    RenderTexture dummyVoxelTextureFixed;

    ///<summary>The main GI data clipmaps that hold GI data referenced during GI tracing</summary>
    Clipmap[] clipmaps;

    ///<summary>The secondary clipmaps that hold irradiance data for infinite bounces</summary>
    Clipmap[] irradianceClipmaps;



    bool notReadyToRender = false;

    Shader voxelizationShader;
    Shader voxelTracingShader;

    ComputeShader clearCompute;
    ComputeShader transferIntsCompute;
    ComputeShader mipFilterCompute;

    const int numClipmaps = 6;
    int clipmapCounter = 0;
    int currentClipmapIndex = 0;

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

    Quaternion rotationFront = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
    Quaternion rotationLeft = new Quaternion(0.0f, 0.7f, 0.0f, 0.7f);
    Quaternion rotationTop = new Quaternion(0.7f, 0.0f, 0.0f, 0.7f);

    int giRenderRes
    {
        get
        {
            return this.halfResolution ? 2 : 1;
        }
    }

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
    public float vramUsage  //TODO: Update vram usage calculation
    {
        get
        {
            if (!enabled) return 0.0f;
            long v = 0;

            if (this.sunDepthTexture != null)
                v += this.sunDepthTexture.width * this.sunDepthTexture.height * 32;

            if (this.previousGIResult != null)
                v += this.previousGIResult.width * this.previousGIResult.height * 16 * 4;

            if (this.previousDepth != null)
                v += this.previousDepth.width * this.previousDepth.height * 32;

            if (this.integerVolume != null)
                v += this.integerVolume.width * this.integerVolume.height * this.integerVolume.volumeDepth * 32;

            if (this.dummyVoxelTextureAAScaled != null)
                v += this.dummyVoxelTextureAAScaled.width * this.dummyVoxelTextureAAScaled.height * 8;

            if (this.dummyVoxelTextureFixed != null)
                v += this.dummyVoxelTextureFixed.width * this.dummyVoxelTextureFixed.height * 8;

            if (this.clipmaps != null)
	            for (int i = 0; i < numClipmaps; i++)
		            if (this.clipmaps[i] != null)
			            v += this.clipmaps[i].volumeTexture0.width * this.clipmaps[i].volumeTexture0.height * this.clipmaps[i].volumeTexture0.volumeDepth * 16 * 4;

            if (this.irradianceClipmaps != null)
	            for (int i = 0; i < numClipmaps; i++)
		            if (this.irradianceClipmaps[i] != null)
			            v += this.irradianceClipmaps[i].volumeTexture0.width * this.irradianceClipmaps[i].volumeTexture0.height * this.irradianceClipmaps[i].volumeTexture0.volumeDepth * 16 * 4;

            float vram = (v / 8388608.0f);

            return vram;
        }
    }

    class Clipmap
    {
        public Vector3 origin;
        public Vector3 originDelta;
        public Vector3 previousOrigin;
        public float localScale;

        public int resolution;

        public RenderTexture volumeTexture0;

        public FilterMode filterMode = FilterMode.Bilinear;
        public RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGBHalf;

        public void UpdateTextures()
        {
            if (this.volumeTexture0)
            {
	            this.volumeTexture0.DiscardContents();
	            this.volumeTexture0.Release();
                DestroyImmediate(this.volumeTexture0);
            }
            this.volumeTexture0 = new RenderTexture(this.resolution, this.resolution, 0, this.renderTextureFormat, RenderTextureReadWrite.Linear);
            this.volumeTexture0.wrapMode = TextureWrapMode.Clamp;
	        this.volumeTexture0.dimension = TextureDimension.Tex3D;
	        this.volumeTexture0.volumeDepth = this.resolution;
	        this.volumeTexture0.enableRandomWrite = true;
	        this.volumeTexture0.filterMode = this.filterMode;
	        this.volumeTexture0.autoGenerateMips = false;
	        this.volumeTexture0.useMipMap = false;
	        this.volumeTexture0.Create();
	        this.volumeTexture0.hideFlags = HideFlags.HideAndDontSave;
        }

        public void CleanupTextures()
        {
            if (this.volumeTexture0)
            {
	            this.volumeTexture0.DiscardContents();
	            this.volumeTexture0.Release();
                DestroyImmediate(this.volumeTexture0);
            }
        }
    }

    public bool gaussianMipFilter
    {
        get
        {
            return false;
        }
        set
        {
            value = false;
        }
    }

    int mipFilterKernel
    {
        get
        {
            return gaussianMipFilter ? 1 : 0;
        }
    }

    public bool voxelAA = false;

    int dummyVoxelResolution
    {
        get
        {
            return (int) this.voxelResolution * (this.voxelAA ? 4 : 1);
        }
    }

    #endregion

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
        doReflections = preset.doReflections;

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
        gaussianMipFilter = preset.gaussianMipFilter;

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

    void BuildClipmaps()
    {
        if (this.clipmaps != null)
	        for (int i = 0; i < numClipmaps; i++)
		        if (this.clipmaps[i] != null)
			        this.clipmaps[i].CleanupTextures();

        this.clipmaps = new Clipmap[numClipmaps];

        for (int i = 0; i < numClipmaps; i++)
        {
	        this.clipmaps[i] = new Clipmap();
	        this.clipmaps[i].localScale = Mathf.Pow(2.0f, (float)i);
	        this.clipmaps[i].resolution = (int) this.voxelResolution;
	        this.clipmaps[i].filterMode = FilterMode.Bilinear;
	        this.clipmaps[i].renderTextureFormat = RenderTextureFormat.ARGBHalf;
	        this.clipmaps[i].UpdateTextures();
        }

        if (this.irradianceClipmaps != null)
	        for (int i = 0; i < numClipmaps; i++)
		        if (this.irradianceClipmaps[i] != null)
			        this.irradianceClipmaps[i].CleanupTextures();

        this.irradianceClipmaps = new Clipmap[numClipmaps];

        for (int i = 0; i < numClipmaps; i++)
        {
	        this.irradianceClipmaps[i] = new Clipmap();
	        this.irradianceClipmaps[i].localScale = Mathf.Pow(2.0f, i);
	        this.irradianceClipmaps[i].resolution = (int) this.voxelResolution;
	        this.irradianceClipmaps[i].filterMode = FilterMode.Point;
	        this.irradianceClipmaps[i].renderTextureFormat = RenderTextureFormat.ARGBHalf;
	        this.irradianceClipmaps[i].UpdateTextures();
        }
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

    void GetBlueNoiseTextures()
    {
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
    }

    void Init()
    {
        //Setup shaders and materials
        if ((this.sunDepthShader = AssetsManager.LoadAsset<Shader>("SEGIRenderSunDepth_C")) == null)
            throw new ApplicationException("Could not find asset \"Hidden/SEGIRenderSunDepth_C\"");
        if ((this.clearCompute = AssetsManager.LoadAsset<ComputeShader>("SEGIClear_C")) == null)
            throw new ApplicationException("Could not find asset \"SEGIClear_C\"");
        if ((this.transferIntsCompute = AssetsManager.LoadAsset<ComputeShader>("SEGITransferInts_C")) == null)
            throw new ApplicationException("Could not find asset \"SEGITransferInts_C\"");
        if ((this.mipFilterCompute = AssetsManager.LoadAsset<ComputeShader>("SEGIMipFilter_C")) == null)
            throw new ApplicationException("Could not find asset \"Hidden/SEGIMipFilter_C\"");

        if ((this.voxelizationShader = AssetsManager.LoadAsset<Shader>("SEGIVoxelizeScene_C")) == null)
            throw new ApplicationException("Could not find asset \"Hidden/SEGIVoxelizeScene_C\"");
        if ((this.voxelTracingShader = AssetsManager.LoadAsset<Shader>("SEGITraceScene_C")) == null)
            throw new ApplicationException("Could not find asset \"Hidden/SEGITraceScene_C\"");

        if (!this.material)
        {
            Shader segi;
            if ((segi = AssetsManager.LoadAsset<Shader>("SEGI_C")) == null)
                throw new ApplicationException("Could not find asset \"Hidden/SEGI_C\"");
            this.material = new Material(segi);
            this.material.hideFlags = HideFlags.HideAndDontSave;
        }

        //Get the camera attached to this game object
        this.attachedCamera = GetComponent<Camera>();
        this.attachedCamera.depthTextureMode |= DepthTextureMode.Depth;
        this.attachedCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
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



        CreateVolumeTextures();
        BuildClipmaps();
        GetBlueNoiseTextures();

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

        float scale = this.clipmaps[numClipmaps - 1].localScale;
        Gizmos.DrawCube(this.clipmaps[0].origin, new Vector3(this.voxelSpaceSize * scale, this.voxelSpaceSize * scale, this.voxelSpaceSize * scale));

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
        CleanupTexture(ref this.previousDepth);
        CleanupTexture(ref this.integerVolume);
        CleanupTexture(ref this.dummyVoxelTextureAAScaled);
        CleanupTexture(ref this.dummyVoxelTextureFixed);

        if (this.clipmaps != null)
	        for (int i = 0; i < numClipmaps; i++)
		        if (this.clipmaps[i] != null)
			        this.clipmaps[i].CleanupTextures();

        if (this.irradianceClipmaps != null)
	        for (int i = 0; i < numClipmaps; i++)
		        if (this.irradianceClipmaps[i] != null)
			        this.irradianceClipmaps[i].CleanupTextures();
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
        this.previousGIResult.Create();
        this.previousGIResult.hideFlags = HideFlags.HideAndDontSave;

        if (this.previousDepth)
        {
	        this.previousDepth.DiscardContents();
	        this.previousDepth.Release();
            DestroyImmediate(this.previousDepth);
        }
        this.previousDepth = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        this.previousDepth.wrapMode = TextureWrapMode.Clamp;
        this.previousDepth.filterMode = FilterMode.Bilinear;
        this.previousDepth.Create();
        this.previousDepth.hideFlags = HideFlags.HideAndDontSave;
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

        if (this.clipmaps[0].resolution != (int) this.voxelResolution)
        {
	        this.clipmaps[0].resolution = (int) this.voxelResolution;
	        this.clipmaps[0].UpdateTextures();
        }

        if (this.dummyVoxelTextureAAScaled.width != dummyVoxelResolution) ResizeDummyTexture();
    }

    Matrix4x4 TransformViewMatrix(Matrix4x4 mat)
    {
#if UNITY_5_5_OR_NEWER
        if (SystemInfo.usesReversedZBuffer)
        {
            mat[2, 0] = -mat[2, 0];
            mat[2, 1] = -mat[2, 1];
            mat[2, 2] = -mat[2, 2];
            mat[2, 3] = -mat[2, 3];
            // mat[3, 2] += 0.0f;
        }
#endif
        return mat;
    }

    int SelectCascadeBinary(int c)
    {
        float counter = c + 0.01f;

        int result = 0;
        for (int i = 1; i < numClipmaps; i++)
        {
            float level = Mathf.Pow(2.0f, i);
            result += Mathf.CeilToInt(((counter / level) % 1.0f) - ((level - 1.0f) / level));
        }

        return result;
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

        Shader.SetGlobalInt("SEGIVoxelAA", this.voxelAA ? 3 : 0);

        //Temporarily disable rendering of shadows on the directional light during voxelization pass. Cache the result to set it back to what it was after voxelization is done
        LightShadows prevSunShadowSetting = LightShadows.None;
        if (this.sun != null)
        {
            prevSunShadowSetting = this.sun.shadows;
            this.sun.shadows = LightShadows.None;
        }

        //Main voxelization work
        if (this.renderState == RenderState.Voxelize)
        {
	        this.currentClipmapIndex = SelectCascadeBinary(this.clipmapCounter);      //Determine which clipmap to update during this frame

            Clipmap activeClipmap = this.clipmaps[this.currentClipmapIndex];          //Set the active clipmap based on which one is determined to render this frame

            //If we're not updating the base level 0 clipmap, get the previous clipmap
            Clipmap prevClipmap = null;
            if (this.currentClipmapIndex != 0) prevClipmap = this.clipmaps[this.currentClipmapIndex - 1];

            float clipmapShadowSize = this.shadowSpaceSize * activeClipmap.localScale;
            float clipmapSize = this.voxelSpaceSize * activeClipmap.localScale;  //Determine the current clipmap's size in world units based on its scale
                                                                            //float voxelTexel = (1.0f * clipmapSize) / activeClipmap.resolution * 0.5f;	//Calculate the size of a voxel texel in world-space units





            //Setup the voxel volume origin position
            float interval = (clipmapSize) / 8.0f;                          //The interval at which the voxel volume will be "locked" in world-space
            Vector3 origin;
            if (this.followTransform)
	            origin = this.followTransform.position;
            else

	            //GI is still flickering a bit when the scene view and the game view are opened at the same time
	            origin = transform.position + transform.forward * clipmapSize / 4.0f;

            //Lock the voxel volume origin based on the interval
            activeClipmap.previousOrigin = activeClipmap.origin;
            activeClipmap.origin = new Vector3(Mathf.Round(origin.x / interval) * interval, Mathf.Round(origin.y / interval) * interval, Mathf.Round(origin.z / interval) * interval);


            //Clipmap delta movement for scrolling secondary bounce irradiance volume when this clipmap has changed origin
            activeClipmap.originDelta = activeClipmap.origin - activeClipmap.previousOrigin;
            Shader.SetGlobalVector("SEGIVoxelSpaceOriginDelta", activeClipmap.originDelta / (this.voxelSpaceSize * activeClipmap.localScale));






            //Calculate the relative origin and overlap/size of the previous cascade as compared to the active cascade. This is used to avoid voxelizing areas that have already been voxelized by previous (smaller) cascades
            Vector3 prevClipmapRelativeOrigin = Vector3.zero;
            float prevClipmapOccupance = 0.0f;
            if (this.currentClipmapIndex != 0)
            {
                prevClipmapRelativeOrigin = (prevClipmap.origin - activeClipmap.origin) / clipmapSize;
                prevClipmapOccupance = prevClipmap.localScale / activeClipmap.localScale;
            }
            Shader.SetGlobalVector("SEGIClipmapOverlap", new Vector4(prevClipmapRelativeOrigin.x, prevClipmapRelativeOrigin.y, prevClipmapRelativeOrigin.z, prevClipmapOccupance));

            //Calculate the relative origin and scale of this cascade as compared to the first (level 0) cascade. This is used during GI tracing/data lookup to ensure tracing is done in the correct space
            for (int i = 1; i < numClipmaps; i++)
            {
                Vector3 clipPosFromMaster = Vector3.zero;
                float clipScaleFromMaster = 1.0f;

                clipPosFromMaster = (this.clipmaps[i].origin - this.clipmaps[0].origin) / (this.voxelSpaceSize * this.clipmaps[i].localScale);
                clipScaleFromMaster = this.clipmaps[0].localScale / this.clipmaps[i].localScale;

                Shader.SetGlobalVector("SEGIClipTransform" + i.ToString(), new Vector4(clipPosFromMaster.x, clipPosFromMaster.y, clipPosFromMaster.z, clipScaleFromMaster));
            }

            //Set the voxel camera (proxy camera used to render the scene for voxelization) parameters
            this.voxelCamera.enabled = false;
            this.voxelCamera.orthographic = true;
            this.voxelCamera.orthographicSize = clipmapSize * 0.5f;
            this.voxelCamera.nearClipPlane = 0.0f;
            this.voxelCamera.farClipPlane = clipmapSize;
            this.voxelCamera.depth = -2;
            this.voxelCamera.renderingPath = RenderingPath.Forward;
            this.voxelCamera.clearFlags = CameraClearFlags.Color;
            this.voxelCamera.backgroundColor = Color.black;
            this.voxelCamera.cullingMask = this.giCullingMask;

            //Move the voxel camera game object and other related objects to the above calculated voxel space origin
            this.voxelCameraGO.transform.position = activeClipmap.origin - Vector3.forward * clipmapSize * 0.5f;
            this.voxelCameraGO.transform.rotation = this.rotationFront;

            this.leftViewPoint.transform.position = activeClipmap.origin + Vector3.left * clipmapSize * 0.5f;
            this.leftViewPoint.transform.rotation = this.rotationLeft;
            this.topViewPoint.transform.position = activeClipmap.origin + Vector3.up * clipmapSize * 0.5f;
            this.topViewPoint.transform.rotation = this.rotationTop;




            //Set matrices needed for voxelization
            //Shader.SetGlobalMatrix("WorldToGI", shadowCam.worldToCameraMatrix);
            //Shader.SetGlobalMatrix("GIToWorld", shadowCam.cameraToWorldMatrix);
            //Shader.SetGlobalMatrix("GIProjection", shadowCam.projectionMatrix);
            //Shader.SetGlobalMatrix("GIProjectionInverse", shadowCam.projectionMatrix.inverse);
            Shader.SetGlobalMatrix("WorldToCamera", this.attachedCamera.worldToCameraMatrix);
            Shader.SetGlobalFloat("GIDepthRatio", this.shadowSpaceDepthRatio);

            Matrix4x4 frontViewMatrix = TransformViewMatrix(this.voxelCamera.transform.worldToLocalMatrix);
            Matrix4x4 leftViewMatrix = TransformViewMatrix(this.leftViewPoint.transform.worldToLocalMatrix);
            Matrix4x4 topViewMatrix = TransformViewMatrix(this.topViewPoint.transform.worldToLocalMatrix);

            Shader.SetGlobalMatrix("SEGIVoxelViewFront", frontViewMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelViewLeft", leftViewMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelViewTop", topViewMatrix);
            Shader.SetGlobalMatrix("SEGIWorldToVoxel", this.voxelCamera.worldToCameraMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelProjection", this.voxelCamera.projectionMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelProjectionInverse", this.voxelCamera.projectionMatrix.inverse);

            Shader.SetGlobalMatrix("SEGIVoxelVPFront", GL.GetGPUProjectionMatrix(this.voxelCamera.projectionMatrix, true) * frontViewMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelVPLeft", GL.GetGPUProjectionMatrix(this.voxelCamera.projectionMatrix, true) * leftViewMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelVPTop", GL.GetGPUProjectionMatrix(this.voxelCamera.projectionMatrix, true) * topViewMatrix);

            Shader.SetGlobalMatrix("SEGIWorldToVoxel" + this.currentClipmapIndex.ToString(), this.voxelCamera.worldToCameraMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelProjection" + this.currentClipmapIndex.ToString(), this.voxelCamera.projectionMatrix);

            Matrix4x4 voxelToGIProjection = this.shadowCam.projectionMatrix * this.shadowCam.worldToCameraMatrix * this.voxelCamera.cameraToWorldMatrix;
            Shader.SetGlobalMatrix("SEGIVoxelToGIProjection", voxelToGIProjection);
            Shader.SetGlobalVector("SEGISunlightVector", this.sun ? Vector3.Normalize(this.sun.transform.forward) : Vector3.up);


            //Set paramteters
            Shader.SetGlobalInt("SEGIVoxelResolution", (int) this.voxelResolution);

            Shader.SetGlobalColor("GISunColor", this.sun == null ? Color.black : new Color(Mathf.Pow(this.sun.color.r, 2.2f), Mathf.Pow(this.sun.color.g, 2.2f), Mathf.Pow(this.sun.color.b, 2.2f), Mathf.Pow(this.sun.intensity, 2.2f)));
            Shader.SetGlobalColor("SEGISkyColor", new Color(Mathf.Pow(this.skyColor.r * this.skyIntensity * 0.5f, 2.2f), Mathf.Pow(this.skyColor.g * this.skyIntensity * 0.5f, 2.2f), Mathf.Pow(this.skyColor.b * this.skyIntensity * 0.5f, 2.2f), Mathf.Pow(this.skyColor.a, 2.2f)));
            Shader.SetGlobalFloat("GIGain", this.giGain);
            Shader.SetGlobalFloat("SEGISecondaryBounceGain", this.infiniteBounces ? this.secondaryBounceGain : 0.0f);
            Shader.SetGlobalFloat("SEGISoftSunlight", this.softSunlight);
            Shader.SetGlobalInt("SEGISphericalSkylight", this.sphericalSkylight ? 1 : 0);
            Shader.SetGlobalInt("SEGIInnerOcclusionLayers", this.innerOcclusionLayers);




            //Render the depth texture from the sun's perspective in order to inject sunlight with shadows during voxelization
            if (this.sun != null)
            {
	            this.shadowCam.cullingMask = this.giCullingMask;

                Vector3 shadowCamPosition = activeClipmap.origin + Vector3.Normalize(-this.sun.transform.forward) * clipmapShadowSize * 0.5f * this.shadowSpaceDepthRatio;

                this.shadowCamTransform.position = shadowCamPosition;
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
                Shader.SetGlobalMatrix("SEGIVoxelToGIProjection", voxelToGIProjection);


                Graphics.SetRenderTarget(this.sunDepthTexture);
                this.shadowCam.SetTargetBuffers(this.sunDepthTexture.colorBuffer, this.sunDepthTexture.depthBuffer);

                this.shadowCam.RenderWithShader(this.sunDepthShader, "");

                Shader.SetGlobalTexture("SEGISunDepth", this.sunDepthTexture);
            }







            //Clear the volume texture that is immediately written to in the voxelization scene shader
            this.clearCompute.SetTexture(0, "RG0", this.integerVolume);
            this.clearCompute.SetInt("Res", activeClipmap.resolution);
            this.clearCompute.Dispatch(0, Math.Max(1, activeClipmap.resolution / 16), Math.Max(1, activeClipmap.resolution / 16), 1);




            //Set irradiance "secondary bounce" texture
            Shader.SetGlobalTexture("SEGICurrentIrradianceVolume", this.irradianceClipmaps[this.currentClipmapIndex].volumeTexture0);


            Graphics.SetRandomWriteTarget(1, this.integerVolume);
            this.voxelCamera.targetTexture = this.dummyVoxelTextureAAScaled;
            this.voxelCamera.RenderWithShader(this.voxelizationShader, "");
            Graphics.ClearRandomWriteTargets();


            //Transfer the data from the volume integer texture to the main volume texture used for GI tracing.
            this.transferIntsCompute.SetTexture(0, "Result", activeClipmap.volumeTexture0);
            this.transferIntsCompute.SetTexture(0, "RG0", this.integerVolume);
            this.transferIntsCompute.SetInt("VoxelAA", this.voxelAA ? 3 : 0);
            this.transferIntsCompute.SetInt("Resolution", activeClipmap.resolution);
            this.transferIntsCompute.Dispatch(0, Math.Max(1, activeClipmap.resolution / 16), Math.Max(1, activeClipmap.resolution / 16), 1);



            //Push current voxelization result to higher levels
            for (int i = 0 + 1; i < numClipmaps; i++)
            {
                Clipmap sourceClipmap = this.clipmaps[i - 1];
                Clipmap targetClipmap = this.clipmaps[i];


                Vector3 sourceRelativeOrigin = Vector3.zero;
                float sourceOccupance = 0.0f;

                sourceRelativeOrigin = (sourceClipmap.origin - targetClipmap.origin) / (targetClipmap.localScale * this.voxelSpaceSize);
                sourceOccupance = sourceClipmap.localScale / targetClipmap.localScale;

                this.mipFilterCompute.SetTexture(0, "Source", sourceClipmap.volumeTexture0);
                this.mipFilterCompute.SetTexture(0, "Destination", targetClipmap.volumeTexture0);
                this.mipFilterCompute.SetVector("ClipmapOverlap", new Vector4(sourceRelativeOrigin.x, sourceRelativeOrigin.y, sourceRelativeOrigin.z, sourceOccupance));
                this.mipFilterCompute.SetInt("destinationRes", targetClipmap.resolution);
                this.mipFilterCompute.Dispatch(0, Math.Max(1, targetClipmap.resolution / 16), Math.Max(1, targetClipmap.resolution / 16), 1);
            }



            for (int i = 0; i < numClipmaps; i++) Shader.SetGlobalTexture("SEGIVolumeLevel" + i.ToString(), this.clipmaps[i].volumeTexture0);

            if (this.infiniteBounces)
            {
	            this.renderState = RenderState.Bounce;
            }
            else
            {
                //Increment clipmap counter
                this.clipmapCounter++;
                if (this.clipmapCounter >= (int)Mathf.Pow(2.0f, numClipmaps)) this.clipmapCounter = 0;
            }
        }
        else if (this.renderState == RenderState.Bounce)
        {
            //Calculate the relative position and scale of the current clipmap as compared to the first (level 0) clipmap. Used to ensure tracing is performed in the correct space
            Vector3 translateToZero = Vector3.zero;
            translateToZero = (this.clipmaps[this.currentClipmapIndex].origin - this.clipmaps[0].origin) / (this.voxelSpaceSize * this.clipmaps[this.currentClipmapIndex].localScale);
            float scaleToZero = 1.0f / this.clipmaps[this.currentClipmapIndex].localScale;
            Shader.SetGlobalVector("SEGICurrentClipTransform", new Vector4(translateToZero.x, translateToZero.y, translateToZero.z, scaleToZero));

            //Clear the volume texture that is immediately written to in the voxelization scene shader
            this.clearCompute.SetTexture(0, "RG0", this.integerVolume);
            this.clearCompute.SetInt("Res", this.clipmaps[this.currentClipmapIndex].resolution);
            this.clearCompute.Dispatch(0, Math.Max(1, (int) this.voxelResolution / 16), Math.Max(1, (int) this.voxelResolution / 16), 1);

            //Only render infinite bounces for clipmaps 0, 1, and 2
            if (this.currentClipmapIndex <= 2)
            {
                Shader.SetGlobalInt("SEGISecondaryCones", this.secondaryCones);
                Shader.SetGlobalFloat("SEGISecondaryOcclusionStrength", this.secondaryOcclusionStrength);

                Graphics.SetRandomWriteTarget(1, this.integerVolume);
                this.voxelCamera.targetTexture = this.dummyVoxelTextureFixed;
                this.voxelCamera.RenderWithShader(this.voxelTracingShader, "");
                Graphics.ClearRandomWriteTargets();

                this.transferIntsCompute.SetTexture(1, "Result", this.irradianceClipmaps[this.currentClipmapIndex].volumeTexture0);
                this.transferIntsCompute.SetTexture(1, "RG0", this.integerVolume);
                this.transferIntsCompute.SetInt("Resolution", (int) this.voxelResolution);
                this.transferIntsCompute.Dispatch(1, Math.Max(1, (int) this.voxelResolution / 16), Math.Max(1, (int) this.voxelResolution / 16), 1);
            }

            //Increment clipmap counter
            this.clipmapCounter++;
            if (this.clipmapCounter >= (int)Mathf.Pow(2.0f, numClipmaps)) this.clipmapCounter = 0;

            this.renderState = RenderState.Voxelize;

        }
        Matrix4x4 giToVoxelProjection = this.voxelCamera.projectionMatrix * this.voxelCamera.worldToCameraMatrix * this.shadowCam.cameraToWorldMatrix;
        Shader.SetGlobalMatrix("GIToVoxelProjection", giToVoxelProjection);



        RenderTexture.active = previousActive;

        //Set the sun's shadow setting back to what it was before voxelization
        if (this.sun != null) this.sun.shadows = prevSunShadowSetting;
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
        Shader.SetGlobalFloat("SEGIVoxelScaleFactor", voxelScaleFactor);

        this.material.SetMatrix("CameraToWorld", this.attachedCamera.cameraToWorldMatrix);
        this.material.SetMatrix("WorldToCamera", this.attachedCamera.worldToCameraMatrix);
        this.material.SetMatrix("ProjectionMatrixInverse", this.attachedCamera.projectionMatrix.inverse);
        this.material.SetMatrix("ProjectionMatrix", this.attachedCamera.projectionMatrix);
        this.material.SetInt("FrameSwitch", this.frameCounter);
        Shader.SetGlobalInt("SEGIFrameSwitch", this.frameCounter);
        this.material.SetVector("CameraPosition", transform.position);
        this.material.SetFloat("DeltaTime", Time.deltaTime);

        this.material.SetInt("StochasticSampling", this.stochasticSampling ? 1 : 0);
        this.material.SetInt("TraceDirections", this.cones);
        this.material.SetInt("TraceSteps", this.coneTraceSteps);
        this.material.SetFloat("TraceLength", this.coneLength);
        this.material.SetFloat("ConeSize", this.coneWidth);
        this.material.SetFloat("OcclusionStrength", this.occlusionStrength);
        this.material.SetFloat("OcclusionPower", this.occlusionPower);
        this.material.SetFloat("ConeTraceBias", this.coneTraceBias);
        this.material.SetFloat("GIGain", this.giGain);
        this.material.SetFloat("NearLightGain", this.nearLightGain);
        this.material.SetFloat("NearOcclusionStrength", this.nearOcclusionStrength);
        this.material.SetInt("DoReflections", doReflections ? 1 : 0);
        this.material.SetInt("HalfResolution", this.halfResolution ? 1 : 0);
        this.material.SetInt("ReflectionSteps", this.reflectionSteps);
        this.material.SetFloat("ReflectionOcclusionPower", this.reflectionOcclusionPower);
        this.material.SetFloat("SkyReflectionIntensity", this.skyReflectionIntensity);
        this.material.SetFloat("FarOcclusionStrength", this.farOcclusionStrength);
        this.material.SetFloat("FarthestOcclusionStrength", this.farthestOcclusionStrength);
        this.material.SetTexture("NoiseTexture", this.blueNoise[this.frameCounter]);
        this.material.SetFloat("BlendWeight", this.temporalBlendWeight);

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
        if (doReflections) reflections = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);

        //Get the camera depth and normals
        RenderTexture currentDepth = RenderTexture.GetTemporary(source.width / giRenderRes, source.height / giRenderRes, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        currentDepth.filterMode = FilterMode.Point;
        RenderTexture currentNormal = RenderTexture.GetTemporary(source.width / giRenderRes, source.height / giRenderRes, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        currentNormal.filterMode = FilterMode.Point;

        //Get the camera depth and normals
        Graphics.Blit(source, currentDepth, this.material, Pass.GetCameraDepthTexture);
        this.material.SetTexture("CurrentDepth", currentDepth);
        Graphics.Blit(source, currentNormal, this.material, Pass.GetWorldNormals);
        this.material.SetTexture("CurrentNormal", currentNormal);

        //Set the previous GI result and camera depth textures to access them in the shader
        this.material.SetTexture("PreviousGITexture", this.previousGIResult);
        Shader.SetGlobalTexture("PreviousGITexture", this.previousGIResult);
        this.material.SetTexture("PreviousDepth", this.previousDepth);

        //Render diffuse GI tracing result
        Graphics.Blit(source, gi2, this.material, Pass.DiffuseTrace);
        if (doReflections)
        {
            //Render GI reflections result
            Graphics.Blit(source, reflections, this.material, Pass.SpecularTrace);
            this.material.SetTexture("Reflections", reflections);
        }


        //Perform bilateral filtering
        if (this.useBilateralFiltering && this.temporalBlendWeight >= 0.99999f)
        {
	        this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
            Graphics.Blit(gi2, gi1, this.material, Pass.BilateralBlur);

            this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
            Graphics.Blit(gi1, gi2, this.material, Pass.BilateralBlur);

            this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
            Graphics.Blit(gi2, gi1, this.material, Pass.BilateralBlur);

            this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
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
            this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
            Graphics.Blit(gi4, gi3, this.material, Pass.BilateralUpsample);
            this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));



            //Perform a bilateral blur to be applied in newly revealed areas that are still noisy due to not having previous data blended with it
            RenderTexture blur0 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
            RenderTexture blur1 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
            this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
            Graphics.Blit(gi3, blur1, this.material, Pass.BilateralBlur);

            this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
            Graphics.Blit(blur1, blur0, this.material, Pass.BilateralBlur);

            this.material.SetVector("Kernel", new Vector2(0.0f, 2.0f));
            Graphics.Blit(blur0, blur1, this.material, Pass.BilateralBlur);

            this.material.SetVector("Kernel", new Vector2(2.0f, 0.0f));
            Graphics.Blit(blur1, blur0, this.material, Pass.BilateralBlur);

            this.material.SetTexture("BlurredGI", blur0);


            //Perform temporal reprojection and blending
            if (this.temporalBlendWeight < 1.0f)
            {
                Graphics.Blit(gi3, gi4);
                Graphics.Blit(gi4, gi3, this.material, Pass.TemporalBlend);
                Graphics.Blit(gi3, this.previousGIResult);
                Graphics.Blit(source, this.previousDepth, this.material, Pass.GetCameraDepthTexture);


                //Perform bilateral filtering on temporally blended result
                if (this.useBilateralFiltering)
                {
	                this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
                    Graphics.Blit(gi3, gi4, this.material, Pass.BilateralBlur);

                    this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
                    Graphics.Blit(gi4, gi3, this.material, Pass.BilateralBlur);

                    this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
                    Graphics.Blit(gi3, gi4, this.material, Pass.BilateralBlur);

                    this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
                    Graphics.Blit(gi4, gi3, this.material, Pass.BilateralBlur);
                }
            }





            //Set the result to be accessed in the shader
            this.material.SetTexture("GITexture", gi3);

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
            if (this.temporalBlendWeight < 1.0f)
            {
                //Perform a bilateral blur to be applied in newly revealed areas that are still noisy due to not having previous data blended with it
                RenderTexture blur0 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
                RenderTexture blur1 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
                this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
                Graphics.Blit(gi2, blur1, this.material, Pass.BilateralBlur);

                this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
                Graphics.Blit(blur1, blur0, this.material, Pass.BilateralBlur);

                this.material.SetVector("Kernel", new Vector2(0.0f, 2.0f));
                Graphics.Blit(blur0, blur1, this.material, Pass.BilateralBlur);

                this.material.SetVector("Kernel", new Vector2(2.0f, 0.0f));
                Graphics.Blit(blur1, blur0, this.material, Pass.BilateralBlur);

                this.material.SetTexture("BlurredGI", blur0);




                //Perform temporal reprojection and blending
                Graphics.Blit(gi2, gi1, this.material, Pass.TemporalBlend);
                Graphics.Blit(gi1, this.previousGIResult);
                Graphics.Blit(source, this.previousDepth, this.material, Pass.GetCameraDepthTexture);



                //Perform bilateral filtering on temporally blended result
                if (this.useBilateralFiltering)
                {
	                this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
                    Graphics.Blit(gi1, gi2, this.material, Pass.BilateralBlur);

                    this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
                    Graphics.Blit(gi2, gi1, this.material, Pass.BilateralBlur);

                    this.material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
                    Graphics.Blit(gi1, gi2, this.material, Pass.BilateralBlur);

                    this.material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
                    Graphics.Blit(gi2, gi1, this.material, Pass.BilateralBlur);
                }



                RenderTexture.ReleaseTemporary(blur0);
                RenderTexture.ReleaseTemporary(blur1);
            }

            //Actually apply the GI to the scene using gbuffer data
            this.material.SetTexture("GITexture", this.temporalBlendWeight < 1.0f ? gi1 : gi2);
            Graphics.Blit(source, destination, this.material, this.visualizeGI ? Pass.VisualizeGI : Pass.BlendWithScene);

            //Release temporary textures
            RenderTexture.ReleaseTemporary(gi1);
            RenderTexture.ReleaseTemporary(gi2);
        }

        //Release temporary textures
        RenderTexture.ReleaseTemporary(currentDepth);
        RenderTexture.ReleaseTemporary(currentNormal);

        if (this.visualizeSunDepthTexture)
            Graphics.Blit(this.sunDepthTexture, destination);

        //Release the temporary reflections result texture
        if (doReflections) RenderTexture.ReleaseTemporary(reflections);

        //Set matrices/vectors for use during temporal reprojection
        this.material.SetMatrix("ProjectionPrev", this.attachedCamera.projectionMatrix);
        this.material.SetMatrix("ProjectionPrevInverse", this.attachedCamera.projectionMatrix.inverse);
        this.material.SetMatrix("WorldToCameraPrev", this.attachedCamera.worldToCameraMatrix);
        this.material.SetMatrix("CameraToWorldPrev", this.attachedCamera.cameraToWorldMatrix);
        this.material.SetVector("CameraPositionPrev", transform.position);

        //Advance the frame counter
        this.frameCounter = (this.frameCounter + 1) % (64);
    }
}