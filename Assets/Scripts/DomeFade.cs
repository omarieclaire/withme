using UnityEngine;
using System.Collections;

public class DomeFade : MonoBehaviour
{
    public Material fadeMaterial;  // Assign the material via Inspector (the sphere's material)

    void Start()
    {
        // Check if the material is assigned
        if (fadeMaterial == null)
        {
            // Attempt to get the material from the attached object's MeshRenderer
            fadeMaterial = GetComponent<MeshRenderer>().material;
        }

        if (fadeMaterial == null)
        {
            Debug.LogError("[ERROR] Fade material is not assigned or found!");
        }
        else
        {
            Debug.Log("[INFO] Fade material assigned successfully. Using shader: " + fadeMaterial.shader.name);
        }
    }

    public void FadeIn(float duration)
    {
        StartCoroutine(Fade(0, 1, duration));  // Fade from transparent to opaque (black)
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(Fade(1, 0, duration));  // Fade from opaque (black) to transparent
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float currentTime = 0;
        Color color = fadeMaterial.color;  // Get the current color of the material
        Debug.Log($"[INFO] Starting fade with initial alpha: {color.a}");

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, currentTime / duration);  // Interpolate alpha
            color.a = alpha;
            fadeMaterial.color = color;  // Apply the new alpha value to the material

            Debug.Log($"[INFO] Fade in progress. Alpha: {alpha}");
            yield return null;
        }

        // Ensure the alpha is fully set at the end
        color.a = endAlpha;
        fadeMaterial.color = color;
        Debug.Log($"[INFO] Fade complete. Alpha is now {endAlpha}");
    }
}
