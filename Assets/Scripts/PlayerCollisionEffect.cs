using UnityEngine;
using System.Collections;

public class PlayerCollisionEffect : MonoBehaviour
{
   [Tooltip("Color used to make the player glow upon collision.")]
    public Color glowColor = Color.yellow;  // Color to glow on collision

    [Tooltip("Duration of the glow effect.")]
    public float glowDuration = 2f;  // Duration in seconds

    private Renderer playerRenderer;
    private MaterialPropertyBlock propBlock;
    private Color originalEmissionColor;

    void Start()
    {
        playerRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Store the original emission color (to restore later)
        playerRenderer.GetPropertyBlock(propBlock);
        originalEmissionColor = playerRenderer.sharedMaterial.GetColor("_EmissionColor");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with another player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Start the temporary glow effect
            StartCoroutine(ApplyGlowEffect());
        }
    }

    private IEnumerator ApplyGlowEffect()
    {
        // Set the emission color to make the object glow
        playerRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_EmissionColor", glowColor);
        playerRenderer.SetPropertyBlock(propBlock);

        // Wait for the glow duration (e.g., 2 seconds)
        yield return new WaitForSeconds(glowDuration);

        // Restore the original emission color
        playerRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_EmissionColor", originalEmissionColor);
        playerRenderer.SetPropertyBlock(propBlock);
    }
}