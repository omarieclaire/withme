using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;

    // Settings for line width animation
    public AnimationCurve widthCurve;
    public float widthMultiplier = 1.0f;

    // Settings for color gradient
    public Gradient colorGradient;

    // Settings for dynamic wave movement
    public bool applyWaveEffect = true;
    public float waveFrequency = 1.0f;
    public float waveAmplitude = 0.1f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Initialize the width curve to create a thinner start and end section with a slightly thicker middle
        if (widthCurve == null || widthCurve.length == 0)
        {
            // Set default width curve (extra thin at start and end, slightly thicker in the middle)
            widthCurve = new AnimationCurve();
            widthCurve.AddKey(0.0f, 0.01f); // Extra thin at the player (0.01)
            widthCurve.AddKey(0.5f, 0.1f);  // Thicker in the middle (0.1)
            widthCurve.AddKey(1.0f, 0.01f); // Extra thin at Florp (0.01)
        }

        // Apply initial width and color settings
        lineRenderer.widthCurve = widthCurve;
        lineRenderer.widthMultiplier = widthMultiplier;
        lineRenderer.colorGradient = colorGradient;
    }

    void Update()
    {
        // Apply wave effect dynamically if enabled
        if (applyWaveEffect)
        {
            ApplyWaveEffect();
        }
    }

    void ApplyWaveEffect()
    {
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 currentPosition = lineRenderer.GetPosition(i);
            float offset = Mathf.Sin(Time.time * waveFrequency + i) * waveAmplitude;
            lineRenderer.SetPosition(i, new Vector3(currentPosition.x, currentPosition.y + offset, currentPosition.z));
        }
    }
}
