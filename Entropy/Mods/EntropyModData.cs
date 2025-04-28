using Entropy.Attributes;
using Entropy.Patches;
using HarmonyLib;
using System.Reflection;

namespace Entropy.Mods;

/// <summary>
/// A class storing info about a mod.
/// </summary>
public class EntropyModData : ModData
{
	private readonly FileSystemWatcher fileSystemWatcher;
	/// <summary>
	/// Name of the mod, as specified in the mod's About.xml file, or the name of the assembly of the mod.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Description of the mod, if available.
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Mod's version.
	/// </summary>
	public string Version { get; private set; }

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
	/// Creates a new instance of the <see cref="EntropyModData"/> class, using the specified mod path.
	/// </summary>
	/// <param name="modPath">The path to the mod directory.</param>
	/// <returns> Returns a new instance of the <see cref="EntropyModData"/> class if the mod is valid Entropy framework mod, otherwise null.</returns>
	public static EntropyModData? Create(string modPath)
	{
		if(!Directory.Exists(modPath))
			return null;
		var assemblies = Directory.GetFiles(modPath, "*.dll", SearchOption.TopDirectoryOnly);
		if(assemblies.Length == 0)
			return null;
		var dirName = Path.GetDirectoryName(modPath + Path.DirectorySeparatorChar + "x"); // get the name of mod directory;
		// We'll use domain to pre-load assemblies, so we can unload them after checking if they're Entropy mods.
		// If they are - we'll load them again in the main domain, otherwise - we won't hold them in memory for nothing.
		var domain = AppDomain.CreateDomain(dirName);
		try
		{
			foreach(var assemblyPath in assemblies)
			{
				Assembly? assembly;
				try
				{
					assembly = domain.Load(new AssemblyName { CodeBase = assemblyPath });
					// Ensure the mod is using Entropy framework.
					if(assembly.GetReferencedAssemblies().Any(x => x.Name == Assembly.GetExecutingAssembly().FullName))
					{
						return new EntropyModData(modPath, assemblyPath);
					}
				}
				// Suppress exceptions, we're only interested if the assembly uses Entropy framework and don't care if something's goes wrong with some assemblies in mods folders.
				catch { }
			}
		}
		finally
		{
			AppDomain.Unload(domain);
		}
		return null;
	}

	/// <summary>
	/// An internal constructor for the Entropy plugin itself.
	/// </summary>
	internal EntropyModData() : this(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().Location) => 
		Config = new Config(this, EntropyPlugin.Config.ConfigFilePath);

	private EntropyModData(string modPath, string assemblyPath)
		: this(Assembly.LoadFrom(assemblyPath) ?? throw new ApplicationException($"Failed to load '{assemblyPath}' assembly!"), modPath) => 
		Config = new Config(this, Path.Combine(this.DirectoryPath, "config.cfg"));

	private EntropyModData(Assembly assembly, string modPath)
	{
		this.DirectoryPath = new PathReference(modPath);
		this.fileSystemWatcher = new FileSystemWatcher(modPath)
		{
			IncludeSubdirectories = true,
			EnableRaisingEvents = true
		};
		this.fileSystemWatcher.Changed += FileSystemWatcher_Changed;
		var about = GetAboutData();
		if(about is null)
		{
			var selfTraverse = Traverse.Create(this);
			about = new ModAbout()
			{
				Name = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.GetName().Name,
				Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description,
				Version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "0.0",
			};
			selfTraverse.Field<ModAbout>("_modAboutData").Value = about;
		}
		Name = about.Name;
		Description = about.Description;
		Version = about.Version;
		Assembly = assembly;
		AnalyzeAssembly(assembly);
		// This constructor used only by other constructors, which set this field.
		Config = null!;
	}

	private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e) => Outdated = true;

	private void AnalyzeAssembly(Assembly assembly)
	{
		var types = AccessTools.GetTypesFromAssembly(assembly);
		foreach(var type in types)
		{
			var patchCategory = type.GetCustomAttribute<PatchCategoryDefinitionAttribute>();
			if(patchCategory != null)
				PatchCategory.Define(this, patchCategory.Name, patchCategory.DisplayName, patchCategory.Description);

			var patch = HarmonyPatchInfo.Create(type);
			if(patch != null)
			{
				PatchesCollector.AddPatch(this, patch);
			}
		}
	}
}