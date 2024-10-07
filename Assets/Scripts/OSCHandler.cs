// Handles OSC messages to track the positions of players. 
// Listens for incoming blob position data via the OSC protocol, processes the data by parsing the message 
// to extract player IDs and coordinates, and stores incomplete positions until both x and y coordinates are 
// received. Once a complete position is available, it queues the player position message for processing. 
// The Update method dequeues messages and updates the player's position in the game, notifying the game 
// controller of the player's new position or creation. It also manages player data, including tracking the 
// last time a message was received for each player

using System.Collections.Concurrent;  // Provides thread-safe collection classes like ConcurrentQueue
using System.Collections.Generic;      // Provides generic collection types like Dictionary and HashSet
using UnityEngine;                     // Core Unity engine classes for game development
using extOSC;                          // OSC (Open Sound Control) library for receiving OSC messages
using UnityEngine.UI;                  // UI classes in Unity for handling Text elements
using System;

public class OSCHandler : MonoBehaviour
{
    public bool debug;  // Boolean flag to enable/disable debug logs

    public OSCReceiver Receiver;  // OSC Receiver to handle incoming messages

    [Header("Receiver UI Settings")]
    public Text ReceiverTextBlob;  // UI element to display information about blobs

    private const string _blobAddress = "/livepose/blobs/0/*/center*";  // OSC address to listen for blob center positions

    public Controller controller;  // Reference to the game controller that handles player actions

    // private HashSet<int> activePlayerIds = new HashSet<int>();  // Set of currently active player IDs
    private ConcurrentQueue<PlayerPositionMessage> playerPositionMessages = new ConcurrentQueue<PlayerPositionMessage>();  // Queue to store incoming player position messages
    private Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();  // Dictionary to store player data, keyed by player ID

    private Dictionary<int, Vector2> incompletePositions = new Dictionary<int, Vector2>();  // Dictionary to store incomplete position data (x or y not yet received)

    private double inactivityThreshold = 5.0;  // Threshold for player inactivity (5 seconds)

    private void Start()
    {
        // Initialize the OSC receiver and bind the blob address if the receiver is assigned
        if (Receiver != null)
        {
            if (debug)
            {
                Debug.Log($"OSC Receiver initialized and bound to {_blobAddress}");
            }
            Receiver.Bind(_blobAddress, ReceiveBlob);  // Bind the OSC receiver to the blob address
        }
        else
        {
            Debug.LogError("OSCReceiver is not assigned!");  // Error if no receiver is assigned
        }
    }
    private void Update()
    {
        double currentTime = Time.unscaledTimeAsDouble;  // Get the current time without scaling (unaffected by game speed)

        // Process all messages in the queue
        while (playerPositionMessages.TryDequeue(out PlayerPositionMessage msg))
        {
            if (debug)
            {
                Debug.Log($"[DEBUG] Processing message for player ID: {msg.PlayerId}, Blob Position: {msg.BlobPosition}");
            }

            int playerId = msg.PlayerId;
            Vector2 blobPosition = msg.BlobPosition;

            // Log before attempting to get or create player data
            if (debug)
            {
                Debug.Log($"[DEBUG] Attempting to get or create player data for Player ID: {playerId}");
            }

            // Attempt to get or create the player data
            PlayerData playerData = GetOrCreatePlayer(playerId, currentTime);

            // Check if playerData is null (though it should not be, add extra logging for safety)
            if (playerData == null)
            {
                Debug.LogError($"[ERROR] PlayerData is null for Player ID: {playerId}. Cannot proceed.");
                continue;  // Skip this iteration if playerData is null
            }

            // Log before updating the player's timestamp
            if (debug)
            {
                Debug.Log($"[DEBUG] Updating last OSC timestamp for Player ID: {playerId} to {currentTime}");
            }

            // Update the player's last message timestamp
            playerData.LastOSCTimeStamp = currentTime;

            // Log before calling the controller method to update player position
            if (controller == null)
            {
                Debug.LogError($"[ERROR] Controller is null, cannot update player position for Player ID: {playerId}");
            }
            else
            {
                if (debug)
                {
                    Debug.Log($"[DEBUG] Calling OnPlayerPositionUpdate for Player ID: {playerId}, Blob Position: {blobPosition}");
                }

                // Notify the controller about the player's new position
                controller.OnPlayerPositionUpdate(playerId, blobPosition);
            }
        }
    }


    private PlayerData GetOrCreatePlayer(int playerId, double oscTime)
    {
        // Check if player already exists, otherwise create a new player
        if (players.TryGetValue(playerId, out PlayerData playerData))
        {
            return playerData;
        }
        else
        {
            playerData = new PlayerData(playerId);
            players.Add(playerId, playerData);  // Add the new player to the dictionary
            controller.OnPlayerCreate(playerId);  // Notify the controller of the new player
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

        var addressParts = message.Address.Split('/');
        if (addressParts.Length >= 6 && int.TryParse(addressParts[4], out int playerId))
        {
            string part = addressParts[5];
            float value = message.Values[0].FloatValue;

            // Initialize if necessary
            // Get or initialize incomplete positions
            if (!incompletePositions.TryGetValue(playerId, out Vector2 position))
            {
                position = new Vector2(float.NaN, float.NaN);  // Initialize if the playerId is not present
            }

            // Update x or y based on part (center1 or center2)
            int index = (part == "center1") ? 0 : 1;
            position[index] = value;  // Update the correct part of the position (x or y)
            incompletePositions[playerId] = position;  // Update the dictionary with the new position


            // Queue the message once both x and y are valid
            if (!float.IsNaN(position.x) && !float.IsNaN(position.y))
            {
                playerPositionMessages.Enqueue(new PlayerPositionMessage(message.Address, playerId, position));
                incompletePositions.Remove(playerId); // Remove once processed
            }



        }
        else
        {
            Debug.LogWarning($"Received message at {message.Address} does not match the expected address structure.");
        }
    }


    // Class representing a player's position message
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

    // Class to store data about individual players
    private class PlayerData
    {
        public int PlayerId { get; }
        public bool IsActive { get; set; }
        public double LastOSCTimeStamp { get; set; }  // Last time a message was received for the player

        public PlayerData(int playerId)
        {
            PlayerId = playerId;
            IsActive = true;  // Player starts as active
        }
    }
}
