using UnityEngine;

public class ColourfulPlayerAvatar : MonoBehaviour
{
    [Tooltip("Time it takes to transition to a new color (in seconds).")]
    public float transitionDuration = 3f;  // Time between color transitions

    private Color targetColor;  // The color we're transitioning to
    private Color currentColor;  // The current color of the trail
    private float transitionTimer = 0f;  // Timer to track transition progress
    private float currentHue;  // Keep track of the current hue
    private TrailRenderer trailRenderer;  // This player's trail renderer

    [Tooltip("Saturation of the color (0 = grayscale, 1 = full color).")]
    public float colorSaturation = 0.8f;

    [Tooltip("Brightness/Value of the color (0 = black, 1 = full brightness).")]
    public float colorValue = 1f;

    // Called when the object is created
    private void Start()
    {
        // Find the TrailRenderer component
        trailRenderer = GetComponent<TrailRenderer>();

        // Initialize a unique starting color for each player (based on the player's ID)
        currentHue = Random.value;  // Random hue for each player to ensure uniqueness

        currentColor = Color.HSVToRGB(currentHue, colorSaturation, colorValue);
        targetColor = GetNextColor(currentHue);  // Set initial target color

        // Immediately set the trail color
        UpdateTrailRendererColor();
    }

    // Update is called once per frame
    private void Update()
    {
        // Increment the transition timer
        transitionTimer += Time.deltaTime;

        // If the timer exceeds the transition duration, set a new target color
        if (transitionTimer > transitionDuration)
        {
            transitionTimer = 0f;  // Reset the timer
            currentHue = Mathf.Repeat(currentHue + 0.1f, 1f);  // Move hue forward for smooth progression
            targetColor = GetNextColor(currentHue);  // Get the next color in the sequence
        }

        // Smoothly interpolate between the current color and the target color
        currentColor = Color.Lerp(currentColor, targetColor, transitionTimer / transitionDuration);

        // Apply the new color to the TrailRenderer
        UpdateTrailRendererColor();
    }

    // Apply the color to the TrailRenderer
    private void UpdateTrailRendererColor()
    {
        if (trailRenderer != null)
        {
            trailRenderer.startColor = currentColor;
            trailRenderer.endColor = currentColor;
            Debug.Log($"[DEBUG] TrailRenderer Color: {currentColor}");
        }
    }

    // Get the next color in the sequence based on the current hue
    private Color GetNextColor(float hue)
    {
        return Color.HSVToRGB(hue, colorSaturation, colorValue);
    }
}
