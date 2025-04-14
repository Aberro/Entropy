# Building
To properly load the project, a "local.props" file needs to be created with the following content:

<Project>
	<PropertyGroup>
		<StationeersPath>Replace this with path to Stationeers game folder.</StationeersPath>
		<InstallMods>true</InstallMods>
	</PropertyGroup>
</Project>

Don't forget to set the proper path to the game. It might be either an absolute path, or a relative path (relative to the solution folder). No escaping nor quotes are requires, just don't forget '\' symbol at the end of the path.

The game needs to be modded by BepInEx for the project to build (because of dependencies).

For the first run, build SEGI project alone, then open the Unity project (open UnityAssetsProject folder in Unity Hub), in Unity Editor, click "Assets" -> "Build AssetBundles" in the main menu. This will generate the asset bundles for the mod. After that, you can build the whole solution, the mod will be in "bin\Debug\net472", additionally, by default the mod will be added into "<GameFolder>\BepInEx\plugins", this can be configured in local.props by InstallMods value.

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