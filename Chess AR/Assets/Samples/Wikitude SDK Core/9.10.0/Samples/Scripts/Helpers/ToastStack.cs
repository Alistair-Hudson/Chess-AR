using System.Collections.Generic;

using UnityEngine;

public class ToastStack : MonoBehaviour {
    
    public Toast ToastPrefab;
    public RectTransform TargetPosition;

    [Range(0.0f, 500.0f)]
    public float ToastOffset = 90.0f;

    private List<Toast> _toasts = new List<Toast>();
    private List<Toast> _activeToasts = new List<Toast>();
    private int _activeToastCount = 0;

    public int CreateToast(string text) {
        Toast toast = Instantiate<Toast>(
            ToastPrefab,
            TargetPosition.position,
            Quaternion.identity,
            transform
        );

        toast.Text.text = text;
        toast.gameObject.SetActive(false);

        toast.ToastDismissed.AddListener(OnToastDismissed);

        _toasts.Add(toast);

        return _toasts.Count - 1;
    }

    public void ShowToast(int toastID) {
        if (toastID >= _toasts.Count) {
            return;
        }

        Toast toast = _toasts[toastID];

        if (toast.ToastCoroutine is null) {
            toast.gameObject.SetActive(true);
            toast.ActiveToastID = _activeToastCount;

            AdjustToastPosition(toast, 0, ToastOffset * _activeToastCount);

            _activeToasts.Add(toast);
            _activeToastCount++;
        } else {
            StopCoroutine(toast.ToastCoroutine);
        }

        toast.ToastCoroutine = StartCoroutine(toast.ShowToast());
    }

    public void SetToastText(int toastID, string text) {
        if (toastID < _toasts.Count) {
            _toasts[toastID].Text.text = text;
        }
    }

    private void OnToastDismissed(int oldToastID) {
        Toast oldToast = _activeToasts[oldToastID];
        oldToast.ToastCoroutine = null;

        AdjustToastPosition(oldToast, 0, -ToastOffset * oldToastID);

        for (int i = oldToastID + 1; i < _activeToasts.Count; i++) {
            Toast toast = _activeToasts[i];

            toast.ActiveToastID--;
            AdjustToastPosition(toast, 0, -ToastOffset);
        }

        _activeToasts.RemoveAt(oldToastID);
        _activeToastCount--;
    }

    private void AdjustToastPosition(Toast toast, float x, float y) {
        toast.RectTransform.localPosition += new Vector3(x, y);
    }
}
