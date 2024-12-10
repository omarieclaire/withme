using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    [Header("Display Settings")]
    public int displayIndex = 1;  // Index of the display (1 for second display)
    public int resolutionWidth = 1920;  // Width of the display resolution
    public int resolutionHeight = 1080; // Height of the display resolution
    public bool fullscreen = false; // Set whether the game should run fullscreen on the second display

    void Start()
    {
        // Check if multiple displays are available
        if (Display.displays.Length > 1)
        {
            Debug.Log("Displays connected: " + Display.displays.Length);

            // Activate all available displays
            for (int i = 0; i < Display.displays.Length; i++)
            {
                Display.displays[i].Activate();
                Debug.Log("Activated display: " + i);
            }

            // Make sure the chosen display exists before using it
            if (displayIndex < Display.displays.Length)
            {
                // Activate the specified display
                Display.displays[displayIndex].Activate();
                Debug.Log("Activated display " + displayIndex + ": " + Display.displays[displayIndex].systemWidth + "x" + Display.displays[displayIndex].systemHeight);

                // Set resolution for the second display
                Screen.SetResolution(resolutionWidth, resolutionHeight, fullscreen, displayIndex);
                Debug.Log($"Set resolution to {resolutionWidth}x{resolutionHeight} on display {displayIndex}");
            }
            else
            {
                Debug.LogError("Display index out of range. Please check available displays.");
            }
        }
        else
        {
            Debug.LogError("Only one display detected. No second display available.");
        }

        // If you have a main camera and want to target it to the second display
        Camera.main.targetDisplay = displayIndex;  // Render the main camera on the specified display
    }
}
