using UnityEngine;

public class ColourfulPlayerAvatar : PlayerAvatar
{
    [Tooltip("Time it takes to transition to a new color (in seconds).")]
    public float transitionDuration = 3f;  // Time between color transitions

    private Color targetColor;  // The color we're transitioning to
    private float transitionTimer = 0f;  // Timer to track transition progress

    public override void SetData(string name)
    {
        base.SetData(name);  // Call the base setup logic

        // Initialize player with a unique starting color based on ID
        color = Color.HSVToRGB((Mathf.Sin(id) + 1) / 2, 1, 1);  // Unique starting color

        // Set the initial target color as the current color
        targetColor = GetNextColor(color);

        UpdatePlayerColor();  // Ensure the rings and text get the color
    }

    // Override Update to add color transition logic
    public override void Update()
    {
        base.Update();  // Keep the base Update behavior

        // Increment the transition timer
        transitionTimer += Time.deltaTime;

        // If the timer exceeds the transition duration, set a new target color
        if (transitionTimer > transitionDuration)
        {
            transitionTimer = 0f;  // Reset the timer
            targetColor = GetNextColor(color);  // Get the next color in the sequence
        }

        // Smoothly interpolate between the current color and the target color
        color = Color.Lerp(color, targetColor, transitionTimer / transitionDuration);

        // Apply the new color
        UpdatePlayerColor();
    }

    // Get the next color in the sequence based on the current color's hue
    private Color GetNextColor(Color currentColor)
    {
        // Convert the current color to HSV
        Color.RGBToHSV(currentColor, out float hue, out float saturation, out float value);

        // Move the hue forward (looping around if it exceeds 1)
        hue = Mathf.Repeat(hue + 0.2f, 1f);  // Adjust this value for different hue steps

        // Return the new color with adjusted hue, maintaining the same saturation and value
        return Color.HSVToRGB(hue, saturation, value);
    }
}
