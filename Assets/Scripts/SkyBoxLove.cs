using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [Header("Skybox Info")]
    [Tooltip("Skybox material to control the visual aspects.")]
    public Material skyboxMaterial;

    [Tooltip("Sun position for skybox. Y IS NEGATIVE SORRY")]
    public Vector3 SunPosition = new Vector3(0, -1, 0);

    public float SunHue = 0;
    public float sunIntensity = 1;
    public float sunHueSize = .1f;
    public float auroraIntensity = 1;
    public float auroraSpeed = 1;
    public float auroraHueStart = 0;
    public float auroraHueSize = 1;
    public float auroraHorizonImportance = .1f;
    public float auroraNoiseSize = 1;
    public float auroraVibrantness = 1;

    void Start()
    {
        UpdateSkybox();
    }

    // Method to update the skybox settings based on current values
    public void UpdateSkybox()
    {
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetVector("_LightDir", SunPosition.normalized);
            skyboxMaterial.SetFloat("_SunIntensity", sunIntensity);
            skyboxMaterial.SetFloat("_SunHue", SunHue);
            skyboxMaterial.SetFloat("_SunHueSize", sunHueSize);
            skyboxMaterial.SetFloat("_AuroraIntensity", auroraIntensity);
            skyboxMaterial.SetFloat("_AuroraSpeed", auroraSpeed);
            skyboxMaterial.SetFloat("_AuroraHueStart", auroraHueStart);
            skyboxMaterial.SetFloat("_AuroraHueSize", auroraHueSize);
            skyboxMaterial.SetFloat("_AuroraHorizonImportance", auroraHorizonImportance);
            skyboxMaterial.SetFloat("_AuroraNoiseSize", auroraNoiseSize);
            skyboxMaterial.SetFloat("_AuroraVibrantness", auroraVibrantness);
        }
    }
}
