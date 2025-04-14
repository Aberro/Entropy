using System.IO;
using UnityEditor;

public class CreateAssetBundles
{
	[MenuItem("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
		if (Directory.Exists("AssetBundles"))
		{
			Directory.Delete("AssetBundles", true);
		}
		Directory.CreateDirectory("AssetBundles");

		foreach (var assetBundle in AssetDatabase.GetAllAssetBundleNames())
		{
			string[] assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundle);

			AssetBundleBuild bundleBuild = new AssetBundleBuild();
			bundleBuild.assetBundleName = assetBundle;
			bundleBuild.assetNames = assetNames;

			AssetBundleBuild[] buildMap = { bundleBuild };

			BuildPipeline.BuildAssetBundles(
				"AssetBundles",
				buildMap,
				BuildAssetBundleOptions.ForceRebuildAssetBundle
				| BuildAssetBundleOptions.UncompressedAssetBundle,
				BuildTarget.StandaloneWindows);
		}
		// Some garbage files Unity creates for some reason, we don't need those.
		File.Delete("AssetBundles\\AssetBundles");
        File.Delete("AssetBundles\\AssetBundles.manifest");
    }
}