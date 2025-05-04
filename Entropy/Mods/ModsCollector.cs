using Steamworks;
using System.Reflection;

namespace Entropy.Mods;

/// <summary>
/// A class to monitor the mods folder and the workshop mods folder for changes and load and update mods.
/// </summary>
public static class ModsCollector
{
	private static FileSystemWatcher UserModsWatcher { get; }
	private static FileSystemWatcher? WorkshopModsWatcher { get; }
	private static readonly Dictionary<string, EntropyMod> LocatedMods = [];
	private static readonly Dictionary<Assembly, EntropyMod> ModAssemblies = [];

	/// <summary>
	/// The path to mods folder in user data directory.
	/// </summary>
	public static string UserModsPath { get; set; }

	/// <summary>
	/// The path to the workshop mods folder.
	/// </summary>
	public static string? WorkshopModsPath { get; set; }

	static ModsCollector()
	{
		UserModsPath = Path.Combine(StationSaveUtils.GetSavePath(), "mods");
		UserModsWatcher = new FileSystemWatcher(UserModsPath)
		{
			EnableRaisingEvents = true
		};
		UserModsWatcher.Changed += ModsFolderChanged;
		if(SteamClient.IsValid)
		{
			WorkshopModsPath = Path.GetFullPath(Path.Combine(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName, "..", "..", "..", "workshop", "content", SteamClient.AppId.ToString()));
			WorkshopModsWatcher = new FileSystemWatcher(WorkshopModsPath)
			{
				EnableRaisingEvents = true
			};
			WorkshopModsWatcher.Changed += ModsFolderChanged;
		}

		// First, add the main mod (Entropy itself) to the list of mods.
		var mainModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
			throw new ApplicationException("Failed to get the main mod folder path!");
		var mainModData = WorkshopMenu.ModsConfig.Mods.Find(x => x.DirectoryPath == mainModFolder) ?? new LocalModData(mainModFolder, true);
		var mainMod = EntropyMod.CreateMain(mainModData);
		AddMod(mainMod);
		// Let the plugin instance know it may swap the config instance, bad coding, but we're still in launching phase and have to hack a few things.
		EntropyPlugin.Instance.SwapConfig();

		// Then, load all mods from the mods folders, trying to find their entries in ModsConfig.
		foreach(var modFolder in Directory.GetDirectories(UserModsPath))
			LocateMod(modFolder);
		if(WorkshopModsPath is not null)
			foreach(var modFolder in Directory.GetDirectories(WorkshopModsPath))
				LocateMod(modFolder);
	}

	/// <summary>
	/// Static initializer for the <see cref="ModsCollector"/> class. Does nothing and exists as a way to invoke the static constructor.
	/// </summary>
	public static void Init() { }

	private static void ModsFolderChanged(object sender, FileSystemEventArgs e)
	{
		// Ignore changes of config.
		if(e.Name.EndsWith(".cfg"))
			return;

		// First, determine if the change comes from local mods folder or workshop mods folder.
		string modFolderPath = null!;
		if(e.FullPath.StartsWith(UserModsPath))
		{
			// Then, extract the mod by the folder name after the mods folder.
			// We need to get only the next folder name after the mods folder, so exclude mods folder path, and everything after the first slash.
			var modFolderName = e.FullPath[UserModsPath.Length..]
				.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).FirstOrDefault();
			// Now we can get the proper path to the mod folder.
			if(modFolderName is null)
				return;
			modFolderPath = Path.Combine(UserModsPath, modFolderName);
		}
		else if(WorkshopModsPath is not null && e.FullPath.StartsWith(WorkshopModsPath))
		{
			// Similarly for workshop mods.
			var modFolderName = e.FullPath[WorkshopModsPath.Length..]
				.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).FirstOrDefault();
			if(modFolderName is null)
				return;
			modFolderPath = Path.Combine(WorkshopModsPath, modFolderName);
		}
		if(!LocatedMods.ContainsKey(modFolderPath))
			LocateMod(modFolderPath);
	}

	/// <summary>
	/// Adds a mod to the list of mods.
	/// </summary>
	/// <param name="mod">The mod to add.</param>
	public static void AddMod(EntropyMod mod)
	{
		var modFolderPath = mod.DirectoryPath;
		if(mod is null)
			throw new ArgumentNullException(nameof(mod));
		try
		{
			LocatedMods[modFolderPath] = mod;
			ModAssemblies[mod.Assembly] = mod;
			mod.AnalyzeAssembly();

			var idx = WorkshopMenu.ModsConfig.Mods.IndexOf(mod.ModData);
			if(idx >= 0)
			{
				var gameModData = WorkshopMenu.ModsConfig.Mods[idx];
				mod.ModData.Enabled = gameModData.Enabled;
				// Replace in-game mod data instance with our mod data.
				WorkshopMenu.ModsConfig.Mods[idx] = mod.ModData;
			}
			else
				WorkshopMenu.ModsConfig.Mods.Add(mod.ModData);
		}
		catch (Exception e)
		{
			EntropyPlugin.LogError(mod.Name + " mod failed to load: " + e.Message);
			LocatedMods.Remove(modFolderPath);
			ModAssemblies.Remove(mod.Assembly);
		}
		if(mod.ModData.Enabled)
			mod.Configure();
	}

	/// <summary>
	/// Gets the mod data by its main assembly.
	/// </summary>
	/// <param name="assembly">The assembly to get the mod data for.</param>
	/// <returns>The mod data for the given assembly, or null if not found.</returns>
	public static EntropyMod? GetByAssembly(Assembly assembly) => ModAssemblies.GetValueOrDefault(assembly);

	/// <summary>
	/// Checks if the given folder path contains a mod, and if it does, adds it to the list of mods.
	/// </summary>
	/// <param name="modFolderPath">The path to the mod folder.</param>
	private static void LocateMod(string modFolderPath)
	{
		var modData = WorkshopMenu.ModsConfig.Mods.Find(x => x.DirectoryPath == modFolderPath);
		if(modData == null)
		{
			if(modFolderPath.StartsWith(UserModsPath))
				modData = new LocalModData(modFolderPath, true);
			else if(WorkshopModsPath is not null && modFolderPath.StartsWith(WorkshopModsPath))
			{
				// We need to get mod ID, which is the folder name after the workshop mods folder.
				// So we'll do folder name extraction like in ModsFolderChanged method.
				var modFolderName = modFolderPath[UserModsPath.Length..]
				.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).FirstOrDefault();
				// Now get the mod ID.
				if(!ulong.TryParse(modFolderName, out var id))
					return;
				modData = new WorkshopModData
				{
					WorkshopId = new UnsignedLongReference(id),
					DirectoryPath = new PathReference(modFolderPath),
					Enabled = true,
				};
			}
			else
				return;
		}
		var mod = EntropyMod.Create(modData);
		if(mod is not null)
			AddMod(mod);
	}
}