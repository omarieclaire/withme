using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{



    [Header("Camera Live Pose Feed Info")]

    // dimensions of the livepose camera

    public int cameraResolution = 640;
    // maps 0 to the negative remap value, and maps 640 to the positive remap value
    // note, Y becomes Z because the osc only has two values and we need 3

    public Vector2 RemapValues = new Vector2(1, -1);


    // how far up the wall of the dome we might go - a low vertical value means you snap from the base of the dome to the base of the dome
    // a high value means you'll spend more time at the top of the dome as you cross over the center
    [Header("Positioning Info")]
    public float verticalValue = 1;
    // we could call this playsphere size. it's the radius of the sphere where the dots and players exist
    // it's a way to visually scale the game
    public float sphereSize = 10;
    public Vector3 maxSize = new Vector3(1, 0, 1);


    // the players that have been generated (osc data receiver generates)


    [Header("Player Info")]

    public List<GameObject> players;
    public List<PlayerAvatar> playerAvatars;
    public List<int> playerIDS;

    public List<float> playerLastSeenTimestamp;
    public List<float> playerSeenScaler;

    public List<Vector3> playerTargetPositions;

    // Speed at which players lerp towards their target position,
    // smaller value means slower speed

    public GameObject playerPrefab;
    public float startSize;
    // a transform to be the parent of the spawned prefabs

    public Transform playerHolder;

    [Header("Movement Info")]
    // when a player hits a dot, that dot is considered collected by the player and moves towards the player
    // 0 would be the dot force doesn't move. 
    public float playerLerpSpeed;


    public float playerFadeInSpeed;
    public float playerFadeOutSpeed;


    public Vector3 averagePosition;
    public int numActivePlayers;



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


    // Update is called once per frame
    void Update()
    {

        numActivePlayers = 0;
        averagePosition = Vector3.zero;

        for (int i = 0; i < players.Count; i++)
        {

            // start fading only if the player hasn't been seen a few frames
            if (Time.time - playerLastSeenTimestamp[i] > .1f)
            {
                playerSeenScaler[i] = Mathf.Lerp(playerSeenScaler[i], 0, playerFadeOutSpeed);
            }
            else
            {
                playerSeenScaler[i] = Mathf.Lerp(playerSeenScaler[i], 1, playerFadeInSpeed);
            }



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
                // only turn on if we aren't already on
                if (!players[i].activeSelf)
                {
                    players[i].SetActive(true);
                }
            }

            if (players[i].activeSelf)
            {
                numActivePlayers++;
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


}
