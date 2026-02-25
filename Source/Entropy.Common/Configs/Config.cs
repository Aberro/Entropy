using BepInEx.Configuration;
using Entropy.Common.Utils;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using UnityEngine;
using EntropyConfigEntryBase = Entropy.Common.Configs.ConfigEntryBase;
using BepInExConfigEntryBase = BepInEx.Configuration.ConfigEntryBase;
using Entropy.Common.Mods;
using System.Diagnostics;

namespace Entropy.Common.Configs;

public class SettingChangedEventArgs(EntropyConfigEntryBase entry) : EventArgs
{
	public EntropyConfigEntryBase Entry { get; } = entry;
}
/// <summary>
/// A class to store the configuration for the mod.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Config"/> class.
/// </remarks>
public class Config
{
	ConfigFile _config;
	private readonly EntropyModBase _mod;

	private readonly Dictionary<EntropyConfigEntryBase, BepInExConfigEntryBase> _map = [];
	private readonly Dictionary<BepInExConfigEntryBase, EntropyConfigEntryBase> _reverseMap = [];

	public event EventHandler<SettingChangedEventArgs>? SettingChanged;

	/// <param name="mod">The mod that this configuration belongs to.</param>
	/// <param name="configFilePath">The path to the configuration file.</param>
	public Config(EntropyModBase mod, ConfigFile configFile)
	{
		this._config = configFile;
		this._mod = mod;
		this._config.SettingChanged += (sender, args) =>
		{
			if (_reverseMap.TryGetValue(args.ChangedSetting, out var entry))
			{
				if (entry.OnPropertyChanging(args.ChangedSetting.BoxedValue))
				{
					entry.OnPropertyChanged();
					SettingChanged?.Invoke(this, new SettingChangedEventArgs(entry));
				}
			}
		};
	}

	internal void BindConfigEntry<T>(EntropyConfigEntryBase entry, Optional<T> defaultValue, Optional<T> minValue, Optional<T> maxValue)
	{
		ArgumentNullException.ThrowIfNull(entry);
		if(this._mod != entry.Mod)
			throw new ArgumentException($"The entry {entry.Mod.Info.ModID}.{entry.Name} does not belong to the mod {this._mod.Info.Name}.");
		if(this._map.TryGetValue(entry, out var existingValue))
			return;
		AcceptableValueBase? acceptableValues = null;
		if (minValue != null && maxValue != null && typeof(T).IsNumeric())
		{
			// We have to bypass constraint of AcceptableValueRange, but it should be safe because we know T is a numeric value, so it must implement IComparable
			acceptableValues = (AcceptableValueBase)Activator.CreateInstance(typeof(AcceptableValueRange<>).MakeGenericType(typeof(T)), minValue.Value, maxValue.Value);
		}
		var description = new ConfigDescription(
			entry.Description,
			acceptableValues,
			new KeyValuePair<string, int>("Order", entry.Order),
			new KeyValuePair<string, string?>("DisplayName", entry.DisplayName ?? entry.Name),
			new KeyValuePair<string, bool>("Visible", entry.Visible),
			new KeyValuePair<string, bool>("Disabled", entry.Disabled),
			new KeyValuePair<string, Func<BepInExConfigEntryBase, bool>?>("CustomDrawer", ConvertCustomDrawer(entry.CustomDrawer)),
			new KeyValuePair<string, bool>("RequiresRestart", entry.RequireRestart),
			new KeyValuePair<string, string>("Format", entry.Format));
		var bepInExEntry = _config.Bind(entry.Category.DisplayName, entry.DisplayName, defaultValue.GetValueOrDefault(), description);
		var entryType = entry.GetType();
		if (entryType.IsGenericType)
		{
			var genericArgument = entryType.GetGenericArguments()[0];
			var bepInExGenericArgument = bepInExEntry.GetType().GetGenericArguments()[0];
			if (genericArgument != bepInExGenericArgument)
			{
				this._config.Remove(bepInExEntry.Definition);
				bepInExEntry = _config.Bind(entry.Category.DisplayName, entry.DisplayName, defaultValue.GetValueOrDefault(), entry.Description ?? "");
			}
		}
		this._map.Add(entry, bepInExEntry);
		this._reverseMap.Add(bepInExEntry, entry);
		this._config.Save();
		entry.BindBepInExEntry(bepInExEntry);
	}
	internal Func<BepInExConfigEntryBase, bool>? ConvertCustomDrawer(Func<EntropyConfigEntryBase, bool>? customDrawer) => 
		customDrawer is null ? null : (entry) => customDrawer(_reverseMap[entry]);
}