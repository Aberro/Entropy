using Entropy.Common.Mods;
using System.Reflection;

namespace Entropy.Common.Utils;


[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050:Declare types in namespaces", Justification = "This is a global utils file")]
internal static class AssemblyUtils
{
	//private static readonly System.Text.RegularExpressions.Regex dependencyRegex = new(@"^(ModID=""(?<modid>[^""]+)"")( Version=""(?<version>[^""]+)"")?( Type=""(?<type>Hard|Soft)"")?$", System.Text.RegularExpressions.RegexOptions.Compiled);

	public static ModInfo GetModInfo(EntropyModBase mod)
	{
		var assembly = mod.GetType().Assembly;
		return new ModInfo()
		{
			ModID = GetId(assembly),
			Name = GetName(assembly),
			Version = GetVersion(assembly),
			WorkshopId = GetWorkshopId(assembly),
		};
	}
	public static Version GetVersion(Assembly fromAssembly) => fromAssembly.GetName().Version;
	public static string GetId(Assembly fromAssembly) => fromAssembly.GetName().Name;
	public static string GetName(Assembly fromAssembly) => fromAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? GetId(fromAssembly);
	public static ulong GetWorkshopId(Assembly fromAssembly)
	{
		var metadata = fromAssembly.GetCustomAttributes<AssemblyMetadataAttribute>();
		return ulong.TryParse(metadata.FirstOrDefault(x => x.Key == "WorkshopHandle")?.Value, out var workshopId) ? workshopId : 0ul;
	}
	//public static GameType GetGameType(Assembly fromAssembly)
	//{
	//	var metadata = fromAssembly.GetCustomAttributes<AssemblyMetadataAttribute>();
	//	var gameTypeStr = metadata.FirstOrDefault(x => x.Key == "GameType")?.Value;
	//	if (Enum.TryParse<GameType>(gameTypeStr, out var gameType))
	//	{
	//		return gameType;
	//	}
	//	throw new FormatException($"Invalid or missing GameType metadata in assembly {GetId(fromAssembly)}");
	//}

	//public static List<ModDependencyInfo> GetDependencies(Assembly fromAssembly)
	//{
	//	var metadata = fromAssembly.GetCustomAttributes<AssemblyMetadataAttribute>();
	//	var dependencies = metadata.Where(x => x.Key == "DependsOn").Select(x => x.Value).ToArray();
	//	return ParseDependencies(dependencies).ToList();

	//	IEnumerable<ModDependencyInfo> ParseDependencies(string[] dependencies)
	//	{
	//		foreach (var dependency in dependencies)
	//		{
	//			var (guid, version, dependencyType) = ParseReference(dependency);
	//			yield return new ModDependencyInfo()
	//			{
	//				Guid = guid,
	//				Version = version,
	//				DependencyType = dependencyType,
	//			};
	//		}
	//	}
	//}

	//public static List<ModIncompatibilityInfo> GetIncompatibilities(Assembly fromAssembly)
	//{
	//	var metadata = fromAssembly.GetCustomAttributes<AssemblyMetadataAttribute>();
	//	var incompatibilities = metadata.Where(x => x.Key == "IncompatibleWith").Select(x => x.Value).ToArray();
	//	return ParseIncompatibilities(incompatibilities).ToList();
	//	IEnumerable<ModIncompatibilityInfo> ParseIncompatibilities(string[] incompatibilities)
	//	{
	//		foreach (var incompatibility in incompatibilities)
	//		{
	//			var (guid, version, _) = ParseReference(incompatibility);
	//			yield return new ModIncompatibilityInfo()
	//			{
	//				Guid = guid,
	//				Version = version,
	//			};
	//		}
	//	}
	//}


	//// The value for reference contains the reference itself and optionally a version and dependency type ("Hard" for required, "Soft" for optional).
	//// By default, the version is null (any version), and the type is "Hard".
	//// The reference should be in format "ModID=\"modid\""
	//// Version should be in Version parseable format "major.minor.build.revision".
	//// Example of a reference string: "ModID=\"stationeerslibrary\" Version=\"2.0\" Type=\"Hard\""
	//private static (string guid, Version? version, DependencyType dependencyType) ParseReference(string reference)
	//{
	//	var match = dependencyRegex.Match(reference);
	//	if (match.Success)
	//	{
	//		var guid = match.Groups["modid"].Success ? match.Groups["modid"].Value : null;
	//		var version = match.Groups["version"].Success && Version.TryParse(match.Groups["version"].Value, out Version ver) ? ver : null;
	//		var dependencyType = match.Groups["type"].Success && Enum.TryParse(match.Groups["type"].Value, out DependencyType dtype) ? dtype : DependencyType.Hard;
	//		if (guid == null)
	//		{
	//			throw new FormatException($"Invalid dependency format (missing ModID): {reference}");
	//		}
	//		return (guid, version, dependencyType);
	//	} else
	//	{
	//		throw new FormatException($"Invalid dependency format: {reference}");
	//	}
	//}
}
