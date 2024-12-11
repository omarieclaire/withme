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
    public bool displayPlayerIDText = false;

    [Header("~~~Movement Settings~~~")]
    public float playerLerpSpeed;

    public Vector3 averagePosition;
    public int numActivePlayers;

    public SoundEventSender soundEventSender;
    public static bool enableOldSoundSystem = true;
    public static bool enableNewSoundSystem = false;

    public Transform tree;
    public TreeController treeController;

    private Dictionary<int, double> _lastLoggedTimestamp = new Dictionary<int, double>();
    private const double LOG_INTERVAL = 0.5f; // Log every 0.5 seconds per player


    void Start()
    {
        Debug.Log("[Controller] Start method called.");
        playerActivityManager.playerSetupManager = playerSetupManager;
        playerActivityManager.soundEventSender = soundEventSender;
        playerActivityManager.Initialize();
        SetUp();
        Debug.Log("[Controller] SetUp completed in Start.");
    }


    public void OnPlayerPositionUpdate(int playerID, Vector2 blobPosition)
    {
        var info = playerSetupManager.GetPlayerInfo(playerID);

        // If player doesn't exist, create them
        if (info == null)
        {
            OnPlayerCreate(playerID);
            info = playerSetupManager.GetPlayerInfo(playerID);
        }

        // Check if position has actually changed
        float v1 = blobPosition.x / cameraAndPlayAreaSettings.actualCameraResolution;
        float v2 = blobPosition.y / cameraAndPlayAreaSettings.actualCameraResolution;

        v1 = Mathf.Lerp(-cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.x, cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.x, v1);
        v2 = Mathf.Lerp(-cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.y, cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.y, v2);

        Vector3 remappedPosition = new Vector3(v1, 0, v2);
        Vector3 finalPosition = cameraAndPlayAreaSettings.getFinalPosition(remappedPosition);

        // Only update position and timestamp if position has changed
        if (Vector3.Distance(info.TargetPosition, finalPosition) > 0.01f)
        {
            double currentTime = Time.unscaledTimeAsDouble;

            // Update position
            playerSetupManager.UpdatePlayerPosition(playerID, finalPosition);
            info.PlayerObject.transform.position = Vector3.Lerp(
                info.PlayerObject.transform.position,
                finalPosition,
                playerLerpSpeed
            );

            // Update timestamp and handle activity
            playerSetupManager.UpdatePlayerTimestamp(playerID, currentTime);
            playerActivityManager.OnNewPositionUpdate(playerID);
            Debug.LogFormat("[Controller] Updated position and timestamp for player {0}", playerID);

            // if (debug)
            // {
            // }
        }
    }

    public void OnPlayerCreate(int playerID)
    {
        Debug.Log($"[Controller] OnPlayerCreate called for Player ID: {playerID}");
        Vector3 startPosition = Vector3.zero;
        GameObject newPlayer = playerSetupManager.CreatePlayer(playerID, startPosition);

        if (newPlayer != null)
        {
            Debug.Log($"[Controller] Successfully created player with ID: {playerID}");
        }
        else
        {
            Debug.LogError($"[Controller] Failed to create player with ID: {playerID}");
        }
    }

    // void Update()
    // {
    //     double currentTime = Time.unscaledTimeAsDouble;
    //     numActivePlayers = 0;
    //     averagePosition = Vector3.zero;

    //     foreach (var kvp in playerSetupManager.playersDict)
    //     {
    //         int playerID = kvp.Key;
    //         var info = kvp.Value;

    //         // Log time since last OSC update for each player
    //         if (!_lastLoggedTimestamp.ContainsKey(playerID) || 
    //             (currentTime - _lastLoggedTimestamp[playerID]) >= LOG_INTERVAL)
    //         {
    //             Debug.LogFormat(
    //                 "[TimeSinceOSC] Player {0} - LastOSCTimeStamp: {1:F2}, TimeSince: {2:F2}s",
    //                 playerID, info.LastOSCTimeStamp, currentTime - info.LastOSCTimeStamp
    //             );
    //             _lastLoggedTimestamp[playerID] = currentTime;
    //         }

    //         playerActivityManager.HandlePlayerActivity(playerID);

    //         if (info.PlayerObject.activeSelf)
    //         {
    //             numActivePlayers++;
    //             averagePosition += info.PlayerObject.transform.position;
    //         }
    //     }

    //     if (numActivePlayers > 0)
    //     {
    //         averagePosition /= numActivePlayers;
    //     }
    //     else
    //     {
    //         averagePosition = Vector3.zero;
    //     }
    // }

    void Update()
    {
        numActivePlayers = 0;
        averagePosition = Vector3.zero;

        // Only calculate average position here
        foreach (var kvp in playerSetupManager.playersDict)
        {
            var info = kvp.Value;
            if (info.PlayerObject.activeSelf)
            {
                numActivePlayers++;
                averagePosition += info.PlayerObject.transform.position;
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

    public virtual void OnPlayersCollided(PlayerAvatar p1, PlayerAvatar p2) { }
    public virtual void OnPlayerCollideWithDot(PlayerAvatar player, GameObject collider) { }
    public virtual void RegenerateWorld() { }
    public virtual void OnWorldComplete() { }
    public virtual void OnCleanUp() { }
    public virtual void AddGameMechanic() { }

    public virtual void _SetUp()
    {
        Debug.Log("[Controller] _SetUp common logic.");
    }

    public virtual void SetUp()
    {
        _SetUp();
    }

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