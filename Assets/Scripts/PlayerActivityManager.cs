using System.Collections.Generic;
using UnityEngine;

public class PlayerActivityManager : MonoBehaviour
{
        public PlayerSetupManager playerSetupManager;

    public OSCHandler oscHandler; // Reference to OSCHandler to access PlayerData
    public float playerFadeInSpeed;
    public float playerFadeOutSpeed;
    public float minPlayerScale = 0.01f;
    public float Time2Wait4PlayerFadeOut;
    public float playerDeactivationTime = 1.0f;

    private List<GameObject> players;
    private List<float> playerSeenScaler;
    public Dictionary<int, float> playerStationaryTimes = new Dictionary<int, float>();

    public void Initialize(List<GameObject> players, List<float> playerSeenScaler)
    {
        this.players = players;
        this.playerSeenScaler = playerSeenScaler;

        foreach (var player in players)
        {
            player.SetActive(true);
        }
        Debug.Log("[PlayerActivityManager] All players set active for testing.");
    }

    public void UpdateVisibilityAndSound(int playerIndex, SoundEventSender soundEventSender, bool enableNewSoundSystem)
    {
        if (playerSeenScaler[playerIndex] < 0.03f)
        {
            if (players[playerIndex].activeSelf)
            {
                players[playerIndex].SetActive(false);
                if (enableNewSoundSystem && soundEventSender != null)
                {
                    string soundID = $"p{playerIndex}";
                    soundEventSender.StopContinuousSound(soundID, players[playerIndex].transform.position);
                }
            }
        }
        else
        {
            if (!players[playerIndex].activeSelf)
            {
                players[playerIndex].SetActive(true);
                if (enableNewSoundSystem && soundEventSender != null)
                {
                    string soundID = $"p{playerIndex}";
                    soundEventSender.SendOrUpdateContinuousSound(soundID, players[playerIndex].transform.position);
                }
            }
        }
    }

    public void HandlePlayerActivity(int playerIndex)
    {
        double timeSinceLastSeen = GetTimeSinceLastSeen(playerIndex);

        if (timeSinceLastSeen > Time2Wait4PlayerFadeOut)
        {
            ShrinkSilenceAndDeactivatePlayer(playerIndex);
        }
        else
        {
            ReactivatePlayer(playerIndex);
        }
    }

    private double GetTimeSinceLastSeen(int playerIndex)
    {
        int playerId = playerIndex; // Assuming playerIndex corresponds to playerId
        if (oscHandler.players.TryGetValue(playerId, out var playerData))
        {
            return Time.unscaledTimeAsDouble - playerData.LastOSCTimeStamp;
        }
        Debug.LogWarning($"[PlayerActivityManager] No timestamp for Player ID {playerId}");
        return double.MaxValue;
    }

    private void ReactivatePlayer(int playerIndex)
    {
        FadePlayerIn(playerIndex);
        Collider playerCollider = players[playerIndex].GetComponent<Collider>();
        if (playerCollider != null && !playerCollider.enabled)
        {
            playerCollider.enabled = true;
        }

        if (!players[playerIndex].activeSelf)
        {
            players[playerIndex].SetActive(true);
        }

        ScalePlayer(playerIndex);
    }



    private void ShrinkSilenceAndDeactivatePlayer(int playerIndex)
    {
        float timeSinceLastSeen = (float)GetTimeSinceLastSeen(playerIndex);
        if (timeSinceLastSeen >= Time2Wait4PlayerFadeOut)
        {
            playerSeenScaler[playerIndex] = Mathf.Lerp(playerSeenScaler[playerIndex], minPlayerScale, playerFadeOutSpeed * Time.deltaTime);

            if (timeSinceLastSeen >= playerDeactivationTime)
            {
                Collider playerCollider = players[playerIndex].GetComponent<Collider>();
                if (playerCollider != null && playerCollider.enabled)
                {
                    playerCollider.enabled = false;
                }
            }

            if (playerSeenScaler[playerIndex] <= minPlayerScale + 0.001f)
            {
                if (players[playerIndex].activeSelf)
                {
                    players[playerIndex].SetActive(false);
                }
            }
            else
            {
                FadePlayerOut(playerIndex);
            }

            ScalePlayer(playerIndex);
        }
    }

    private void FadePlayerIn(int playerIndex)
    {
        float previousScale = playerSeenScaler[playerIndex];
        playerSeenScaler[playerIndex] = Mathf.Lerp(playerSeenScaler[playerIndex], 1, playerFadeInSpeed * Time.deltaTime);
        playerSeenScaler[playerIndex] = Mathf.Clamp(playerSeenScaler[playerIndex], 0, 1);
    }

    private void FadePlayerOut(int playerIndex)
    {
        playerSeenScaler[playerIndex] = Mathf.Lerp(playerSeenScaler[playerIndex], 0, playerFadeOutSpeed * Time.deltaTime);
    }

    private void ScalePlayer(int playerIndex)
    {
        Vector3 newScale = Vector3.one * playerSeenScaler[playerIndex];
        players[playerIndex].transform.localScale = newScale;
    }

    public void InitializeStationaryTime(int playerID)
{
    if (!playerStationaryTimes.ContainsKey(playerID))
    {
        playerStationaryTimes[playerID] = 0f;
    }
    else
    {
        playerStationaryTimes[playerID] = 0f;
    }
}

}
