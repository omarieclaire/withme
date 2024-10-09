using UnityEngine;

public class ColourfulPlayerAvatar : PlayerAvatar
{
    // Override SetData to use the older color logic for this level
    public override void SetData(string name)
    {
        base.SetData(name);  // Call the base class to handle common setup

        // Use the older color generation logic based on player ID
        color = Color.HSVToRGB((Mathf.Sin(id) + 1) / 2, 1, 1);  // Generate unique color

        // Apply the color to the player elements
        UpdatePlayerColor();  // Ensure the rings and text get the color
    }

    // Override Update to maintain color consistency
    public override void Update()
    {
        base.Update();  // Use base Update logic for common behavior

        // No need for time-based color changing here, keeping the color static
        UpdatePlayerColor();  // Apply the static color from SetData
    }
}
