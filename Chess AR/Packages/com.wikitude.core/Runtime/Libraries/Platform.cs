#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Wikitude
{
    /// <summary>
    /// Class used by the Wikitude SDK for platform and version dependent compilation. For internal use only.
    /// </summary>
    public class Platform : PlatformBase {

    #if UNITY_EDITOR
        [InitializeOnLoadMethod]
    #endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {
            if (_instance == null) {
                _instance = new Platform();
            }
        }

        public override void LoadImage(Texture2D texture, byte[] data) {
            texture.LoadImage(data);
        }

        public override byte[] EncodeToPNG(Texture2D texture) {
            return texture.EncodeToPNG();
        }

        public override byte[] EncodeToJPG(Texture2D texture) {
            return texture.EncodeToJPG();
        }

        public override string GetApplicationIdentifier() {
#if UNITY_EDITOR
            return PlayerSettings.applicationIdentifier;
#else
            return "";
#endif
        }
    }
}
