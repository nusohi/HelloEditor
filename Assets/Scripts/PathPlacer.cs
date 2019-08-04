using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlacer : MonoBehaviour
{
    private PathCreator creator;
    private Transform spacingParent;
    [Range(0,1)]
    public float Spacing = 0.1f;
    public float Resolution = 1f;
    public float Scale = 0.5f;

    void Awake()
    {
        creator = GetComponent<PathCreator>();
        spacingParent = gameObject.transform.Find("Spacings");
        Placer();
    }

    public void Placer() {
        DestroyPlacer();

        Vector2[] points = creator.path.CalEvenSpacedPoints(Spacing, Resolution);
        foreach(Vector2 point in points) {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = spacingParent;
            go.transform.position = point;
            go.transform.localScale = Vector3.one * Spacing * Scale;
        }
    }

    public void DestroyPlacer() {
        for (int i = 0; i < spacingParent.childCount; i++) {
            Destroy(spacingParent.GetChild(i).gameObject);
        }
    }
}
