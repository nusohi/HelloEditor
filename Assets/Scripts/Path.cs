using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField]
    private List<Vector2> points;

    [SerializeField]
    private bool isClosed = false;

    public int NumSegments { get { return points.Count / 3; } }
    public int NumPoints { get { return points.Count; } }

    public Vector2 this[int i]
    {
        get { return points[i]; }
    }

    // 默认路径有 4 个锚点（0、1、2、3）
    public Path(Vector2 center) {
        points = new List<Vector2>() {
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * 0.5f,
            center + (Vector2.right + Vector2.down) * 0.5f,
            center + Vector2.right
        };
    }

    // 添加一个曲线段需要添加 3 个锚点（4、5、6）
    public void AddSegment(Vector2 seg) {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + seg) / 2);
        points.Add(seg);
    }

    // 删除一个曲线段上的点
    public void DeleteSegment(int index) {
        if (NumSegments <= 2 && (isClosed || NumSegments <= 1))
            return;

        if (index == 0) {
            if (isClosed)
                points[points.Count - 1] = points[2];
            points.RemoveRange(0, 3);
        }
        else if (index == points.Count - 1 && !isClosed) {
            points.RemoveRange(index - 2, 3);
        }
        else {
            points.RemoveRange(index - 1, 3);
        }
    }

    // 获取一个曲线段的 4 个锚点
    public Vector2[] GetPointsInSegment(int i) {
        return new Vector2[] {
            points[i * 3],
            points[i * 3 + 1],
            points[i * 3 + 2],
            points[LoopIndex(i * 3 + 3)]
        };
    }

    // 移动点
    public void MovePoint(int i, Vector2 pos) {
        Vector2 offset = pos - points[i];
        points[i] = pos;

        switch (i % 3) {
            // 移动的点为曲线上的锚点，两侧的点同时移动
            case 0:
                if (i - 1 >= 0 || isClosed)
                    points[LoopIndex(i - 1)] += offset;
                if (i + 1 < NumPoints || isClosed)
                    points[LoopIndex(i + 1)] += offset;
                break;
            case 1:
                if ((i - 2 >= 0 || isClosed) && !Event.current.control) {
                    float dst = (points[LoopIndex(i - 1)] - points[LoopIndex(i - 2)]).magnitude;
                    Vector2 dir = (points[LoopIndex(i - 1)] - pos).normalized;
                    points[LoopIndex(i - 2)] = points[LoopIndex(i - 1)] + dir * dst;
                }
                break;
            case 2:
                if ((i + 2 < NumPoints || isClosed) && !Event.current.control) {
                    float dst = (points[LoopIndex(i + 1)] - points[LoopIndex(i + 2)]).magnitude;
                    Vector2 dir = (points[LoopIndex(i + 1)] - pos).normalized;
                    points[LoopIndex(i + 2)] = points[LoopIndex(i + 1)] + dir * dst;
                }
                break;
            default:
                break;
        }
    }

    // toggle 闭合曲线 -> 添加 2 个锚点
    public void ToggleClose() {
        isClosed = !isClosed;

        if (isClosed) {
            points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
            points.Add(points[0] * 2 - points[1]);
        }
        else {
            points.RemoveRange(points.Count - 2, 2);
        }
    }

    // index 循环
    public int LoopIndex(int i) {
        return (i + points.Count) % points.Count;
    }
}
