using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Cube))]
public class CubeEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("COLORIT")) {
            ((Cube)target).RandomColor();
        }
        if (GUILayout.Button("Reset")) {
            ((Cube)target).ResetColor();
        }
        EditorGUILayout.EndHorizontal();
    }
}
