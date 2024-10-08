using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugFace : MonoBehaviour
{
    [Header("References")]
    public NoGoZoneManager noGoZoneManager;
    public Controller controller;
    public Hug hug;
    public SoundEventSender soundEventSender;
    public AudioPlayer audioPlayer;

    [Header("Face States")]
    public GameObject preDiscovered; // Represents the default state of the HugFace before interaction
    public GameObject discovered;    // Represents the state when the HugFace is discovered/flipped
    public GameObject finished;      // Represents the state when the HugFace interaction is complete

    [Header("Effects")]
    public ParticleSystem matchParticlesPrefab; // Prefab for the particle effects when a match happens
    private ParticleSystem instantiatedParticles; // Holds the instantiated particle system

    [Header("Audio")]
    public AudioClip HugFaceSongSoundClip;   // Sound clip to play when a HugFace is discovered
    public AudioClip HugFaceFlipSoundClip;   // Sound clip to play when a HugFace is flipped
    public List<AudioClip> HugFaceSighClips; // List of sigh sound clips that may play when a match happens

    [Header("Properties")]
    public List<HugFace> partners; // List of partner HugFaces for matching
    public int smileID;            // ID associated with this HugFace's smile
    public Color color;            // Color of the HugFace
    public bool fullComplete;      // Flag indicating if the HugFace has been fully completed

    private bool wasFlipped = false; // Flag indicating if this HugFace has been flipped before
    private bool soundsPlayed = false; // Flag indicating if sounds have been played for this HugFace
    private int state; // Tracks the current state of the HugFace (initialized at 0)
    private bool inside; // Tracks whether the player is inside the HugFace's interaction radius
    private bool flipped; // Tracks whether this HugFace has been flipped
    private Renderer faceRenderer; // Renderer for the HugFace

    private void Start()
    {
        OnEnable(); // Calls OnEnable to set initial states when the object is first created
        SetupParticles(); // Prepares the particle system for this HugFace
    }

    public void OnEnable()
    {
        ResetFaceState(); // Resets this HugFace to its initial state (not discovered)
        ApplyTextures();  // Applies appropriate textures to different states of the HugFace
    }

    public void OnDisable()
    {
        SetAllStatesInactive(); // Deactivates all visual states of the HugFace when disabled
    }

    public void WhileInside(PlayerAvatar player, HugFace face)
    {
        if (fullComplete) return; // If this HugFace is already completed, exit the method

        if (!wasFlipped && !flipped)
        {
            HandleFirstFlip(player); // Handles flipping for the first time when the player interacts
        }

        UpdateVisualState(); // Updates the visual state of the HugFace to reflect interaction
        CheckForFullHug(player); // Checks if the full hug (matching) is complete
    }

    public void Update()
    {
        transform.LookAt(Vector3.zero); // Keeps the HugFace looking towards the center of the dome

        if (fullComplete)
        {
            WhileFinished(); // If fully completed, perform the "finished" behavior
        }

        if (flipped && preDiscovered.activeSelf)
        {
            WhileOutside(); // If flipped and not interacted with anymore, reset to neutral
        }
    }

    public void OnFullComplete()
    {
        // Optional behavior when HugFace is fully completed (like triggering events, etc.)
    }

    public void WhileOutside()
    {
        ResetToNeutralState(); // Resets the HugFace to its pre-discovered state if the player moves away
    }

    public void WhileFinished()
    {
        SetFinishedStateActive(); // Updates visuals to "finished" state
        MoveToTopOfDome();        // Moves the HugFace to the top of the dome once it's completed
    }

    public void ApplyColor(Color newColor)
    {
        if (faceRenderer != null && faceRenderer.material != null)
        {
            faceRenderer.material.color = newColor; // Changes the color of the HugFace
        }
    }

    private void SetupParticles()
    {
        if (matchParticlesPrefab != null && instantiatedParticles == null)
        {
            // Instantiates the match particle system and ensures it's not playing by default
            instantiatedParticles = Instantiate(matchParticlesPrefab, transform.position, Quaternion.identity, transform);
            instantiatedParticles.Stop();
        }
    }

    public void PlayMatchParticles()
    {
        if (instantiatedParticles != null)
        {
            instantiatedParticles.Play(); // Plays the match particle effect
        }
        else
        {
            Debug.LogWarning("Match particles not set up for " + gameObject.name); // Warns if particles are missing
        }
    }

    private void ResetFaceState()
    {
        state = 0; // Resets the state to the initial value
        flipped = false; // Indicates the HugFace is not flipped
        preDiscovered.SetActive(true); // Sets the pre-discovered state to active
        discovered.SetActive(false);   // Ensures the discovered state is inactive
        finished.SetActive(false);     // Ensures the finished state is inactive
    }

    private void ApplyTextures()
    {
        // Applies textures and colors to the HugFace based on its state
        ApplyTextureToState(preDiscovered, hug.pensNeutralFace, Color.white);
        ApplyTextureToState(discovered, GetDiscoveredTexture(), color);
        ApplyTextureToState(finished, hug.pensPostHugFace, color);
    }

    private void ApplyTextureToState(GameObject stateObject, Texture texture, Color stateColor)
    {
        MeshRenderer meshRenderer = stateObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.SetTexture("_MainTex", texture); // Sets the texture for the current state
            meshRenderer.material.SetColor("_Color", stateColor);  // Sets the color for the current state
        }
    }

    private Texture GetDiscoveredTexture()
    {
        // Retrieves the texture for the discovered state based on the smileID
        int textureCount = hug.pensTrueFaces.Count;
        return (smileID >= 0 && smileID < textureCount) ? hug.pensTrueFaces[smileID] : null;
    }

    private void SetAllStatesInactive()
    {
        // Deactivates all visual states of the HugFace
        preDiscovered.SetActive(false);
        discovered.SetActive(false);
        finished.SetActive(false);
    }

    private void PlayHugSound(PlayerAvatar player, string soundType)
    {
        // Plays a sound when a certain HugFace event occurs, like flipping or matching
        string soundID = $"p{player.id}EffectsHugFace{soundType}";
        Vector3 pointPosition = player.transform.position;
        Debug.Log($"Step 1: Playing hug {soundType.ToLower()} sound: {soundID} at {pointPosition}");
        soundEventSender.SendOneShotSound(soundID, pointPosition); // Sends OSC message to play sound
    }

    private void HandleFirstFlip(PlayerAvatar player)
    {
        // Plays sounds and updates the flipped state when the HugFace is flipped for the first time
        if (!soundsPlayed)
        {
            if (Controller.enableOldSoundSystem != null)
            {
                audioPlayer.Play(HugFaceFlipSoundClip);
                audioPlayer.Play(HugFaceSongSoundClip);
                soundsPlayed = true;
            }
            if (Controller.enableNewSoundSystem)
            {
                PlayHugSound(player, "SongSound");
                soundsPlayed = true;
            }
        }
        flipped = true;  // Sets flipped state to true
        wasFlipped = true; // Marks that the HugFace has been flipped
        inside = true; // Sets the interaction as happening inside
    }

    private void UpdateVisualState()
    {
        // Updates the visual state to discovered, marking the HugFace as flipped
        discovered.SetActive(true);
        preDiscovered.SetActive(false);
        finished.SetActive(false);
    }

    private void CheckForFullHug(PlayerAvatar player)
    {
        // Checks if all partner HugFaces are inside the interaction range, completing the Hug
        if (partners.TrueForAll(partner => partner.inside))
        {
            if (Controller.enableOldSoundSystem)
            {
                if (HugFaceSighClips.Count > 0)
                {
                    int randomIndex = Random.Range(0, HugFaceSighClips.Count);
                    AudioClip selectedSighClip = HugFaceSighClips[randomIndex];
                    Debug.Log($"Playing random sigh clip: {selectedSighClip.name}");
                    audioPlayer.Play(selectedSighClip);
                }
                else
                {
                    Debug.LogWarning("No sigh clips available to play.");
                }
            }
            if (Controller.enableNewSoundSystem)
            {
                // PlayHugSound(player, "Sigh");
            }

            hug.HUG(this, smileID); // Calls the HUG method in Hug to mark this face as fully complete
            partners.ForEach(partner => hug.HUG(partner, partner.smileID)); // Completes the partner faces as well
        }
    }

    private void ResetToNeutralState()
    {
        // Resets the HugFace to the pre-discovered state
        discovered.SetActive(false);
        preDiscovered.SetActive(true);
        finished.SetActive(false);
        inside = false;

        if (flipped)
        {
            flipped = false;
            wasFlipped = false;
            soundsPlayed = false; // Resets sound flags for future interactions
        }
    }

    private void SetFinishedStateActive()
    {
        // Sets the HugFace to its final, completed state
        discovered.SetActive(false);
        preDiscovered.SetActive(false);
        finished.SetActive(true);
    }

    private void MoveToTopOfDome()
    {
        // Moves the HugFace to the top of the dome after completion
        float currentX = transform.position.x;
        float currentZ = transform.position.z;

        float threshold = 0.3f; // Define a threshold for movement

        if (Mathf.Abs(currentX) < threshold && Mathf.Abs(currentZ) < threshold)
        {
            return; // Stops moving if near the center
        }

        Vector3 topOfDomePosition = new Vector3(0, 3.3f, 0); // Position at the top of the dome
        float offsetRange = 0.01f;
        Vector3 randomOffset = new Vector3(Random.Range(-offsetRange, offsetRange), 0, Random.Range(-offsetRange, offsetRange));
        Vector3 finalPosition = topOfDomePosition + randomOffset;

        // Gradually moves the HugFace towards the top of the dome
        transform.position = Vector3.Lerp(transform.position, finalPosition, hug.howFastWeHug);
        transform.LookAt(Vector3.zero); // Keeps the HugFace facing the center of the dome
    }
}
