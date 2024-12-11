using System.Collections.Generic;
using UnityEngine;

public class PlayerActivityManager : MonoBehaviour
{
    public PlayerSetupManager playerSetupManager;
    public float playerFadeInSpeed;
    public float playerFadeOutSpeed;
    public float minPlayerScale = 0.01f;
    public float Time2Wait4PlayerFadeOut;
    public float playerDeactivationTime = 1.0f;

    public SoundEventSender soundEventSender;
    public static bool enableNewSoundSystem = false;

    // Track last logged time for each player to prevent spam
    private Dictionary<int, double> _lastLogTime = new Dictionary<int, double>();
    private const double LOG_INTERVAL = 0.5f; // Log every 0.5 seconds for each player

    public void Initialize()
    {
        Debug.Log("[PlayerActivityManager] Initialized.");
    }

   public void HandlePlayerActivity(int playerID)
    {
        var info = playerSetupManager.GetPlayerInfo(playerID);
        if (info == null) return;

double currentTime = Time.unscaledTimeAsDouble;
    Debug.LogFormat("[Activity-Check] Player {0}: Using timestamp {1:F3} at time {2:F3}", 
        playerID, info.LastOSCTimeStamp, currentTime);
        double timeSinceLastSeen = currentTime - info.LastOSCTimeStamp;

        Debug.LogFormat(
            "[PlayerActivity] Player {0} - Decision point: Current={1:F2}, Last={2:F2}, Delta={3:F2}, Threshold={4:F2}",
            playerID, currentTime, info.LastOSCTimeStamp, timeSinceLastSeen, Time2Wait4PlayerFadeOut);

        if (timeSinceLastSeen > Time2Wait4PlayerFadeOut)
        {
            Debug.LogFormat("[PlayerActivity] Player {0} - SHOULD SHRINK - Time since last seen ({1:F2}) > threshold ({2:F2})",
                playerID, timeSinceLastSeen, Time2Wait4PlayerFadeOut);
            ShrinkSilenceAndDeactivatePlayer(playerID);
        }
        else
        {
            Debug.LogFormat("[PlayerActivity] Player {0} - SHOULD BE ACTIVE - Time since last seen ({1:F2}) < threshold ({2:F2})",
                playerID, timeSinceLastSeen, Time2Wait4PlayerFadeOut);
            ReactivatePlayer(playerID);
        }
    }

    private void ReactivatePlayer(int playerID)
    {
        var info = playerSetupManager.GetPlayerInfo(playerID);
        if (info == null) return;

        float prevScale = info.SeenScaler;
        FadePlayerIn(info);

        if (Mathf.Abs(prevScale - info.SeenScaler) > 0.001f)
        {
            Debug.LogFormat("[PlayerReactivate] Player {0} - Scale: {1:F3} -> {2:F3}, LastOSCTimeStamp: {3:F2}", 
                playerID, prevScale, info.SeenScaler, info.LastOSCTimeStamp);
        }

        Collider playerCollider = info.PlayerObject.GetComponent<Collider>();
        if (playerCollider != null && !playerCollider.enabled)
        {
            playerCollider.enabled = true;
        }

        if (!info.PlayerObject.activeSelf)
        {
            info.PlayerObject.SetActive(true);
            StartPlayerSound(playerID);
        }

        ScalePlayer(info);
    }

    private void ShrinkSilenceAndDeactivatePlayer(int playerID)
    {
        var info = playerSetupManager.GetPlayerInfo(playerID);
        if (info == null) return;

        float prevScale = info.SeenScaler;
        info.SeenScaler = Mathf.Lerp(info.SeenScaler, minPlayerScale, playerFadeOutSpeed * Time.deltaTime);

        if (Mathf.Abs(prevScale - info.SeenScaler) > 0.001f)
        {
            Debug.LogFormat("[PlayerShrink] Player {0} - Scale: {1:F3} -> {2:F3}, LastOSCTimeStamp: {3:F2}", 
                playerID, prevScale, info.SeenScaler, info.LastOSCTimeStamp);
        }

        double timeSinceLastSeen = Time.unscaledTimeAsDouble - info.LastOSCTimeStamp;
        if (timeSinceLastSeen >= playerDeactivationTime)
        {
            Collider playerCollider = info.PlayerObject.GetComponent<Collider>();
            if (playerCollider != null && playerCollider.enabled)
            {
                playerCollider.enabled = false;
            }
        }

        if (info.SeenScaler <= minPlayerScale + 0.001f)
        {
            if (info.PlayerObject.activeSelf)
            {
                info.PlayerObject.SetActive(false);
                StopPlayerSound(playerID);
            }
        }
        else
        {
            FadePlayerOut(info);
        }

        ScalePlayer(info);
    }

    private void FadePlayerIn(PlayerSetupManager.PlayerInfo info)
    {
        info.SeenScaler = Mathf.Lerp(info.SeenScaler, 1, playerFadeInSpeed * Time.deltaTime);
        info.SeenScaler = Mathf.Clamp(info.SeenScaler, 0, 1);
    }

    private void FadePlayerOut(PlayerSetupManager.PlayerInfo info)
    {
        info.SeenScaler = Mathf.Lerp(info.SeenScaler, 0, playerFadeOutSpeed * Time.deltaTime);
    }

    private void ScalePlayer(PlayerSetupManager.PlayerInfo info)
    {
        Vector3 newScale = Vector3.one * info.SeenScaler * playerSetupManager.startSize;
        info.PlayerObject.transform.localScale = newScale;
    }

    private void StartPlayerSound(int playerID)
    {
        if (soundEventSender == null || !enableNewSoundSystem) return;
        string soundID = $"p{playerID}";
        var info = playerSetupManager.GetPlayerInfo(playerID);
        if (info == null) return;
        soundEventSender.SendOrUpdateContinuousSound(soundID, info.PlayerObject.transform.position);
    }

    private void StopPlayerSound(int playerID)
    {
        if (!enableNewSoundSystem) return;
        string soundID = $"p{playerID}";
        var info = playerSetupManager.GetPlayerInfo(playerID);
        if (info == null) return;
        soundEventSender.StopContinuousSound(soundID, info.PlayerObject.transform.position);
    }
}