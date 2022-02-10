using UnityEngine;
using UnityEditor;
using Wikitude;

namespace WikitudeEditor
{
    [CustomEditor(typeof(ARFoundationPlugin), true)]
    public class ARFoundationPluginInspector : Editor
    {
        // Events
        private SerializedProperty _onArFoundationCameraPermissionGranted;
        private SerializedProperty _onArFoundationCameraError;
        private SerializedProperty _onArFoundationPostUpdate;

        // Properties
        private SerializedProperty _editorCameraFrameScale;

        private bool _eventsFoldout = true;
        private bool _editorOnlySettingsFoldout = true;

        private void OnEnable() {
            _onArFoundationCameraPermissionGranted = serializedObject.FindProperty("OnArFoundationCameraPermissionGranted");
            _onArFoundationCameraError = serializedObject.FindProperty("OnArFoundationCameraError");
            _onArFoundationPostUpdate = serializedObject.FindProperty("OnArFoundationPostUpdate");
            _editorCameraFrameScale = serializedObject.FindProperty("_editorCameraFrameScale");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.Space();
            _eventsFoldout = EditorGUILayout.Foldout(_eventsFoldout, "Events", true);
            if (_eventsFoldout) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_onArFoundationCameraPermissionGranted);
                EditorGUILayout.PropertyField(_onArFoundationCameraError);
                EditorGUILayout.PropertyField(_onArFoundationPostUpdate);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            _editorOnlySettingsFoldout = EditorGUILayout.Foldout(_editorOnlySettingsFoldout, "Editor Only Settings", true);
            if (_editorOnlySettingsFoldout) {
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                EditorGUILayout.PropertyField(_editorCameraFrameScale, new GUIContent("Camera Frame Scale"));
                EditorGUILayout.HelpBox("AR Foundation scenes can also be tested in the Editor via the AR Foundation Editor Remote from the Asset Store! Downscaling the remote application's camera frame can improve framerates in the Editor.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawLinkButton("Open Documentation", "https://www.wikitude.com/external/doc/expertedition/UnitySupportedPackages.html#ar-foundation-editor-remote");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLinkButton(string title, string url) {
            GUIStyle linkButtonStyle = new GUIStyle(GUI.skin.label);
            linkButtonStyle.normal.textColor = new Color(0f, 0.5f, 0.95f, 1f);
            linkButtonStyle.hover.textColor = linkButtonStyle.normal.textColor;
            linkButtonStyle.fixedWidth = EditorStyles.label.CalcSize(new GUIContent(title + " ")).x;
            linkButtonStyle.margin = new RectOffset(33, 0, 0, 0);

            if (GUILayout.Button(title, linkButtonStyle)) {
                Application.OpenURL(url);
            }

            Rect buttonRect = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(buttonRect.x, buttonRect.y + buttonRect.height, buttonRect.width, 1), GUIContent.none);
        }
    }
}
