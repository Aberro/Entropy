using Entropy.Attributes;
using Entropy.Patches;
using HarmonyLib;
using System.Reflection;

namespace Entropy.Mods;

/// <summary>
/// A class storing info about a mod.
/// </summary>
public class EntropyMod
{
	private readonly FileSystemWatcher _fileSystemWatcher;
	private readonly HashSet<PatchCategory> _categories = [];

	internal ModData ModData { get; }

	/// <summary>
	/// Path to the mod's directory.
	/// </summary>
	public string DirectoryPath => ModData.DirectoryPath.ToString();

	/// <summary>
	/// Name of the mod, as specified in the mod's About.xml file, or the name of the assembly of the mod.
	/// </summary>
	public string Name => ModData.GetAboutData().Name;

	/// <summary>
	/// Description of the mod, if available.
	/// </summary>
	public string? Description => ModData.GetAboutData().Description;

	/// <summary>
	/// Mod's version.
	/// </summary>
	public string Version => ModData.GetAboutData().Version;

	/// <summary>
	/// Mod configuration.
	/// </summary>
	public Config Config { get; private set; }

	/// <summary>
	/// A flag indicating if the mod files were updated since the last time it was loaded.
	/// </summary>
	public bool Outdated { get; private set; }

	/// <summary>
	/// Mod's main assembly.
	/// </summary>
	public Assembly Assembly { get; private set; }

	/// <summary>
	/// List of all patch categories defined in the mod.
	/// </summary>
	public IReadOnlyCollection<PatchCategory> Categories => this._categories.AsReadOnly();

	/// <summary>
	/// Creates a new instance of the <see cref="EntropyMod"/> class, using the specified mod path.
	/// </summary>
	/// <param name="modData">The <see cref="global::ModData"/> instance for the mod.</param>
	/// <returns> Returns a new instance of the <see cref="EntropyMod"/> class if the mod is valid Entropy framework mod, otherwise null.</returns>
	public static EntropyMod? Create(ModData modData)
	{
		var modPath = modData.DirectoryPath;
		if(!Directory.Exists(modPath))
			return null;
		var assemblies = Directory.GetFiles(modPath, "*.dll", SearchOption.TopDirectoryOnly);
		if(assemblies.Length == 0)
			return null;
		foreach(var assemblyPath in assemblies)
			try
			{
				var assemblyDef = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyPath);
				foreach(var reference in assemblyDef.MainModule.AssemblyReferences)
					if(reference.Name == Assembly.GetExecutingAssembly().GetName().Name)
						return new EntropyMod(Assembly.LoadFrom(assemblyPath), modData, Path.Combine(modData.DirectoryPath, "config.cfg"));
			}
			// Suppress exceptions, we're only interested if the assembly uses Entropy framework and don't care if something's goes wrong with some assemblies in mods folders.
			catch
			{
				// ignored
			}

		return null;
	}

	internal static EntropyMod CreateMain(ModData modData) =>
		new(Assembly.GetExecutingAssembly(), modData, Path.Combine(BepInEx.Paths.ConfigPath, EntropyPlugin.PluginGuid + ".cfg"));

	private EntropyMod(Assembly assembly, ModData modData, string configPath)
	{
		Config = new Config(this, configPath);
		ModData = modData;
		this._fileSystemWatcher = new FileSystemWatcher(DirectoryPath)
		{
			IncludeSubdirectories = true,
			EnableRaisingEvents = true
		};
		this._fileSystemWatcher.Changed += FileSystemWatcher_Changed;
		var about = modData.GetAboutData();
		if(about is null || !about.IsValid)
		{
			var selfTraverse = Traverse.Create(this);
			about = new ModAbout()
			{
				Author = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "Authors")?.Value
					?? assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Stationeers modding community",
				Name = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.GetName().Name,
				Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description,
				Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0",
			};
			selfTraverse.Field<ModAbout>("_modAboutData").Value = about;
		}
		Assembly = assembly;
	}

	internal void AnalyzeAssembly()
	{
		var types = AccessTools.GetTypesFromAssembly(Assembly);
		PatchesCollector.RegisterMod(this);
		foreach(var type in types)
		{
			var patchCategory = type.GetCustomAttribute<PatchCategoryDefinitionAttribute>();
			if(patchCategory != null)
				PatchCategory.Define(this, patchCategory.Name, patchCategory.DisplayName, patchCategory.Description);

			var patch = HarmonyPatchInfo.Create(type);
			if(patch != null)
				PatchesCollector.AddPatch(this, patch);
		}
	}
	internal void Configure() => PatchesCollector.ConfigureMod(this, ModData.Enabled);

	internal void AddCategory(PatchCategory result) => this._categories.Add(result);

	private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e) => Outdated = true;
}