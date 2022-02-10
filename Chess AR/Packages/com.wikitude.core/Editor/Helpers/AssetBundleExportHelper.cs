using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WikitudeEditor
{
    public class AssetBundleExportHelper
    {
        [MenuItem("Window/Wikitude/AssetBundles Export Helper")]
        static void CreateAssetBundles() {
            string assetsDirectory = EditorUtility.OpenFolderPanel("Select folder to bundle", "Assets", "");
            assetsDirectory = assetsDirectory.Remove(0, Application.dataPath.Length - ("Assets").Length);

            if (String.IsNullOrEmpty(assetsDirectory)) {
                return;
            }

            string exportDirectory = assetsDirectory + "/Export";
            if(!Directory.Exists(exportDirectory)) {
                Directory.CreateDirectory(exportDirectory);
            }

            List<string> assetNames = new List<string>();
            FileInfo[] fileInfos = (new DirectoryInfo(assetsDirectory)).GetFiles("*.*");
            foreach (FileInfo fileInfo in fileInfos) {
                if (fileInfo.Extension != ".meta" && fileInfo.Extension != ".DS_Store") {
                    Debug.Log("Adding file " + assetsDirectory + "/" + fileInfo.Name + " to the to-bundle-list.");
                    assetNames.Add(assetsDirectory + "/" + fileInfo.Name);
                }
            }

            BuildAssetBundle(exportDirectory, "assetbundle-android", assetNames, BuildTarget.Android);
            BuildAssetBundle(exportDirectory, "assetbundle-ios", assetNames, BuildTarget.iOS);
        }

        static void BuildAssetBundle(string directory, string name, List<string> assets, BuildTarget target) {
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = name;
            buildMap[0].assetNames = assets.ToArray();
            BuildPipeline.BuildAssetBundles(directory, buildMap, BuildAssetBundleOptions.None, target);
        }
    }
}
