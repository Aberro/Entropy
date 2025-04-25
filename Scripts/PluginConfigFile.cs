using BepInEx.Configuration;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using BepInEx;

namespace Entropy.Scripts
{
	public class PluginConfigFile : ConfigFile
	{
		public enum ConfigEntry
		{
			[DisplayName("Cascaded SEGI")]
			[Description("Experimental version of SEGI that should works better at long distances, but has fewer presets and has some missing features.")]
			CascadedSEGI,
			[DisplayName("SEGI preset")]
			[Description("Preconfigured settings for SEGI.")]
			SEGIPreset,
			[DisplayName("Cascaded SEGI preset")]
			[Description("Preconfigured settings for Cascaded SEGI.")]
			CascadedSEGIPreset,
			[DisplayName("Voxel resolution")]
			[Description("Higher resolutions are more accurate, but require more memory expensive.")]
			VoxelResolution,
			[DisplayName("Shadow map resolution")]
			[Description("Higher resolutions are more accurate, but require more memory expensive.")]
			ShadowMapResolution,
			[DisplayName("Voxel antialiasing")]
			[Description("Smooths out the edges of global illumination voxels.")]
			VoxelAA,
			[DisplayName("Inner occlusion layers")]
			InnerOcclusionLayers,
			[DisplayName("Infinite bounces")]
			[Description("Allows light to bounce infinitely until light source is hit, but is more performance expensive.")]
			InfiniteBounces,
			[DisplayName("Temporal blend weight")]
			[Description("Blends between the current frame and the previous frame to reduce temporal flickering.")]
			TemporalBlendWeight,
			[DisplayName("Use bilateral filtering")]
			UseBilateralFiltering,
			[DisplayName("Half resolution")]
			HalfResolution,
			[DisplayName("Stochastic sampling")]
			[Description("Randomizes the sampling pattern to smooth out light transport.")]
			StochasticSampling,
			[DisplayName("Reflections")]
			[Description("Enables reflections.")]
			DoReflections,
			[DisplayName("Cones")]
			[Description("Number of cones to use for cone tracing.")]
			Cones,
			[DisplayName("Cone trace steps")]
			[Description("Number of steps to use for cone tracing.")]
			ConeTraceSteps,
			[DisplayName("Cone length")]
			[Description("Length of the cone to use for cone tracing.")]
			ConeLength,
			[DisplayName("Cone width")]
			[Description("Width of the cone to use for cone tracing.")]
			ConeWidth,
			[Description("Cone tracing bias")]
			ConeTraceBias,
			[DisplayName("Occlusion strength")]
			OcclusionStrength,
			[DisplayName("Near occlusion strength")]
			NearOcclusionStrength,
			[DisplayName("Occlusion power")]
			OcclusionPower,
			[DisplayName("Near light gain")]
			NearLightGain,
			[DisplayName("GI gain")]
			[Description("Global illumination gain.")]
			GIGain,
			[DisplayName("Secondary bounce gain")]
			SecondaryBounceGain,
			[DisplayName("Reflection steps")]
			[Description("Number of steps to use for reflection tracing.")]
			ReflectionSteps,
			[DisplayName("Reflection occlusion power")]
			ReflectionOcclusionPower,
			[DisplayName("Sky reflection intensity")]
			SkyReflectionIntensity,
			[DisplayName("Gaussian mip filter")]
			GaussianMipFilter,
			[DisplayName("Far occlusion strength")]
			FarOcclusionStrength,
			[DisplayName("Farthest occlusion strength")]
			FarthestOcclusionStrength,
			[DisplayName("Secondary cones")]
			SecondaryCones,
			[DisplayName("Secondary occlusion strength")]
			SecondaryOcclusionStrength,

			[DisplayName("Small pump power")]
			[Description("Power of small pumps (volume pump, regulator, active vent, etc).")]
			SmallPumpPower,
			[DisplayName("Large pump power")]
			[Description("Power of large pumps (Powered vent, turbo pump).")]
			LargePumpPower,
			[DisplayName("Air conditioner power")]
			[Description("Power consumption of air conditioner.")]
			AirConditionerPower,
			[DisplayName("Air conditioner heat transfer efficiency")]
			[Description("Amount of heat power per electricity power consumption under ideal conditions.")]
			AirConditionerEfficiency,
		}
		public Dictionary<PatchCategory, ConfigEntry<bool>> Features { get; } = [];

		#region Atmospheric patches settings
		public ConfigEntry<float> SmallPumpPower;
		public ConfigEntry<float> LargePumpPower;
		public ConfigEntry<float> AirConditionerPower;
		public ConfigEntry<float> AirConditionerEfficiency;
		#endregion

		public PluginConfigFile(string configPath, bool saveOnInit) : base(configPath, saveOnInit)
		{
			Init();
		}

		public PluginConfigFile(string configPath, bool saveOnInit, BepInPlugin ownerMetadata) : base(configPath, saveOnInit, ownerMetadata)
		{
			Init();
		}

		[MemberNotNull(nameof(SmallPumpPower),
			nameof(LargePumpPower),
			nameof(AirConditionerPower),
			nameof(AirConditionerEfficiency))]
		private void Init()
		{
			foreach (PatchCategory category in Enum.GetValues(typeof(PatchCategory)))
			{
				if (category == PatchCategory.None)
					continue;
				if (!Features.TryGetValue(category, out var feature))
				{
					feature = Bind(category.GetDisplayName(), category.GetDisplayName() + " Enabled", false, category.GetDescription());
					Features.Add(category, feature);
				}
			}

			this.SmallPumpPower = Bind(PatchCategory.AtmosphericPatches.GetDisplayName(), ConfigEntry.SmallPumpPower.GetDisplayName(), 500f,
				new ConfigDescription(ConfigEntry.SmallPumpPower.GetDescription(), new AcceptableValueRange<float>(10f, 2000f)));
			this.LargePumpPower = Bind(PatchCategory.AtmosphericPatches.GetDisplayName(), ConfigEntry.LargePumpPower.GetDisplayName(), 1500f,
				new ConfigDescription(ConfigEntry.SmallPumpPower.GetDescription(), new AcceptableValueRange<float>(100f, 10000f)));
			this.AirConditionerPower = Bind(PatchCategory.AtmosphericPatches.GetDisplayName(), ConfigEntry.AirConditionerPower.GetDisplayName(), 1000f,
				new ConfigDescription(ConfigEntry.AirConditionerPower.GetDescription(), new AcceptableValueRange<float>(100f, 10000f)));
			this.AirConditionerEfficiency = Bind(PatchCategory.AtmosphericPatches.GetDisplayName(), ConfigEntry.AirConditionerEfficiency.GetDisplayName(), 4f,
				new ConfigDescription(ConfigEntry.AirConditionerPower.GetDescription(), new AcceptableValueRange<float>(0.5f, 10f)));
		}
	}
}
