using System;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using Wikitude;
using UnityEngine.UI;
using UnityEngine;

public class ArBridgeController : SampleController
{
    public WikitudeSDK WikitudeSDK;
    public Text ArBridgeAvailabilityText;
    public bool DestroyIfUnsupported;
    private bool _arFoundationWarningShown = false;
    private Popup arFoundationSupportWarning;
    private bool _showARFoundationSupportWarning = false;

    private string _arFoundationUnsupportedWarningText = "This sample requires ARCore or ARKit to work, which your device does not seem to support.";

#if UNITY_ANDROID
    private bool _cameraPermissionPopupShown = false;
#endif

    protected override void Awake() {
        base.Awake();
        _showCameraPermissionPopup = false;
    }

    protected override void Update() {
        base.Update();
        if (WikitudeSDK == null) {
            return;
        }

        switch (WikitudeSDK.ArBridgeAvailability) {
            case ArBridgeAvailability.IndeterminateQueryFailed:
                ArBridgeAvailabilityText.text = "AR Bridge support couldn't be determined.";
                break;
            case ArBridgeAvailability.CheckingQueryOngoing:
                ArBridgeAvailabilityText.text = "AR Bridge support check ongoing.";
                break;
            case ArBridgeAvailability.Unsupported:
                ShowCameraPermissionPopup();
                ArBridgeAvailabilityText.text = "AR Bridge is not supported.";
                break;
            case ArBridgeAvailability.SupportedUpdateRequired:
                ShowCameraPermissionPopup();
                ArBridgeAvailabilityText.text = "AR Bridge is supported, but an update is available.";
                break;
            case ArBridgeAvailability.Supported:
                ShowCameraPermissionPopup();
                ArBridgeAvailabilityText.text = "AR Bridge is supported.";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (DestroyIfUnsupported) {
            OnArBridgeEnabledCheckFinished();
        }
    }

    public void OnArBridgeEnabledCheckFinished() {
        /* Outside of the editor environment, this sample should not run at all if AR Bridge is unsupported,
        since AR Bridge support is crucial to it functioning. This is why we destroy the SDK object in that case. */
        if ((WikitudeSDK.ArBridgeAvailability == ArBridgeAvailability.Unsupported ||
            WikitudeSDK.ArBridgeAvailability == ArBridgeAvailability.IndeterminateQueryFailed) &&
            !_arFoundationWarningShown) {
            if (!UnityEngine.Application.isEditor) {
                Destroy(WikitudeSDK.gameObject);
                Debug.Log("Destroyed SDK because AR Bridge is not supported.");
                Camera.main.backgroundColor = Color.black;
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
#if UNITY_ANDROID
                _arFoundationUnsupportedWarningText = "This sample requires ARCore to work, which your device does not seem to support.";
#elif UNITY_IOS
                _arFoundationUnsupportedWarningText = "This sample requires ARKit to work, which your device does not seem to support.";
#endif
                arFoundationSupportWarning = new Popup(_arFoundationUnsupportedWarningText, Popup.PopupType.INFO);
                _showARFoundationSupportWarning = true;
                _arFoundationWarningShown = true;
                _showCameraPermissionPopup = false;
            }
        }
    }

    private void ShowCameraPermissionPopup() {
#if UNITY_ANDROID
        if (!_cameraPermissionPopupShown && !Permission.HasUserAuthorizedPermission(Permission.Camera)) {
            _showCameraPermissionPopup = true;
            _cameraPermissionPopupShown = true;
        }
#endif
    }

    protected override void OnGUI() {
        base.OnGUI();
        if (_showARFoundationSupportWarning) {
            arFoundationSupportWarning.ShowPopup();
        }
    }
}
