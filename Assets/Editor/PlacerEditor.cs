using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathPlacer))]
public class PlacerEditor : Editor
{
    private PathPlacer placer;


    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Placer")) {
            placer.Placer();
        }
    }

    private void OnEnable() {
        placer = (PathPlacer)target;
    }
}
