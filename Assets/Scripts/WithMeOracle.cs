// Game Initialization (SetUp):

// SetUp creates a series of "dots" (collectibles) in random positions within the dome space. The number of dots is determined by numDots.
// Each dot is instantiated from the dotPrefab and stored in two lists: dots (for their transforms) and dotAvatars (for their data and behaviors).
// A dot regeneration cycle is started using the BlueMoonDotRegenerationRoutine coroutine, which regenerates a specified number of dots (dotsToRegenerate) at intervals defined by dotRegenerationInterval.

// Dot Collection Mechanic:

// When a player collides with a dot, the OnPlayerCollideWithDot method is triggered. The code checks if the collider is a dot, and if the player hasn't reached their maximum dot limit (maxDotsPerPlayer) and the dot hasn’t already been collected.
// If valid, the dot is marked as collected, attached to the player, and the player grows by an amount (sizeIncrementOnCollect). A sound is played for the collection event, and a particle effect is triggered at the dot’s position. Additionally, an OSC message is sent to trigger the sound in external systems.

// Player Collision Mechanic:

// When two players collide, the OnPlayersCollided method is called. If both players have collected enough dots (determined by minNumDotsForCollision), the collected dots are released and no longer attached to the players. An explosion sound is played, and a particle effect is triggered between the two players.
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
    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager for handling no-go areas

    public Transform tree; // Reference to the "tree" object (central goal)

    [Header("~~~Collectable Info~~~")]
    [Tooltip("Number of dots in the game.")]
    public int numDots;  // Total number of collectible dots in the game

    public int inty = 2;  // Not sure what this variable is used for – might need clarification

    [Tooltip("Prefab used to instantiate dots.")]
    public GameObject dotPrefab;  // Prefab representing the dot object

    [Tooltip("Actual size of the dot.")]
    public float dotSize;  // Size of each dot

    [Tooltip("Lower value spawns near the top of the dome, a higher value towards the ring of the dome.")]
    public float dotFlatnessPower;  // Used to control how dots are distributed in the dome (placement logic)

    [Tooltip("List of transforms for each dot in the game.")]
    public List<Transform> dots;  // List holding the transform component of each instantiated dot

    [Tooltip("List of Dot objects representing the dot avatars.")]
    public List<Dot> dotAvatars;  // List holding the Dot components (data and behavior)

    [Tooltip("Transform that holds all the dot objects.")]
    public Transform dotHolder;  // Parent transform to organize all dots under a single hierarchy

    [Tooltip("How slowly the dot moves to the new position, after being in the tree or at the beginning of its life.")]
    public float dotForceTowardsPosition;  // Force applied to move dots to their position

    [Tooltip("Dampening factor for the dot's movement.")]
    public float dotDampening;  // Damping applied to the dot's movement to smooth the transitions

    [Header("~~~Game Info~~~")]
    [Tooltip("Minimum number of dots required for the possibility of a player collision.")]
    public int minNumDotsForCollision;  // Threshold for dots collected before players can collide and release dots

    [Tooltip("Maximum number of dots a player can collect.")]
    public int maxDotsPerPlayer;  // Maximum number of dots a player can hold at one time

    [Tooltip("Size increment of the player when a dot is collected.")]
    public float sizeIncrementOnCollect;  // How much the player's size increases when collecting a dot

    [Header("~~~Dot Regeneration Settings~~~")]
    [Tooltip("Time in seconds between each dot regeneration cycle.")]
    public float dotRegenerationInterval = 30f;  // Time interval between regeneration of dots

    [Tooltip("Number of dots to regenerate in each cycle.")]
    public int dotsToRegenerate = 5;  // Number of new dots created each regeneration cycle

    private float dotRegenerationTimer;  // Tracks time for dot regeneration

    [Header("~~~Audio Info~~~~")]
    [Tooltip("Sound played when a dot is collected.")]
    public AudioClip onDotCollectClip;  // Audio clip for collecting a dot

    [Tooltip("Sound played when players collide.")]
    public AudioClip onExplodeClip;  // Audio clip for player collision

    [Tooltip("Sound played when the flowers bloom.")]
    public AudioClip onLevelCompleteClip;  // Audio clip for level completion (victory sound)

    public AudioPlayer audioPlayer;  // Handles audio playback

    public ParticleSystem explosionParticles;  // Particle effect for player collision
    public ParticleSystem playerCollectDotParticleSystem;  // Particle effect for dot collection

    [Tooltip("Number of dots that have been brought to the tree.")]
    public int totalDotsCollected;  // Total number of dots collected by players and delivered to the tree

    [Tooltip("Number of dots that need to reach the tree before we turn on the flowers / game over! Win!")]
    public int totalDotsCollectedForCompletion;  // Threshold for winning the game (number of dots needed)

    [Tooltip("Force from the dots towards the player who collected them")]
    public float dotForceTowardsCollector;  // Force applied to dots when they are being collected by a player

    [Tooltip("Force from the dots towards the tree")]
    public float dotForceTowardsTree;  // Force applied to dots when moving toward the tree

    public ControlTreeMaterialValues controlTreeMaterialValues;  // Controls visuals related to the tree (e.g., bark, flowers)

    [Tooltip("Image used to fade the screen to black.")]
    public RawImage fadeImage;  // UI element used to fade the screen to black upon game completion

    private bool isLevelComplete = false;  // Tracks whether the level has already been marked as complete



    private void CreateDot(int id)
    {
        if (noGoZoneManager == null || noGoZoneManager.doorCollider == null ||
    noGoZoneManager.soundBoothCollider == null || noGoZoneManager.stageCollider == null)
        {
            // Debug.LogError("NoGoZoneManager or one of its colliders is not assigned!");
            return;
        }

        // Instantiate dot
        GameObject dot = Instantiate(dotPrefab, Vector3.zero, Quaternion.identity);
        Vector3 randomPos;
        bool isBlockedByNoGoZone;

        // Loop to ensure the dot is not placed in the line of sight of forbidden zones
        do
        {
            // Generate random position
            randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 2f), Random.Range(-1f, 1f));
            randomPos = getFinalPosition(randomPos);  // Map the random position

            // Cast a ray from the center of the dome to the random dot position
            Ray ray = new Ray(Vector3.zero, randomPos.normalized); // Ensure direction is normalized
            RaycastHit hit;

            // Perform raycast to check if anything is between the center and the random position
            if (Physics.Raycast(ray, out hit, randomPos.magnitude))
            {
                // Check if the ray hits a forbidden zone
                isBlockedByNoGoZone = hit.collider == noGoZoneManager.doorCollider ||
                                      hit.collider == noGoZoneManager.soundBoothCollider ||
                                      hit.collider == noGoZoneManager.stageCollider;

                if (isBlockedByNoGoZone)
                {
                    // Debug.Log($"Dot {id} placement blocked by {hit.collider.name} at position {randomPos}.");
                }
                else
                {
                    // Debug.Log($"Ray hit {hit.collider.name} but not a forbidden zone.");
                }
            }
            else
            {
                isBlockedByNoGoZone = false; // No collision, it's a valid position
                // Debug.Log($"No hit for dot {id} at position {randomPos}.");
            }


        } while (isBlockedByNoGoZone);  // Repeat if the position is blocked by a forbidden zone

        // Ensure the NoGoZoneManager is set
        if (noGoZoneManager == null)
        {
            // Debug.LogWarning("[WARNING] NoGoZoneManager is not assigned. Dot placement may be incorrect.");
        }

        // Assign dot properties
        Dot dotAvatar = dot.GetComponent<Dot>();
        if (dotAvatar == null)
        {
            // Debug.LogError("Dot component missing from dot prefab!");
            return;
        }

        dotAvatar.controller = this;
        dotAvatar.dotId = id;
        dotAvatar.originalDotPosition = randomPos;
        dotAvatar.dotVelocity = Vector3.zero;
        dotAvatar.targetPosition = randomPos;
        dotAvatar.collectedDot = false;
        dotAvatar.SetData();

        // Set the position of the dot and scale
        dot.transform.position = randomPos;
        dot.transform.SetParent(dotHolder);
        dot.transform.localScale = Vector3.one * dotSize;

        dot.SetActive(true);  // Activate dot

        // Add to dots and avatars lists
        dots.Add(dot.transform);
        dotAvatars.Add(dotAvatar);
    }


    public override void SetUp()


    {
        // Debug.Log("[INFO] DotGameController SetUp called.");

        // First, run the common setup logic
        base.SetUp();

        // Then, run the scene-specific setup logic for the dots

        // Initialize dots list
        dots = new List<Transform>();
        dotAvatars = new List<Dot>();

        for (int i = 0; i < numDots; i++)
        {
            CreateDot(i);
        }

        dotRegenerationTimer = dotRegenerationInterval;
        StartCoroutine(BlueMoonDotRegenerationRoutine());

        // Debug.Log("[INFO] DotGameController setup completed.");
    }


    void Start()
    {
        SetUp();  // This ensures SetUp is called when the scene starts
    }



    public override Vector3 GetGameSpecificScale(int playerIndex)
    {
        // Calculate scale based on the player's starting size and number of collected dots
        float scaleFactor = startSize + playerAvatars[playerIndex].numDotsCollected * sizeIncrementOnCollect;

        // Return the new scale factor (doesn't include player visibility, which GetScale will handle)
        return Vector3.one * scaleFactor;
    }



    // this is where we actually deal with player collisions
    public override void OnPlayersCollided(PlayerAvatar p1, PlayerAvatar p2)
    {

        // Dont do it if they dont have enough dots
        if (p1.numDotsCollected < minNumDotsForCollision || p2.numDotsCollected < minNumDotsForCollision)
        {
            return;
        }

        for (int i = 0; i < dots.Count; i++)
        {
            if (dotAvatars[i].dotCollector == p1.transform || dotAvatars[i].dotCollector == p2.transform)
            {
                dotAvatars[i].collectedDot = false;
                dotAvatars[i].dotCollector = null;
                dotAvatars[i].OnPlayersHoldingMeCollided();
            }
        }

        // Debug.Log($"[ooo About to enter RESET] Player ids are {p1} and {p2}: controller startSize: {startSize}");

        p1.Reset();
        // Debug.Log($"[POST RESET] Player {p1.id}: Scale after reset: {p1.transform.localScale}");
        p2.Reset();
        // Debug.Log($"[POST RESET] Player {p2.id}: Scale after reset: {p2.transform.localScale}");



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
        // Debug.Log($"Sent collision sound for player {player.id} at position {pointPosition}.");
    }

    // this is where we actually deal with player/dot collisions
    public override void OnPlayerCollideWithDot(PlayerAvatar player, GameObject collider)
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
                if (dots[index].GetComponent<Dot>().collectedDot)
                {
                    return;
                }

                dotAvatars[index].collectedDot = true;
                dotAvatars[index].dotCollector = player.transform;
                player.OnDotCollect(player.numDotsCollected >= minNumDotsForCollision, player.numDotsCollected >= maxDotsPerPlayer);

                player.transform.localScale = GetScale(player.id);

                if (Controller.enableOldSoundSystem && onDotCollectClip != null)
                {
                    audioPlayer.Play(onDotCollectClip); // <-- old sound keep for debugging


                }
                if (Controller.enableNewSoundSystem)
                {
                    string soundID = $"p{player.id}EffectsWithMePointCollision";
                    Vector3 pointPosition = player.transform.position;
                    soundEventSender.SendOneShotSound(soundID, pointPosition);
                }

                playerCollectDotParticleSystem.transform.position = collider.transform.position;
                playerCollectDotParticleSystem.Play();

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
        // Debug.Log("Creating new dots...");

        int currentDotCount = dots.Count;
        for (int i = 0; i < dotsToRegenerate; i++)
        {
            CreateDot(currentDotCount + i);
        }

        // Debug.Log($"{dotsToRegenerate} new dots created.");
    }




    public void OnTreeCollect()
    {
        totalDotsCollected++;
        controlTreeMaterialValues.barkShown = (float)totalDotsCollected / (float)totalDotsCollectedForCompletion;

        // Check if the win condition has been met

        if (totalDotsCollected >= totalDotsCollectedForCompletion)
        {
            // Called when the level is complete (win state)

            OnLevelComplete();
        }
        else
        {

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
        // StartCoroutine(FadeToBlack(onLevelCompleteClip.length));
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