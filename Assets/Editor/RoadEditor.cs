using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadCreator))]
public class RoadEditor : Editor
{
    private RoadCreator creator;
    private bool autoUpdate = true;


    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        autoUpdate = GUILayout.Toggle(autoUpdate, "Auto Update");
        if (Event.current.type == EventType.Repaint && autoUpdate) {
            creator.UpdateRoad();
        }
    }
    
    private void OnEnable() {
        creator = (RoadCreator)target;
    }

}
