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

    // This method can be called in Controller SetUp if needed.
    public void Initialize()
    {
        Debug.Log("[PlayerActivityManager] Initialized.");
    }

    public void HandlePlayerActivity(int playerID)
    {
        var info = playerSetupManager.GetPlayerInfo(playerID);
        if (info == null) return;

        double timeSinceLastSeen = Time.unscaledTimeAsDouble - info.LastOSCTimeStamp;

        if (timeSinceLastSeen > Time2Wait4PlayerFadeOut)
        {
            ShrinkSilenceAndDeactivatePlayer(playerID);
        }
        else
        {
            ReactivatePlayer(playerID);
        }
    }

    private void ReactivatePlayer(int playerID)
    {
        var info = playerSetupManager.GetPlayerInfo(playerID);
        if (info == null) return;

        FadePlayerIn(info);
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

        double timeSinceLastSeen = Time.unscaledTimeAsDouble - info.LastOSCTimeStamp;
        info.SeenScaler = Mathf.Lerp(info.SeenScaler, minPlayerScale, playerFadeOutSpeed * Time.deltaTime);

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
