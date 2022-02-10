using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Wikitude
{
    internal enum ArFoundationCameraErrorCodes
    {
        CameraPermissionsNotGranted = 12001,
    }

    public class ARFoundationPlugin : MonoBehaviour
    {
        internal const string ArFoundationCameraErrorDomain = "com.wikitude.unity.arfoundation.camera";

        [Serializable]
        public class OnArFoundationCameraPermissionGrantedEvent : UnityEvent
        { }

        [Serializable]
        public class OnArFoundationCameraErrorEvent : UnityEvent<Error>
        { }

        [Serializable]
        public class OnArFoundationPostUpdateEvent : UnityEvent
        { }

        /// <summary>
        /// Called whenever the camera permission is being granted.
        /// </summary>
        [SerializeField]
        public OnArFoundationCameraPermissionGrantedEvent OnArFoundationCameraPermissionGranted = new OnArFoundationCameraPermissionGrantedEvent();

        /// <summary>
        /// Called whenever the device camera encounters an error.
        /// </summary>
        [SerializeField]
        public OnArFoundationCameraErrorEvent OnArFoundationCameraError = new OnArFoundationCameraErrorEvent();

        /// <summary>
        /// Called whenever the camera frame was being send to the SDK.
        /// </summary>
        [SerializeField]
        public OnArFoundationPostUpdateEvent OnArFoundationPostUpdate = new OnArFoundationPostUpdateEvent();

        /// <summary>
        /// Defines the scale of the camera image inputed into the SDK via the input plugin in the Editor
        /// if the AR Foundation Editor Remote Plugin is installed.
        /// </summary>
        [SerializeField][Range(0.1f, 1)]
        private float _editorCameraFrameScale = 0.5f;

        private bool TargetIsAndroid {
            get {
#if UNITY_ANDROID
                // This will also work in the Editor if the Build Target is set to Android.
                return true;
#else
                return false;
#endif
            }
        }

        private WikitudeSDK _wikitudeSDK;
        private Plugin _arFoundationPlugin;
        private ARCameraManager _arCameraManager;
        private TrackedPoseDriver _trackedPoseDriver;
        private bool _onCameraPermissionInvoked = false;
        private bool _onCameraPermissionErrorInvoked = false;

        private Color32[] _colorData;
        private int _frameIndex;

        private ScreenOrientation _screenOrientation = ScreenOrientation.LandscapeLeft;

        private void Awake() {
            _arFoundationPlugin = gameObject.AddComponent<Plugin>();
            _arFoundationPlugin.Identifier = "AR Foundation Plugin";
            _arFoundationPlugin.HasInputModule = true;

            _arFoundationPlugin.OnPluginError.AddListener(error => {
                Debug.Log($"OnPluginError Code: {error.Code}, Domain: {error.Domain}, Message: {error.Message}");
            });
        }

        private void Start() {
            _arCameraManager = FindObjectOfType<ARCameraManager>();
            if (_arCameraManager == null) {
                Debug.LogError("No object of type ARCameraManager found in scene");
            } else {
                _arCameraManager.frameReceived += OnFrameReceived;
            }

            _wikitudeSDK = FindObjectOfType<WikitudeSDK>();
            if (_wikitudeSDK == null) {
                Debug.LogError("No object of type WikitudeSDK found in scene");
            } else {
                _wikitudeSDK.UpdateMode = UpdateMode.Explicit;
            }

            Application.onBeforeRender += BeforeRender;

            _trackedPoseDriver = FindObjectOfType<TrackedPoseDriver>();
        }

        private void OnFrameReceived(ARCameraFrameEventArgs args) {
            if (!args.displayMatrix.HasValue) {
                return;
            }

            /* Calculate the Screen.orientation based on the displayMatrix.
             * The main differences are in the m00 and m01 values of the matrix.
             */
            Matrix4x4 displayMatrix = args.displayMatrix.Value;
            int m00 = Mathf.RoundToInt(displayMatrix[0,0]);
            int m01 = Mathf.RoundToInt(displayMatrix[0,1]);

            _screenOrientation = ScreenOrientation.LandscapeLeft;

            if (m00 == 0) {
                if (m01 == 1) {
                    _screenOrientation = TargetIsAndroid ? ScreenOrientation.Portrait : ScreenOrientation.PortraitUpsideDown;
                }
                if (m01 == -1) {
                    _screenOrientation = TargetIsAndroid ? ScreenOrientation.PortraitUpsideDown : ScreenOrientation.Portrait;
                }
            }
            if (m00 == -1) {
                _screenOrientation = ScreenOrientation.LandscapeRight;
            }
        }

        unsafe void BeforeRender() {
            if (ARSession.state == ARSessionState.SessionInitializing) {
                if (!_arCameraManager.permissionGranted && !_onCameraPermissionErrorInvoked) {
                    _onCameraPermissionErrorInvoked = true;
                    OnArFoundationCameraError.Invoke(new Error((int)ArFoundationCameraErrorCodes.CameraPermissionsNotGranted,
                        ArFoundationCameraErrorDomain,
                        "Permission denied. Make sure to have camera permissions before trying to access the camera."));
                } else if (_arCameraManager.permissionGranted && !_onCameraPermissionInvoked) {
                    _onCameraPermissionInvoked = true;
                    OnArFoundationCameraPermissionGranted.Invoke();
                }
            }

            if (_arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage cameraImage)) {
                var metadata = new ColorCameraFrameMetadata {
                    Width = cameraImage.width,
                    Height = cameraImage.height,
                    CameraPosition = _arCameraManager.currentFacingDirection == CameraFacingDirection.World ? CaptureDevicePosition.Back : CaptureDevicePosition.Front,
                    ColorSpace = TargetIsAndroid ? FrameColorSpace.YUV_420_888 : FrameColorSpace.YUV_420_NV21,
                    TimestampScale = 1
                };

                IDisposable disposableData = null;
                var planes = new List<CameraFramePlane>();

                if (Application.isEditor && _editorCameraFrameScale != 1f) {
                    var conversionParams = new XRCpuImage.ConversionParams {
                        inputRect = new RectInt(0, 0, cameraImage.width, cameraImage.height),
                        outputDimensions = new Vector2Int(Mathf.RoundToInt(cameraImage.width * _editorCameraFrameScale), Mathf.RoundToInt(cameraImage.height * _editorCameraFrameScale)),
                        outputFormat = TextureFormat.ARGB32
                    };

                    var buffer = new NativeArray<byte>(cameraImage.GetConvertedDataSize(conversionParams), Allocator.Temp);
                    try {
                        cameraImage.Convert(conversionParams, buffer);
                    } catch (Exception e) {
                        Debug.LogError(e.ToString());
                        buffer.Dispose();
                        cameraImage.Dispose();
                        return;
                    }

                    var wikitudePlane = new CameraFramePlane {
                        Data = (IntPtr)buffer.GetUnsafePtr(),
                        DataSize = (uint)buffer.Length,
                        PixelStride = 1,
                        RowStride = conversionParams.outputDimensions.x
                    };
                    planes.Add(wikitudePlane);

                    disposableData = buffer;
                    metadata.ColorSpace = FrameColorSpace.RGBA;
                    metadata.Width = conversionParams.outputDimensions.x;
                    metadata.Height = conversionParams.outputDimensions.y;
                } else {
                    for (int planeIndex = 0; planeIndex < cameraImage.planeCount; planeIndex++) {
                        var unityPlane = cameraImage.GetPlane(planeIndex);
                        var wikitudePlane = new CameraFramePlane {
                            Data = (IntPtr)unityPlane.data.GetUnsafePtr(),
                            DataSize = (uint)unityPlane.data.Length,
                            PixelStride = unityPlane.pixelStride,
                            RowStride = unityPlane.rowStride
                        };

                        planes.Add(wikitudePlane);
                    }
                }

                if (_arCameraManager.TryGetIntrinsics(out var intrinsics)) {
                    float scale = Application.isEditor ? _editorCameraFrameScale : 1f;
                    metadata.HasIntrinsicsCalibration = true;
                    metadata.IntrinsicsCalibration = new IntrinsicsCalibration {
                        PrincipalPoint = new Point {
                            X = intrinsics.principalPoint.x * scale,
                            Y = intrinsics.principalPoint.y * scale
                        },
                        FocalLength = new Point {
                            X = intrinsics.focalLength.x * scale,
                            Y = intrinsics.focalLength.y * scale
                        },
                    };
                }

                var flipZAxis = Matrix4x4.Scale(new Vector3(1, 1, -1));
                var flipYAxis = Matrix4x4.Scale(new Vector3(1, -1, 1));
                var rotate90AroundX = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));

                float cameraToSurfaceAngle;
                switch (_screenOrientation) {
                    case ScreenOrientation.Portrait:
                        cameraToSurfaceAngle = 90.0f;
                        break;
                    case ScreenOrientation.PortraitUpsideDown:
                        cameraToSurfaceAngle = -90.0f;
                        break;
                    case ScreenOrientation.LandscapeLeft:
                        cameraToSurfaceAngle = 0.0f;
                        break;
                    case ScreenOrientation.LandscapeRight:
                        cameraToSurfaceAngle = 180.0f;
                        break;
                    default:
                        cameraToSurfaceAngle = 0.0f;
                        break;
                }

                if (SystemInfo.deviceName.Contains("Nexus 5X")) {
                    /* The Nexus 5X camera is flipped upside-down, so the cameraToSurfaceAngle also needs to be flipped.
                     * Since there is no other API to detect this, we have to rely on the device name.
                     */
                    cameraToSurfaceAngle += 180.0f;
                }

                var cameraToSurfaceRotation = Matrix4x4.Rotate(Quaternion.Euler(0.0f, 0.0f, cameraToSurfaceAngle));

                var arPose = Matrix4x4.TRS(_trackedPoseDriver.transform.localPosition, _trackedPoseDriver.transform.localRotation, Vector3.one);
                var pose = rotate90AroundX * flipZAxis * arPose * flipYAxis * cameraToSurfaceRotation;

                var cameraFrame = new CameraFrame(++_frameIndex, 0, metadata, planes, pose);
                _arFoundationPlugin.NotifyNewCameraFrame(cameraFrame);

                cameraImage.Dispose();
                if (disposableData != null) {
                    disposableData.Dispose();
                }

                _wikitudeSDK.ExplicitUpdate();
                OnArFoundationPostUpdate.Invoke();
            }
        }
    }
}