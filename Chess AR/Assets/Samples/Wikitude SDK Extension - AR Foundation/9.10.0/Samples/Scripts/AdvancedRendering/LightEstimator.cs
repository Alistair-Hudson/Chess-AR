using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]
public class LightEstimator : MonoBehaviour {

    public ARCameraManager CameraManager = null;

    private Light _light;

    private void Awake() {
        _light = GetComponent<Light>();
        _light.useColorTemperature = true;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    private void OnEnable() {
        if (CameraManager != null) {
            CameraManager.frameReceived += FrameChanged;
        }
    }

    private void OnDisable() {
        if (CameraManager != null) {
            CameraManager.frameReceived -= FrameChanged;
        }
    }

    private void FrameChanged(ARCameraFrameEventArgs args) {
        ARLightEstimationData lightData = args.lightEstimation;

        if (lightData.mainLightColor.HasValue) {
            _light.color = lightData.mainLightColor.Value;
        } else if (lightData.colorCorrection.HasValue) {
            _light.color = lightData.colorCorrection.Value;
        }

        if (lightData.averageMainLightBrightness.HasValue) {
            _light.intensity = lightData.averageMainLightBrightness.Value;
        } else if (lightData.averageBrightness.HasValue) {
            _light.intensity = lightData.averageBrightness.Value;
        }

        if (lightData.averageColorTemperature.HasValue) {
            _light.colorTemperature = lightData.averageColorTemperature.Value;
        }

        if (lightData.mainLightDirection.HasValue) {
            Quaternion rotation = Quaternion.LookRotation(
                lightData.mainLightDirection.Value
            );

            _light.transform.rotation = rotation;
        }

        if (lightData.ambientSphericalHarmonics.HasValue) {
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientProbe = lightData.ambientSphericalHarmonics.Value;
        }
    }
}
