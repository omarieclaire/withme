// Game Initialization (SetUp):

// SetUp creates a series of "dots" (collectibles) in random positions within the dome space. The number of dots is determined by numDots.
// Each dot is instantiated from the dotPrefab and stored in two lists: dots (for their transforms) and dotAvatars (for their data and behaviors).
// A dot regeneration cycle is started using the BlueMoonDotRegenerationRoutine coroutine, which regenerates a specified number of dots (dotsToRegenerate) at intervals defined by dotRegenerationInterval.

// Dot Collection Mechanic:

// When a player collides with a dot, the OnPlayerTrigger method is triggered. The code checks if the collider is a dot, and if the player hasn't reached their maximum dot limit (maxDotsPerPlayer) and the dot hasn’t already been collected.
// If valid, the dot is marked as collected, attached to the player, and the player grows by an amount (sizeIncrementOnCollect). A sound is played for the collection event, and a particle effect is triggered at the dot’s position. Additionally, an OSC message is sent to trigger the sound in external systems.

// Player Collision Mechanic:

// When two players collide, the OnPlayersWithDotsCollided method is called. If both players have collected enough dots (determined by minNumDotsForCollision), the collected dots are released and no longer attached to the players. An explosion sound is played, and a particle effect is triggered between the two players.
// The players are reset to their initial state, and the game proceeds.
// Dot Regeneration:

// Every dotRegenerationInterval seconds, the coroutine BlueMoonDotRegenerationRoutine triggers the creation of new dots using MakeNewDotsOnceInABlueMoon. This process adds additional dots to the game for players to collect, ensuring the game continues with a steady flow of collectibles.
// Game Completion:

// As dots are collected and brought to the "tree" (presumably a central goal in the game), the OnTreeCollect method is called. The progress toward game completion is tracked by the number of dots collected (totalDotsCollected).
// When enough dots have been collected (totalDotsCollectedForCompletion), the OnLevelComplete method is triggered, which plays a sound, shows the flowers (or equivalent win visual), and starts a fade-to-black effect after a short delay.
// Audio and Visual Feedback:

// ToDo:
// Fade to black on level completion: The FadeToBlack coroutine is not fully set up, and there’s a comment indicating that it was intended to be completed. It fades the screen to black over time, but the logic may need refinement.
// Regenerating the earth (TODO): There's a placeholder comment about regenerating the earth, suggesting that a key mechanic might still be under development.
// Overall Behavior:
// Players collect dots, which grow their size. When players collide, their collected dots are released back into the world (or regenerated). The game continues until enough dots are collected to trigger a win state, indicated by the flowers blooming and a fade-out sequence. Regular dot regeneration ensures the game remains playable even as dots are consumed.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FantasyTree;
using UnityEngine;
using UnityEngine.UI;


public class DotGameController : Controller
{

    public Transform tree;

    [Header("Collectable Info")]
    [Tooltip("Number of dots in the game.")]
    public int numDots;

    public int inty = 2;


    [Tooltip("Prefab used to instantiate dots.")]
    public GameObject dotPrefab;

    [Tooltip("Actual size of the dot.")]
    public float dotSize;

    [Tooltip("Lower value spawns near the top of the dome, a higher value towards the ring of the dome.")]
    public float dotFlatnessPower;

    [Tooltip("List of transforms for each dot in the game.")]
    public List<Transform> dots;

    [Tooltip("List of Dot objects representing the dot avatars.")]
    public List<Dot> dotAvatars;

    [Tooltip("Transform that holds all the dot objects.")]
    public Transform dotHolder;
    [Tooltip("How slowly the dot moves to the new position, after being in the tree or at the beginning of its life.")]
    public float dotForceTowardsPosition;

    [Tooltip("Dampening factor for the dot's movement.")]
    public float dotDampening;

    [Header("Game Info")]
    [Tooltip("Minimum number of dots required for the possibility of a player collision.")]
    public int minNumDotsForCollision;

    [Tooltip("Maximum number of dots a player can collect.")]
    public int maxDotsPerPlayer;

    [Tooltip("Size increment of the player when a dot is collected.")]
    public float sizeIncrementOnCollect;

    [Header("Dot Regeneration Settings")]
    [Tooltip("Time in seconds between each dot regeneration cycle.")]
    public float dotRegenerationInterval = 30f;

    [Tooltip("Number of dots to regenerate in each cycle.")]
    public int dotsToRegenerate = 5;

    private float dotRegenerationTimer;

    [Header("Audio Info")]

    [Tooltip("Sound played when a dot is collected.")]
    public AudioClip onDotCollectClip;

    [Tooltip("Sound played when players collide.")]
    public AudioClip onExplodeClip;

    [Tooltip("Sound played when the flowers bloom.")]
    public AudioClip onLevelCompleteClip;

    public AudioPlayer audioPlayer;

    public ParticleSystem explosionParticles;
    public ParticleSystem playerCollectDotParticleSystem;


    [Tooltip("Number of dots that have been brought to the tree.")]
    public int totalDotsCollected;

    [Tooltip("Number of dots that need to reach the tree before we turn on the flowers / game over! Win!")]
    public int totalDotsCollectedForCompletion;


    [Tooltip("Force from the dots towards the player who collected them")]
    public float dotForceTowardsCollector;
    [Tooltip("Force from the dots towards the tree")]
    public float dotForceTowardsTree;

    public ControlTreeMaterialValues controlTreeMaterialValues;


    [Tooltip("Image used to fade the screen to black.")]
    public RawImage fadeImage;

    private bool isLevelComplete = false;


    private void CreateDot(int id)
    {
        // Instantiate dot at a random position within the dome
        GameObject dot = Instantiate(dotPrefab, Vector3.zero, Quaternion.identity);

        Vector3 randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 2f), Random.Range(-1f, 1f));
        randomPos = getFinalPosition(randomPos);

        Dot dotAvatar = dot.GetComponent<Dot>();
        if (dotAvatar == null)
        {
            Debug.LogError("Dot component missing from dot prefab!");
            return;
        }

        // Initialize dot properties
        dotAvatar.controller = this;
        dotAvatar.id = id;
        dotAvatar.originalPosition = randomPos;
        dotAvatar.velocity = Vector3.zero;
        dotAvatar.targetPosition = randomPos;
        dotAvatar.collected = false;
        dotAvatar.SetData();

        // Set dot transform properties
        dot.transform.position = randomPos;
        dot.transform.SetParent(dotHolder);
        dot.transform.localScale = Vector3.one * dotSize;

        dot.SetActive(true); // Activate dot

        dots.Add(dot.transform); // Add to dots list
        dotAvatars.Add(dotAvatar); // Add to dotAvatars list
    }

    public override void SetUp()


    {
        Debug.Log("[INFO] DotGameController SetUp called.");

        // First, run the common setup logic
        base.SetUp();

        // Then, run the scene-specific setup logic for the dots
        Debug.Log("[INFO] DotGameController specific setup called.");

        // Initialize dots list
        dots = new List<Transform>();
        dotAvatars = new List<Dot>();

        for (int i = 0; i < numDots; i++)
        {
            CreateDot(i);
        }

        dotRegenerationTimer = dotRegenerationInterval;
        StartCoroutine(BlueMoonDotRegenerationRoutine());

        Debug.Log("[INFO] DotGameController setup completed.");
    }


    void Start()
    {
        SetUp();  // This ensures SetUp is called when the scene starts
    }

    public override Vector3 GetScale(int i)
    {
        float fScale = (startSize + (float)playerAvatars[i].numDotsCollected * sizeIncrementOnCollect) * playerSeenScaler[i];
        return Vector3.one * fScale;
    }

    public override void OnPlayersWithDotsCollided(PlayerAvatar p1, PlayerAvatar p2)
    {

        // Dont do it if they dont have enought
        if (p1.numDotsCollected < minNumDotsForCollision || p2.numDotsCollected < minNumDotsForCollision)
        {
            return;
        }

        for (int i = 0; i < dots.Count; i++)
        {
            if (dotAvatars[i].collector == p1.transform || dotAvatars[i].collector == p2.transform)
            {
                dotAvatars[i].collected = false;
                dotAvatars[i].collector = null;
                dotAvatars[i].OnPlayersHoldingMeCollided();
            }
        }

        p1.Reset();
        p2.Reset();

        PlayCollisionSound(p1);
        // PlayCollisionSound(p2);

        explosionParticles.transform.position = (p1.transform.position + p2.transform.position) / 2;
        explosionParticles.Play();
    }

    private void PlayCollisionSound(PlayerAvatar player)
    {
        
        // audioPlayer.Play(onExplodeClip); // <-- old sound keep for debugging
        // Construct the sound ID and get the player's position
        string soundID = $"p{player.id}EffectsWithMePlayerCollision";
        Vector3 pointPosition = player.transform.position;
        soundEventSender.SendOneShotSound(soundID, pointPosition);
        Debug.Log($"Sent collision sound for player {player.id} at position {pointPosition}.");
    }

    public override void OnPlayerTrigger(PlayerAvatar player, GameObject collider)
    {
        // Check if the collider is a dot
        if (collider.CompareTag("Dot"))
        {

            int index = dots.IndexOf(collider.transform);
            if (index != -1)
            {

                // Dont collect if we have reached the max
                if (player.numDotsCollected > maxDotsPerPlayer)
                {
                    return;
                }

                // Dont Recollect
                if (dots[index].GetComponent<Dot>().collected)
                {
                    return;
                }

                dotAvatars[index].collected = true;
                dotAvatars[index].collector = player.transform;
                player.OnDotCollect(player.numDotsCollected >= minNumDotsForCollision, player.numDotsCollected >= maxDotsPerPlayer);

                // audioPlayer.Play(onDotCollectClip); // <-- old sound keep for debugging




                string soundID = $"p{player.id}EffectsWithMePointCollision";
                Vector3 pointPosition = player.transform.position;
                soundEventSender.SendOneShotSound(soundID, pointPosition);

                playerCollectDotParticleSystem.transform.position = collider.transform.position;
                playerCollectDotParticleSystem.Play();
                Debug.Log($"Sending OSC message to play point sound: {soundID}");

                // dots[index].gameObject.SetActive(false);
                // dots[index].position = dotOriginalPositions[index];
                // dots[index].gameObject.SetActive(true);
            }
        }
        else
        {
            // print("NOT A DOT");
            // print("COLLIDER TAG: " + collider.tag);
        }
    }

    private IEnumerator BlueMoonDotRegenerationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(dotRegenerationInterval);
            MakeNewDotsOnceInABlueMoon();
        }
    }

    private void MakeNewDotsOnceInABlueMoon()
    {
        Debug.Log("Creating new dots...");

        int currentDotCount = dots.Count;
        for (int i = 0; i < dotsToRegenerate; i++)
        {
            CreateDot(currentDotCount + i);
        }

        Debug.Log($"{dotsToRegenerate} new dots created.");
    }




    public void OnTreeCollect()
    {
        totalDotsCollected++;
        controlTreeMaterialValues.barkShown = (float)totalDotsCollected / (float)totalDotsCollectedForCompletion;

        if (totalDotsCollected >= totalDotsCollectedForCompletion)
        {
            OnLevelComplete();
        }

    }
    public void OnLevelComplete()
    {
        if (isLevelComplete) return; // Ensure this runs only once
        isLevelComplete = true;

        print("LEVEL COMPLETE");
        // audioPlayer.Play(onLevelCompleteClip);
        controlTreeMaterialValues.flowersShown = 1;


        // Start coroutine to fade to black after the sound finishes - I started setting this up but didn't finish!
        StartCoroutine(FadeToBlack(onLevelCompleteClip.length));
    }


    //  I started setting this up but didn't finish!
    private IEnumerator FadeToBlack(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the sound to finish

        float fadeDuration = 2f; // Duration for the fade effect
        Color color = fadeImage.color;
        float startAlpha = color.a;
        float endAlpha = 1f;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            fadeImage.color = color;
            yield return null;
        }

        color.a = Mathf.Lerp(startAlpha, endAlpha, 1f);
        fadeImage.color = color;

    }

}