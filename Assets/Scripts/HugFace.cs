using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugFace : MonoBehaviour
{
    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager

    public Controller controller;

    public Hug hug;
    public List<HugFace> partners;

    private bool wasFlipped = false;  // Tracks the previous state of the face

    public int smileID;
    public int state;

    public AudioClip hugFaceSong;  // Unique song for this HugFace pair

    public SoundEventSender soundEventSender;

    public AudioPlayer audioPlayer;
    public AudioClip onFlipClip;
    public AudioClip SongSoundClip;
    public AudioClip SighClip;

    public GameObject preDiscovered;
    public GameObject discovered;
    public GameObject finished;

    public bool fullComplete;

    public Color color;

    private Renderer renderer;

    public bool inside;
    public bool flipped;

    public LineRenderer lineRenderer;

    // Ensure sounds only play once
    private bool soundsPlayed = false;

    void Start()
    {
        // Debug.Log($"{name} - Start: Initialized with flipped = {flipped}, wasFlipped = {wasFlipped}, soundsPlayed = {soundsPlayed}");
        OnEnable();
    }

    public void OnEnable()
    {
        state = 0;
        flipped = false;    // Reset flip state when enabled
        // Reset sound playback flag only when the face flips back to its original state
        // Debug.Log($"{name} - OnEnable: State reset: flipped = {flipped}, wasFlipped = {wasFlipped}");

        preDiscovered.SetActive(true);  // Set the neutral texture state as active
        discovered.SetActive(false);    // Hide the discovered state (flipped state)
        finished.SetActive(false);      // Hide the finished state

        // Apply textures and colors to various face states
        preDiscovered.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
        discovered.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        finished.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

        preDiscovered.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", hug.pensNeutralFace);
        discovered.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", hug.pensTrueFaces[smileID]);
        finished.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", hug.pensPostHugFace);

        // Debug.Log($"{name} - OnEnable: Textures and colors set for different face states.");
    }

    public void OnDisable()
    {
        preDiscovered.SetActive(false);  // Hide all face states when disabled
        discovered.SetActive(false);
        finished.SetActive(false);
        // Debug.Log($"{name} - OnDisable: All face states set to inactive.");
    }

    public void WhileInside(PlayerAvatar player, HugFace face)


    {

        Debug.Log($"xxx Player ID: {player.id}, Face Name: {face.name}");

        // If the HugFace is fully completed, do nothing further
        if (fullComplete)
        {
            // Debug.Log($"{name} - Full complete, skipping interaction.");
            return;
        }

        // Check if the face is flipped for the first time
        if (!wasFlipped && !flipped)
        {
            // Debug.Log($"{name} - Player entered the hug zone for the first time, flipping the face.");

            // Play the sound for the first flip if it hasn't been played yet
            if (!soundsPlayed)
            {
                if (Controller.enableNewSoundSystem)
                {
                    // PlayHugSound(player, "FlipSound");
                    // PlayHugSound(player, "SongSound");
                }

                if (Controller.enableOldSoundSystem)
                {
                    // Debug.Log($"{name} - Playing unique sound for this HugFace.");
                    if (hugFaceSong != null)
                    {
                        // audioPlayer.Play(hugFaceSong);  // Play the unique song for this HugFace pair
                        soundsPlayed = true;  // Mark that the sound has been played
                        // Debug.Log($"{name} - Sound played, soundsPlayed = {soundsPlayed}.");
                    }
                }
            }

            // Mark the face as flipped
            flipped = true;
            wasFlipped = true;  // Record that the face was flipped
            inside = true;  // Mark the player as inside the hug zone
        }
        else
        {
            // Debug.Log($"{name} - Face already flipped, no sound or additional actions needed.");
        }

        // Visual update while the player is inside the hug zone
        discovered.SetActive(true);  // Show the "flipped" state
        preDiscovered.SetActive(false);  // Hide the neutral state
        finished.SetActive(false);  // Hide the finished state

        // Check if all partners are inside the hug zone as well
        bool allInside = true;
        for (int i = 0; i < partners.Count; i++)
        {
            if (!partners[i].inside)
            {
                allInside = false;
                break;
            }
        }

        // If all partners are inside, trigger the full hug interaction
        if (allInside)
        {
            // Debug.Log("hug: all HugFaces are inside the hug zone.");
            if (Controller.enableNewSoundSystem)
            {
                PlayHugSound(player, "Sigh");
            }

            if (Controller.enableOldSoundSystem)
            {
                if (SighClip != null)
                {
                    audioPlayer.Play(SighClip);
                }
            }

            hug.HUG(this, smileID);  // Complete the hug

            // Call HUG on each partner
            for (int i = 0; i < partners.Count; i++)
            {
                hug.HUG(partners[i], partners[i].smileID);  // Complete the hug for each partner
            }
        }
    }

    private void PlayHugSound(PlayerAvatar player, string soundType)
    {
        string soundID = $"p{player.id}EffectsHugFace{soundType}";
        Vector3 pointPosition = player.transform.position;
        // Debug.Log($"Playing hug {soundType} sound: {soundID} at {pointPosition}");
        soundEventSender.SendOneShotSound(soundID, pointPosition);
    }

    public void Update()
    {
        // Keep the HugFace looking at the center of the scene (can be adjusted if needed)
        transform.LookAt(Vector3.zero);

        // If the face has been fully completed, perform the finished logic
        if (fullComplete)
        {
            WhileFinished();
        }

        // Trigger sound reset logic when the face visually flips back to its neutral state
        if (flipped && preDiscovered.activeSelf)
        {
            WhileOutside();  // Trigger reset after flip-back
        }
    }

    public void OnFullComplete()
    {
        // Debug.Log($"{name} - Full completion of HugFace.");
    }

    public void WhileOutside()
    {
        // Reset the face to its neutral state
        discovered.SetActive(false);
        preDiscovered.SetActive(true);
        finished.SetActive(false);
        inside = false;

        // Reset the flip and sound state only after the face flips back to neutral
        if (flipped)
        {
            flipped = false;
            wasFlipped = false;  // Allow the face to be flipped again in the future
            soundsPlayed = false;  // Reset sound state to allow replay
            // Debug.Log($"{name} - Face flipped back to neutral, sounds reset. flipped = {flipped}, soundsPlayed = {soundsPlayed}.");
        }
    }

    // public void WhileFinished()
    // {
    //     // Show the finished state visually
    //     discovered.SetActive(false);
    //     preDiscovered.SetActive(false);
    //     finished.SetActive(true);

    //     // Calculate an average position based on all partners to adjust the final position
    //     Vector3 averagePosition = Vector3.zero;
    //     int totalCounted = 0;
    //     for (int i = 0; i < partners.Count; i++)
    //     {
    //         if (partners[i].transform.position != transform.position)
    //         {
    //             totalCounted++;
    //             averagePosition += partners[i].transform.position;
    //         }
    //     }

    //     // Smoothly move to the average position
    //     if (totalCounted > 0)
    //     {
    //         averagePosition /= totalCounted;
    //         transform.position = Vector3.Lerp(transform.position, averagePosition, hug.howFastWeHug);
    //         // Debug.Log($"{name} - Moving to average position: {averagePosition}");
    //     }
    // }

 public void WhileFinished()
{
    // Show the finished state visually
    discovered.SetActive(false);
    preDiscovered.SetActive(false);
    finished.SetActive(true);

    // Define the top of the dome (you can adjust the y-value based on your dome's height)
    Vector3 topOfDomePosition = new Vector3(0, 1.3f, 0);  // Assuming the top is at (0, 1, 0). Adjust the y-value based on dome size.

    // Add a small random offset to the final position to avoid overlap
    float offsetRange = 0.01f;  // Adjust the range of the offset as needed
    Vector3 randomOffset = new Vector3(Random.Range(-offsetRange, offsetRange), 0, Random.Range(-offsetRange, offsetRange));

    // Apply the random offset to the top position
    Vector3 finalPosition = topOfDomePosition + randomOffset;

    // Smoothly move the face towards the final position with the offset
    transform.position = Vector3.Lerp(transform.position, finalPosition, hug.howFastWeHug);

    // Optionally, make the face look at the center of the dome during the movement
    transform.LookAt(Vector3.zero);

    // Debug.Log($"{name} - Moving to top of the dome at position: {finalPosition}");
}


    private void ApplyTexture(Texture texture)
    {
        if (renderer != null && renderer.material != null)
        {
            renderer.material.mainTexture = texture;
            // Debug.Log($"{name} - Applied texture {texture.name} to the face.");
        }
    }

    public void ApplyColor(Color color) // Change to public
    {
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = color;
            // Debug.Log($"{name} - Applied color {color} to the face.");
        }
    }
}
