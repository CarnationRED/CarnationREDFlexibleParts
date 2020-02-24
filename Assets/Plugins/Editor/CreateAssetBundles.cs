using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    //设定AssetBundle的储存路径
    static string AssetbundlePath = "Assets" + Path.DirectorySeparatorChar + "CarnationVariablePart" + Path.DirectorySeparatorChar + "Assetbundle" + Path.DirectorySeparatorChar;
    //编辑器扩展
    [MenuItem("Assets/Build AssetBundle")]
    static void BuildAssetsBundles()
    {
        //创建路径
        if (Directory.Exists(AssetbundlePath) == false)
        {
            Directory.CreateDirectory(AssetbundlePath);
        }
        //使用LZMA算法打包
        BuildPipeline.BuildAssetBundles(AssetbundlePath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}