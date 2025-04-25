#pragma warning disable CS0436 // Type conflicts with imported type
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Entropy.Scripts.Utilities;
using UnityEngine;
using Util;

namespace Entropy.Scripts.SEGI;

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

	private global::SEGI? _currentSEGI;
	private SEGIPreset _currentPreset = null!;
	private Dictionary<SettingPreset, SEGIPreset> _presetsDictionary = new Dictionary<SettingPreset, SEGIPreset>();
	private Dictionary<SettingCascadedPreset, SEGIPreset> _cascadedPresetsDictionary = new Dictionary<SettingCascadedPreset, SEGIPreset>();
	private bool _ignoreSettingChanged = false;

	public static void Enable()
	{
		var manager = FindObjectOfType<SEGIManager>();
		if (manager == null)
		{
			var gameObject = new GameObject("@@SEGI");
			DontDestroyOnLoad(gameObject);
			manager = gameObject.AddComponent<SEGIManager>();
			global::SEGI? segi = manager._currentSEGI;
			segi.ApplyPreset(manager._currentPreset);
			if(segi.IsValid() && segi.enabled)
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
		Plugin.Config.SettingChanged += Config_SettingChanged;
		LoadPresets();
		EnsureCurrentSEGI();
	}

	private void Config_SettingChanged(object sender, BepInEx.Configuration.SettingChangedEventArgs e)
	{
		if(this._ignoreSettingChanged)
			return;
		if(e.ChangedSetting == Plugin.Config.CascadedSEGIEntry)
		{
			EnsureCurrentSEGI();
		}

		bool presetChanged = false;
		if(e.ChangedSetting == Plugin.Config.SEGIPresetEntry && !Plugin.Config.CascadedSEGI && this._currentSEGI.IsValid())
		{
			presetChanged = true;
		}
		if(e.ChangedSetting == Plugin.Config.SEGICascadedPresetEntry && Plugin.Config.CascadedSEGI && this._currentSEGI.IsValid())
		{
			presetChanged = true;
		}
		if(e.ChangedSetting == Plugin.Config.VoxelResolutionEntry
			|| e.ChangedSetting == Plugin.Config.ShadowMapResolutionEntry
			|| e.ChangedSetting == Plugin.Config.VoxelAAEntry
			|| e.ChangedSetting == Plugin.Config.InnerOcclusionLayersEntry
			|| e.ChangedSetting == Plugin.Config.InfiniteBouncesEntry
			|| e.ChangedSetting == Plugin.Config.TemporalBlendWeightEntry
			|| e.ChangedSetting == Plugin.Config.UseBilateralFilteringEntry
			|| e.ChangedSetting == Plugin.Config.HalfResolutionEntry
			|| e.ChangedSetting == Plugin.Config.StochasticSamplingEntry
			|| e.ChangedSetting == Plugin.Config.DoReflectionsEntry
			|| e.ChangedSetting == Plugin.Config.ConesEntry
			|| e.ChangedSetting == Plugin.Config.ConeTraceStepsEntry
			|| e.ChangedSetting == Plugin.Config.ConeLengthEntry
			|| e.ChangedSetting == Plugin.Config.ConeWidthEntry
			|| e.ChangedSetting == Plugin.Config.ConeTraceBiasEntry
			|| e.ChangedSetting == Plugin.Config.OcclusionStrengthEntry
			|| e.ChangedSetting == Plugin.Config.NearOcclusionStrengthEntry
			|| e.ChangedSetting == Plugin.Config.OcclusionPowerEntry
			|| e.ChangedSetting == Plugin.Config.NearLightGainEntry
			|| e.ChangedSetting == Plugin.Config.GIGainEntry
			|| e.ChangedSetting == Plugin.Config.SecondaryBounceGainEntry
			|| e.ChangedSetting == Plugin.Config.ReflectionStepsEntry
			|| e.ChangedSetting == Plugin.Config.ReflectionOcclusionPowerEntry
			|| e.ChangedSetting == Plugin.Config.SkyReflectionIntensityEntry
			|| e.ChangedSetting == Plugin.Config.GaussianMipFilterEntry
			|| e.ChangedSetting == Plugin.Config.FarOcclusionStrengthEntry
			|| e.ChangedSetting == Plugin.Config.FarthestOcclusionStrengthEntry
			|| e.ChangedSetting == Plugin.Config.SecondaryConesEntry
			|| e.ChangedSetting == Plugin.Config.SecondaryOcclusionStrengthEntry)
		{
			if(Plugin.Config.CascadedSEGI)
			{
				SEGIPreset customPreset = this._cascadedPresetsDictionary[SettingCascadedPreset.Custom];
				UpdatePresetFromConfig(customPreset);
				if(Plugin.Config.SEGICascadedPreset != SettingCascadedPreset.Custom)
					Plugin.Config.SEGICascadedPreset = SettingCascadedPreset.Custom;
				else if (this._currentSEGI.IsValid())
					this._currentSEGI.ApplyPreset(customPreset);
			}
			else
			{
				var customPreset = this._presetsDictionary[SettingPreset.Custom];
				UpdatePresetFromConfig(customPreset);
				if(Plugin.Config.SEGIPreset != SettingPreset.Custom)
					Plugin.Config.SEGIPreset = SettingPreset.Custom;
				else if(this._currentSEGI.IsValid())
					this._currentSEGI.ApplyPreset(customPreset);
			}
		}
		if(presetChanged)
		{
			this._currentPreset = Plugin.Config.CascadedSEGI
				? this._cascadedPresetsDictionary[Plugin.Config.SEGICascadedPreset]
				: this._presetsDictionary[Plugin.Config.SEGIPreset];
			UpdateConfigFromPreset(this._currentPreset);
			if(this._currentSEGI.IsValid())
				this._currentSEGI.ApplyPreset(this._currentPreset);
		}
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
		Plugin.Config.SettingChanged -= Config_SettingChanged;
	}

	private void EnsureCurrentSEGI()
	{
		if (!Camera.main)
			return;
		// Check if the current SEGI is valid and if it is not, try to get a valid one from the main camera
		if(!this._currentSEGI.IsValid())
			this._currentSEGI = Plugin.Config.CascadedSEGI ? Camera.main.gameObject.GetComponent<SEGICascaded>() : Camera.main.gameObject.GetComponent<SEGIBehavior>();
		// Check if the current SEGI is attached to the main camera, or that it's of the correct type, if not - destroy it.
		else if(this._currentSEGI.gameObject != Camera.main.gameObject || (Plugin.Config.CascadedSEGI ? this._currentSEGI is SEGIBehavior : this._currentSEGI is SEGICascaded))
		{
			this._currentSEGI.DestroyComponent();
			// Additionally, ensure that camera don't have a SEGI of invalid type attached
			if(Plugin.Config.CascadedSEGI)
				Camera.main.GetComponent<SEGIBehavior>()?.DestroyComponent();
			else
				Camera.main.GetComponent<SEGICascaded>()?.DestroyComponent();
		}
		// If we destroyed the current SEGI, try again to get a valid one from the camera.
		if(!this._currentSEGI.IsValid())
			this._currentSEGI = Plugin.Config.CascadedSEGI ? Camera.main.gameObject.GetComponent<SEGICascaded>() : Camera.main.gameObject.GetComponent<SEGIBehavior>();
		// Most likely, we won't get one, so create a new one.
		if(!this._currentSEGI.IsValid())
			this._currentSEGI = Plugin.Config.CascadedSEGI ? Camera.main.gameObject.AddComponent<SEGICascaded>() : Camera.main.gameObject.AddComponent<SEGIBehavior>();
		// Ensure that we have a valid SEGI at the end.
		if(!this._currentSEGI.IsValid())
			throw new ApplicationException("Unable to initialize SEGI!");
		// Finally, apply the current preset
		this._currentSEGI.ApplyPreset(this._currentPreset);
		//LightmapSettings.
		RenderSettings.ambientIntensity = 0;
	}

	[MemberNotNull(nameof(_currentPreset))]
	private void LoadPresets()
	{
		foreach (var preset in Enum.GetValues(typeof(SettingPreset)).Cast<SettingPreset>())
			if (preset != SettingPreset.Custom)
			{
				var asset = AssetsManager.LoadAsset<SEGIPreset>(preset.GetDisplayName());
				if(asset == null)
				{
					Debug.Log($"Unable to find preset {preset.GetDisplayName()} in \"entropy.assets\" file");
					continue;
				}
				this._presetsDictionary.Remove(preset);
				this._presetsDictionary.Add(preset, asset);
			}
		this._presetsDictionary.Add(SettingPreset.Custom, ScriptableObject.CreateInstance<SEGIPreset>());

		foreach (var preset in Enum.GetValues(typeof(SettingCascadedPreset)).Cast<SettingCascadedPreset>())
			if (preset != SettingCascadedPreset.Custom)
			{
				var asset = AssetsManager.LoadAsset<SEGIPreset>(preset.GetDisplayName());
				if(asset == null)
				{
					Debug.Log($"Unable to find preset {preset.GetDisplayName()} in \"entropy.assets\" file");
					continue;
				}
				this._cascadedPresetsDictionary.Remove(preset);
				this._cascadedPresetsDictionary.Add(preset, asset);
			}
		this._cascadedPresetsDictionary.Add(SettingCascadedPreset.Custom, ScriptableObject.CreateInstance<SEGIPreset>());

		this._currentPreset = Plugin.Config.CascadedSEGI
			? this._cascadedPresetsDictionary[Plugin.Config.SEGICascadedPreset]
			: this._presetsDictionary[Plugin.Config.SEGIPreset];
	}

	private void UpdateConfigFromPreset(SEGIPreset preset)
	{
		this._ignoreSettingChanged = true;
		Plugin.Config.VoxelResolution = preset.voxelResolution;
		Plugin.Config.ShadowMapResolution = preset.sunShadowResolution;
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
		this._ignoreSettingChanged = false;
	}

	void UpdatePresetFromConfig(SEGIPreset preset)
	{
		preset.voxelResolution = Plugin.Config.VoxelResolution;
		preset.sunShadowResolution = Plugin.Config.ShadowMapResolution;
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
		if (!this._currentSEGI.IsValid())
			EnsureCurrentSEGI();
		if(this._currentSEGI.IsValid())
			this._currentSEGI.sun = WorldManager.Instance.WorldSun?.TargetLight;
	}
}