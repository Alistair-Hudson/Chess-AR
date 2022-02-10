using UnityEngine;
using Wikitude;

public class AlignmentDrawable : MonoBehaviour
{
    public ObjectTracker TargetObjectTracker;
    [Tooltip("Centers the drawable around its volume center and automatically sets the distance to the camera.")]
    public bool AutoFocusPosition;

    [Tooltip("Sets if the drawable should be affected by gestures.")]
    public bool TakesGestureControls = true;

    [Tooltip("Sets if the drawable should be rotated around its original center or the center of its volume.")]
    public bool RotateAroundVolumeCenter = true;

    [Tooltip("Sets if the drawable's children should be hidden upon tracking.")]
    public bool HideWhileTracking;

    [HideInInspector]
    public bool TargetLoaded;

    [HideInInspector]
    /* If the smooth transition from the drawable's position to the target's is done, this flag is set to true. */
    public bool AlignedWithTarget;

    private Vector3 _positionInitial;
    private Vector3 _positionLast;
    private Quaternion _rotationInitial;
    private Quaternion _rotationLast;
    private GameObject _boundingBox;

    private Camera _arCamera;
    private float _currentFieldOfView;

    private float _zoomMin = 0.5f;
    private float _zoomMax = 1.5f;
    private float _zoomAdjustment = 1f;

    /* The recognized target is set to visualize a smooth transition from the drawable to the target. */
    private GameObject _recognizedTargetObject = null;

    public void Initialize() {
        _arCamera = FindObjectOfType<WikitudeSDK>().ARCamera;
        _currentFieldOfView = _arCamera.fieldOfView;
        CalculateBoundingBox();
        if (AutoFocusPosition) {
            /* Fitting distance calculation of the bounding box found in the Unity forums: https://forum.unity.com/threads/fit-object-exactly-into-perspective-cameras-field-of-view-focus-the-object.496472/ */
            float minDistance = (_boundingBox.transform.localScale.magnitude) / Mathf.Sin(Mathf.Deg2Rad * _arCamera.fieldOfView / 2f);
            transform.localPosition = new Vector3(0, 0, minDistance) - _boundingBox.transform.localPosition;
        }
        _positionInitial = transform.localPosition;
        _positionLast = _positionInitial;
        _rotationInitial = transform.localRotation;
        _rotationLast = _rotationInitial;

        /* Callbacks are set up to disable or re-enable the alignment initializer. */
        TargetObjectTracker.GetComponentInChildren<ObjectTrackable>(true).OnObjectRecognized.AddListener(OnObjectRecognized);
        TargetObjectTracker.GetComponentInChildren<ObjectTrackable>(true).OnObjectLost.AddListener(OnObjectLost);
    }

    public void ResetPose() {
        transform.localPosition = GetAdjustedPosition();
        transform.localRotation = _rotationInitial;
        transform.localScale = Vector3.one;
    }

    public void AddRotation(Vector3 value) {
        if (TakesGestureControls) {
            Vector3 target = RotateAroundVolumeCenter ? _boundingBox.transform.position : transform.position;
            transform.RotateAround(target, Vector3.right, value.x);
            transform.RotateAround(target, Vector3.up, value.y);
            transform.RotateAround(target, Vector3.forward, value.z);
            _rotationLast = transform.localRotation;
        }
    }

    public void AddZoom(float value) {
        float zoom = GetZoom();
        SetZoom(zoom - value);
    }

    public void SetZoom(float value) {
        if (TakesGestureControls) {
            SetZoomInternal(value);
        }
    }

    private void SetZoomInternal(float value) {
        Vector3 position = transform.localPosition;
        Vector3 adjustedPosition = GetAdjustedPosition();
        position.z = Mathf.Clamp(adjustedPosition.z * value, _zoomMin * adjustedPosition.z, _zoomMax * adjustedPosition.z);
        transform.localPosition = position;
        _positionLast = position;
    }

    public float GetZoom() {
        return transform.localPosition.z / (_positionInitial.z * _zoomAdjustment);
    }

    public void SetZoomRange(float min, float max) {
        _zoomMin = min;
        _zoomMax = max;
    }

    private Vector3 GetAdjustedPosition() {
        return new Vector3(_positionInitial.x, _positionInitial.y, _positionInitial.z * _zoomAdjustment);
    }

    private void Update() {
        /* The zoom value has to be adjusted, if the scene camera FOV changes. */
        if (!Mathf.Approximately(_arCamera.fieldOfView, _currentFieldOfView)) {
            float zoom = GetZoom();
            _zoomAdjustment = _currentFieldOfView / _arCamera.fieldOfView;
            SetZoomInternal(zoom);
            _currentFieldOfView = _arCamera.fieldOfView;
        }

        if (_recognizedTargetObject != null) {
            if (AlignedWithTarget == false) {
                /* The drawable is smoothly moved, rotated and scaled to the pose of a recognized target. */
                transform.position = Vector3.Lerp(transform.position , _recognizedTargetObject.transform.position, 10f * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation , _recognizedTargetObject.transform.rotation, 10f * Time.deltaTime);
                transform.localScale = Vector3.Lerp(transform.localScale , _recognizedTargetObject.transform.localScale, 10f * Time.deltaTime);

                /* If the transition of the drawable is close to the recognized target's position, the drawable will be considered aligned with the target. */
                if ((transform.position - _recognizedTargetObject.transform.position).magnitude < 0.01f) {
                    AlignedWithTarget = true;
                }
            } else {
                /* The alignment drawable is fully aligned here. */
                if (HideWhileTracking) {
                    this.enabled = false;
                    foreach(Transform child in transform) {
                        child.gameObject.SetActive(false);
                    }
                }
                transform.position = _recognizedTargetObject.transform.position;
                transform.rotation = _recognizedTargetObject.transform.rotation;
                transform.localScale = _recognizedTargetObject.transform.localScale;
            }
        } else {
            /* The pose of the alignment drawable is sent to the object tracker, to help it find the object in the desired pose. */
            TargetObjectTracker.UpdateAlignmentPose(Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale));
        }
    }

    private void OnObjectRecognized(ObjectTarget target) {
        _recognizedTargetObject = target.Drawable;
    }

    private void OnObjectLost(ObjectTarget target) {
        this.enabled = true;
        _recognizedTargetObject = null;
        AlignedWithTarget = false;
        transform.localPosition = _positionLast;
        transform.localRotation = _rotationLast;
        transform.localScale = Vector3.one;
        foreach(Transform child in transform) {
            child.gameObject.SetActive(true);
        }
    }

    private void CalculateBoundingBox() {
        Quaternion currentRotation = transform.rotation;
        transform.rotation = Quaternion.identity;
        Bounds boundingBox = new Bounds(this.transform.position, Vector3.zero);
        foreach(Renderer renderer in GetComponentsInChildren<Renderer>(true)) {
            boundingBox.Encapsulate(renderer.bounds);
        }
        _boundingBox = new GameObject();
        _boundingBox.hideFlags = HideFlags.HideAndDontSave;
        _boundingBox.transform.parent = transform;
        _boundingBox.transform.localScale = boundingBox.size;
        _boundingBox.transform.position = boundingBox.center;
        transform.rotation = currentRotation;
    }
}
