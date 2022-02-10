using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.SceneManagement;

public class Popup {
    private const int _modalWindowWidth = 600;
    private const int _modalWindowHeight = 300;
    private string _message = "";
    public bool IsVisible {get ; private set;} = true;
    private PopupType _type;
    public enum PopupType {
        INFO,
        PERMISSION
    };

    public Popup(string message, PopupType type) {
        _message = message;
        _type = type;
    }

    public void SetVisibility(bool isVisible) {
        IsVisible = isVisible;
    }

    private void CreatePopupWindow(int id) {
        GUIStyle labelGuiStyle = new GUIStyle(GUI.skin.label) {
            fontSize = 34,
            padding = new RectOffset(10, 20, 0, 0)
        };
        labelGuiStyle.normal.textColor = Color.black;
        GUIStyle buttonGuiStyle = new GUIStyle(GUI.skin.button) {
            fontSize = 32
        };
        buttonGuiStyle.normal.textColor = Color.blue;
        buttonGuiStyle.active.background = Texture2D.whiteTexture;
        buttonGuiStyle.normal.background = Texture2D.whiteTexture;
        buttonGuiStyle.focused.background = Texture2D.whiteTexture;
        buttonGuiStyle.hover.background = Texture2D.whiteTexture;
        GUI.Label(new Rect(20, 30, _modalWindowWidth - 20, _modalWindowHeight - 50), _message, labelGuiStyle);

#if UNITY_ANDROID
        if (this._type == PopupType.PERMISSION) {
            if (GUI.Button(new Rect(20, _modalWindowHeight - 70, 140, 60), "Cancel", buttonGuiStyle)) {
                IsVisible = false;
            }
            if (GUI.Button(new Rect(_modalWindowWidth - 160, _modalWindowHeight - 70, 140, 60), "OK", buttonGuiStyle)) {
                Permission.RequestUserPermission(Permission.Camera);
            }
        }
#endif

        if (this._type == PopupType.INFO) {
            if (GUI.Button(new Rect(20, _modalWindowHeight - 70, 140, 60), "OK", buttonGuiStyle)) {
                IsVisible = false;
            }
        }
    }
    public bool ShowPopup() {
        if (IsVisible) {

#if UNITY_ANDROID
            if (this._type == PopupType.PERMISSION && !Permission.HasUserAuthorizedPermission(Permission.Camera)) {
                Rect rect = new Rect((Screen.width / 2) - (_modalWindowWidth / 2), (Screen.height / 2) - (_modalWindowHeight / 2), _modalWindowWidth, _modalWindowHeight);
                GUIStyle windowGuiStyle = new GUIStyle(GUI.skin.box);
                windowGuiStyle.normal.background = Texture2D.whiteTexture;
                GUI.Window(0, rect, CreatePopupWindow, "", windowGuiStyle);
            } else if (this._type == PopupType.PERMISSION) {
                /*
                    When requesting for the camera permission the scene is not stopped at all and the WikitudeSDK
                    is loaded as usual, with a black screen because of the missing permission. Once the permission
                    is accepted we need to reload the scene to initialize everything in a proper way.
                */
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
#endif

            if (this._type == PopupType.INFO) {
                Rect rect = new Rect((Screen.width / 2) - (_modalWindowWidth / 2), (Screen.height / 2) - (_modalWindowHeight / 2), _modalWindowWidth, _modalWindowHeight);
                GUIStyle windowGuiStyle = new GUIStyle(GUI.skin.box);
                windowGuiStyle.normal.background = Texture2D.whiteTexture;
                GUI.Window(0, rect, CreatePopupWindow, "", windowGuiStyle);
            }
        }
        return IsVisible;
    }
}