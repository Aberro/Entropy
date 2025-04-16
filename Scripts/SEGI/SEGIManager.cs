using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Entropy.Scripts.Utilities;
using UnityEngine;
using Util;
using Object = UnityEngine.Object;

namespace Entropy.Scripts.SEGI
{
    public class SEGIManager : MonoBehaviour
    {
        public enum SettingPreset
        {
            [DisplayName("Custom")]
            Custom,
            [DisplayName("Low")]
            Low,
            [DisplayName("Medium")]
            Medium,
            [DisplayName("High")]
            High,
            [DisplayName("Ultra")]
            Ultra,
            [DisplayName("Insane")]
            Insane,
            [DisplayName("Sponza Low")]
            SponzaLow,
            [DisplayName("Sponza Medium")]
            SponzaMedium,
            [DisplayName("Sponza High")]
            SponzaHigh,
            [DisplayName("Sponza Ultra")]
            SponzaUltra,
            [DisplayName("Bright")]
            Bright,
            [DisplayName("Ultra Clean")]
            UltraClean,
        }

        public enum SettingCascadedPreset
        {
            [DisplayName("Custom")]
            Custom,
            [DisplayName("Lite")]
            Lite,
            [DisplayName("Standard")]
            Standard,
            [DisplayName("Accurate")]
            Accurate,
            [DisplayName("Accurate 2")]
            Accurate2,
        }

        private SEGIBehavior _currentSEGI;
        private SEGICascaded _currentCascadedSEGI;
        private SEGIPreset _currentPreset;
        private SEGIPreset _currentCascadedPreset;
        private Dictionary<SettingPreset, SEGIPreset> _presetsDictionary = new Dictionary<SettingPreset, SEGIPreset>();
        private Dictionary<SettingCascadedPreset, SEGIPreset> _cascadedPresetsDictionary = new Dictionary<SettingCascadedPreset, SEGIPreset>();

        public static void Enable()
        {
            var manager = FindObjectOfType<SEGIManager>();
            if (manager == null)
            {
                var gameObject = new GameObject("@@SEGI");
                DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<SEGIManager>();
                Plugin.Log("SEGI enabled");
            }
        }

        public static void Disable()
        {
            var manager = FindObjectOfType<SEGIManager>();
            if (manager != null)
            {
                var go = manager.gameObject;
                manager.DestroyComponent();
                go.DestroyGameObject();
            }
            Plugin.Log("SEGI disabled");
        }

        private void Awake()
        {
            LoadPresets();
            EnsureCurrentSEGI();
        }

        private void OnDestroy()
        {
            if (Camera.main)
            {
                var segi = Camera.main.gameObject.GetComponent<SEGIBehavior>();
                var segiCascaded = Camera.main.gameObject.GetComponent<SEGICascaded>();
                segi?.DestroyComponent();
                segiCascaded?.DestroyComponent();
            }
            RenderSettings.ambientIntensity = 1;
        }

        private void EnsureCurrentSEGI()
        {
            if (!Camera.main)
                return;
            if (Plugin.Config.CascadedSEGI)
            {
                if (!this._currentCascadedSEGI)
                {
	                this._currentCascadedSEGI = Camera.main.gameObject.GetComponent<SEGICascaded>();
                }
                else if (this._currentCascadedSEGI.gameObject != Camera.main.gameObject)
                {
	                this._currentCascadedSEGI.DestroyComponent();
	                this._currentCascadedSEGI = Camera.main.gameObject.GetComponent<SEGICascaded>();
	                this._currentCascadedSEGI.ApplyPreset(this._currentCascadedPreset);
                }
                if (!this._currentCascadedSEGI)
                {
	                this._currentCascadedSEGI = Camera.main.gameObject.AddComponent<SEGICascaded>();
	                this._currentCascadedSEGI.ApplyPreset(this._currentCascadedPreset);
                }
                if (this._currentSEGI)
                {
	                this._currentSEGI.enabled = false;
	                this._currentSEGI.DestroyComponent();
                }
                this._currentCascadedSEGI.enabled = true;
            }
            else
            {
                if (!this._currentSEGI)
                {
	                this._currentSEGI = Camera.main.gameObject.GetComponent<SEGIBehavior>();
                }
                else if (this._currentSEGI.gameObject != Camera.main.gameObject)
                {
	                this._currentSEGI.DestroyComponent();
	                this._currentSEGI = Camera.main.gameObject.GetComponent<SEGIBehavior>();
	                this._currentSEGI.ApplyPreset(this._currentPreset);
                }
                if (!this._currentSEGI)
                {
	                this._currentSEGI = Camera.main.gameObject.AddComponent<SEGIBehavior>();
	                this._currentSEGI.ApplyPreset(this._currentPreset);
                }
                if (this._currentCascadedSEGI)
                {
	                this._currentCascadedSEGI.enabled = false;
	                this._currentCascadedSEGI.DestroyComponent();
                }
                this._currentSEGI.enabled = true;
            }
            RenderSettings.ambientIntensity = 0;
            if (!this._currentCascadedSEGI && !this._currentSEGI)
                throw new ApplicationException("Unable to initialize SEGI!");
        }

        private void LoadPresets()
        {
            foreach (var preset in Enum.GetValues(typeof(SettingPreset)).Cast<SettingPreset>())
                if (preset != SettingPreset.Custom)
                {
                    var asset = AssetsManager.LoadAsset<SEGIPreset>(preset.GetDisplayName());
                    if (asset == null)
                        Debug.Log($"Unable to find preset {preset.GetDisplayName()} in \"entropy.assets\" file");
                    this._presetsDictionary.Remove(preset);
                    this._presetsDictionary.Add(preset, asset);
                }
            this._presetsDictionary.Add(SettingPreset.Custom, this._currentPreset = ScriptableObject.CreateInstance<SEGIPreset>());

            foreach (var preset in Enum.GetValues(typeof(SettingCascadedPreset)).Cast<SettingCascadedPreset>())
                if (preset != SettingCascadedPreset.Custom)
                {
                    var asset = AssetsManager.LoadAsset<SEGIPreset>(preset.GetDisplayName());
                    if (asset == null)
                        Debug.Log($"Unable to find preset {preset.GetDisplayName()} in \"entropy.assets\" file");
                    this._cascadedPresetsDictionary.Remove(preset);
                    this._cascadedPresetsDictionary.Add(preset, asset);
                }
            this._cascadedPresetsDictionary.Add(SettingCascadedPreset.Custom, this._currentCascadedPreset = ScriptableObject.CreateInstance<SEGIPreset>());
        }

        private void UpdateSettings()
        {
            SEGIPreset preset;
            bool customPreset = Plugin.Config.SEGICascadedPreset == SettingCascadedPreset.Custom;
            if (Plugin.Config.CascadedSEGI)
            {
                preset = this._cascadedPresetsDictionary[Plugin.Config.SEGICascadedPreset];
                if (preset == null)
                    LoadPresets();
                preset = this._cascadedPresetsDictionary[Plugin.Config.SEGICascadedPreset];
                if (preset == null)
                    throw new ApplicationException("Unable to find preset!");
                if (preset != this._currentCascadedPreset)
                {
	                this._currentCascadedPreset = preset;
                    UpdateConfigFromPreset(preset);
                    Plugin.Log($"{Plugin.Config.SEGICascadedPreset} preset applied");
                }
                else if (customPreset)
                {
                    UpdatePresetFromConfig(this._currentCascadedPreset);
                }
                this._currentCascadedSEGI.ApplyPreset(this._currentCascadedPreset);
            }
            else
            {
                preset = this._presetsDictionary[Plugin.Config.SEGIPreset];
                if (preset == null)
                    LoadPresets();
                preset = this._presetsDictionary[Plugin.Config.SEGIPreset];
                if (preset == null)
                    throw new ApplicationException("Unable to find preset!");
                if (preset != this._currentPreset)
                {
	                this._currentPreset = preset;
                    UpdateConfigFromPreset(preset);
                    Plugin.Log($"{Plugin.Config.SEGIPreset} preset applied");
                }
                else if (customPreset)
                {
                    UpdatePresetFromConfig(this._currentPreset);
                }
                this._currentSEGI.ApplyPreset(this._currentPreset);
            }
        }

        private void UpdateConfigFromPreset(SEGIPreset preset)
        {
            Plugin.Config.VoxelResolution = preset.voxelResolution;
            Plugin.Config.VoxelAA = preset.voxelAA;
            Plugin.Config.InnerOcclusionLayers = preset.innerOcclusionLayers;
            Plugin.Config.InfiniteBounces = preset.infiniteBounces;
            Plugin.Config.TemporalBlendWeight = preset.temporalBlendWeight;
            Plugin.Config.UseBilateralFiltering = preset.useBilateralFiltering;
            Plugin.Config.HalfResolution = preset.halfResolution;
            Plugin.Config.StochasticSampling = preset.stochasticSampling;
            Plugin.Config.DoReflections = preset.doReflections;
            Plugin.Config.Cones = preset.cones;
            Plugin.Config.ConeTraceSteps = preset.coneTraceSteps;
            Plugin.Config.ConeLength = preset.coneLength;
            Plugin.Config.ConeWidth = preset.coneWidth;
            Plugin.Config.ConeTraceBias = preset.coneTraceBias;
            Plugin.Config.OcclusionStrength = preset.occlusionStrength;
            Plugin.Config.NearOcclusionStrength = preset.nearOcclusionStrength;
            Plugin.Config.OcclusionPower = preset.occlusionPower;
            Plugin.Config.NearLightGain = preset.nearLightGain;
            Plugin.Config.GIGain = preset.giGain;
            Plugin.Config.SecondaryBounceGain = preset.secondaryBounceGain;
            Plugin.Config.ReflectionSteps = preset.reflectionSteps;
            Plugin.Config.ReflectionOcclusionPower = preset.reflectionOcclusionPower;
            Plugin.Config.SkyReflectionIntensity = preset.skyReflectionIntensity;
            Plugin.Config.GaussianMipFilter = preset.gaussianMipFilter;
            Plugin.Config.FarOcclusionStrength = preset.farOcclusionStrength;
            Plugin.Config.FarthestOcclusionStrength = preset.farthestOcclusionStrength;
            Plugin.Config.SecondaryCones = preset.secondaryCones;
            Plugin.Config.SecondaryOcclusionStrength = preset.secondaryOcclusionStrength;
        }

        void UpdatePresetFromConfig(SEGIPreset preset)
        {
            preset.voxelResolution = Plugin.Config.VoxelResolution;
            preset.voxelAA = Plugin.Config.VoxelAA;
            preset.innerOcclusionLayers = Plugin.Config.InnerOcclusionLayers;
            preset.infiniteBounces = Plugin.Config.InfiniteBounces;
            preset.temporalBlendWeight = Plugin.Config.TemporalBlendWeight;
            preset.useBilateralFiltering = Plugin.Config.UseBilateralFiltering;
            preset.halfResolution = Plugin.Config.HalfResolution;
            preset.stochasticSampling = Plugin.Config.StochasticSampling;
            preset.doReflections = Plugin.Config.DoReflections;
            preset.cones = Plugin.Config.Cones;
            preset.coneTraceSteps = Plugin.Config.ConeTraceSteps;
            preset.coneLength = Plugin.Config.ConeLength;
            preset.coneWidth = Plugin.Config.ConeWidth;
            preset.coneTraceBias = Plugin.Config.ConeTraceBias;
            preset.occlusionStrength = Plugin.Config.OcclusionStrength;
            preset.nearOcclusionStrength = Plugin.Config.NearOcclusionStrength;
            preset.occlusionPower = Plugin.Config.OcclusionPower;
            preset.nearLightGain = Plugin.Config.NearLightGain;
            preset.giGain = Plugin.Config.GIGain;
            preset.secondaryBounceGain = Plugin.Config.SecondaryBounceGain;
            preset.reflectionSteps = Plugin.Config.ReflectionSteps;
            preset.reflectionOcclusionPower = Plugin.Config.ReflectionOcclusionPower;
            preset.skyReflectionIntensity = Plugin.Config.SkyReflectionIntensity;
            preset.gaussianMipFilter = Plugin.Config.GaussianMipFilter;
            preset.farOcclusionStrength = Plugin.Config.FarOcclusionStrength;
            preset.farthestOcclusionStrength = Plugin.Config.FarthestOcclusionStrength;
            preset.secondaryCones = Plugin.Config.SecondaryCones;
            preset.secondaryOcclusionStrength = Plugin.Config.SecondaryOcclusionStrength;
        }

        private void Update()
        {

            if (Plugin.Config.CascadedSEGI)
            {
                if (!this._currentCascadedSEGI)
                    EnsureCurrentSEGI();
                UpdateSettings();
                this._currentCascadedSEGI.sun = WorldManager.Instance.WorldSun?.TargetLight;
            }
            else
            {
                if (!this._currentSEGI)
                    EnsureCurrentSEGI();
                UpdateSettings();
                this._currentSEGI.sun = WorldManager.Instance.WorldSun?.TargetLight;
            }
        }
    }
}