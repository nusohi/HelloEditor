using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path path;

    private void OnSceneGUI() {
        Input();
        Draw();
    }

    private void Input() {
        Event guiEvent = Event.current;
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift) {
            Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
            Undo.RecordObject(creator, "添加锚点");
            path.AddSegment(mousePos);
        }
    }

    private void Draw() {
        // 绘制锚点
        Handles.color = Color.red;
        for(int i = 0; i < path.NumPoints; i++) {
            Vector2 pos = Handles.FreeMoveHandle(path[i], Quaternion.identity, 0.1f, Vector3.zero, Handles.CylinderHandleCap);
            if (pos != path[i]) {
                Undo.RecordObject(creator, "移动锚点");
                path.MovePoint(i, pos);
            }
        }

        // 绘制曲线
        for(int i = 0; i < path.NumSegments; i++) {
            Vector2[] points = path.GetPointsInSegment(i);
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 4f);
        }
    }

    private void OnEnable() {
        creator = (PathCreator)target;
        if (creator.path == null) {
            creator.CreatePath();
        }
        path = creator.path;
    }

}
