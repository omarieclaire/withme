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
    private Dictionary<int, double> lastMessageTime = new Dictionary<int, double>();

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
            Debug.LogFormat("[OSCHandler] Processing player {0} at time {1:F3}", 
                msg.PlayerId, Time.unscaledTimeAsDouble);
        }

        if (controller != null)
        {
            // First update the position
            controller.OnPlayerPositionUpdate(msg.PlayerId, msg.BlobPosition);

            // Then handle activity only for this player
            controller.playerActivityManager.HandlePlayerActivity(msg.PlayerId);
        }
        else
        {
            Debug.LogError("[ERROR] Controller is null, cannot update player position.");
        }
    }
}

    public void ReceiveBlob(OSCMessage message)
    {
            Debug.LogFormat("[OSCTrace] ReceiveBlob Start - Time: {0:F3}", Time.unscaledTimeAsDouble);

        if (debug)
        {
            Debug.Log($"[OSCHandler] Raw message: {message.Address}");
        }

    var addressParts = message.Address.Split('/');
    if (addressParts.Length >= 6 && int.TryParse(addressParts[4], out int playerId))
        {
                    Debug.LogFormat("[OSC-Raw] Message for Player {0}: Address={1}, Time={2:F3}", 
            playerId, message.Address, Time.unscaledTimeAsDouble);

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
    if (debug)
    {
        Debug.LogFormat("[OSC-Complete] Player {0} - Got complete position ({1:F3}, {2:F3}) at time {3:F3}", 
            playerId, position.x, position.y, Time.unscaledTimeAsDouble);
    }
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
