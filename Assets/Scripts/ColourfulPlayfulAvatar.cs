// using UnityEngine;

// public class ColourfulPlayerAvatar : PlayerAvatar
// {
//     // Override SetData to handle colorful behavior
//     public override void SetData(string name)
//     {
//         base.SetData(name);  // Call the base class to handle common setup

//         // Set the initial color based on player ID
//         float initialHue = Mathf.Repeat((Mathf.Sin(id) + initialHueOffset) / 2, 1f);
//         color = Color.HSVToRGB(initialHue, colorSaturation, colorValue);

//         UpdatePlayerColor();  // Ensure the rings get the starting color
//     }

//     // Override Update to add time-based color-changing behavior
//     public override void Update()
//     {
//         base.Update();  // Use base Update logic for common behavior

//         // Time-based hue value cycling through the full rainbow (0 to 1)
//         float hue = Mathf.Repeat(Time.time * colorRotationSpeed, 1f);  // Continually cycle hue over time
//         color = Color.HSVToRGB(hue, colorSaturation, colorValue);

//         UpdatePlayerColor();  // Apply the updated color
//     }
// }
