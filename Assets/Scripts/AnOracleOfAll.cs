using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Controller : MonoBehaviour
{
    public CameraAndPlayAreaSettings cameraAndPlayAreaSettings;
    public PlayerSetupManager playerSetupManager;

    public PlayerActivityManager playerActivityManager;

    [Header("~~~Player Settings~~~")]
    [Tooltip("Toggle for displaying player IDs on-screen.")]
    public bool displayPlayerIDText = false;


    [Header("~~~Movement Settings~~~")]
    [Tooltip("Speed at which players interpolate to their target position. 0 means no movement.")]
    public float playerLerpSpeed;
    // [Tooltip("Playerholder.")]

    // public Transform playerHolder;
    // [Tooltip("Prefab used for instantiating player GameObjects.")]
    // public GameObject playerPrefab;

    // [Tooltip("Initial size for players when they are created.")]
    // public float startSize;

    // ~~~~~~~~~~~~~~
    [Header("~~~Player Info~~~")]

    [Tooltip("Average position of active players.")]
    public Vector3 averagePosition;

    [Tooltip("Number of currently active players.")]
    public int numActivePlayers;

    // [Tooltip("List of player GameObjects.")]
    // public List<GameObject> players;

    // [Tooltip("List of PlayerAvatar components associated with each player.")]
    // public List<PlayerAvatar> playerAvatars;

    // [Tooltip("List of player IDs.")]
    // public List<int> playerIDS;

    // [Tooltip("List of timestamps for when each player was last seen.")]
    // public List<float> playerLastSeenTimestamp;

    // [Tooltip("List of scaling factors for player visibility (used for fade in/out).")]
    // public List<float> playerSeenScaler;

    // [Tooltip("List of target positions for players, where they should be moved.")]
    // public List<Vector3> playerTargetPositions;

    [Tooltip("List of active player GameObjects.")]
    public List<PlayerAvatar> activePlayers;

    [Header("~~~Sound~~~")]

    [Tooltip("SoundEventSender for sending sound events based on player actions.")]
    public SoundEventSender soundEventSender;

    public static bool enableOldSoundSystem = true;
    public static bool enableNewSoundSystem = false;

    // public TreeController treeController;  // Reference to TreeController

    public Transform tree; // Reference to the "tree" object (central goal)
    public TreeController treeController;  // Reference to TreeController

    void Start()
    {
        Debug.Log("[Controller] Start method called.");
        playerActivityManager.playerSetupManager = playerSetupManager;
        SetUp();
        Debug.Log("[Controller] SetUp completed in Start.");
    }


    public void OnPlayerPositionUpdate(int playerID, Vector2 blobPosition)
    {
        if (!playerSetupManager.playerIDs.Contains(playerID))
        {
            OnPlayerCreate(playerID);
        }

        int id = playerSetupManager.playerIDs.IndexOf(playerID); // Use playerSetupManager.playerIDs
        if (id != -1)
        {
            float v1 = blobPosition.x / cameraAndPlayAreaSettings.actualCameraResolution;
            float v2 = blobPosition.y / cameraAndPlayAreaSettings.actualCameraResolution;

            v1 = Mathf.Lerp(-cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.x, cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.x, v1);
            v2 = Mathf.Lerp(-cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.y, cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.y, v2);

            Vector3 remappedPosition = new Vector3(v1, 0, v2);
            Vector3 finalPosition = cameraAndPlayAreaSettings.getFinalPosition(remappedPosition);

            if (Vector3.Distance(playerSetupManager.playerTargetPositions[id], finalPosition) > 0.01f)
            {
                playerSetupManager.playerTargetPositions[id] = finalPosition;
                playerSetupManager.players[id].transform.position = Vector3.Lerp(
                    playerSetupManager.players[id].transform.position,
                    finalPosition,
                    playerLerpSpeed
                );
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

    private void UpdatePlayerVisibilityAndSound(int playerIndex)
    {
        playerActivityManager.UpdateVisibilityAndSound(playerIndex, soundEventSender, enableNewSoundSystem);
    }
    private void StartPlayerSound(int playerIndex)
    {
        if (soundEventSender == null) return;

        if (enableNewSoundSystem)
        {
            string soundID = GetSceneSpecificSoundID(playerSetupManager.playerIDs[playerIndex]);
            soundEventSender.SendOrUpdateContinuousSound(soundID, playerSetupManager.players[playerIndex].transform.position);
        }
    }

    private void StopPlayerSound(int playerIndex)
    {
        if (enableNewSoundSystem)
        {
            string soundID = GetSceneSpecificSoundID(playerSetupManager.playerIDs[playerIndex]);
            soundEventSender.StopContinuousSound(soundID, playerSetupManager.players[playerIndex].transform.position); // Stop sound if it's active
        }
    }

    public virtual Vector3 GetScale(int playerIndex)
    {
        Vector3 gameSpecificScale = GetGameSpecificScale(playerIndex);
        Vector3 finalScale = gameSpecificScale * playerSetupManager.playerSeenScalers[playerIndex]; // Use playerSetupManager.playerSeenScalers
        return finalScale;
    }
    public virtual Vector3 GetGameSpecificScale(int playerIndex)
    {
        return Vector3.one * playerSetupManager.startSize;
    }

    public void OnPlayerCreate(int playerID)
    {
        Debug.Log($"[Controller] OnPlayerCreate called for Player ID: {playerID}");
        Vector3 startPosition = Vector3.zero;
        GameObject newPlayer = playerSetupManager.CreatePlayer(playerID, startPosition);

        if (newPlayer != null)
        {
            Debug.Log($"[Controller] Successfully created player with ID: {playerID}");
            playerActivityManager.InitializeStationaryTime(playerID);
            StartPlayerSound(playerSetupManager.players.Count - 1);
        }
        else
        {
            Debug.LogError($"[Controller] Failed to create player with ID: {playerID}");
        }
    }


    void Update()
    {
        numActivePlayers = 0;
        averagePosition = Vector3.zero;
        activePlayers.Clear();

        for (int i = 0; i < playerSetupManager.players.Count; i++)
        {
            playerActivityManager.HandlePlayerActivity(i);

            if (playerSetupManager.players[i].activeSelf)
            {
                numActivePlayers++;
                activePlayers.Add(playerSetupManager.playerAvatars[i]);
                averagePosition += playerSetupManager.players[i].transform.position;
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


    public virtual void OnPlayersCollided(PlayerAvatar p1, PlayerAvatar p2)
    {
        // Placeholder for use in other scenes
    }

    public virtual void OnPlayerCollideWithDot(PlayerAvatar player, GameObject collider)
    {
        // Placeholder for use in other scenes
    }

    public virtual void RegenerateWorld()
    {
        // Placeholder for regenerating world logic in specific game controllers
    }

    public virtual void OnWorldComplete()
    {
        // Placeholder for handling world complete events
    }

    public virtual void OnCleanUp()
    {
        // Placeholder for game cleanup logic
    }

    public virtual void AddGameMechanic()
    {
        // Placeholder for adding new game mechanics or logic in specific games
    }
    public virtual void _SetUp()
    {
        if (playerActivityManager != null)
        {
            Debug.Log("[Controller] Initializing PlayerActivityManager...");
            playerActivityManager.Initialize(playerSetupManager.players, playerSetupManager.playerSeenScalers);
            Debug.Log("[Controller] PlayerActivityManager initialized successfully.");
        }
        else
        {
            Debug.LogError("[Controller] PlayerActivityManager is not assigned!");
        }
    }



    public virtual void SetUp()
    {
        // Ensure that the common setup is always run
        _SetUp();
    }

    // Method to enable the tree in any game
    public virtual void EnableTree()
    {
        if (treeController != null)
        {
            treeController.EnableTree();
        }
        else
        {
            Debug.LogWarning("TreeController reference is missing.");
        }
    }

    // Method to start growing the tree
    public virtual void StartGrowingTree(float growDuration, float targetYScale, float shrinkScale, float finalShrinkDuration)
    {
        if (treeController != null)
        {
            treeController.StartGrowingTree(growDuration, targetYScale, shrinkScale, finalShrinkDuration);
        }
        else
        {
            Debug.LogWarning("TreeController reference is missing.");
        }
    }

    // Method to change tree material to timeout
    public virtual void ChangeTreeMaterialToTimeout()
    {
        if (treeController != null)
        {
            treeController.ChangeMaterialToTimeout();
        }
        else
        {
            Debug.LogWarning("TreeController reference is missing.");
        }
    }
}
