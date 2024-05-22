using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using OscJack;

public class CustomOSCEventReceiverScript : MonoBehaviour
{

    public int port;

    public MainController controller;

    [SerializeField]
    private GameObject playerSpherePrefab; // The prefab to instantiate for each player

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

    private void Update()
    {
        double currentTime = Time.unscaledTimeAsDouble;

        // Process player position messages
        while (playerPositionMessages.TryDequeue(out PlayerPositionMessage msg))
        {
            int playerId = msg.PlayerId;
            Vector3 position = msg.Position;

            PlayerData playerData = GetOrCreatePlayer(playerId, currentTime);
            ReactivatePlayer(playerData, currentTime);

            playerData.LastOSCTimeStamp = currentTime;
            playerData.Sphere.transform.position = controller.getFinalPosition(position);

            Debug.Log($"Updated position for player {playerId} to {position}");
        }

        // Deactivate missing players
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
                playerData.Sphere.SetActive(false);
                activePlayerIds.Remove(playerId);

                Debug.Log($"Deactivated player {playerId} due to inactivity.");
            }
        }
    }

    private void ReactivatePlayer(PlayerData playerData, double currentTime)
    {
        if (!playerData.IsActive)
        {
            playerData.IsActive = true;
            playerData.Sphere.SetActive(true);
            activePlayerIds.Add(playerData.PlayerId);

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
            // Instantiate a new sphere for the player
            playerData = new PlayerData(playerId, Instantiate(playerSpherePrefab));
            // Assign a unique color to each player's sphere
            playerData.Sphere.GetComponent<Renderer>().material.color = GetUniqueColor(playerId);
            playerData.Sphere.name = $"Player_{playerId}_Sphere";
            players.Add(playerId, playerData);

            Debug.Log($"Created new player {playerId} with sphere.");
            return playerData;
        }
    }

    private void OscReceiver1(string address, OscDataHandle data)
    {
        if (address.Contains("/center"))
        {
            PlayerPositionMessage msg = new PlayerPositionMessage(address, data);
            playerPositionMessages.Enqueue(msg);

            Debug.Log($"Enqueued position message for player {msg.PlayerId} with position {msg.Position}");
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

        public PlayerPositionMessage(string address, OscDataHandle data)
        {
            Address = address;
            PlayerId = GetPlayerNumber(address);
            Position = new Vector3(
                MapToRange(data.GetElementAsFloat(0), 0, 1200, -10, 10), 
                0f, // Y is the ground plane
                MapToRange(data.GetElementAsFloat(1), 0, 1200, -10, 10)  
            );

            Debug.Log($"Created PlayerPositionMessage: Address={Address}, PlayerId={PlayerId}, Position={Position}");
        }

        private static int GetPlayerNumber(string address)
        {
            string[] splitString = address.Split('/');
            if (splitString.Length > 4)
            {
                int playerId = int.Parse(splitString[4]);
                Debug.Log($"Extracted Player ID: {playerId} from address: {address}");
                return playerId;
            }
            else
            {
                Debug.LogWarning($"Failed to extract Player ID from address: {address}");
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
        public GameObject Sphere { get; }
        public bool IsActive { get; set; }
        public double LastOSCTimeStamp { get; set; }

        public PlayerData(int playerId, GameObject sphere)
        {
            PlayerId = playerId;
            Sphere = sphere;
            IsActive = true;
        }
    }
}
