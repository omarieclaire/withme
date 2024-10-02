// Controller Class:

// Handles the core player management and positioning logic. It tracks player avatars, their positions, visibility, and sounds.
// It serves as a base class for the DotGameController and contains shared functionality like camera input remapping, player creation, fading in/out, and the setup of a "playsphere" (the game space).
// The players' positions are updated using camera input through the OnPlayerPositionUpdate method, and the remapped 2D coordinates from the camera are used to compute the players' positions inside the dome.

// Live Pose Camera and Play Area Setup:

// cameraResolution: Refers to the resolution of the live pose feed, tracking player positions.
// sphereSize: Defines the radius of the play area where players and dots exist (how far out the world is from the camera)
// maxDegrees: Limits the range of motion for positioning players in the dome.
// pushTowardsBottom: Pushes positions toward the bottom of the dome for more natural distribution.
// RemapValues: Used to map camera coordinates to the game's coordinate system.

// 2. Player Info and Management:

// players, playerAvatars, playerIDS, playerLastSeenTimestamp, and other lists store information about players (their GameObjects, IDs, timestamps for when they were last seen, etc.).
// playerPrefab: A prefab for instantiating new players.
// playerLerpSpeed: Controls how quickly players move to new positions.
// soundTimeout: Defines how long to wait before stopping a player’s sound after they’ve been inactive.

// 3. Player Creation and Positioning:

// OnPlayerCreate: Instantiates a new player, adds it to the appropriate lists, and starts playing their sound.
// OnPlayerPositionUpdate: Updates the position of a player based on live pose data (likely blob tracking) and sends sound updates to the sound system.
// The player's position is remapped from the camera feed to fit within the dome’s space.
// If the player is moving, their position is updated and they are smoothly moved toward their target position using lerping.

// 4. Sound and Visibility Handling:

// soundEventSender: Manages sound events for each player. Each player's sound is updated based on their position or stopped when they become inactive.
// FadePlayerIn/FadePlayerOut: Handles the gradual fading in/out of players based on visibility.
// HandlePlayerSound: Manages whether a player's sound should continue or stop based on their last seen timestamp.

// 5. Stationary Player Logic:

// Tracks how long players have been stationary using a Dictionary.
// HandlePlayerActivity: Deactivates players if they’ve been inactive for too long and fades them back in when they become active again.
// ShrinkSilenceAndDeactivatePlayer: Gradually shrinks and silences players that have been stationary for a long time.

// 6. Position Conversion to Dome Coordinates:

// getFinalPosition: Converts 2D player positions from the live feed into 3D coordinates that fit the dome’s spherical space, using polar/spherical coordinates.
// SphericalToCartesian: Converts spherical coordinates into Cartesian coordinates, useful for positioning objects inside the dome.

// 7. Common Setup for Scenes:

// _SetUp/SetUp: Sets up the player lists and other components. The base SetUp method can be overridden by subclasses for game-specific logic.

// 8. Placeholder Methods:

// Several placeholder methods are present (OnWorldComplete, OnPlayerTrigger, etc.), which are filled in for specific game mechanics in derived classes.

using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Camera Live Pose Feed Info")]
    [Tooltip("Resolution of the live pose camera.")]
    public int cameraResolution = 640;

    [Header("BIG INFO")]
    [Tooltip("Radius of the 'playsphere' where dots and players exist, used to visually scale the game.")]
    public float sphereSize = 10;
    [Tooltip("Maximum degrees for positioning calculations.")]
    public float maxDegrees = 210;

    [Tooltip("Factor for pushing positions towards the bottom of the dome.")]
    public float pushTowardsBottom = .5f;

    [Tooltip("Initial size for players.")]
    public float startSize;

    [Header("Positioning Info")]
    [Tooltip("Vertical positioning value; higher values keep objects closer to the top of the dome.")]
    public float verticalValue = 1;

    [Tooltip("Remap values for camera input: maps 0 to the negative remap value and 640 to positive.")]
    public Vector2 RemapValues = new Vector2(1, -1);

    [Tooltip("Maximum size for position scaling along each axis.")]
    public Vector3 maxSize = new Vector3(1, 0, 1);

    [Header("Player Info")]
    [Tooltip("List of player GameObjects.")]
    public List<GameObject> players;

    [Tooltip("List of player avatars.")]
    public List<PlayerAvatar> playerAvatars;

    [Tooltip("List of player IDs.")]
    public List<int> playerIDS;

    [Tooltip("List of last seen timestamps for players.")]
    public List<float> playerLastSeenTimestamp;
    public float soundTimeout = 600f; // Time in seconds to stop sound after inactivity

    [Tooltip("List of scaling factors for player visibility.")]
    public List<float> playerSeenScaler;

    [Tooltip("List of target positions for players.")]
    public List<Vector3> playerTargetPositions;

    [Tooltip("Prefab for player instances.")]
    public GameObject playerPrefab;

    [Tooltip("Parent transform for player instances.")]
    public Transform playerHolder;

    [Header("Movement Info")]
    [Tooltip("Speed for player movement lerping. 0 would be it doesn't move.")]
    public float playerLerpSpeed;

    [Tooltip("Time to wait until fading out player.")]
    public float Time2Wait4PlayerFadeOut = 0.3f;

    [Tooltip("Speed for player fade-in.")]
    public float playerFadeInSpeed;

    [Tooltip("Speed for player fade-out.")]
    public float playerFadeOutSpeed;

    [Tooltip("Average position of active players.")]
    public Vector3 averagePosition;

    [Tooltip("Number of active players.")]
    public int numActivePlayers;

    [Tooltip("List of active player GameObjects.")]
    public List<PlayerAvatar> activePlayers;

    [Header("Stationary Player Logic")]
    [Tooltip("Minimum scale factor to shrink a player down to when stationary.")]
    public float minPlayerScale = 0.01f;

    [Tooltip("Time in seconds before we remove a stationary player entirely.")]
    public float removePlayerAfterStationaryTime = 300f;

    [Tooltip("Enable or disable player reassignment logic.")]
    public bool enablePlayerReassignment = true;

    // New variable to control player deactivation time based on OSC system behavior
    [Tooltip("Time in seconds to deactivate a player after losing OSC data.")]
    public float playerDeactivationTime = 1.0f; // New variable for player deactivation time

    // Dictionary to track stationary times
    private Dictionary<int, float> playerStationaryTimes = new Dictionary<int, float>();

    // New attributes
    public Transform center;


    [Header("Sound Event Sender")]
    public SoundEventSender soundEventSender;

    void Start()
    {
        SetUp();
    }

    // Method to handle player position updates
    public void OnPlayerPositionUpdate(int playerID, Vector2 blobPosition)
    {
        // Ensure the player ID exists before updating
        if (!playerIDS.Contains(playerID))
        {
            Debug.LogWarning($"[WARNING] Player ID {playerID} not found. Creating player.");
            OnPlayerCreate(playerID);
        }

        float v1 = blobPosition.x / cameraResolution;
        float v2 = blobPosition.y / cameraResolution;

        // Initialize the player's stationary time if it doesn't exist in the dictionary
        if (!playerStationaryTimes.ContainsKey(playerID))
        {
            playerStationaryTimes[playerID] = 0f;
            Debug.Log($"[INITIALIZED] Player {playerID}'s stationary time initialized.");
        }

        // Reset stationary time when the player moves
        playerStationaryTimes[playerID] = 0f;

        // Remap the X and Y values from the camera to fit within the dome's space
        v1 = Mathf.Lerp(-RemapValues.x, RemapValues.x, v1);
        v2 = Mathf.Lerp(-RemapValues.y, RemapValues.y, v2);

        Vector3 remappedPosition = new Vector3(v1, 0, v2);
        Vector3 fPos = getFinalPosition(remappedPosition);

        int id = playerIDS.IndexOf(playerID);
        if (id != -1)
        {
            // Only change position if it's different from the current target with a small tolerance
            if (Vector3.Distance(playerTargetPositions[id], fPos) > 0.01f) // Tolerance check
            {
                playerTargetPositions[id] = fPos;
                playerLastSeenTimestamp[id] = Time.time;

                string soundID = GetSceneSpecificSoundID(playerID);
            }

            // Smoothly move player to the new target position
            players[id].transform.position = Vector3.Lerp(players[id].transform.position, playerTargetPositions[id], playerLerpSpeed);
        }
    }


    private string GetSceneSpecificSoundID(int playerID)
    {
        // Check if the current scene is "WithMe"
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "WithMe")
        {
            // Return playerID with "WithMePlayerSound"
            string id = $"p{playerID}WithMePlayerSound";
            Debug.Log($"Generated sound ID: {id}");  // Log the generated ID
            return id;
        }
        else
        {
            // Return default sound ID
            string defaultID = $"p{playerID}";
            Debug.Log($"Generated default sound ID: {defaultID}");  // Log the default ID
            return defaultID;
        }
    }


    private void FadePlayerIn(int playerIndex)
    {
        playerSeenScaler[playerIndex] = Mathf.Lerp(playerSeenScaler[playerIndex], 1, playerFadeInSpeed * Time.deltaTime);
        playerSeenScaler[playerIndex] = Mathf.Clamp(playerSeenScaler[playerIndex], 0, 1);
    }

    private void FadePlayerOut(int playerIndex)
    {
        playerSeenScaler[playerIndex] = Mathf.Lerp(playerSeenScaler[playerIndex], 0, playerFadeOutSpeed * Time.deltaTime);
    }

    private void UpdatePlayerVisibilityAndSound(int playerIndex)
    {
        if (playerSeenScaler[playerIndex] < .03f)
        {
            if (players[playerIndex].activeSelf)
            {
                players[playerIndex].SetActive(false);
                StopPlayerSound(playerIndex);
                // Debug.Log($"[CONFIRMED] Player {playerIDS[playerIndex]} has been hidden due to low visibility.");
            }
        }
        else
        {
            if (!players[playerIndex].activeSelf)
            {
                players[playerIndex].SetActive(true);
                StartPlayerSound(playerIndex);
                // Debug.Log($"[CONFIRMED] Player {playerIDS[playerIndex]} has been shown as visibility increased.");
            }
        }
    }

    private void StartPlayerSound(int playerIndex)
    {
        if (soundEventSender == null)
        {
            Debug.LogError("[ERROR] soundEventSender is not assigned.");
            return;
        }

        string soundID = GetSceneSpecificSoundID(playerIDS[playerIndex]);
        // string soundID = $"p{playerIDS[playerIndex]}";
        // soundEventSender.StartContinuousSoundOrUpdatePosForContinuousSound(soundID, players[playerIndex].transform.position);
    }


    private void StopPlayerSound(int playerIndex)
    {

        // string soundID = $"p{playerIDS[playerIndex]}";

        // // Ask SoundEventSender if the sound is active
        // if (soundEventSender.IsSoundActive(soundID))
        // {
        string soundID = GetSceneSpecificSoundID(playerIDS[playerIndex]);
        soundEventSender.StopContinuousSound(soundID);  // Stop sound if it's active
        Debug.Log($"[INFO] Sound {soundID} stopped successfully.");
        // }
        // else
        // {
        //     Debug.LogWarning($"Sound {soundID} is not active, so it cannot be stopped.");
        // }
    }




    private void HandlePlayerSound(int playerIndex, float timeSinceLastSeen)
    {
        // If the player has been inactive for too long, stop the sound
        string soundID = GetSceneSpecificSoundID(playerIDS[playerIndex]);

        if (timeSinceLastSeen > soundTimeout)
        {
            soundEventSender.StopContinuousSound(soundID);
            Debug.Log($"[CONFIRMED] Player {playerIDS[playerIndex]} has been inactive for too long sound is being stopped.");
        }
        else
        {

            soundEventSender.StartContinuousSoundOrUpdatePosForContinuousSound(soundID, players[playerIndex].transform.position);
        }
    }

    void HandleNonPlayerSound(string soundID, Vector3 position, bool playSound)
    {
        if (playSound)
        {
            soundEventSender.StartContinuousSoundOrUpdatePosForContinuousSound(soundID, position);
        }
        else
        {
            soundEventSender.StopContinuousSound(soundID);
        }
    }


    private void ScalePlayer(int playerIndex)
    {
        players[playerIndex].transform.localScale = GetScale(playerIndex);
    }


    private void HandlePlayerActivity(int playerIndex)
    {
        float timeSinceLastSeen = Time.time - playerLastSeenTimestamp[playerIndex];

        // Shrink and deactivate the player if it has been inactive for too long
        if (timeSinceLastSeen > Time2Wait4PlayerFadeOut)
        {
            ShrinkSilenceAndDeactivatePlayer(playerIndex);  // Player fades out and sound stops only after grace period
        }
        else
        {
            ReactivatePlayer(playerIndex);  // Player fades in and scales up when a message is received
        }
    }

    private void ReactivatePlayer(int playerIndex)
    {
        FadePlayerIn(playerIndex);  // Player fades back in
        string soundID = GetSceneSpecificSoundID(playerIDS[playerIndex]);
        soundEventSender.StartContinuousSoundOrUpdatePosForContinuousSound(soundID, players[playerIndex].transform.position);

        UpdatePlayerVisibilityAndSound(playerIndex);  // Ensure the player becomes visible
        ScalePlayer(playerIndex);  // Scale the player back to its original size
    }



    // Method to handle shrinking, silencing, and deactivating a player
    private void ShrinkSilenceAndDeactivatePlayer(int playerIndex)
    {
        // Gradually shrink the player's scale
        playerSeenScaler[playerIndex] = Mathf.Lerp(playerSeenScaler[playerIndex], minPlayerScale, playerFadeOutSpeed * Time.deltaTime);

        // If the player is shrunk below the minimum scale, deactivate it
        if (playerSeenScaler[playerIndex] <= minPlayerScale + 0.001f)  // Adding a small epsilon for float precision
        {
            if (players[playerIndex].activeSelf)
            {

                players[playerIndex].SetActive(false);  // Deactivate the player
                StopPlayerSound(playerIndex);  // Stop the player's sound
                Debug.Log($"[CONFIRMED] Player {playerIDS[playerIndex]} has been deactivated and silenced.");
            }
        }
        else
        {
            FadePlayerOut(playerIndex);  // Continue fading the player out
        }
    }


    public virtual Vector3 GetScale(int i)
    {
        return Vector3.one * playerSeenScaler[i] * startSize;
    }

    public void OnPlayerCreate(int playerID)
    {
        if (playerIDS.Contains(playerID))
        {
            Debug.LogError($"[ERROR] Player with ID {playerID} already exists.");
            return;
        }

        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.transform.position = getFinalPosition(player.transform.position);
        player.transform.localScale = Vector3.one * startSize;
        player.transform.SetParent(playerHolder);
        player.name = $"Player_{playerID}_Sphere";

        PlayerAvatar playerAvatar = player.GetComponent<PlayerAvatar>();

        playerAvatar.controller = this;
        playerAvatar.id = playerID;
        playerAvatar.SetData("P" + playerID.ToString());

        player.SetActive(true);
        players.Add(player);
        playerAvatars.Add(playerAvatar);
        playerIDS.Add(playerID);
        playerLastSeenTimestamp.Add(Time.time);
        playerSeenScaler.Add(.001f);
        playerTargetPositions.Add(player.transform.position);
        // playerSoundStates.Add(false);
        playerAvatar.Reset();
        playerStationaryTimes[playerID] = 0f;

        StartPlayerSound(players.Count - 1);  // Start sound for the new player
        Debug.Log($"[CONFIRMED] Player {playerID} created at position {player.transform.position}.");
    }



    void Update()
    {
        numActivePlayers = 0;
        averagePosition = Vector3.zero;
        activePlayers.Clear();

        for (int i = 0; i < players.Count; i++)
        {
            HandlePlayerActivity(i);

            if (players[i].activeSelf)
            {
                numActivePlayers++;
                activePlayers.Add(playerAvatars[i]);
                averagePosition += players[i].transform.position;
            }
        }

        if (numActivePlayers > 0)
        {
            averagePosition /= numActivePlayers;
        }
        else
        {
            averagePosition = Vector3.zero;
        }
    }


    public Vector3 SphericalToCartesian(float radius, float polar, float elevation)
    {
        float a = radius * Mathf.Cos(elevation);
        Vector3 outCart = new Vector3();
        outCart.x = a * Mathf.Cos(polar);
        outCart.y = radius * Mathf.Sin(elevation);
        outCart.z = a * Mathf.Sin(polar);
        return outCart;
    }

    public Vector3 getFinalPosition(Vector3 position)
    {
        Vector3 finalPosition = position;

        float nX = finalPosition.x / maxSize.x;
        float nZ = finalPosition.z / maxSize.z;

        Vector2 nXZ = new Vector2(nX, nZ);
        float l = nXZ.magnitude;
        l = Mathf.Pow(l, pushTowardsBottom);
        float angleAround = Mathf.Atan2(nZ, nX);
        float angleDown = l * (maxDegrees / 360) * 2 * Mathf.PI;


        Vector3 fPosition = SphericalToCartesian(sphereSize, angleAround, Mathf.PI - angleDown);
        fPosition = transform.TransformPoint(fPosition);

        //     // Ensure the position doesn't go below the floor (y = 0)
        //     fPosition.y = Mathf.Max(fPosition.y, 0f);

        return fPosition;
    }

    // Empty functions for use in other scenes
    public virtual void OnPlayersWithDotsCollided(PlayerAvatar p1, PlayerAvatar p2)
    {
        // Placeholder for use in other scenes
    }

    public virtual void OnPlayerTrigger(PlayerAvatar player, GameObject collider)
    {
        // Placeholder for use in other scenes
    }

    public virtual void RegenerateWorld()
    {
        // Placeholder for regenerating world logic in specific game controllers
    }

    // Placeholder for handling world completion, can be used for triggering end-of-level events.
    public virtual void OnWorldComplete()
    {
        // Placeholder for handling world complete events
    }

    public virtual void OnCleanUp()
    {
        // Placeholder for game cleanup logic
    }

    // Placeholder for adding new game-specific mechanics in subclasses
    public virtual void AddGameMechanic()
    {
        // Placeholder for adding new game mechanics or logic in specific games
    }



    public virtual void _SetUp()
    {
        // Common initialization logic
        players = new List<GameObject>();
        playerAvatars = new List<PlayerAvatar>();
        playerIDS = new List<int>();
        playerLastSeenTimestamp = new List<float>();
        playerSeenScaler = new List<float>();
        playerTargetPositions = new List<Vector3>();
        // playerSoundStates = new List<bool>();
        activePlayers = new List<PlayerAvatar>();

        Debug.Log("[INFO] Common setup for player lists completed.");

        if (soundEventSender != null)
        {
            soundEventSender.sphereSize = this.sphereSize;  // Pass sphereSize to the SoundEventSender
            Debug.Log("yooo" + this.sphereSize);
        }
    }

    public virtual void SetUp()
    {
        // Ensure that the common setup is always run
        _SetUp();

        // This method can be overridden by subclasses for additional setup logic
        Debug.Log("[INFO] Base SetUp called.");
    }
}