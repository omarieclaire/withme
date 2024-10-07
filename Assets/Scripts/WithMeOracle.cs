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

// As dots are collected and brought to the "tree" (a central goal in the game), the OnTreeCollect method is called. The progress toward game completion is tracked by the number of dots collected (totalDotsCollected).
// When enough dots have been collected (totalDotsCollectedForCompletion), the OnLevelComplete method is triggered, which plays a sound, shows the flowers (or equivalent win visual), and starts a fade-to-black effect after a short delay.
// Audio and Visual Feedback:

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FantasyTree;
using UnityEngine;
using UnityEngine.UI;


public class DotGameController : Controller
{
    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager for handling no-go areas
    public GameItemPlacer gameItemPlacer;


    public GameManager gameManager;  // Add a reference to the GameManager


    // public Transform tree; // Reference to the "tree" object (central goal)
    // public TreeController treeController;  // Reference to TreeController


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
    public AudioClip withMePointCollisionClip;
    public AudioClip withMePointCollisionClip1;
    public AudioClip withMePointCollisionClip2;
    public AudioClip withMePointCollisionClip3;
    public AudioClip withMePointCollisionClip4;
    public AudioClip withMePointCollisionClip5;

    // Array to hold all the collision audio clips
    private AudioClip[] pointCollisionClips;


    [Tooltip("Sound played when players collide.")]
    public AudioClip withMePlayerCollisionClip;

    // public AudioClip withMeP0Clip;  
    // public AudioClip withMeP1Clip;  
    // public AudioClip withMeP2Clip;  
    // public AudioClip withMeP3Clip;  
    // public AudioClip withMeP4Clip;  
    // public AudioClip withMeP5Clip;  
    // public AudioClip withMeP6Clip;  
    // public AudioClip withMeP7Clip;  
    // public AudioClip withMeP8Clip;  
    // public AudioClip withMeP9Clip;  
    // public AudioClip withMeP10Clip;  

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
        if (gameItemPlacer == null)
        {
            Debug.LogError("GameItemPlacer is not assigned!");
            return;
        }

        // Use GameItemPlacer to get a valid position, avoiding no-go zones
        Vector3 randomPos = gameItemPlacer.PlaceItem(dotHolder);

        if (randomPos == Vector3.zero)
        {
            Debug.LogError("Failed to place the dot!");
            return;
        }

        // Instantiate the dot
        GameObject dot = Instantiate(dotPrefab, randomPos, Quaternion.identity);

        // Assign dot properties
        Dot dotAvatar = dot.GetComponent<Dot>();
        if (dotAvatar == null)
        {
            Debug.LogError("Dot component missing from dot prefab!");
            return;
        }

        dotAvatar.controller = this;
        dotAvatar.dotId = id;
        dotAvatar.originalDotPosition = randomPos;
        dotAvatar.dotVelocity = Vector3.zero;
        dotAvatar.targetPosition = randomPos;
        dotAvatar.collectedDot = false;
        dotAvatar.SetData();

        // Set the position and scale of the dot
        dot.transform.SetParent(dotHolder);
        dot.transform.localScale = Vector3.one * dotSize;

        dot.SetActive(true);  // Activate the dot

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
        pointCollisionClips = new AudioClip[] {
        withMePointCollisionClip,
        withMePointCollisionClip1,
        withMePointCollisionClip2,
        withMePointCollisionClip3,
        withMePointCollisionClip4,
        withMePointCollisionClip5
    };
    }



    public override Vector3 GetGameSpecificScale(int playerIndex)
    {
        // Calculate scale based on the player's starting size and number of collected dots
        float scaleFactor = startSize + playerAvatars[playerIndex].numDotsCollected * sizeIncrementOnCollect;

        // Return the new scale factor (doesn't include player visibility, which GetScale will handle)
        return Vector3.one * scaleFactor;
    }



    public override void OnPlayersCollided(PlayerAvatar p1, PlayerAvatar p2)
    {
        // Don't proceed if either player hasn't collected enough dots
        if (p1.numDotsCollected < minNumDotsForCollision || p2.numDotsCollected < minNumDotsForCollision)
        {
            return;
        }

        // Reset dots collected by both players
        for (int i = 0; i < dots.Count; i++)
        {
            if (dotAvatars[i].dotCollector == p1.transform || dotAvatars[i].dotCollector == p2.transform)
            {
                dotAvatars[i].collectedDot = false;
                dotAvatars[i].dotCollector = null;
                dotAvatars[i].OnPlayersHoldingMeCollided();
            }
        }

        // Reset both players
        p1.Reset();
        p2.Reset();

        // Play collision sounds for both players
        PlayCollisionSound(p1);
        // PlayCollisionSound(p2);

        // Set particle system to follow Player 1
        explosionParticles.transform.SetParent(p1.transform);  // Make particles follow Player 1
        explosionParticles.transform.localPosition = Vector3.zero;  // Optional: Center the particles on the player
        explosionParticles.Play();  // Play particles for Player 1

        // Optionally create and play particle system for Player 2 if needed
        ParticleSystem explosionParticlesP2 = Instantiate(explosionParticles, p2.transform.position, Quaternion.identity);
        explosionParticlesP2.transform.SetParent(p2.transform);  // Make particles follow Player 2
        explosionParticlesP2.transform.localPosition = Vector3.zero;  // Optional: Center the particles on Player 2
        explosionParticlesP2.Play();  // Play particles for Player 2
    }




    private void PlayCollisionSound(PlayerAvatar player)
    {
        if (Controller.enableOldSoundSystem && withMePlayerCollisionClip != null)
        {
            // Check if the collision clip is already playing
            if (!audioPlayer.IsClipPlaying(withMePlayerCollisionClip))
            {
                audioPlayer.Play(withMePlayerCollisionClip);
            }
        }
        if (Controller.enableNewSoundSystem)
        {
            string soundID = $"p{player.id}EffectsWithMePlayerCollision";
            Vector3 pointPosition = player.transform.position;
            soundEventSender.SendOneShotSound(soundID, pointPosition);
        }

    }

    public void PlayRandomPointCollisionClip()
    {
        if (pointCollisionClips.Length == 0)
        {
            Debug.LogWarning("No point collision clips available to play.");
            return;
        }
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

                if (Controller.enableOldSoundSystem)
                {
                    int randomIndex = Random.Range(0, pointCollisionClips.Length);
                    audioPlayer.Play(pointCollisionClips[randomIndex]);
                }
                if (Controller.enableNewSoundSystem)
                {
                    string soundID = $"p{player.id}EffectsWithMePointCollision";
                    Vector3 pointPosition = player.transform.position;
                    soundEventSender.SendOneShotSound(soundID, pointPosition);
                }

                // Move the particle system to the dot's position and parent it to the dot so it moves with the player
                playerCollectDotParticleSystem.transform.position = collider.transform.position;
                playerCollectDotParticleSystem.transform.SetParent(collider.transform); // Attach the particle system to the dot

                // Set particle system color to match the dot's color
                Renderer dotRenderer = collider.GetComponent<Renderer>();
                if (dotRenderer != null)
                {
                    Color dotColor = dotRenderer.material.color;
                    var mainModule = playerCollectDotParticleSystem.main;  // Access the main module of the particle system
                    mainModule.startColor = dotColor;  // Set the start color of the particles to match the dot's color
                }

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
        Debug.Log($"{dotsToRegenerate} new dots created.");
    }

    public void OnTreeCollect()
    {
        totalDotsCollected++;
        controlTreeMaterialValues.barkShown = (float)totalDotsCollected / (float)totalDotsCollectedForCompletion;

        // Check if the win condition has been met
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

        // Remove sound and scene handling; delegate to GameManager
        if (gameManager != null)
        {
            gameManager.HandleWinScenario();  // Let GameManager handle win-related events
        }
        else
        {
            Debug.LogError("GameManager reference is missing.");
        }
    }

    // Method to destroy all dots
    private void DestroyAllDots()
    {
        foreach (Transform dot in dots)
        {
            if (dot != null)
            {
                Collider collider = dot.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;  // Disable the collider
                }

                // Optionally, make the dot invisible
                Renderer renderer = dot.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;  // Hide the dot
                }
            }
        }

        Debug.Log("All dots have been disabled.");
    }

    public void OnGameEnd()
    {
        if (isLevelComplete) return;  // Prevent multiple triggers of game end

        // Tree visibility and growth
        treeController.EnableTree();
        treeController.StartGrowingTree(10f, 20f, 0.5f, 3f);

        // Destroy all dots
        DestroyAllDots();

        // Mark the level as complete and let GameManager handle it
        OnLevelComplete();
    }
}
