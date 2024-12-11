using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSetupManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform playerHolder;
    public float startSize = 1f;
    public bool displayPlayerIDText = false;

    // Unified dictionary storing all player data
    public Dictionary<int, PlayerInfo> playersDict = new Dictionary<int, PlayerInfo>();

    public class PlayerInfo
    {
        public int PlayerID; // Add this field
        public GameObject PlayerObject;
        public PlayerAvatar Avatar;
        public double LastOSCTimeStamp;
        public Vector3 TargetPosition;
        public float SeenScaler;
    }

    public GameObject CreatePlayer(int playerID, Vector3 startPosition)
    {
        if (playersDict.ContainsKey(playerID))
        {
            Debug.LogWarning($"Player with ID {playerID} already exists.");
            return null;
        }

        GameObject player = Instantiate(playerPrefab, startPosition, Quaternion.identity, playerHolder);
        player.transform.localScale = Vector3.one * startSize;
        player.name = $"Player_{playerID}_Sphere";

        PlayerAvatar avatar = player.GetComponent<PlayerAvatar>();
        avatar.id = playerID;
        avatar.SetData($"P{playerID}");

        PlayerInfo info = new PlayerInfo
        {
            PlayerID = playerID, // Set the PlayerID
            PlayerObject = player,
            Avatar = avatar,
            LastOSCTimeStamp = Time.unscaledTimeAsDouble,
            TargetPosition = startPosition,
            SeenScaler = 0.01f
        };

        playersDict[playerID] = info;

        TextMeshPro tmp = player.GetComponentInChildren<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = $"ID: {playerID}";
            tmp.enabled = displayPlayerIDText;
        }

        player.SetActive(true);
        Debug.Log($"[PlayerSetupManager] Player {playerID} is successfully activated.");

        return player;
    }

    public PlayerInfo GetPlayerInfo(int playerID)
    {
        playersDict.TryGetValue(playerID, out var info);
        return info;
    }

    public void UpdatePlayerTimestamp(int playerID, double time)
    {
        if (playersDict.TryGetValue(playerID, out var info))
        {
            Debug.LogFormat("[TimestampUpdate] Player {0} - Raw update requested from {1:F3} to {2:F3}",
                playerID, info.LastOSCTimeStamp, time);
            info.LastOSCTimeStamp = time;
        }
    }

    public void UpdatePlayerPosition(int playerID, Vector3 position)
    {
        if (playersDict.TryGetValue(playerID, out var info))
        {
            info.TargetPosition = position;
        }
    }

    public int GetActivePlayerCount()
    {
        int count = 0;
        foreach (var kvp in playersDict)
        {
            if (kvp.Value.PlayerObject.activeSelf)
                count++;
        }
        return count;
    }

    public PlayerAvatar GetPlayerAvatar(GameObject playerObject)
    {
        foreach (var kvp in playersDict)
        {
            if (kvp.Value.PlayerObject == playerObject)
            {
                return kvp.Value.Avatar;
            }
        }
        return null; // Return null if the playerObject is not found
    }


    public Vector3 GetPlayerScale(int playerID)
    {
        if (playersDict.TryGetValue(playerID, out var info))
        {
            return Vector3.one * startSize * info.SeenScaler;
        }
        return Vector3.one * startSize; // fallback if not found
    }


    public List<PlayerSetupManager.PlayerInfo> GetActivePlayers()
    {
        List<PlayerSetupManager.PlayerInfo> activeList = new List<PlayerSetupManager.PlayerInfo>();
        foreach (var kvp in playersDict)
        {
            if (kvp.Value.PlayerObject.activeSelf)
            {
                activeList.Add(kvp.Value);
            }
        }
        return activeList;
    }
}
