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
using TMPro;


public class Controller : MonoBehaviour
{


    // ~~~~~~~~~~~~~~
    [Header("~~~Camera Settings~~~")]

    [Tooltip("Resolution of the live pose camera feed used to track player positions.")]
    public int cameraResolution = 640;
    [Tooltip("Mapping for camera input: maps 0 (camera min) to the negative remap value, and 640 (camera max) to the positive value.")]
    public Vector2 RemapValues = new Vector2(1, -1);
    // ~~~~~~~~~~~~~~
    [Header("~~~Play Area Settings~~~")]
    [Tooltip("Center point of the game or play area, used for positioning calculations.")]
    public Transform center;

    [Tooltip("Radius of the play area where players and dots exist, used to scale the game world.")]
    public float sphereSize = 10;

    [Tooltip("Maximum angular range for positioning players around the dome in degrees.")]
    public float maxDegrees = 210;

    [Tooltip("Minimum height for player/object positioning, to prevent going below the dome's surface.")]
    public float minY = 0f;

    [Tooltip("Factor pushing player/object positions towards the bottom of the dome for more natural distribution.")]
    public float pushTowardsBottom = 0.5f;

    [Tooltip("Vertical positioning value; higher values place objects closer to the top of the dome.")]
    public float verticalValue = 1;

    [Tooltip("Maximum size for scaling positions along each axis.")]
    public Vector3 maxSize = new Vector3(1, 0, 1);
    // ~~~~~~~~~~~~~~
    [Header("~~~Player Settings~~~")]
    [Tooltip("Toggle for displaying player IDs on-screen.")]
    public bool displayPlayerIDText = false;


    [Header("~~~Movement Settings~~~")]
    [Tooltip("Speed at which players interpolate to their target position. 0 means no movement.")]
    public float playerLerpSpeed;
    [Tooltip("Playerholder.")]

    public Transform playerHolder;
    [Tooltip("Prefab used for instantiating player GameObjects.")]
    public GameObject playerPrefab;

    [Tooltip("Initial size for players when they are created.")]
    public float startSize;

    // ~~~~~~~~~~~~~~
    [Header("~~~Stationary Player Settings~~~")]
    [Tooltip("Time (in seconds) to wait before fading out a player due to inactivity.")]
    public float Time2Wait4PlayerFadeOut;

    [Tooltip("Speed at which players fade in.")]
    public float playerFadeInSpeed;

    [Tooltip("Speed at which players fade out.")]
    public float playerFadeOutSpeed;

    [Tooltip("Minimum scale factor to shrink a player down to when stationary.")]
    public float minPlayerScale = 0.01f;

    [Tooltip("Time (in seconds) before deactivating a player after losing OSC data.")]
    public float playerDeactivationTime = 1.0f;

    [Tooltip("Dictionary to track how long players have been stationary.")]
    private Dictionary<int, float> playerStationaryTimes = new Dictionary<int, float>();
    [Header("Sound Settings")]
    [Tooltip("Time (in seconds) before stopping a player's sound after inactivity.")]
    public float soundTimeout = 600f;

    // ~~~~~~~~~~~~~~
    [Header("~~~Player Info~~~")]

    [Tooltip("Average position of active players.")]
    public Vector3 averagePosition;

    [Tooltip("Number of currently active players.")]
    public int numActivePlayers;

    [Tooltip("List of player GameObjects.")]
    public List<GameObject> players;

    [Tooltip("List of PlayerAvatar components associated with each player.")]
    public List<PlayerAvatar> playerAvatars;

    [Tooltip("List of player IDs.")]
    public List<int> playerIDS;

    [Tooltip("List of timestamps for when each player was last seen.")]
    public List<float> playerLastSeenTimestamp;

    [Tooltip("List of scaling factors for player visibility (used for fade in/out).")]
    public List<float> playerSeenScaler;

    [Tooltip("List of target positions for players, where they should be moved.")]
    public List<Vector3> playerTargetPositions;

    [Tooltip("List of active player GameObjects.")]
    public List<PlayerAvatar> activePlayers;

    [Header("~~~Sound~~~")]

    [Tooltip("SoundEventSender for sending sound events based on player actions.")]
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

                // Smoothly move player to the new target position
                players[id].transform.position = Vector3.Lerp(players[id].transform.position, playerTargetPositions[id], playerLerpSpeed);
                Debug.Log($"[POSITION UPDATE] Player {playerID} moved to {players[id].transform.position}.");


                // Check if the sound needs to be updated based on position change
                if (Vector3.Distance(players[id].transform.position, fPos) > 0.05f) // 0.05f as a significant threshold
                {
                    string soundID = GetSceneSpecificSoundID(playerID);
                    soundEventSender.SendOrUpdateContinuousSound(soundID, players[id].transform.position);
                    Debug.Log($"[SOUND UPDATE] Player {playerID}, Position {players[id].transform.position}");

                }
            }
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
            // Debug.Log($"Generated sound ID: {id}");  // Log the generated ID
            return id;
        }
        else
        {
            // Return default sound ID
            string defaultID = $"p{playerID}";
            // Debug.Log($"Generated default sound ID: {defaultID}");  // Log the default ID
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
        soundEventSender.SendOrUpdateContinuousSound(soundID, players[playerIndex].transform.position);
    }


    private void StopPlayerSound(int playerIndex)
    {

        // string soundID = $"p{playerIDS[playerIndex]}";

        // // Ask SoundEventSender if the sound is active
        // if (soundEventSender.IsSoundActive(soundID))
        // {
        string soundID = GetSceneSpecificSoundID(playerIDS[playerIndex]);
        soundEventSender.StopContinuousSound(soundID, players[playerIndex].transform.position);  // Stop sound if it's active
        Debug.Log($"[INFO] Sound {soundID} stopped successfully.");
        // }
        // else
        // {
        //     Debug.LogWarning($"Sound {soundID} is not active, so it cannot be stopped.");
        // }
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
        soundEventSender.SendOrUpdateContinuousSound(soundID, players[playerIndex].transform.position);

        UpdatePlayerVisibilityAndSound(playerIndex);  // Ensure the player becomes visible
        ScalePlayer(playerIndex);  // Scale the player back to its original size
    }



    // Method to handle shrinking, silencing, and deactivating a player
    private void ShrinkSilenceAndDeactivatePlayer(int playerIndex)
    {
        float timeSinceLastSeen = Time.time - playerLastSeenTimestamp[playerIndex];

        // Start shrinking and fading after Time2Wait4PlayerFadeOut
        if (timeSinceLastSeen >= Time2Wait4PlayerFadeOut)
        {
            // Shrink the player's scale and fade out the player gradually
            playerSeenScaler[playerIndex] = Mathf.Lerp(playerSeenScaler[playerIndex], minPlayerScale, playerFadeOutSpeed * Time.deltaTime);

            // Disable the collider after 1 second of inactivity
            if (timeSinceLastSeen >= 1.0f)
            {
                Collider playerCollider = players[playerIndex].GetComponent<Collider>();
                if (playerCollider != null && playerCollider.enabled)
                {
                    playerCollider.enabled = false;
                    Debug.Log($"[INFO] Collider for Player {playerIDS[playerIndex]} disabled after 1 second.");
                }
            }

            // If the player shrinks below the minimum scale, deactivate it
            if (playerSeenScaler[playerIndex] <= minPlayerScale + 0.001f)
            {
                if (players[playerIndex].activeSelf)
                {
                    players[playerIndex].SetActive(false);
                    StopPlayerSound(playerIndex);
                    Debug.Log($"[DEACTIVATION] Player {playerIDS[playerIndex]} has been deactivated and silenced.");
                }
            }
            else
            {
                // Apply gradual fade out
                FadePlayerOut(playerIndex); // Continue fading the player out
                Debug.Log($"[FADE OUT] Player {playerIDS[playerIndex]} is fading out.");
            }

            // Apply scaling to shrink the player
            ScalePlayer(playerIndex);
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

        Debug.Log($"[CREATION] Player {playerID} created at position {player.transform.position}.");

        TextMeshPro tmp = player.GetComponentInChildren<TextMeshPro>();
        if (tmp != null)
        {
            if (displayPlayerIDText)
            {
                // Set the player ID text for debugging purposes
                tmp.text = $"ID: {playerID}";
                tmp.enabled = true;  // Ensure it's visible
            }
            else
            {
                tmp.enabled = false;  // Hide the text if debugging is off
            }
        }


        StartPlayerSound(players.Count - 1);  // Start sound for the new player
        Debug.Log($"[CONFIRMED] Player {playerID} initialized and sound started.");
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

        // Convert to spherical coordinates
        Vector3 fPosition = SphericalToCartesian(sphereSize, angleAround, Mathf.PI - angleDown);
        fPosition = transform.TransformPoint(fPosition);

        // Ensure the position doesn't go below the ground (minY) or above the dome height (maxY)
        fPosition.y = Mathf.Clamp(fPosition.y, minY, sphereSize);

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

        // Debug.Log("[INFO] Common setup for player lists completed.");
    }

    public virtual void SetUp()
    {
        // Ensure that the common setup is always run
        _SetUp();

        // This method can be overridden by subclasses for additional setup logic
        // Debug.Log("[INFO] Base SetUp called.");
    }
}
