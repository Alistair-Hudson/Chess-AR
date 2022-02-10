using UnityEngine;
using UnityEngine.UI;

public class LightEstimationUI : MonoBehaviour {

    public Light MainLight = null;

    [Header("UI Text Fields")]
    public Text BrightnessText = null;
    public Text TemperatureText = null;
    public Text ColorText = null;
    public Text DirectionText = null;

    private void Update() {
        BrightnessText.text = $"B: {MainLight.intensity}";
        TemperatureText.text = $"T: {MainLight.colorTemperature}";
        ColorText.text = $"C: {MainLight.color}";
        DirectionText.text = $"D: {MainLight.transform.rotation}";
    }
}
