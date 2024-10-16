using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using OscJack;

public class OSCDataReceiver : MonoBehaviour
{

    public int port;
    public Controller controller;

    [SerializeField]
    public float timeToWaitForMissingPlayers = 0.5f; // Time to wait before deactivating missing players

    private HashSet<int> activePlayerIds = new HashSet<int>();
    private ConcurrentQueue<PlayerPositionMessage> playerPositionMessages = new ConcurrentQueue<PlayerPositionMessage>();
    private Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

    private OscServer server;

    private void Start()
    {
        Debug.Log("Initializing OSC Server...");
        server = OscMaster.GetSharedServer(port);
        server.MessageDispatcher.AddCallback(string.Empty, OscReceiver1); // Listen to all messages
        Debug.Log("OSC Server initialized and listening on port " + port);
    }

    // Update function dequeing player position messages and updating player positions

    private void Update()
    {
        double currentTime = Time.unscaledTimeAsDouble;

        // Process player position messages
        while (playerPositionMessages.TryDequeue(out PlayerPositionMessage msg))
        {
            int playerId = msg.PlayerId;
            Vector3 position = msg.Position;
            Vector2 blobPosition = msg.BlobPosition;

            PlayerData playerData = GetOrCreatePlayer(playerId, currentTime);
            // ReactivatePlayer(playerData, currentTime);

            playerData.LastOSCTimeStamp = currentTime;

            controller.OnPlayerPositionUpdate(playerId, blobPosition);

            //Debug.Log($"Updated position for player {playerId} to {position}");
        }

        // Deactivate missing players
        // CheckForAndDeactivateMissingPlayers(currentTime);
    }

    // private void CheckForAndDeactivateMissingPlayers(double currentTime)
    // {
    //     HashSet<int> activePlayersCopy = new HashSet<int>(activePlayerIds);

    //     foreach (int playerId in activePlayersCopy)
    //     {
    //         PlayerData playerData = players[playerId];
    //         if (playerData.IsActive && (currentTime - playerData.LastOSCTimeStamp > timeToWaitForMissingPlayers))
    //         {
    //             playerData.IsActive = false;
    //             controller.DeactivatePlayer(playerId);
    //             activePlayerIds.Remove(playerId);

    //             //Debug.Log($"Deactivated player {playerId} due to inactivity.");
    //         }
    //     }
    // }

    // private void ReactivatePlayer(PlayerData playerData, double currentTime)
    // {
    //     if (!playerData.IsActive)
    //     {
    //         playerData.IsActive = true;
    //         activePlayerIds.Add(playerData.PlayerId);

    //         // Reactivating the player in the game
    //         controller.ReactivatePlayer(playerData.PlayerId);

    //         //Debug.Log($"Reactivated player {playerData.PlayerId}.");
    //     }
    // }

    // Getting or creating a player depending on if we have them or not!
    private PlayerData GetOrCreatePlayer(int playerId, double oscTime)
    {
        if (players.TryGetValue(playerId, out PlayerData playerData))
        {
            return playerData;
        }
        else
        {
            // Instantiate a new sphere for the player
            playerData = new PlayerData(playerId);
            players.Add(playerId, playerData);
            controller.OnPlayerCreate(playerId);
            //Debug.Log($"Created new player {playerId} with sphere.");
            return playerData;
        }
    }

    // Getting a new player Position
    private void OscReceiver1(string address, OscDataHandle data)
    {
        if (address.Contains("/center"))
        {
            PlayerPositionMessage msg = new PlayerPositionMessage(address, data);
            playerPositionMessages.Enqueue(msg);
            //Debug.Log($"Enqueued position message for player {msg.PlayerId} with position {msg.Position}");
        }
    }

    private Color GetUniqueColor(int playerId)
    {
        // Generate a unique color based on the player ID
        Random.InitState(playerId);
        return new Color(Random.value, Random.value, Random.value);
    }

    private class PlayerPositionMessage
    {
        public string Address { get; }
        public int PlayerId { get; }
        public Vector3 Position { get; }
        public Vector2 BlobPosition { get; }

        public PlayerPositionMessage(string address, OscDataHandle data)
        {


            Address = address;
            PlayerId = GetPlayerNumber(address);
            BlobPosition = new Vector2(data.GetElementAsFloat(0), data.GetElementAsFloat(1));
            }

        private static int GetPlayerNumber(string address)
        {
            string[] splitString = address.Split('/');
            if (splitString.Length > 4)
            {
                int playerId = int.Parse(splitString[4]);
                //  Debug.Log($"Extracted Player ID: {playerId} from address: {address}");
                return playerId;
            }
            else
            {
                //  Debug.LogWarning($"Failed to extract Player ID from address: {address}");
                return -1;
            }
        }

        private static float MapToRange(float numberInRange1, float start1, float end1, float start2, float end2)
        {
            float distance = numberInRange1 - start1;
            float distanceRatio = distance / (end1 - start1);
            float amountInRange2 = distanceRatio * (end2 - start2);
            return start2 + amountInRange2;
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
