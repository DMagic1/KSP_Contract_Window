using UnityEditor;

public class Bundler
{
	const string dir = "AssetBundles";
	const string extension = ".cwp";

    [MenuItem("Contracts Window +/Build Bundles")]
    static void BuildAllAssetBundles()
    {
		BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);

		FileUtil.ReplaceFile(dir + "/contracts_window_prefabs", dir + "/contracts_window_prefabs" + extension);
        FileUtil.ReplaceFile(dir + "/unity_skin", dir + "/unity_skin" + extension);

        FileUtil.DeleteFileOrDirectory(dir + "/contracts_window_prefabs");
        FileUtil.DeleteFileOrDirectory(dir + "/unity_skin");
    }
}