using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using extOSC;
using UnityEngine.UI;
using System;

public class OSCHandler : MonoBehaviour
{

    public bool debug;

    public OSCReceiver Receiver;

    [Header("Receiver UI Settings")]
    public float timeToWaitForMissingPlayers = 0.5f; // Time to wait before deactivating missing players

    public Text ReceiverTextBlob;

    private const string _blobAddress = "/livepose/blobs/0/*/center*";

    public Controller controller;

    private HashSet<int> activePlayerIds = new HashSet<int>();
    private ConcurrentQueue<PlayerPositionMessage> playerPositionMessages = new ConcurrentQueue<PlayerPositionMessage>();
    private Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

    private Dictionary<int, Vector2> incompletePositions = new Dictionary<int, Vector2>();

    private void Start()
    {
        if (Receiver != null)
        {
            if (debug)
            {
                Debug.Log($"OSC Receiver initialized and bound to {_blobAddress}");
            }
            Receiver.Bind(_blobAddress, ReceiveBlob);
        }
        else
        {
            Debug.LogError("OSCReceiver is not assigned!");
        }
    }

    private void Update()
    {
        double currentTime = Time.unscaledTimeAsDouble;

        while (playerPositionMessages.TryDequeue(out PlayerPositionMessage msg))
        {
            if (debug)
            {
                Debug.Log($"Processing message for player {msg.PlayerId} at position {msg.BlobPosition}");
            }

            int playerId = msg.PlayerId;
            Vector2 blobPosition = msg.BlobPosition;
            PlayerData playerData = GetOrCreatePlayer(playerId, currentTime);
            ReactivatePlayer(playerData, currentTime);
            playerData.LastOSCTimeStamp = currentTime;
            controller.OnPlayerPositionUpdate(playerId, blobPosition);
        }

        CheckForAndDeactivateMissingPlayers(currentTime);
    }

    private void CheckForAndDeactivateMissingPlayers(double currentTime)
    {
        HashSet<int> activePlayersCopy = new HashSet<int>(activePlayerIds);

        foreach (int playerId in activePlayersCopy)
        {
            PlayerData playerData = players[playerId];
            if (playerData.IsActive && (currentTime - playerData.LastOSCTimeStamp > timeToWaitForMissingPlayers))
            {
                playerData.IsActive = false;
                controller.DeactivatePlayer(playerId);
                activePlayerIds.Remove(playerId);
                if (debug)
                {
                    Debug.Log($"Deactivated player {playerId} due to inactivity.");
                }
            }
        }
    }

    private void ReactivatePlayer(PlayerData playerData, double currentTime)
    {
        if (!playerData.IsActive)
        {
            playerData.IsActive = true;
            activePlayerIds.Add(playerData.PlayerId);

            controller.ReactivatePlayer(playerData.PlayerId);
            Debug.Log($"Reactivated player {playerData.PlayerId}.");
        }
    }

    private PlayerData GetOrCreatePlayer(int playerId, double oscTime)
    {
        if (players.TryGetValue(playerId, out PlayerData playerData))
        {
            return playerData;
        }
        else
        {
            playerData = new PlayerData(playerId);
            players.Add(playerId, playerData);
            controller.OnPlayerCreate(playerId);
            if (debug)
            {
                Debug.Log($"Created new player {playerId}");
            }
            return playerData;
        }
    }

    public void ReceiveBlob(OSCMessage message)
    {
        if (debug)
        {
            Debug.Log($"Received OSC message at address: {message.Address} with {message.Values.Count} values");
        }
        // Check if the address matches the specific structure for center1 or center2
        if (message.Address.EndsWith("center1") || message.Address.EndsWith("center2"))
        {
            var addressParts = message.Address.Split('/');
            if (addressParts.Length >= 6 && int.TryParse(addressParts[4], out int playerId))
            {
                string part = addressParts[5];
                float value = message.Values[0].FloatValue;

                if (!incompletePositions.ContainsKey(playerId))
                {
                    incompletePositions[playerId] = new Vector2(float.NaN, float.NaN);
                }

                if (part == "center1")
                {
                    incompletePositions[playerId] = new Vector2(value, incompletePositions[playerId].y);
                }
                else if (part == "center2")
                {
                    incompletePositions[playerId] = new Vector2(incompletePositions[playerId].x, value);
                }

                Vector2 position = incompletePositions[playerId];
                if (!float.IsNaN(position.x) && !float.IsNaN(position.y))
                {
                    PlayerPositionMessage msg = new PlayerPositionMessage(message.Address, playerId, position);
                    playerPositionMessages.Enqueue(msg);
                    incompletePositions.Remove(playerId);
                }
            }
        }
        else
        {


            Debug.LogWarning($"Received message at {message.Address} does not match the expected address structure.");
        }
    }

    private class PlayerPositionMessage
    {
        public string Address { get; }
        public int PlayerId { get; }
        public Vector2 BlobPosition { get; }

        public PlayerPositionMessage(string address, int playerId, Vector2 blobPosition)
        {
            Address = address;
            PlayerId = playerId;
            BlobPosition = blobPosition;

            //            Debug.Log($"Created PlayerPositionMessage: Address={Address}, PlayerId={PlayerId}, BlobPosition={BlobPosition}");

        }
    }

    private class PlayerData
    {
        public int PlayerId { get; }
        public bool IsActive { get; set; }
        public double LastOSCTimeStamp { get; set; }

        public PlayerData(int playerId)
        {
            PlayerId = playerId;
            IsActive = true;
        }
    }
}
