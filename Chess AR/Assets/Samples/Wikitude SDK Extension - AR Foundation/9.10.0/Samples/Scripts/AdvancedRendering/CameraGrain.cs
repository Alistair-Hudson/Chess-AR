/*
 * AR Foundation Samples copyright © 2020 Unity Technologies ApS
 * Licensed under the Unity Companion License for Unity-dependent projects--see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
 * Unless expressly provided otherwise, the Software under this license is made available strictly on an "AS IS" BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.
 */

using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CameraGrain : MonoBehaviour {

    private static readonly int _noiseTexID = Shader.PropertyToID("_NoiseTex");
    private static readonly int _noiseIntensityID = Shader.PropertyToID("_NoiseIntensity");
    private static readonly int _estimatedLightColorID = Shader.PropertyToID("_EstimatedLightColor");

    private ARCameraManager _cameraManager;
    private Renderer _renderer;
    private Light _light;

    private void Awake() {
        _cameraManager = FindObjectOfType<ARCameraManager>();
        _renderer = GetComponent<Renderer>();
        _light = FindObjectOfType<LightEstimator>().GetComponent<Light>();
    }

    private void OnEnable() {
        _cameraManager.frameReceived += OnReceivedFrame;
    }

    private void OnDisable() {
        _cameraManager.frameReceived -= OnReceivedFrame;
    }

    private void OnReceivedFrame(ARCameraFrameEventArgs args) {
        if (_renderer && args.cameraGrainTexture) {
            _renderer.material.SetTexture(_noiseTexID, args.cameraGrainTexture);
            _renderer.material.SetFloat(_noiseIntensityID, args.noiseIntensity);
            _renderer.material.SetColor(_estimatedLightColorID, Mathf.CorrelatedColorTemperatureToRGB(_light.colorTemperature) * _light.intensity);
        }
    }
}
