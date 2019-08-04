using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator creator;
    private Path path;

    private float minDistSegment = 0.06f;    // 曲线段的最短选择距离

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("创建新曲线")) {
            creator.CreatePath();
            path = creator.path;
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("闭合/打开曲线")) {
            path.ToggleClose();
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI() {
        Input();
        Draw();
    }

    private void Input() {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        // Shift + 左键 = 添加节点
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift) {
            // 计算是否点击线段（插入节点）
            float minDist = minDistSegment;
            int segIndex = -1;
            for (int i = 0; i < path.NumSegments; i++) {
                Vector2[] p = path.GetPointsInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousePos, p[0], p[3], p[1], p[2]);
                if (dst < minDist) {
                    minDist = dst;
                    segIndex = i;
                }
            }
            // 插入或者添加节点
            if (segIndex != -1) {
                Undo.RecordObject(creator, "插入节点");
                path.SplitSegment(mousePos, segIndex);
            }
            else if (!path.IsClosed) {
                Undo.RecordObject(creator, "添加节点");
                path.AddSegment(mousePos);
            }
        }

        // 右键删除节点
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1) {
            float minDist = 0.05f;
            int index = -1;
            for (int i = 0; i < path.NumPoints; i += 3) {
                float dst = Vector2.Distance(path[i], mousePos);
                if (dst < minDist) {
                    minDist = dst;
                    index = i;
                }
            }
            if (index != -1) {
                Undo.RecordObject(creator, "删除节点");
                path.DeleteSegment(index);
            }
        }

        HandleUtility.AddDefaultControl(0);     // 点击空白地方时仍选中 PathCreator
    }

    private void Draw() {
        // 绘制锚点
        Handles.color = creator.AnchorColor;
        for(int i = 0; i < path.NumPoints; i++) {
            Vector2 pos = Handles.FreeMoveHandle(path[i], Quaternion.identity, creator.HandleSize, Vector3.zero, Handles.CylinderHandleCap);
            if (pos != path[i]) {
                Undo.RecordObject(creator, "移动锚点");
                path.MovePoint(i, pos);                        
            }
        }

        // 绘制曲线
        Handles.color = creator.ControlLineColor;
        for(int i = 0; i < path.NumSegments; i++) {
            Vector2[] points = path.GetPointsInSegment(i);
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawBezier(points[0], points[3], points[1], points[2], creator.SegmentColor, null, 4f);
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
