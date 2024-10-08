using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WithMeSparkleEffect : MonoBehaviour
{
    [Header("Sparkle Settings")]
    [Tooltip("Minimum sparkle intensity (0 means no sparkle, 1 means full sparkle).")]
    public float minSparkleIntensity = 0.2f;

    [Tooltip("Maximum sparkle intensity (0 means no sparkle, 1 means full sparkle).")]
    public float maxSparkleIntensity = 0.5f;

    [Tooltip("Frequency of the sparkle effect (higher values increase the frequency).")]
    public float sparkleFrequency = 2f;

    [Header("Twinkle Settings")]
    [Tooltip("Color used during twinkle effect. Temporarily overlays the object's color.")]
    public Color twinkleColor = Color.white;

    [Tooltip("Intensity of the twinkle effect (how much the color changes during twinkle).")]
    public float twinkleIntensity = 0.3f;

    [Tooltip("Frequency of the twinkle effect (higher values increase the frequency).")]
    public float twinkleFrequency = 1f;

    [Tooltip("How fast the sparkle and twinkle effects should change (smoothing factor).")]
    public float transitionSpeed = 1.5f;

    private Renderer objRenderer;
    private MaterialPropertyBlock propBlock;
    private Color originalEmissionColor;
    private Color originalColor;
    private float sparkleTimer;
    private float twinkleTimer;

    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Store the original colors for emission and base color
        objRenderer.GetPropertyBlock(propBlock);
        originalEmissionColor = objRenderer.sharedMaterial.GetColor("_EmissionColor");
        originalColor = objRenderer.sharedMaterial.color;
    }

    void Update()
    {
        // Sparkle effect: change the emission intensity in a subtle way
        sparkleTimer += Time.deltaTime * sparkleFrequency;
        float sparkle = Mathf.Lerp(minSparkleIntensity, maxSparkleIntensity, Mathf.PingPong(sparkleTimer, 1f));
        Color sparkleEmission = originalEmissionColor * sparkle;

        // Set the emission color using MaterialPropertyBlock
        objRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_EmissionColor", sparkleEmission);
        objRenderer.SetPropertyBlock(propBlock);

        // Twinkle effect: briefly overlay the object's color with the twinkleColor
        twinkleTimer += Time.deltaTime * twinkleFrequency;
        float twinkleAmount = Mathf.PingPong(twinkleTimer, 1f) * twinkleIntensity;
        Color twinkleOverlay = Color.Lerp(originalColor, twinkleColor, twinkleAmount);

        // Apply the twinkle overlay color
        objRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", Color.Lerp(originalColor, twinkleOverlay, transitionSpeed * Time.deltaTime));
        objRenderer.SetPropertyBlock(propBlock);
    }

    void OnDisable()
    {
        // Reset the color and emission to original when the script is disabled
        objRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_EmissionColor", originalEmissionColor);
        propBlock.SetColor("_Color", originalColor);
        objRenderer.SetPropertyBlock(propBlock);
    }
}
