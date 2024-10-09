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

    [Tooltip("Number of dots collected by the player.")]
    public int numDotsCollected;

    [Tooltip("Renderer for the regular ring.")]
    public Renderer regularRing;

    [Tooltip("Renderer for the charged ring.")]
    public Renderer chargedRing;

    [Tooltip("Renderer for the maximum ring.")]
    public Renderer maxRing;

    [Tooltip("Material for the regular ring.")]
    public Material regularRingMaterial;

    [Tooltip("Material for the charged ring.")]
    public Material chargedRingMaterial;

    [Tooltip("Material for the maximum ring.")]
    public Material maxRingMaterial;

    [Tooltip("Color assigned to the player.")]
    public Color color;

    [Tooltip("Checkbox to toggle player name display.")]
    public bool showPlayerName;

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

    [Header("3D Model Settings")]
    [Tooltip("3D model for the player avatar.")]
    public GameObject playerModel;

    [Tooltip("Check to enable or disable the 3D model.")]
    public bool usePlayerModel = true; // Default is to show the 3D model

    [Tooltip("Original color of the player, used for resetting or consistent rendering.")]
    public Color originalColor;

    public bool useRainbowColorLogic = false; // Set this to true for the game where you want the rainbow effect


    // Audio fields
    public AudioSource audioSource; // Reference to the AudioSource on the player

    public AudioClip withMeP0Clip;
    public AudioClip withMeP1Clip;
    public AudioClip withMeP2Clip;
    public AudioClip withMeP3Clip;
    public AudioClip withMeP4Clip;
    public AudioClip withMeP5Clip;
    public AudioClip withMeP6Clip;
    public AudioClip withMeP7Clip;
    public AudioClip withMeP8Clip;
    public AudioClip withMeP9Clip;

    private void Start()
    {
        if (audioSource == null)
        {
            // Debug.LogError("AudioSource is missing on PlayerAvatar.");
            return;
        }

        // Assign the correct audio clip if the scene is "WithMe"
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "WithMe")
        {
            AssignWithMeClip(id); // Assign the appropriate clip based on the player's ID
            audioSource.loop = true;
            // audioSource.Play(); // Start the audio clip for the player
        }
    }



    private void AssignWithMeClip(int playerId)
    {
        switch (playerId)
        {
            case 0:
                audioSource.clip = withMeP0Clip;
                break;
            case 1:
                audioSource.clip = withMeP1Clip;
                break;
            case 2:
                audioSource.clip = withMeP2Clip;
                break;
            case 3:
                audioSource.clip = withMeP3Clip;
                break;
            case 4:
                audioSource.clip = withMeP4Clip;
                break;
            case 5:
                audioSource.clip = withMeP5Clip;
                break;
            case 6:
                audioSource.clip = withMeP6Clip;
                break;
            case 7:
                audioSource.clip = withMeP7Clip;
                break;
            case 8:
                audioSource.clip = withMeP8Clip;
                break;
            case 9:
                audioSource.clip = withMeP9Clip;
                break;
            default:
                Debug.LogWarning("No audio clip assigned for player ID: " + playerId);
                break;
        }
    }


 public virtual void SetData(string name)
{
    // Check if the player count is valid before calculating the hue
    int playerCount = Mathf.Max(controller.players.Count, 1); // Ensure at least 1 to avoid division by zero
    float initialHue = Mathf.Repeat((id + initialHueOffset) / playerCount, 1f);

    Debug.Log($"[DEBUG] Player ID: {id} | Initial Hue: {initialHue}, Saturation: {colorSaturation}, Value: {colorValue}");

    // Set the player's color using HSV values
    color = Color.HSVToRGB(initialHue, colorSaturation, colorValue);

    // Ensure the color is not too dark
    if (color == Color.black || color.grayscale < 0.1f)
    {
        Debug.LogWarning($"[WARNING] Player ID: {id} received a very dark color. Adjusting...");
        color = Color.HSVToRGB(0.1f, colorSaturation, colorValue); // Fallback to a bright color
    }

    // Log the final color assigned to the player
    Debug.Log($"[DEBUG] Player ID: {id} | Final Color (R,G,B): {color.r}, {color.g}, {color.b}");

    // Save the original color
    originalColor = color;
    Debug.Log($"[DEBUG] Player ID: {id} | Original Color (R,G,B): {originalColor.r}, {originalColor.g}, {originalColor.b}");

    ApplyMaterialsToRings();
    UpdatePlayerColor();
    
    if (playerModel != null)
    {
        playerModel.SetActive(usePlayerModel);
    }
}


    public void UpdateLineRendererColor(TrailRenderer trailRenderer)
    {
        // Set the trail renderer's start and end color to the player's original color
        trailRenderer.startColor = originalColor;
        trailRenderer.endColor = originalColor;
    }



    public virtual void ApplyMaterialsToRings()
{
    if (regularRing != null && regularRingMaterial != null)
    {
        regularRing.material = new Material(regularRingMaterial); // Clone the material
        Debug.Log($"[DEBUG] Player ID: {id} | Regular Ring Material Assigned: {regularRing.material.name}");
    }

    if (chargedRing != null && chargedRingMaterial != null)
    {
        chargedRing.material = new Material(chargedRingMaterial); // Clone the material
        Debug.Log($"[DEBUG] Player ID: {id} | Charged Ring Material Assigned: {chargedRing.material.name}");
    }

    if (maxRing != null && maxRingMaterial != null)
    {
        maxRing.material = new Material(maxRingMaterial); // Clone the material
        Debug.Log($"[DEBUG] Player ID: {id} | Max Ring Material Assigned: {maxRing.material.name}");
    }
}


    public virtual void OnDotCollect(bool chargeRingOn, bool maxRingOn)
    {
        if (chargeRingOn)
        {
            chargedRing.enabled = true;
        }

        if (maxRingOn)
        {
            maxRing.enabled = true;
        }

        numDotsCollected++;
        transform.localScale = controller.GetScale(id);
    }
    public virtual void Reset()
    {
        numDotsCollected = 0;
        maxRing.enabled = false;
        chargedRing.enabled = false;
        regularRing.enabled = true;

        transform.localScale = Vector3.one * controller.startSize;

        if (audioSource != null)
        {
            audioSource.mute = true; // Mute the sound when the player is reset (disabled)
        }
    }

    public void Reactivate()
    {
        if (audioSource != null)
        {
            audioSource.mute = false; // Unmute the sound when the player is reactivated (enabled)
        }
    }


    public virtual void OnTriggerEnter(Collider collider)
    {
        controller.OnPlayerCollideWithDot(this, collider.gameObject);
    }

    public virtual void Update()
    {
        if (gameObject.activeSelf && audioSource != null && audioSource.mute)
        {
            audioSource.mute = false;
        }
        // Log the number of active players
        // Debug.Log($"Number of active players: {controller.players.Count}");

        // Rotate the hue through the full range (0 to 1) to create a rainbow effect
        // float hue = Mathf.Repeat(Time.time * colorRotationSpeed, 1f);
        // color = Color.HSVToRGB(hue, colorSaturation, colorValue);

        // float hue = Mathf.Lerp(hueRangeStart, hueRangeEnd, Mathf.PingPong(Time.time * colorRotationSpeed, 1f));
        // color = Color.HSVToRGB(hue, colorSaturation, colorValue);

        UpdatePlayerColor();

        transform.LookAt(controller.center);

        // Collision detection between players
        for (int i = 0; i < controller.players.Count; i++)
        {
            if (controller.players[i] != this.gameObject)
            {
                float distance = Vector3.Distance(controller.players[i].transform.position, transform.position);
                distance -= transform.localScale.x / 2;
                distance -= controller.players[i].transform.localScale.x / 2;

                if (distance < collisionThreshold)
                {
                    controller.OnPlayersCollided(this, controller.playerAvatars[i]);
                }
            }
        }
    }


   public virtual void UpdatePlayerColor()
{
    if (regularRing != null)
    {
        regularRing.material.color = color;
        Debug.Log($"[DEBUG] Player ID: {id} | Regular Ring Color: {regularRing.material.color}");
    }
    if (chargedRing != null)
    {
        chargedRing.material.color = Color.Lerp(color, Color.white, 0.5f); // Give the charged ring a glow effect
        Debug.Log($"[DEBUG] Player ID: {id} | Charged Ring Color: {chargedRing.material.color}");
    }
    if (maxRing != null)
    {
        float pulse = Mathf.PingPong(Time.time * 2f, 1f); // Pulsing effect for max ring
        maxRing.material.color = Color.Lerp(color, Color.yellow, pulse); // Make max ring flash yellow
        Debug.Log($"[DEBUG] Player ID: {id} | Max Ring Color: {maxRing.material.color}");
    }
    if (text != null)
    {
        text.color = color;
        Debug.Log($"[DEBUG] Player ID: {id} | Text Color: {text.color}");
    }
}

}
