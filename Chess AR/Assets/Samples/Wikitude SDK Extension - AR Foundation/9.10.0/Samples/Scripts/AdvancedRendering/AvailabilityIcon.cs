using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AvailabilityIcon : MonoBehaviour {

    public ToastStack ToastStack;
    public Image Icon;

    private static Color _enabledColor = Color.green;
    private static Color _disabledColor = Color.red;

    private Button _button;

    private string _prefix;

    private int _toastID = -1;

    public static void SetColors(Color enabledColor, Color disabledColor) {
        _enabledColor = enabledColor;
        _disabledColor = disabledColor;
    }

    public void SetAvailable() {
        ToastStack.SetToastText(_toastID, $"{_prefix} is enabled and active");

        Icon.color = _enabledColor;
    }

    public void SetDisabled() {
        ToastStack.SetToastText(_toastID, $"{_prefix} is disabled");

        Icon.color = _disabledColor;
    }

    public void SetUnavailable() {
        ToastStack.SetToastText(_toastID, $"{_prefix} is not supported on this device");

        Icon.color = _disabledColor;
    }

    private void Awake() {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnIconClicked);

        _prefix = $"\"{_button.name}\"";

        _toastID = ToastStack.CreateToast($"{_prefix} is enabled and active");
    }

    private void OnIconClicked() {
        ToastStack.ShowToast(_toastID);
    }
}
