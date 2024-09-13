using UnityEngine;
using System.Collections.Generic;

public class DomeTrail : MonoBehaviour
{
    public Material trailMaterial;
    public float trailWidth = 0.1f;
    public int maxSegments = 50;

    private List<Vector3> points;
    private LineRenderer lineRenderer;
    private Vector3 lastPosition;

    void Start()
    {
        // Initialize the points list and LineRenderer
        points = new List<Vector3>();
        
        // Add a LineRenderer if not already attached
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Set LineRenderer properties
        lineRenderer.material = trailMaterial;
        lineRenderer.startWidth = trailWidth;
        lineRenderer.endWidth = trailWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true; // Ensure the trail renders in world space

        // Set initial last position to the current object position
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;

        // Add a new point to the trail if the player has moved
        if (Vector3.Distance(currentPosition, lastPosition) > 0.1f)
        {
            points.Add(currentPosition);
            lastPosition = currentPosition;

            // If we exceed the max number of segments, remove the oldest point
            if (points.Count > maxSegments)
            {
                points.RemoveAt(0);
            }

            // Update the LineRenderer with the current points
            UpdateLineRenderer();
        }
    }

    void UpdateLineRenderer()
    {
        if (points.Count < 2)
        {
            return; // We need at least two points to form a trail
        }

        // Update the LineRenderer with the current points
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
