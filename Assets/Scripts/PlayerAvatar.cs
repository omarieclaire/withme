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
        Collider playerCollider = GetComponent<Collider>();
        Rigidbody playerRigidbody = GetComponent<Rigidbody>();

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

    public virtual void OnDrawGizmos()
    {
        if (gameObject.activeSelf)
        {
            // Draw wire sphere to show collision area
            Collider col = GetComponent<Collider>();
            if (col != null && col is SphereCollider)
            {
                Gizmos.color = Color.yellow;
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawWireSphere(transform.position, sphere.radius * transform.lossyScale.x);
            }
        }
    }


    public virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PlayerAvatar] OnTriggerEnter - Player {id} triggered by {other.gameObject.name}");
        HandleCollision(other);
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[PlayerAvatar] OnCollisionEnter - Player {id} collided with {collision.gameObject.name}");
        HandleCollision(collision.collider);
    }

    private void HandleCollision(Collider other)
    {
        bool isDot = other.CompareTag("Dot");
        PlayerAvatar otherPlayer = other.GetComponent<PlayerAvatar>();

        // Handle both types of collisions independently
        if (isDot)
        {
            Debug.Log($"[PlayerAvatar] Player {id} collided with dot");
            controller.OnPlayerCollideWithDot(this, other.gameObject);
        }

        if (otherPlayer != null)
        {
            Debug.Log($"[PlayerAvatar] Player {id} collided with Player {otherPlayer.id}");
            Debug.Log($"[PlayerAvatar] Dots collected - Player {id}: {numDotsCollected}, Player {otherPlayer.id}: {otherPlayer.numDotsCollected}");
            controller.OnPlayersCollided(this, otherPlayer);
        }
    }

    public virtual void Update()
    {
        if (gameObject.activeSelf && audioSource != null && audioSource.mute)
        {
            audioSource.mute = false;
        }

        UpdatePlayerColor();

        transform.LookAt(cameraAndPlayAreaSettings.center);

        // Collision detection between players
        var activePlayers = controller.playerSetupManager.GetActivePlayers();

        foreach (var otherPlayerInfo in activePlayers)
        {
            var otherPlayerObject = otherPlayerInfo.PlayerObject;

            // Ensure we're not comparing this object with itself
            if (otherPlayerObject != this.gameObject)
            {
                float distance = Vector3.Distance(otherPlayerObject.transform.position, transform.position);

                // Adjust for the radii of both players
                float thisRadius = transform.localScale.x / 2;
                float otherRadius = otherPlayerObject.transform.localScale.x / 2;
                distance -= thisRadius + otherRadius;

                if (distance < collisionThreshold)
                {
                    var otherPlayerAvatar = otherPlayerInfo.Avatar;
                    controller.OnPlayersCollided(this, otherPlayerAvatar);
                }
            }
        }
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
