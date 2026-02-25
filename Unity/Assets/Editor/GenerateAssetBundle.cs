using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
	[MenuItem("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
		if (!Directory.Exists("Assets/AssetBundles"))
			Directory.CreateDirectory("Assets/AssetBundles");

		string[] assetNames = AssetDatabase.GetAssetPathsFromAssetBundle("entropy");

		AssetBundleBuild bundleBuild = new AssetBundleBuild();
		bundleBuild.assetBundleName = "entropy.asset";
		bundleBuild.assetNames = assetNames;

		AssetBundleBuild[] buildMap = new AssetBundleBuild[] { bundleBuild };

		BuildPipeline.BuildAssetBundles(
			"Assets/AssetBundles",
			buildMap,
			BuildAssetBundleOptions.ForceRebuildAssetBundle
			| BuildAssetBundleOptions.UncompressedAssetBundle,
			BuildTarget.StandaloneWindows);
	}
}