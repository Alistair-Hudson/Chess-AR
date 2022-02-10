using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Wikitude;

// This class is used to store where a certain entry was added in the error log, which is a StringBuilder object.
class StringBuilderPointer {
    public int StartIndex { get; set; }
    public int LengthOfMessage { get; set; }

    public StringBuilderPointer(int startIndex, int lengthOfMessage) {
        StartIndex = startIndex;
        LengthOfMessage = lengthOfMessage;
    }
}

public class ARFoundationController : SampleController
{
    public GameObject Instructions;
    public GameObject UnsupportedDeviceText;

    protected override bool ShouldRequestCameraPermission { get; } = false;

    private StringBuilderPointer _cameraPermissionDeniedError;

    private IEnumerator CheckARFoundationSupport() {
        void ShowUnsupportedDeviceMessage() {
            Instructions.SetActive(false);
            UnsupportedDeviceText.SetActive(true);
        }

        if (Application.platform == RuntimePlatform.Android) {
            /* On Android, first check if the Android version could support ARFoundation, because otherwise the API would not work */
            using (var version = new AndroidJavaClass("android.os.Build$VERSION")) {
                int versionNumber = version.GetStatic<int>("SDK_INT");
                if (versionNumber < 24) {
                    ShowUnsupportedDeviceMessage();
                }
            }
        }

        bool arFoundationStateDetermined = false;
        while (!arFoundationStateDetermined) {
            Debug.Log($"Checking ARFoundation support - {ARSession.state}.");
            switch (ARSession.state) {
                case ARSessionState.CheckingAvailability:
                case ARSessionState.Installing:
                case ARSessionState.SessionInitializing:
                    yield return new WaitForSeconds(0.1f);
                    break;
                case ARSessionState.None:
                case ARSessionState.Unsupported:
                    ShowUnsupportedDeviceMessage();
                    arFoundationStateDetermined = true;
                    break;
                default:
                    arFoundationStateDetermined = true;
                    break;
            }
        }
    }

    public void OnArFoundationCameraPermissionGranted() {
        // Remove the camera permissions denied error if the permissions are granted.
        if(_cameraPermissionDeniedError != null) {
            _errorLog.Remove(_cameraPermissionDeniedError.StartIndex, _cameraPermissionDeniedError.LengthOfMessage + 1);
            _showConsole = _errorLog.Length != 0;
        }
    }

    public void OnArFoundationCameraError(Error error) {
        // Print an error, except it is the camera permission denied error.
        // In that case, store a pointer to the error to later remove it if the permission is granted.
        if (error.Code == 12001 /* Camera permission denied error code */) {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("AR Foundation Camera Error!");
            stringBuilder.AppendLine($"        Error Code: {error.Code}");
            stringBuilder.AppendLine($"        Error Domain: {error.Domain}");
            stringBuilder.AppendLine($"        Error Message: {error.Message}");

            Debug.LogError(stringBuilder.ToString());

            /* Adds the error to the log and displays the Error Console */
            string completeErrorMessage = stringBuilder.ToString();
            _cameraPermissionDeniedError = new StringBuilderPointer(_errorLog.Length, completeErrorMessage.Length); 
            _errorLog.AppendLine(completeErrorMessage);
            _showConsole = true;
        } else {
            PrintError("AR Foundation Camera Error!", error, true);
        }
    }

    protected override void Awake() {
        base.Awake();
        StartCoroutine(CheckARFoundationSupport());
    }

    protected override void OnGUI() {
        if (_showConsole && ARSession.state == ARSessionState.Unsupported) {
            _showConsole = false;
        } 

        base.OnGUI();
    }
}