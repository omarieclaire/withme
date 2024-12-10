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
        // If player doesn't exist, create them
        if (playerSetupManager.GetPlayerInfo(playerID) == null)
        {
            OnPlayerCreate(playerID);
        }

        double currentTime = Time.unscaledTimeAsDouble;
        playerSetupManager.UpdatePlayerTimestamp(playerID, currentTime);

        float v1 = blobPosition.x / cameraAndPlayAreaSettings.actualCameraResolution;
        float v2 = blobPosition.y / cameraAndPlayAreaSettings.actualCameraResolution;

        v1 = Mathf.Lerp(-cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.x, cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.x, v1);
        v2 = Mathf.Lerp(-cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.y, cameraAndPlayAreaSettings.RemapCamCoords2GameCoords.y, v2);

        Vector3 remappedPosition = new Vector3(v1, 0, v2);
        Vector3 finalPosition = cameraAndPlayAreaSettings.getFinalPosition(remappedPosition);

        var info = playerSetupManager.GetPlayerInfo(playerID);
        if (info != null)
        {
            if (Vector3.Distance(info.TargetPosition, finalPosition) > 0.01f)
            {
                playerSetupManager.UpdatePlayerPosition(playerID, finalPosition);
                info.PlayerObject.transform.position = Vector3.Lerp(
                    info.PlayerObject.transform.position,
                    finalPosition,
                    playerLerpSpeed
                );
            }
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

    void Update()
    {
        numActivePlayers = 0;
        averagePosition = Vector3.zero;

        foreach (var kvp in playerSetupManager.playersDict)
        {
            int playerID = kvp.Key;
            var info = kvp.Value;

            playerActivityManager.HandlePlayerActivity(playerID);

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

    // ===== Compatibility Layer =====
    // Some scripts reference controller.activePlayers or GetScale(int)
    // even though we no longer maintain activePlayers as a separate list.
    // We provide a computed property and method to match old code expectations.

    /// <summary>
    /// Returns a dynamic list of active players.  
    /// This was previously maintained as a list directly, now we rebuild it for compatibility.
    /// </summary>
    // public List<PlayerAvatar> activePlayers
    // {
    //     get
    //     {
    //         List<PlayerAvatar> active = new List<PlayerAvatar>();
    //         foreach (var kvp in playerSetupManager.playersDict)
    //         {
    //             if (kvp.Value.PlayerObject.activeSelf)
    //             {
    //                 active.Add(kvp.Value.Avatar);
    //             }
    //         }
    //         return active;
    //     }
    // }

    /// <summary>
    /// Restores the old GetScale(int playerIndex) method that some scripts rely on.  
    /// This uses the playerSetupManager.players and playerSeenScalers properties for indexing.
    /// Be aware that player index ordering may differ from before.
    /// </summary>
    // public Vector3 GetScale(int playerIndex)
    // {
    //     var players = playerSetupManager.players;
    //     var scalers = playerSetupManager.playerSeenScalers;

    //     if (playerIndex < 0 || playerIndex >= players.Count)
    //         return Vector3.one * playerSetupManager.startSize;

    //     float scaler = scalers[playerIndex];
    //     return Vector3.one * playerSetupManager.startSize * scaler;
    // }
}
