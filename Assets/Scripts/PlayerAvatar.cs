using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerAvatar : MonoBehaviour
{
    public CameraAndPlayAreaSettings cameraAndPlayAreaSettings;
    public PlayerSetupManager playerSetupManager;

    public Controller controller; 
    public TextMeshPro text;
    public int id;
    public int numDotsCollected;

    public Renderer regularRing;
    public Renderer chargedRing;
    public Renderer maxRing;

    public Material regularRingMaterial;
    public Material chargedRingMaterial;
    public Material maxRingMaterial;

    public Color color;
    public bool showPlayerName;

    public float colorRotationSpeed = 0.1f;
    public float hueRangeStart = 0.25f;
    public float hueRangeEnd = 0.75f;
    public float colorSaturation = 0.8f;
    public float colorValue = 1f;
    public float initialHueOffset = 0.5f;
    public float collisionThreshold = 0.1f;

    public GameObject playerModel;
    public bool usePlayerModel = true;
    public Color originalColor;
    public bool useRainbowColorLogic = false;

    public AudioSource audioSource;
    public AudioClip withMeP0Clip, withMeP1Clip, withMeP2Clip, withMeP3Clip, withMeP4Clip, withMeP5Clip, withMeP6Clip, withMeP7Clip, withMeP8Clip, withMeP9Clip;

    public virtual void Start()
    {
        if (audioSource != null && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "WithMe")
        {
            AssignWithMeClip(id);
            audioSource.loop = true;
        }
    }

    private void AssignWithMeClip(int playerId)
    {
        switch (playerId)
        {
            case 0: audioSource.clip = withMeP0Clip; break;
            case 1: audioSource.clip = withMeP1Clip; break;
            case 2: audioSource.clip = withMeP2Clip; break;
            case 3: audioSource.clip = withMeP3Clip; break;
            case 4: audioSource.clip = withMeP4Clip; break;
            case 5: audioSource.clip = withMeP5Clip; break;
            case 6: audioSource.clip = withMeP6Clip; break;
            case 7: audioSource.clip = withMeP7Clip; break;
            case 8: audioSource.clip = withMeP8Clip; break;
            case 9: audioSource.clip = withMeP9Clip; break;
            default:
                Debug.LogWarning("No audio clip assigned for player ID: " + playerId);
                break;
        }
    }

    public virtual void SetData(string name)
    {
        int playerCount = 1; // simplified since we're not using indexing for colors anymore

        if (useRainbowColorLogic)
        {
            float initialHue;
            if (id == 0)
            {
                initialHue = Mathf.Repeat(0.1f + initialHueOffset, 1f);
            }
            else
            {
                initialHue = Random.Range(0f, 1f);
            }
            color = Color.HSVToRGB(initialHue, colorSaturation, colorValue);
        }
        else
        {
            color = Color.HSVToRGB(5.1f + initialHueOffset, colorSaturation, colorValue); 
        }

        if (color == Color.black || color.grayscale < 0.1f)
        {
            color = Color.HSVToRGB(0.1f, colorSaturation, colorValue);
        }

        originalColor = color;

        ApplyMaterialsToRings();
        UpdatePlayerColor();

        if (playerModel != null)
        {
            playerModel.SetActive(usePlayerModel);
        }
    }

    public virtual void ApplyMaterialsToRings()
    {
        if (regularRing != null && regularRingMaterial != null)
        {
            regularRing.material = new Material(regularRingMaterial);
        }

        if (chargedRing != null && chargedRingMaterial != null)
        {
            chargedRing.material = new Material(chargedRingMaterial);
        }

        if (maxRing != null && maxRingMaterial != null)
        {
            maxRing.material = new Material(maxRingMaterial);
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
        // Scale handled elsewhere now.
    }

    public virtual void Reset()
    {
        numDotsCollected = 0;
        if (maxRing != null) maxRing.enabled = false;
        if (chargedRing != null) chargedRing.enabled = false;
        if (regularRing != null) regularRing.enabled = true;

        transform.localScale = Vector3.one * playerSetupManager.startSize;

        if (audioSource != null)
        {
            audioSource.mute = true;
        }
    }

    public void Reactivate()
    {
        if (audioSource != null)
        {
            audioSource.mute = false;
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

        UpdatePlayerColor();

        transform.LookAt(cameraAndPlayAreaSettings.center);
    }

    public virtual void UpdatePlayerColor()
    {
        if (regularRing != null)
        {
            regularRing.material.color = color;
        }
        if (chargedRing != null)
        {
            chargedRing.material.color = Color.Lerp(color, Color.white, 0.5f); 
        }
        if (maxRing != null)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 1f);
            maxRing.material.color = Color.Lerp(color, Color.yellow, pulse);
        }
        if (text != null)
        {
            text.color = color;
        }
    }
}
