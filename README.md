# Installing
This is a BepInEx mod, it does not require StationeersMods. But because of that, it needs to be placed in BepInEx\plugins folder of the game installation folder. And ALSO in workshop mods folder, to load xml resources.
To avoid copying files from workshop folder manually every time the mod updates, here's the steps you could do:
* Subscribe to the mod in steam: https://steamcommunity.com/sharedfiles/filedetails/?id=3469850624
* Download mklink.ps1 script file
* Run it.
It should create a symbolic link from the workshop mod folder to the BepInEx\plugins folder. The script will ask for administrative privileges, because creating symbolic links requires elevated permissions.
If you don't trust it, you can do it manually:
* Open powershell as administrator
* Go to the game installation folder
* Run the following commands:
```$target = "..\..\workshop\content\544550\3469850624"
```$path = "BepInEx\plugins\Entropy"
```New-Item -ItemType SymbolicLink -Path $path -Target $target
This will create a link to the workshop folder, so both locations have the same files, not a copy, but the exact same files. So when the mod updates and Steam downloads the updated files, files in BepInEx\plugins will be updates as well.
You only need to do this once after the installation.

# Building
To properly load the project, a "local.props" file needs to be created with the following content:

<Project>
	<PropertyGroup>
		<StationeersPath>Replace this with path to Stationeers game folder.</StationeersPath>
		<UnityPath>Optionally, include this and set path to Unity Editor.</UnityPath>>
		<InstallMods>true</InstallMods>
	</PropertyGroup>
</Project>

Don't forget to set the proper path to the game. It might be either an absolute path, or a relative path (relative to the solution folder). No escaping nor quotes are requires, just don't forget '\' symbol at the end of the path.

The game needs to be modded by BepInEx for the project to build (because of dependencies).

For the first run, build SEGI project alone, then open the Unity project (open UnityAssetsProject folder in Unity Hub), in Unity Editor, click "Assets" -> "Build AssetBundles" in the main menu. This will generate the asset bundles for the mod. After that, you can build the whole solution, the mod will be in "bin\Debug\net472", additionally, by default the mod will be added into "<GameFolder>\BepInEx\plugins", this can be configured in local.props by InstallMods value.

If UnityPath property is set, the build automatically would replace game's exe and dlls to enable debugging. You still need to create/modify boot.config file in rocketstation_Data of the game installation folder and set player-connection-debug=1 and optionally - wait-for-managed-debugger=1, to stop the game from launching until a debugger is connected.

With UnityPath set and boot.config configured, it's possible to attach Unity debugger to the game, but due to unknown reasons, only by inputting IP manually (the game is visible in "Select Unity instance" dialog, but refuses to attach). The correct IP would be 127.0.0.1, port is 1000 and mode is VSTU.

Sometimes, after cleanup or rebuild, or at the first built, the soluiton might need to be built second time because SEGI.pdb isn't found. Not sure what causes it, maybe disk caching, but just build it for the second time - it'll find the pdb file.

# Project structure
### Entropy
Entropy project is the mod itself.
### SEGI
SEGI project is a library to export into Unity project, that contains the data types required for building SEGI presets and the asset bundle imported into the game.
The project is configured to build into UnityAssetsProject/Assets/Assemblies folder, where Unity project will pick it up and use it for assets. The dll is stored in repository to keep the .meta file, as currently preset assets are using that file's guid, store in .meta file. If you don't trust the dll, you can delete it and build it yourself. Just make sure not to open Unity project when there's no .dll, otherwise .meta file will be removed and a new GUID will be generated once you build your dll. Don't forget to uncheck 'Validate References' in .dll's properties in Unity editor - the .dll doesn't go into mod folder, and types are instead defined in the main mod's .dll.

This way, the project is separated from Unity, has much less limitations on implementation, allows using much more versatile SDK project file format, latest language version, and is much, MUCH faster to build.
Implementing the project entirely in Unity was a complete disaster that worth many fruitless weeks of painful debugging of conflicting references.

# Folder structure
* Exports - content of this folder will be exported into mod folder as is.
  * About - Mod description
  * GameData - Mod dependencies and data, localization, etc.
* Properties - project debugging properties
* Scripts - source code
* UnityAssetsProject - Unity project that is used to build the asset bundles for the mod. No need to build this project, just use main menu "Assets" -> "Build AssetBundles". If there's no errors, the project should then pickup the generated asset bundle file(s) and export them into mod's folder upon build.

#FAQ
### Why not use StationeersMods?
Because StationeersMods is too finicky, and couldn't load BepInEx mods, and I need this mod to be BepInEx to inject the code into the game at launch to enable mod settings to work. Also, I don't like how it's written, personal opinion.