using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerAvatar : MonoBehaviour
{
    public Controller controller; // Reference to the game controller

    [Tooltip("TextMeshPro component for displaying player name.")]
    public TextMeshPro text;

    [Tooltip("Unique identifier for the player.")]
    public int id;

    [Tooltip("The amount by which the player grows for each dot collected.")]
    public float sizeIncrement = 3f; // Define a new size increment.

    [Tooltip("Number of dots collected by the player.")]
    public int numDotsCollected;

    [Tooltip("Renderer for the regular ring.")]
    public Renderer regularRing;

    [Tooltip("Renderer for the charged ring.")]
    public Renderer chargedRing;

    [Tooltip("Renderer for the maximum ring.")]
    public Renderer maxRing;

    [Tooltip("Color assigned to the player.")]
    public Color color;

    [Tooltip("Checkbox to toggle player name display.")]
    public bool showPlayerName;

    // Variables for tweaking appearance
    [Header("Color Tweak Variables")]
    [Tooltip("Speed of color rotation through the color wheel.")]
    public float colorRotationSpeed = 0.1f;

    [Tooltip("Starting hue for the color range (e.g., 0.25 for green).")]
    public float hueRangeStart = 0.25f;

    [Tooltip("Ending hue for the color range (e.g., 0.75 for purple).")]
    public float hueRangeEnd = 0.75f;

    [Tooltip("Saturation of the color (0 = grayscale, 1 = full color).")]
    public float colorSaturation = 0.8f;

    [Tooltip("Brightness/Value of the color (0 = black, 1 = full brightness).")]
    public float colorValue = 1f;

    [Tooltip("Initial hue offset to differentiate players' starting colors.")]
    public float initialHueOffset = 0.5f;

    [Tooltip("Distance threshold for triggering a collision between players.")]
    public float collisionThreshold = 0.1f;

    // New 3D model and visibility toggle
    [Header("3D Model Settings")]
    [Tooltip("3D model for the player avatar.")]
    public GameObject playerModel;

    [Tooltip("Check to enable or disable the 3D model.")]
    public bool usePlayerModel = true; // Default is to show the 3D model

    // Sets the player's data including name and color
    public void SetData(string name)
    {
        // if (showPlayerName)
        // {
        //     text.text = name; // Set player name
        // }
        // else
        // {
        //     text.text = ""; // Leave player name blank
        // }

        // Set the initial color based on player's ID using a unique hue and the initial offset
        float initialHue = Mathf.Repeat((Mathf.Sin(id) + initialHueOffset) / 2, 1f);
        color = Color.HSVToRGB(initialHue, colorSaturation, colorValue);

        UpdatePlayerColor(); // Ensure all rings get the starting color

        // Toggle the 3D model based on usePlayerModel bool
        if (playerModel != null)
        {
            playerModel.SetActive(usePlayerModel);
        }
    }

    public void OnDotCollect(bool chargeRingOn, bool maxRingOn)
    {
        Debug.Log($"[DEBUG] controller.startSize = {controller.startSize}");

        numDotsCollected++;  // Increment the number of dots collected by this player

        if (chargeRingOn)
        {
            chargedRing.enabled = true;
        }

        if (maxRingOn)
        {
            maxRing.enabled = true;
        }

        // Calculate the new scale based on the number of dots collected and the size increment
        float newScale = Mathf.Clamp(controller.startSize + numDotsCollected * sizeIncrement, controller.startSize * 0.5f, controller.startSize * 2.5f); // Adjust the max limit

        // Apply the new scale to the player
        transform.localScale = Vector3.one * newScale;

        // Log the player's new scale for debugging
        Debug.Log($"[DOT COLLECTED] Player {id}: New Scale = {transform.localScale}");
    }






    public void Reset()
    {
        // Debug.Log($"[RESET] Player {id}: Resetting. Dots before reset: {numDotsCollected}, Scale before reset: {transform.localScale}");

        // Reset the number of dots collected
        numDotsCollected = 0;

        // Reset the rings
        maxRing.enabled = false;
        chargedRing.enabled = false;
        regularRing.enabled = true;

        // Reset the player's size
        transform.localScale = Vector3.one * controller.startSize; // Assuming startSize is your default size
                                                                   // Debug.Log($"[RESET] Player {id}: Reset complete. Dots now: {numDotsCollected}, Scale after reset: {transform.localScale}");

    }


    public void OnTriggerEnter(Collider collider)
    {
        controller.OnPlayerCollideWithDot(this, collider.gameObject);
    }

    public void Update()
    {
        float hue = Mathf.Lerp(hueRangeStart, hueRangeEnd, Mathf.PingPong(Time.time * colorRotationSpeed, 1f));
        hue = Mathf.Lerp(hueRangeStart, hueRangeEnd, hue);

        color = Color.HSVToRGB(hue, 0.8f, 1f);

        UpdatePlayerColor();

        transform.LookAt(controller.center);

        for (int i = 0; i < controller.players.Count; i++)
        {
            if (controller.players[i] != this.gameObject)
            {
                float distance = Vector3.Distance(controller.players[i].transform.position, transform.position);

                distance -= transform.localScale.x / 2;
                distance -= controller.players[i].transform.localScale.x / 2;

                // Debug.Log($"Distance: {distance}, CollisionThreshold: {collisionThreshold}");

                if (distance < collisionThreshold)
                {
                    controller.OnPlayersCollided(this, controller.playerAvatars[i]);
                }
            }
        }
    }

    private void UpdatePlayerColor()
    {
        if (regularRing != null)
        {
            regularRing.material.color = color;
        }
        if (chargedRing != null)
        {
            chargedRing.material.color = color;
        }
        if (maxRing != null)
        {
            maxRing.material.color = color;
        }
        if (text != null)
        {
            text.color = color;
        }
    }
}
