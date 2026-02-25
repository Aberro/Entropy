using BepInEx.Configuration;
using Entropy.Common.Attributes;
using Entropy.Common.Configs;
using Entropy.Common.Patching;
using Entropy.Common.Utils;
using HarmonyLib;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;

namespace Entropy.Common.Mods;

public abstract class EntropyModBase : MonoBehaviour
{
	private static readonly Dictionary<Assembly, EntropyModBase> _modAssemblies = new();
	private bool _isLoaded;
	private ConfigFile _configFile = null!;
	private Config _config = null!;
	private List<GameObject> _prefabs = null!;
	private readonly HashSet<ConfigCategory> _categories = [];

	/// <summary>
	/// List of all prefabs shipped with the mod.
	/// </summary>
	public IEnumerable<GameObject> Prefabs
	{
		get
		{
			if (!this._isLoaded)
				throw new InvalidOperationException("Mod not loaded yet.");
			return this._prefabs.AsReadOnly();
		}
	}
	/// <summary>
	/// Mod information.
	/// </summary>
	public ModInfo Info { get; }
	/// <summary>
	/// Mod configuration.
	/// </summary>
	public Config Config => this._config;

	/// <summary>
	/// Logger instance for the mod.
	/// </summary>
	public Logger Logger { get; }

	/// <summary>
	/// List of all patch categories defined in the mod.
	/// </summary>
	public IReadOnlyCollection<ConfigCategory> Categories => this._categories.AsReadOnly();

	internal protected EntropyModBase()
	{
		var assembly = this.GetType().Assembly;
		if (_modAssemblies.TryGetValue(assembly, out var mod))
			throw new InvalidOperationException($"Assembly `{assembly.FullName}` already has a mod `{mod.Info.Name}` instance of type `{mod.GetType().FullName}` created. Only one mod class may be defined in an assembly.");
		_modAssemblies.Add(assembly, this);
		Info = AssemblyUtils.GetModInfo(this);
		Logger = Logger.StealLogger(this);
	}
	[SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "This is part of API")]
	public virtual void OnLoaded(List<GameObject> prefabs, ConfigFile config)
	{
		this._prefabs = prefabs;
		this._configFile = config;
		this._config = new Config(this, config);
		AssemblyAnalyzer.AnalyzeAssembly(this);
		AssemblyAnalyzer.ConfigureMod(this, true);
		this._isLoaded = true;
	}

	internal void AddCategory(ConfigCategory result)
	{
		this._categories.Add(result);
	}
	internal static EntropyModBase? GetModByAssembly(Assembly assembly) => _modAssemblies.GetValueOrDefault(assembly);
}