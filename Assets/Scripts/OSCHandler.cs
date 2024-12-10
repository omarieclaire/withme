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
    public Text ReceiverTextBlob;
    private const string _blobAddress = "/livepose/blobs/0/*/center*";
    public Controller controller;

    private ConcurrentQueue<PlayerPositionMessage> playerPositionMessages = new ConcurrentQueue<PlayerPositionMessage>();
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
        while (playerPositionMessages.TryDequeue(out PlayerPositionMessage msg))
        {
            if (debug)
            {
                Debug.Log($"[DEBUG] Processing message for player ID: {msg.PlayerId}, Blob Position: {msg.BlobPosition}");
            }

            if (controller == null)
            {
                Debug.LogError("[ERROR] Controller is null, cannot update player position.");
            }
            else
            {
                controller.OnPlayerPositionUpdate(msg.PlayerId, msg.BlobPosition);
            }
        }
    }

    public void ReceiveBlob(OSCMessage message)
    {
        if (debug)
        {
            Debug.Log($"Received OSC message at address: {message.Address} with {message.Values.Count} values");
        }

        var addressParts = message.Address.Split('/');
        if (addressParts.Length >= 6 && int.TryParse(addressParts[4], out int playerId))
        {
            string part = addressParts[5];
            float value = message.Values[0].FloatValue;

            if (!incompletePositions.TryGetValue(playerId, out Vector2 position))
            {
                position = new Vector2(float.NaN, float.NaN);
            }

            int index = (part == "center1") ? 0 : 1;
            position[index] = value;
            incompletePositions[playerId] = position;

            if (!float.IsNaN(position.x) && !float.IsNaN(position.y))
            {
                playerPositionMessages.Enqueue(new PlayerPositionMessage(message.Address, playerId, position));
                incompletePositions.Remove(playerId);
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
        }
    }
}
