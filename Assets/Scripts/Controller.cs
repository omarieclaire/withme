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

    [Tooltip("List of sound states for players.")]
    public List<bool> playerSoundStates;

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

    // Dictionary to track stationary times
    private Dictionary<int, float> playerStationaryTimes = new Dictionary<int, float>();

    // New attributes
    public Transform center;

    [Header("Sound Event Sender")]
    public SoundEventSender soundEventSender;




    void Start()
    {
        // Start the background music
        string backgroundMusicID = "music";  // Use the same soundID throughout
        Vector3 musicPosition = Vector3.zero;  // Centrally position the music (if not spatialized)
        HandleNonPlayerSound(backgroundMusicID, musicPosition, true);  // Start playing the background music
    }

    public void StopBackgroundMusic()
    {
        string backgroundMusicID = "music";  // Ensure the same soundID is used
        HandleNonPlayerSound(backgroundMusicID, Vector3.zero, false);  // Stop the background music
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
            // Only change position if it's different from the current target
            if (playerTargetPositions[id] != fPos)
            {
                playerTargetPositions[id] = fPos;
                playerLastSeenTimestamp[id] = Time.time;

                soundEventSender.SendOrUpdateContinuousSound($"p{playerID}", remappedPosition);

                Debug.Log($"[CONFIRMED] Player {playerID}'s target position updated to: {fPos}.");
            }

            // Smoothly move player to the new target position
            players[id].transform.position = Vector3.Lerp(players[id].transform.position, playerTargetPositions[id], playerLerpSpeed);
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
                Debug.Log($"[CONFIRMED] Player {playerIDS[playerIndex]} has been hidden due to low visibility.");
            }
        }
        else
        {
            if (!players[playerIndex].activeSelf)
            {
                players[playerIndex].SetActive(true);
                StartPlayerSound(playerIndex);
                Debug.Log($"[CONFIRMED] Player {playerIDS[playerIndex]} has been shown as visibility increased.");
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

        // Send or update continuous sound for player
        string soundID = $"p{playerIDS[playerIndex]}";
        soundEventSender.SendOrUpdateContinuousSound(soundID, players[playerIndex].transform.position);
    }



    private void StopPlayerSound(int playerIndex)
    {
        string soundID = $"p{playerIDS[playerIndex]}";
        soundEventSender.StopContinuousSound(soundID);
    }



    private void HandlePlayerSound(int playerIndex, float timeSinceLastSeen)
    {
        // If the player has been inactive for too long, stop the sound
        string soundID = $"p{playerIDS[playerIndex]}";

        if (timeSinceLastSeen > soundTimeout)
        {
            soundEventSender.StopContinuousSound(soundID);
        }
        else
        {
            soundEventSender.SendOrUpdateContinuousSound(soundID, players[playerIndex].transform.position);
        }
    }


    // Example: sending or stopping non-player sounds
    void HandleNonPlayerSound(string soundID, Vector3 position, bool playSound)
    {
        if (playSound)
        {
            soundEventSender.SendOrUpdateContinuousSound(soundID, position);
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
        HandlePlayerSound(playerIndex, 0f);  // Reset sound, since it's now active again
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
        playerSoundStates.Add(false);
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
        // This handles the common setup logic for all scenes
        Debug.Log("[INFO] Common setup (_SetUp) called.");

        // Common initialization logic
        players = new List<GameObject>();
        playerAvatars = new List<PlayerAvatar>();
        playerIDS = new List<int>();
        playerLastSeenTimestamp = new List<float>();
        playerSeenScaler = new List<float>();
        playerTargetPositions = new List<Vector3>();
        playerSoundStates = new List<bool>();
        activePlayers = new List<PlayerAvatar>();

        Debug.Log("[INFO] Common setup for player lists completed.");
    }

    public virtual void SetUp()
    {
        // Ensure that the common setup is always run
        _SetUp();

        // This method can be overridden by subclasses for additional setup logic
        Debug.Log("[INFO] Base SetUp called.");
    }
}
