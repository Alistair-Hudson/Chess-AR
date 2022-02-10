using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Toast : MonoBehaviour {

    [Serializable]
    public class ToastDismissedEvent : UnityEvent<int> {

    }

    public Text Text { get; private set; }
    public int ActiveToastID { get; set; }
    public RectTransform RectTransform { get; private set; }
    public Coroutine ToastCoroutine { get; set; }
    
    public float ActiveDuration = 1.5f;
    public float FadeDuration = 0.5f;

    public ToastDismissedEvent ToastDismissed;

    private Image _background;

    private Color _textColor;
    private Color _backgroundColor;

    public IEnumerator ShowToast() {
        yield return Fade(true);

        float elapsedTime = 0.0f;

        while (elapsedTime < ActiveDuration) {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return Fade(false);

        Text.color = _textColor;
        _background.color = _backgroundColor;

        ToastDismissed.Invoke(ActiveToastID);

        gameObject.SetActive(false);
    }

    private void Awake() {
        Text = GetComponentInChildren<Text>();
        RectTransform = GetComponent<RectTransform>();
        _background = GetComponent<Image>();

        _textColor = Text.color;
        _backgroundColor = _background.color;
    }

    private IEnumerator Fade(bool isFadingIn) {
        float elapsedTime = 0.0f;

        while (elapsedTime < FadeDuration) {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / FadeDuration;

            Text.color = LerpColor(_textColor, Color.clear, t, isFadingIn);
            _background.color = LerpColor(_backgroundColor, Color.clear, t, isFadingIn);
        
            yield return null;
        }

    }

    private Color LerpColor(Color from, Color to, float t, bool isFadingIn) {
        Color a = isFadingIn ? to : from;
        Color b = isFadingIn ? from : to;

        return Color.Lerp(a, b, t);
    }
}
