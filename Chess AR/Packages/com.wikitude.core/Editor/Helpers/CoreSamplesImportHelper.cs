using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace WikitudeEditor
{
    public class CoreSamplesImportHelper : AssetPostprocessor
    {
        private static ListRequest _listRequest;
        private const string _relativeStreamingAssetsPath = "Assets/StreamingAssets";
        private static string _relativeSamplesPath;
        private static string _relativeSamplesPlaceboFolderPath;
        private static string _relativeSamplesStreamingAssetsPath;

        static CoreSamplesImportHelper() {
            _listRequest = Client.List(true);
            EditorApplication.update += Update;
        }

        private static void Update() {
            if (_listRequest.IsCompleted && _listRequest.Status == StatusCode.Success){
                var package = _listRequest.Result.SingleOrDefault(result => result.name == "com.wikitude.core");
                if (package != null) {
                    _relativeSamplesPath = "Assets/Samples/" + package.displayName + "/" + package.version + "/Samples";
                    _relativeSamplesPlaceboFolderPath = _relativeSamplesPath + "/DeleteOnImport";
                    _relativeSamplesStreamingAssetsPath = package.assetPath + "/StreamingAssets~";
                }
                EditorApplication.update -= Update;
           }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            // If this directoy exists, the streaming assets haven't been moved yet.
            if (importedAssets.Length > 0 && AssetDatabase.IsValidFolder(_relativeSamplesPlaceboFolderPath)) {
                foreach (string str in importedAssets) {
                    if (str.IndexOf(_relativeSamplesPath) != -1) {
                        // Checking if the dialog is canceled to have the same layout as the re-import dialog before this dialog.
                        if (!EditorUtility.DisplayDialog("Importing Core Samples", "Do you also want to import the sample's target collections to the Streaming Assets folder? This is required for the samples to find them at runtime!", "Skip", "Import")) {
                            ImportStreamingAssetsFolder(true);
                        } else {
                            ImportStreamingAssetsFolder(false);
                        }
                        return;
                    }
                }
            }
        }

        private static void ImportStreamingAssetsFolder(bool import) {
            if (import){
                if (!AssetDatabase.IsValidFolder(_relativeStreamingAssetsPath)) {
                    FileUtil.ReplaceDirectory(_relativeSamplesStreamingAssetsPath, _relativeStreamingAssetsPath);
                } else {
                    FileInfo[] files = new DirectoryInfo(_relativeSamplesStreamingAssetsPath).GetFiles("*", SearchOption.AllDirectories);

                    foreach (FileInfo file in files.Where(f => f.Name.EndsWith(".wtc") || f.Name.EndsWith(".wto") || f.Name.EndsWith(".zip"))) {
                        string sourcePath = _relativeSamplesStreamingAssetsPath + "/" + file.Name;
                        string destinationPath = _relativeStreamingAssetsPath + "/" + file.Name;

                        FileUtil.ReplaceFile(sourcePath, destinationPath);
                    }
                }
            }

            FileUtil.DeleteFileOrDirectory(_relativeSamplesPlaceboFolderPath);
            FileUtil.DeleteFileOrDirectory(_relativeSamplesPlaceboFolderPath + ".meta");
            AssetDatabase.Refresh();
        }
    }
}
