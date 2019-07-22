using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public void RandomColor() {
        Renderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null) {
            renderer.material.color = Random.ColorHSV();
        }
    }

    public void ResetColor() {
        Renderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null) {
            renderer.material.color = Color.white;
        }
    }
}
