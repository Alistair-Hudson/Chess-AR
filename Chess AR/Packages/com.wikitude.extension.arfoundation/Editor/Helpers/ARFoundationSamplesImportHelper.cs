using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace WikitudeEditor
{
    public class ARFoundationSamplesImportHelper : AssetPostprocessor
    {
        private static ListRequest _listRequest;
        private static string _relativeCoreSamplesPath;
        private static string _relativeARFoundationSamplesPath;

        static ARFoundationSamplesImportHelper() {
            _listRequest = Client.List(true);
            EditorApplication.update += Update;
        }

        private static void Update() {
            if (_listRequest.IsCompleted && _listRequest.Status == StatusCode.Success){
                var corePackage = _listRequest.Result.SingleOrDefault(result => result.name == "com.wikitude.core");
                if (corePackage != null) {
                    _relativeCoreSamplesPath = "Assets/Samples/" + corePackage.displayName + "/" + corePackage.version + "/Samples";
                }

                var arfoundationPackage = _listRequest.Result.SingleOrDefault(result => result.name == "com.wikitude.extension.arfoundation");
                if (arfoundationPackage != null) {
                    _relativeARFoundationSamplesPath = "Assets/Samples/" + arfoundationPackage.displayName + "/" + arfoundationPackage.version + "/Samples";
                }
                EditorApplication.update -= Update;
           }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (AssetDatabase.IsValidFolder(_relativeARFoundationSamplesPath) && !AssetDatabase.IsValidFolder(_relativeCoreSamplesPath)) {
                Debug.LogWarning("It seems that the Core Samples are missing. Please import the Core Samples from the Wikitude SDK Core package. If they were removed intentionally, please consider moving the AR Foundation Samples out of the Samples folder to dismiss this warning.");
            }
        }
    }
}
