using System.Collections.Generic;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private const int RunningAverageCount = 20;
    private const float Height = 40.0f;
    private const float Width = 220.0f;
    private static readonly Color _backgroundColor = new Color(0.0f, 0.0f, 0.0f,  0.5f);

    private float _currentFPS;
    private float _currentMS;

    private readonly Queue<float> _pastMS = new Queue<float>();

    private static Texture2D _backgroundTexture;
    private static GUIStyle _textureStyle;

    private void Start() {
        _backgroundTexture = Texture2D.whiteTexture;
        _textureStyle = new GUIStyle {
            normal = new GUIStyleState {
                background = _backgroundTexture
            }
        };
    }

    private static void DrawRect(Rect position, Color color) {
        var backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUI.Box(position, GUIContent.none, _textureStyle);
        GUI.backgroundColor = backgroundColor;
    }

    private void OnGUI() {
        float averageMS = 0.0f;
        foreach (float pastMS in _pastMS) {
            averageMS += pastMS;
        }

        averageMS /= _pastMS.Count;
        float averageFPS = 1.0f / (averageMS / 1000.0f);

        string text = $"  Current  FPS: {_currentFPS:F2} - MS: {_currentMS:F2} \n  Average FPS: {averageFPS:F2} - MS: {averageMS:F2}";

        float resX = (float)Screen.width / 800;
        float resY = (float)Screen.height / 600;
        float scaleFactor = Mathf.Max(resX, resY);

        float width = Width * scaleFactor;
        float height = Height * scaleFactor;

        var rect = new Rect(Screen.width - width, Screen.height - height, width, height);

        DrawRect(rect, _backgroundColor);
        var guiStyle = new GUIStyle {
            fontSize = (int) (14 * scaleFactor),
            normal = new GUIStyleState {
                textColor = Color.white
            }
        };
        GUI.Label(rect, text, guiStyle);
    }

    private void Update() {
        _currentFPS = 1.0f / Time.deltaTime;
        _currentMS = Time.deltaTime * 1000.0f;

        _pastMS.Enqueue(_currentMS);
        while (_pastMS.Count > RunningAverageCount) {
            _pastMS.Dequeue();
        }
    }
}
