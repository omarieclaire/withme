using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Camera Live Pose Feed Info")]
    [Tooltip("Resolution of the live pose camera.")]
    public int cameraResolution = 640;

    // maps 0 to the negative remap value, and maps 640 to the positive remap value
    // note, Y becomes Z because the osc only has two values and we need 3
    [Tooltip("Remap values for camera input: maps 0 to negative and 640 to positive.")]
    public Vector2 RemapValues = new Vector2(1, -1);

    [Header("Positioning Info")]
    [Tooltip("Vertical positioning value; higher values keep objects closer to the top of the dome.")]
    public float verticalValue = 1;


    [Tooltip("Radius of the 'playsphere' where dots and players exist, used to visually scale the game.")]
    public float sphereSize = 10;

    [Tooltip("Maximum size for position scaling along each axis.")]
    public Vector3 maxSize = new Vector3(1, 0, 1);

    [Tooltip("Maximum degrees for positioning calculations.")]
    public float maxDegrees = 210;

    [Tooltip("Factor for pushing positions towards the bottom of the dome.")]
    public float pushTowardsBottom = .5f;

    [Header("Player Info")]
    [Tooltip("List of player GameObjects.")]
    public List<GameObject> players;

    [Tooltip("List of player avatars.")]
    public List<PlayerAvatar> playerAvatars;

    [Tooltip("List of player IDs.")]
    public List<int> playerIDS;

    [Tooltip("List of last seen timestamps for players.")]
    public List<float> playerLastSeenTimestamp;

    [Tooltip("List of scaling factors for player visibility.")]
    public List<float> playerSeenScaler;

      // Speed at which players lerp towards their target position,
    // smaller value means slower speed

    [Tooltip("List of target positions for players.")]
    public List<Vector3> playerTargetPositions;

    [Tooltip("Prefab for player instances.")]
    public GameObject playerPrefab;

    [Tooltip("Initial size for players.")]
    public float startSize;

    [Tooltip("Parent transform for player instances.")]
    public Transform playerHolder;

    [Header("Movement Info")]
    [Tooltip("Speed for player movement lerping. 0 would be it doesn't move.")]
    public float playerLerpSpeed;

    [Tooltip("Speed for player fade-in.")]
    public float playerFadeInSpeed;

    [Tooltip("Speed for player fade-out.")]
    public float playerFadeOutSpeed;

    [Tooltip("Average position of active players.")]
    public Vector3 averagePosition;

    [Tooltip("Number of active players.")]
    public int numActivePlayers;

    [Tooltip("List of active player GameObjects.")]
    public List<PlayerAvatar> activePlayers;

    

    public virtual void _SetUp()
    {

        players = new List<GameObject>();
        playerAvatars = new List<PlayerAvatar>();
        playerIDS = new List<int>();
        playerLastSeenTimestamp = new List<float>();
        playerSeenScaler = new List<float>();
        playerTargetPositions = new List<Vector3>();

    }

    public virtual void SetUp()
    {
        _SetUp();
    }

    public Transform center;
    public void OnEnable()
    {

        SetUp();


    }


    public void OnPlayerCreate(int playerID)
    {

        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.transform.position = getFinalPosition(player.transform.position);
        player.transform.localScale = Vector3.one * startSize;
        player.transform.SetParent(playerHolder);
        player.name = $"Player_{playerID}_Sphere";

        PlayerAvatar playerAvatar = player.GetComponent<PlayerAvatar>();

        playerAvatar.controller = this;
        playerAvatar.id = playerID;
        playerAvatar.SetData("P" + playerID.ToString());
        player.SetActive(true);
        players.Add(player);
        playerAvatars.Add(playerAvatar);
        playerIDS.Add(playerID);
        playerLastSeenTimestamp.Add(Time.time);
        playerSeenScaler.Add(.001f);
        playerTargetPositions.Add(player.transform.position);
        playerAvatar.Reset();

    }


    public void ReactivatePlayer(int playerID)
    {
        int index = playerIDS.IndexOf(playerID);
        if (index != -1)
        {
            players[index].SetActive(true);
        }
    }

    public void DeactivatePlayer(int playerID)
    {
        int index = playerIDS.IndexOf(playerID);
        if (index != -1)
        {
            players[index].SetActive(false);
        }
    }

    public void OnPlayerPositionUpdate(int playerID, Vector2 blobPosition)
    {

        float v1 = blobPosition.x / cameraResolution;
        float v2 = blobPosition.y / cameraResolution;

        v1 = Mathf.Lerp(-RemapValues.x, RemapValues.x, v1);
        v2 = Mathf.Lerp(-RemapValues.y, RemapValues.y, v2);


        Vector3 remappedPosition = new Vector3(v1, 0, v2);
        Vector3 fPos = getFinalPosition(remappedPosition);

        int id = playerIDS.IndexOf(playerID);

        if (id != -1)
        {
            // only change position if we have moved
            if (playerTargetPositions[id] != fPos)
            {
                playerTargetPositions[id] = fPos;
                playerLastSeenTimestamp[id] = Time.time;
            }


            players[id].transform.position = Vector3.Lerp(players[id].transform.position, playerTargetPositions[id], playerLerpSpeed);
        }





    }


    // dont think this will ever be called? but should it?
    public void OnPlayerDestroy(int playerID)
    {
        print("DESTRUCTION");
        int index = playerIDS.IndexOf(playerID);
        if (index != -1)
        {
            Destroy(players[index]);
            players.RemoveAt(index);
            playerIDS.RemoveAt(index);
        }


    }


    public virtual void OnPlayersWithDotsCollided(PlayerAvatar p1, PlayerAvatar p2)
    {

    }

    public virtual void OnPlayerTrigger(PlayerAvatar player, GameObject collider)
    {
    }

    // Gets the scale of the player, different games will scale different ways
    public virtual Vector3 GetScale(int i)
    {
        return Vector3.one * startSize;
    }

    void Update()
    {
        numActivePlayers = 0;
        averagePosition = Vector3.zero;
        activePlayers.Clear();

        for (int i = 0; i < players.Count; i++)
        {
            // Start fading only if the player hasn't been seen for a few frames
            if (Time.time - playerLastSeenTimestamp[i] > .1f)
            {
                playerSeenScaler[i] = Mathf.Lerp(playerSeenScaler[i], 0, playerFadeOutSpeed);
            }
            else
            {
                playerSeenScaler[i] = Mathf.Lerp(playerSeenScaler[i], 1, playerFadeInSpeed);
            }

            // Update player scale
            players[i].transform.localScale = GetScale(i);

            if (playerSeenScaler[i] < .03f)
            {
                if (players[i].activeSelf)
                {
                    players[i].SetActive(false);
                }
            }
            else
            {
                // Only turn on if not already active
                if (!players[i].activeSelf)
                {
                    players[i].SetActive(true);
                }
            }

            if (players[i].activeSelf)
            {
                numActivePlayers++;
                activePlayers.Add(playerAvatars[i]);
                averagePosition += players[i].transform.position;
            }
        }

        if (numActivePlayers > 0)
        {
            averagePosition /= numActivePlayers;
        }
        else
        {
            averagePosition = Vector3.zero;
        }
    }

    /*


    Old Method


        public Vector3 getFinalPosition(Vector3 position)
        {
            Vector3 finalPosition = position;

            float nX = finalPosition.x / maxSize.x;
            float nZ = finalPosition.z / maxSize.z;

            Vector2 nXZ = new Vector2(nX, nZ);
            float l = nXZ.magnitude;

            float fVerticalValue = verticalValue * (1 - Mathf.Clamp(l, 0, 1));

            finalPosition.y = fVerticalValue; ;
            finalPosition = finalPosition.normalized * sphereSize;

            finalPosition = transform.TransformPoint(finalPosition);

            return finalPosition;

        }

    */

    // Converts spherical coordinates to Cartesian coordinates
    public Vector3 SphericalToCartesian(float radius, float polar, float elevation)
    {
        float a = radius * Mathf.Cos(elevation);

        Vector3 outCart = new Vector3();
        outCart.x = a * Mathf.Cos(polar);
        outCart.y = radius * Mathf.Sin(elevation);
        outCart.z = a * Mathf.Sin(polar);

        return outCart;
    }


    // Calculates the final position of an object in the dome
    public Vector3 getFinalPosition(Vector3 position)
    {
        Vector3 finalPosition = position;

        float nX = finalPosition.x / maxSize.x;
        float nZ = finalPosition.z / maxSize.z;

        Vector2 nXZ = new Vector2(nX, nZ);
        float l = nXZ.magnitude;

        l = Mathf.Pow(l, pushTowardsBottom);

        float angleAround = Mathf.Atan2(nZ, nX);
        float angleDown = l * (maxDegrees / 360) * 2 * Mathf.PI;

        Vector3 fPosition = SphericalToCartesian(sphereSize, angleAround, Mathf.PI - angleDown);
        fPosition = transform.TransformPoint(fPosition);

        /*
        PolarToCartesian(angleDown, angleAround, out float x, out float z);

        float fVerticalValue = verticalValue * (1 - Mathf.Clamp(l, 0, 1));

        finalPosition.y = fVerticalValue;
        finalPosition = finalPosition.normalized * sphereSize;

        finalPosition = transform.TransformPoint(finalPosition);
        */

        return fPosition;
    }
}