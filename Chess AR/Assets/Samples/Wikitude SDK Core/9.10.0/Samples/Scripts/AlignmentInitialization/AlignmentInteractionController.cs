using UnityEngine;
using UnityEngine.UI;
using Wikitude;

public class AlignmentInteractionController : MonoBehaviour
{
    public AlignmentDrawable StopSignDrawable;
    public AlignmentDrawable FiretruckDrawable;

    public GameObject UIInteractionHint;
    public Slider ZoomSlider;
    public bool ZoomSliderIsDragged { get; set; }

    public GameObject LoadingIndicator;
    public Button StopSignButton;
    public Button FiretruckButton;

    private AlignmentDrawable _activeDrawable;
    private LivePreview _livePreview;

    /* The last mouse position has to be stored to calculate drag gestures. */
    private Vector2 _lastMousePosition;

    private void Start() {
        if (transform.parent != FindObjectOfType<WikitudeSDK>().ARCamera.transform) {
            Debug.LogWarning("Please make sure that the Alignment Initialization GameObject is parented to the main AR Camera.");
        } 

        /* Ensure that the drawables are properly initialized */
        FiretruckDrawable.Initialize();
        StopSignDrawable.Initialize();

        /* Set the stop sign as the alignment initialization object. */
        SetDrawable(StopSignDrawable);

        /* Sets the zoom range. */
        FiretruckDrawable.SetZoomRange(ZoomSlider.minValue, ZoomSlider.maxValue);
        StopSignDrawable.SetZoomRange(ZoomSlider.minValue, ZoomSlider.maxValue);

#if !UNITY_EDITOR
        /* Hide zoom slider if outside of the Unity Editor. */
        ZoomSlider.transform.parent.gameObject.SetActive(false);
#endif
    }

    private void Update() {
        if (_activeDrawable == null) {
            /* The tracker is currently loading the target. */
            return;
        }

        if (_activeDrawable.AlignedWithTarget || !_activeDrawable.TakesGestureControls) {
            UIInteractionHint.SetActive(false);
            ZoomSlider.gameObject.SetActive(false);
            return;
        } else {
            UIInteractionHint.SetActive(true);
            ZoomSlider.gameObject.SetActive(true);
        }

        /* Skip gestures if the zoom slider is interacted with. */
        if (ZoomSliderIsDragged) {
            return;
        }

        /* If the view is mirrored in case of using a mirrored webcam or the remote front camera
           for live preview, the rotation gestures also have to be mirrored correctly. */
        float flipHorizontalValue = 1f;
#if UNITY_EDITOR
        if (_livePreview == null) {
            _livePreview = FindObjectOfType<LivePreview>();
        } else {
            if ((_livePreview.LivePreviewMode == LivePreviewMode.WebCam && _livePreview.WebCamIsMirrored ) ||
                (_livePreview.LivePreviewMode == LivePreviewMode.RemoteCamera && _livePreview.RemoteCameraPosition == CaptureDevicePosition.Front)) {
                flipHorizontalValue = -1f;
            }
        }
#endif

        /* Interaction logic for handling two-finger scale and rotation gestures. */
        if (Input.touchCount >= 2 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)) {
            Touch touchIdZero = Input.GetTouch(0);
            Touch touchIdOne = Input.GetTouch(1);

            Vector2 prevTouchIdZero = touchIdZero.position - touchIdZero.deltaPosition;
            Vector2 prevTouchIdOne = touchIdOne.position - touchIdOne.deltaPosition;

            float prevTouchDistance = (prevTouchIdZero - prevTouchIdOne).magnitude;
            float touchDistance = (touchIdZero.position - touchIdOne.position).magnitude;
            float touchDistancesDelta = touchDistance - prevTouchDistance;

            _activeDrawable.AddZoom(touchDistancesDelta / Mathf.Min(Screen.width, Screen.height));

            if (ZoomSlider != null) {
                ZoomSlider.value = _activeDrawable.GetZoom();
            }

            float rotation = Vector2.SignedAngle(prevTouchIdZero - prevTouchIdOne, touchIdZero.position - touchIdOne.position);
            float rotationMultiplier =  180f / Mathf.Min(Screen.width, Screen.height);
            _activeDrawable.AddRotation(new Vector3(0f, 0f, flipHorizontalValue * rotation * rotationMultiplier));

            /* In case one finger gets lifted, the last mouse position has to be invalidated. */
            _lastMousePosition = Vector2.zero;
        } else if (Input.touchCount < 2 ) {
            /* The mouse input works for both, the mouse input and single finger input. */
            if (Input.GetMouseButtonDown(0)){
                _lastMousePosition = Input.mousePosition;
            } else if (Input.GetMouseButton(0)) {
                /* This condition is met if a finger during two-finger gestures is lifted. */
                if (_lastMousePosition.Equals(Vector2.zero)) {
                    _lastMousePosition = Input.mousePosition;
                } else {
                    Vector2 mousePosition = Input.mousePosition;
                    Vector2 deltaMousePosition = mousePosition - _lastMousePosition;
                    _lastMousePosition = mousePosition;

                    float rotationMultiplier =  180f / Mathf.Min(Screen.width, Screen.height);

                    _activeDrawable.AddRotation(Quaternion.AngleAxis(deltaMousePosition.y  * rotationMultiplier, transform.right).eulerAngles);
                    _activeDrawable.AddRotation(Quaternion.AngleAxis(-flipHorizontalValue * deltaMousePosition.x  * rotationMultiplier, transform.up).eulerAngles);
                }
            }
        }
    }

    public void SetDrawable(AlignmentDrawable drawable) {
        FiretruckDrawable.gameObject.SetActive(false);
        StopSignDrawable.gameObject.SetActive(false);

        /* Disable the buttons, so that the user cannot switch until loading has finished. */
        StopSignButton.enabled = false;
        FiretruckButton.enabled = false;

        StopSignDrawable.TargetObjectTracker.enabled = false;
        FiretruckDrawable.TargetObjectTracker.enabled = false;

        /* By enabling the tracker, the target collection will be loaded and the targets will extracted. Depending on the target collection, this can take a while. */
        drawable.TargetObjectTracker.enabled = true;

        LoadingIndicator.SetActive(true);
        /* The AlignmentDrawable shall only be displayed, once its Target is loaded. */
        if (drawable.TargetLoaded) {
            EnableDrawable(drawable);
        } else {
            drawable.TargetObjectTracker.OnTargetsLoaded.AddListener(() => {
                drawable.TargetLoaded = true;
                EnableDrawable(drawable);
            });
        }

    }

    private void EnableDrawable(AlignmentDrawable drawable) {
        LoadingIndicator.SetActive(false);

        StopSignButton.enabled = true;
        FiretruckButton.enabled = true;

        _activeDrawable = drawable;
        _activeDrawable.gameObject.SetActive(true);

        ZoomSlider.value = 1;

        _activeDrawable.ResetPose();
    }

    public void OnErrorLoadingTargets(Error error) {
        /* The separate error callback will display the error, so we just hide the LoadingIndicator here. */
        LoadingIndicator.SetActive(false);
    }

    public void OnZoomSliderValueChanged(float value) {
        _activeDrawable.SetZoom(value);
    }
}
