using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSetupManager : MonoBehaviour
{
    public PlayerActivityManager playerActivityManager;

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public Transform playerHolder;
    public float startSize = 1f;
    public bool displayPlayerIDText = false;

    public List<GameObject> players { get; private set; } = new List<GameObject>();
    public List<PlayerAvatar> playerAvatars { get; private set; } = new List<PlayerAvatar>();
    public List<int> playerIDs { get; private set; } = new List<int>();
    public List<Vector3> playerTargetPositions { get; private set; } = new List<Vector3>();
    public List<float> playerSeenScalers { get; private set; } = new List<float>();

    public GameObject CreatePlayer(int playerID, Vector3 startPosition)
    {
        if (playerIDs.Contains(playerID))
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

        players.Add(player);
        playerAvatars.Add(avatar);
        playerIDs.Add(playerID);
        playerTargetPositions.Add(startPosition);
        playerSeenScalers.Add(0.01f);

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
}
