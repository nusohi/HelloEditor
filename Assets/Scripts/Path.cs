using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField]
    private List<Vector2> points;

    public int NumSegments { get { return (points.Count - 1) / 3; } }
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

    // 获取一个曲线段的 4 个锚点
    public Vector2[] GetPointsInSegment(int i) {
        return new Vector2[] {
            points[i * 3],
            points[i * 3 + 1],
            points[i * 3 + 2],
            points[i * 3 + 3]
        };
    }

    // 移动点
    public void MovePoint(int i,Vector2 pos) {
        Vector2 offset = pos - points[i];
        points[i] = pos;
        
        switch (i % 3) {
            // 移动的点为曲线上的锚点，两侧的点同时移动
            case 0:
                if (i - 1 >= 0)
                    points[i - 1] += offset;
                if (i + 1 < NumPoints)
                    points[i + 1] += offset;
                break;
            case 1:
                if (i - 2 >= 0 && !Event.current.control) {
                    float dst = (points[i - 1] - points[i - 2]).magnitude;
                    Vector2 dir = (points[i - 1] - pos).normalized;
                    points[i - 2] = points[i - 1] + dir * dst;
                }
                break;
            case 2:
                if (i + 2 < NumPoints && !Event.current.control) {
                    float dst = (points[i + 1] - points[i + 2]).magnitude;
                    Vector2 dir = (points[i + 1] - pos).normalized;
                    points[i + 2] = points[i + 1] + dir * dst;
                }
                break;
            default:
                break;
        }
    }

}
