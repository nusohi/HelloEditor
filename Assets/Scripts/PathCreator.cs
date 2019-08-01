using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    public Color AnchorColor = Color.red;
    public Color SegmentColor = Color.green;
    public Color ControlLineColor = Color.red;
    [Range(0,1)]
    public float HandleSize = 0.1f;
    
    public void CreatePath() {
        path = new Path(transform.position);
    }
}
