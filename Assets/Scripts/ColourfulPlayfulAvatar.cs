using UnityEngine;

public class ColourfulPlayerAvatar : PlayerAvatar
{
    [Tooltip("Time it takes to transition to a new color (in seconds).")]
    public float transitionDuration = 3f;  // Time between color transitions

    private Color targetColor;  // The color we're transitioning to
    private Color currentColor;  // The current color of the trail
    private float transitionTimer = 0f;  // Timer to track transition progress
    private float currentHue;  // Keep track of the current hue
    private TrailRenderer trailRenderer;  // This player's trail renderer

    public override void Start()
    {
        base.Start();  // Call the base Start method to set up common PlayerAvatar settings
        trailRenderer = GetComponent<TrailRenderer>();

        // Start with a random hue and log it
        currentHue = Random.value;
        Debug.Log($"[DEBUG] Starting Hue: {currentHue}");

        currentColor = Color.HSVToRGB(currentHue, colorSaturation, colorValue);
        Debug.Log($"[DEBUG] Initial Color: {currentColor}");

        targetColor = GetNextColor(currentHue);
        Debug.Log($"[DEBUG] Initial Target Color: {targetColor}");

        UpdateTrailRendererColor();
    }

    // Full customization of the Update method
    private void Update()
    {
        // Custom color transition logic
        transitionTimer += Time.deltaTime;
        Debug.Log($"[DEBUG] Transition Timer: {transitionTimer}");

        if (transitionTimer > transitionDuration)
        {
            transitionTimer = 0f;  // Reset the timer

            // Increment the hue and log the updated hue
            currentHue = Mathf.Repeat(currentHue + 0.1f, 1f);
            Debug.Log($"[DEBUG] Updated Hue: {currentHue}");

            // Get the next color and log it
            targetColor = GetNextColor(currentHue);
            Debug.Log($"[DEBUG] New Target Color: {targetColor}");
        }

        // Smoothly interpolate between the current color and the target color
        currentColor = Color.Lerp(currentColor, targetColor, transitionTimer / transitionDuration);
        Debug.Log($"[DEBUG] Interpolated Current Color: {currentColor}");

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
            Debug.Log($"[DEBUG] TrailRenderer Color Applied: {currentColor}");
        }
        else
        {
            Debug.LogWarning("[DEBUG] TrailRenderer is null!");
        }
    }

    // Get the next color in the sequence based on the current hue
    private Color GetNextColor(float hue)
    {
        Color nextColor = Color.HSVToRGB(hue, colorSaturation, colorValue);
        Debug.Log($"[DEBUG] Next Color for Hue {hue}: {nextColor}");
        return nextColor;
    }
}
