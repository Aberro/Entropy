using Assets.Scripts.Serialization;
using Steamworks;
using System.Reflection;

namespace Entropy.Mods;

/// <summary>
/// A class to monitor the mods folder and the workshop mods folder for changes and load and update mods.
/// </summary>
public static class ModsCollector
{
	private static FileSystemWatcher UserModsWatcher { get; set; } = null!;
	private static FileSystemWatcher? WorkshopModsWatcher { get; set; } = null!;
	private static readonly Dictionary<string, EntropyModData> locatedMods = [];
	private static readonly Dictionary<Assembly, EntropyModData> modAssemblies = [];

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
		UserModsPath = Settings.CurrentData.SavePath + "/mods";
		UserModsWatcher = new FileSystemWatcher(UserModsPath)
		{
			EnableRaisingEvents = true
		};
		UserModsWatcher.Changed += ModsFolderChanged;
		if(SteamClient.IsValid)
		{
			WorkshopModsPath = Path.GetFullPath($"{System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName}\\..\\..\\workshop\\content\\{SteamClient.AppId}\\");
			WorkshopModsWatcher = new FileSystemWatcher(WorkshopModsPath)
			{
				EnableRaisingEvents = true
			};
			WorkshopModsWatcher.Changed += ModsFolderChanged;
		}
	}

	private static void ModsFolderChanged(object sender, FileSystemEventArgs e)
	{
		// First, determine if the change comes from local mods folder or workshop mods folder.
		string modFolderPath = null!;
		string modFolderName = null!;
		if(e.FullPath.StartsWith(UserModsPath))
		{
			// Then, determine the mod by the folder name after the mods folder.
			// We need to get only the next folder name after the mods folder, so exclude mods folder path, and everything after the first slash.
			modFolderName = e.FullPath[UserModsPath.Length..]
				.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).FirstOrDefault();
			modFolderPath = Path.Combine(UserModsPath, modFolderName);
		}
		else if(WorkshopModsPath is not null && e.FullPath.StartsWith(WorkshopModsPath))
		{
			// For workshop mods, the mod folder name is it's ID.
			modFolderName = e.FullPath[WorkshopModsPath.Length..]
				.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).FirstOrDefault();
			modFolderPath = Path.Combine(WorkshopModsPath, modFolderName);
		}
		if(modFolderName is null)
			return;
		if(!locatedMods.ContainsKey(modFolderName))
		{
			LocateMod(modFolderName, modFolderPath);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="modData"></param>
	/// <param name="modFolderName"></param>
	public static void AddMod(EntropyModData modData, string modFolderName)
	{
		if(modData is null)
			throw new ArgumentNullException(nameof(modData));
		locatedMods[modFolderName] = modData;
		modAssemblies[modData.Assembly] = modData;
	}

	/// <summary>
	/// Gets the mod data by it's main assembly.
	/// </summary>
	/// <param name="assembly">The assembly to get the mod data for.</param>
	/// <returns>The mod data for the given assembly, or null if not found.</returns>
	public static EntropyModData? GetByAssembly(Assembly assembly) => modAssemblies.TryGetValue(assembly, out var modData) ? modData : null;

	private static void LocateMod(string modFolderName, string modFolderPath)
	{
		var modData = EntropyModData.Create(modFolderPath);
		if(modData is not null)
			AddMod(modData!, modFolderName);
	}
}