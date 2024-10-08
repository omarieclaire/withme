using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathingEffect : MonoBehaviour
{
    // Public variables exposed to the Unity Editor
    public float baseBreathingSpeed = 1f;   // Base speed of the breathing effect
    public float baseBreathingScale = 0.05f; // Base scale change during breathing (in/out)
    public float baseRotationSpeed = 10f;    // Base speed of rotation
    public float randomRotationFactor = 0.5f; // Random rotation factor to add variation

    // Movement properties for lazy circular motion
    public float circleRadius = 0.5f;         // Radius of the circular motion
    public float circleSpeed = 0.1f;          // Speed of the circular motion
    private Vector3 initialPosition;          // Initial position of the object

    // Material properties
    public Color vibrantReflectionColor = Color.cyan;  // More vibrant reflection color
    public Color vibrantCenterOrbColor = Color.magenta;    // Vivid center orb color
    public float baseReflectionSharpness = 0.8f;
    public float baseReflectionMultiplier = 2f;
    public float baseOpaqueness = 0.95f;              // High opaqueness for solid look

    private Material objectMaterial;  // Reference to the object's material
    private Vector3 initialScale;     // To store the initial scale of the object

    // Unique random offsets for each object
    private float uniqueBreathingSpeed;
    private float uniqueBreathingScale;
    private float uniqueRotationSpeed;
    private float sineWaveOffset;

    void Start()
    {
        // Cache the initial scale and position of the object
        initialScale = transform.localScale;
        initialPosition = transform.position;

        // Get the object's material (assuming it has one)
        if (TryGetComponent<Renderer>(out Renderer renderer))
        {
            objectMaterial = renderer.material;
        }

        // Generate unique random values for each object
        uniqueBreathingSpeed = baseBreathingSpeed + Random.Range(-0.2f, 0.2f); // Small variation in breathing speed
        uniqueBreathingScale = baseBreathingScale + Random.Range(-0.01f, 0.01f); // Small variation in scale amount
        uniqueRotationSpeed = baseRotationSpeed + Random.Range(-2f, 2f); // Small variation in rotation speed
        sineWaveOffset = Random.Range(0f, Mathf.PI * 2); // Random phase offset for the sine wave
    }

    void Update()
    {
        // Apply unique breathing effect (in and out scaling)
        float scaleFactor = 1 + Mathf.Sin(Time.time * uniqueBreathingSpeed + sineWaveOffset) * uniqueBreathingScale;
        transform.localScale = initialScale * scaleFactor;

        // Apply slow rotation with some random variation
        float randomRotation = Mathf.PerlinNoise(Time.time, 0f) * randomRotationFactor;
        transform.Rotate(Vector3.up, (uniqueRotationSpeed + randomRotation) * Time.deltaTime);

        // Apply lazy circular movement
        float circleX = Mathf.Sin(Time.time * circleSpeed) * circleRadius;
        float circleZ = Mathf.Cos(Time.time * circleSpeed) * circleRadius;
        transform.position = initialPosition + new Vector3(circleX, 0, circleZ);  // Move object in a circular path

        // If material is available, apply more vibrant material changes
        if (objectMaterial != null)
        {
            // Boost reflection color vibrancy using a cycle between vibrant hues
            Color reflectionColor = Color.Lerp(vibrantReflectionColor, Color.yellow, Mathf.Sin(Time.time * 0.5f + sineWaveOffset) * 0.5f + 0.5f);
            objectMaterial.SetColor("_ReflectionColor", reflectionColor);

            // Sharper and vivid orb color transitions
            Color orbColor = Color.Lerp(vibrantCenterOrbColor, Color.blue, Mathf.Sin(Time.time * 0.7f + sineWaveOffset) * 0.5f + 0.5f);
            objectMaterial.SetColor("_CenterOrbColor", orbColor);

            // Adjust reflection sharpness dynamically for a shinier appearance
            float reflectionSharpness = baseReflectionSharpness + Mathf.Sin(Time.time * 0.8f) * 0.2f;
            objectMaterial.SetFloat("_ReflectionSharpness", reflectionSharpness);

            // Subtle adjustments to opaqueness for added depth
            float opaqueness = baseOpaqueness + Mathf.PerlinNoise(Time.time, 0f) * 0.05f;
            objectMaterial.SetFloat("_Opaqueness", opaqueness);

            // Reflection multiplier for more noticeable highlights
            float reflectionMultiplier = baseReflectionMultiplier + Mathf.PerlinNoise(Time.time * 0.4f, 0f) * 0.3f;
            objectMaterial.SetFloat("_ReflectionMultiplier", reflectionMultiplier);
        }
    }
}
