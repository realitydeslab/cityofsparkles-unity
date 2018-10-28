using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class InstructionLine : MonoBehaviour {

    public float width;

    private LineRenderer lineRenderer;

    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        initLineRenderer();
    }

	void Update() {
        updateLineRenderer();
    }

    private void initLineRenderer() {
        lineRenderer = GetComponent<LineRenderer>();

        List<Vector3> points = new List<Vector3>();	
        for (int i = 0; i < transform.childCount; i++) {
            var child = transform.GetChild(i);
            if (child != gameObject && child.name != "InstructionText" ) {
                points.Add(child.transform.localPosition);
            }
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    private void updateLineRenderer()
    {
        float scaledWidth = width;
        lineRenderer.startWidth = scaledWidth;
        lineRenderer.endWidth = scaledWidth;
    }
}

