using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class AdvancedRenderingController : ARFoundationController {

    [Header("AR Managers")]
    public ARCameraManager CamManager;
    public AREnvironmentProbeManager ProbeManager;

    [Header("Light Estimation")]
    public Light MainLight;
    public GameObject EstimationToggleButtons;

    [Header("Availability Icons")]
    public ToastStack IconsToastStack;

    public AvailabilityIcon EstimationIcon;
    public AvailabilityIcon ProbesIcon;
    public AvailabilityIcon GrainIcon;
    public AvailabilityIcon OcclusionIcon;

    [Space]
    public Color EnabledColor = Color.green;
    public Color DisabledColor = Color.red;

    private LightEstimator _lightEstimator;

    private float _initialBrightness;
    private float _initialColorTemp;
    private Color _initialLightColor;
    private Quaternion _initialLightDirection;

    private bool _checkedSupport = false;

    public void OnCameraPermissionGranted() {
        OnArFoundationCameraPermissionGranted();

        if (!_checkedSupport) {
            CheckSupportedFeatures();
            _checkedSupport = true;
        }
    }

    protected override void Awake() {
        base.Awake();

        _lightEstimator = MainLight.GetComponent<LightEstimator>();

        _initialBrightness = MainLight.intensity;
        _initialColorTemp = MainLight.colorTemperature;
        _initialLightColor = MainLight.color;
        _initialLightDirection = MainLight.transform.rotation;

        AvailabilityIcon.SetColors(EnabledColor, DisabledColor);
    }

    protected override void Start() {
        base.Start();

        if (UnsupportedDeviceText.activeSelf) {
            EstimationNotSupported();

            EstimationIcon.SetUnavailable();
            ProbesIcon.SetUnavailable();
            GrainIcon.SetUnavailable();
            OcclusionIcon.SetUnavailable();

            enabled = false;
            return;
        }

        if (CamManager.permissionGranted && !_checkedSupport) {
            CheckSupportedFeatures();
            _checkedSupport = true;
        }

        CamManager.frameReceived += OnFrameReceived;
    }

    private void CheckSupportedFeatures() {
        if (ProbeManager.subsystem is null) {
            Destroy(ProbeManager);
            ProbesIcon.SetUnavailable();
        } else if (Application.platform == RuntimePlatform.Android) {
            // Android's environment probes do the light estimation for us by
            // setting the main light direction, main light color and placing
            // ambient/reflection probes

            // Since the light estimation is not directly available anymore,
            // we disable and destroy the light estimation button
            EstimationNotSupported();
            EstimationIcon.SetAvailable();
        }

        var occlusionManager = CamManager.GetComponent<AROcclusionManager>();

        if (occlusionManager.subsystem is null) {
            OcclusionIcon.SetUnavailable();
        } else {
            var occDescriptor = occlusionManager.descriptor;

            if (
                !occDescriptor.supportsHumanSegmentationDepthImage
                || !occDescriptor.supportsHumanSegmentationStencilImage
            ) {
                OcclusionIcon.SetUnavailable();
            }
        }

    }

    private void OnFrameReceived(ARCameraFrameEventArgs args) {
        if (args.cameraGrainTexture != null) {
            GrainIcon.SetAvailable();
        } else {
            GrainIcon.SetUnavailable();
        }
    }

    private void EstimationNotSupported() {
        RectTransform buttonTransform = EstimationToggleButtons.GetComponent<RectTransform>();
        RectTransform stackTransform = IconsToastStack.GetComponent<RectTransform>();
        stackTransform.localPosition += new Vector3(0, -90, 0);

        Destroy(EstimationToggleButtons);
    }

    public void OnEnableLightEstimation() {
        EstimationIcon.SetAvailable();

        _lightEstimator.enabled = true;
    }

    public void OnDisableLightEstimation() {
        _lightEstimator.enabled = false;
        EstimationIcon.SetDisabled();

        MainLight.intensity = _initialBrightness;
        MainLight.colorTemperature = _initialColorTemp;
        MainLight.color = _initialLightColor;
        MainLight.transform.rotation = _initialLightDirection;
    }
}
