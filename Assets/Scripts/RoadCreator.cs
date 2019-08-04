using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadCreator : MonoBehaviour
{
    public float RoadWidth = 1f;
    [Range(0.05f, 1f)]
    public float Spacing = 0.1f;

    public void UpdateRoad() {
        Path path = GetComponent<PathCreator>().path;
        Vector2[] points = path.CalEvenSpacedPoints(Spacing);
        GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, path.IsClosed);
    }

    Mesh CreateRoadMesh(Vector2[] points, bool closed) {
        int vertsCount = points.Length * 2;
        int trisCount = closed 
            ? 2 * points.Length * 3 
            : 2 * (points.Length - 1) * 3;
        List<Vector3> verts = new List<Vector3>();          // Mesh 顶点
        int[] tris = new int[trisCount];                    // Mesh 三角形顶点
        List<Vector2> uvs = new List<Vector2>();            // Mesh UV
        
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++) {
            Vector2 forward = Vector2.zero;

            if (i > 0)
                forward += points[i] - points[i - 1];
            if (i < points.Length - 1)
                forward += points[i + 1] - points[i];

            forward.Normalize();
            Vector2 left = new Vector2(-forward.y, forward.x);

            // 左右两顶点
            verts.Add(points[i] + left * RoadWidth);
            verts.Add(points[i] - left * RoadWidth);
            // UV
            float percent = i / (float)(points.Length - 1);
            percent = 1 - Mathf.Abs(percent * 2 - 1);       // 1-|2x-1| 使 x 从原来的 0->1 变成了 0->1->0
            uvs.Add(new Vector2(0, percent));
            uvs.Add(new Vector2(1, percent));


            if (i < points.Length - 1 || closed) {
                int vertIndex = i * 2;
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % vertsCount;
                tris[triIndex + 2] = vertIndex + 1;
                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % vertsCount;
                tris[triIndex + 5] = (vertIndex + 3) % vertsCount;
                triIndex += 6;
            }

        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.triangles = tris;
        mesh.uv = uvs.ToArray();

        return mesh;
    }

}
