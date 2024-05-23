using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainController : MonoBehaviour
{



    [Header("Camera Live Pose Feed Info")]

        // dimensions of the livepose camera

    public int cameraResolution = 640;
    // maps 0 to the negative remap value, and maps 640 to the positive remap value
    // note, Y becomes Z because the osc only has two values and we need 3

    public Vector2 RemapValues = new Vector2(10, -10);


 // how far up the wall of the dome we might go - a low vertical value means you snap from the base of the dome to the base of the dome
    // a high value means you'll spend more time at the top of the dome as you cross over the center
    [Header("Positioning Info")]
    public float verticalValue;
     // we could call this playsphere size. it's the radius of the sphere where the dots and players exist
    // it's a way to visually scale the game
    public float sphereSize;
    public Vector3 maxSize;


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
    public float sizeIncrementOnCollect;
    // a transform to be the parent of the spawned prefabs

    public Transform playerHolder;


    [Header("Collectable Info")]
    public int numDots;

    public GameObject dotPrefab;
    // actual size of the dot

    public float dotSize;
        // lower value spawns near the top fo the dome, a lower balue toward the ring of the dome

    public float dotFlatnessPower;

    public List<Transform> dots;
    public List<Dot> dotAvatars;

    public Transform dotHolder;
        // how slowly it moves to the new postion, 1 means it wil move exactly, 0 means it will never move


    [Header("Movement Info")]


    // when a player hits a dot, that dot is considered collected by the player and moves towards the player
    // 0 would be the dot force doesn't move. 
    public float playerLerpSpeed;

      // if dameping is 1 the velocity is not being reduced at all, like ping pong. Keep it beteen .9 and .99. 
    //The closer the damping is to 1 the lower the force needs to be
    public float dotForceTowardsPosition;
    public float dotDampening;


    [Header("Game Info")]
    public int minNumDotsForCollision;
    public int maxDotsPerPlayer;

    public float playerFadeInSpeed;
    public float playerFadeOutSpeed;


    [Header("Audio Info")]

    // the sound when the dot is collected

    public AudioClip onDotCollectClip;

        // the sound when player collide

    public AudioClip onExplodeClip;

    public AudioPlayer audioPlayer;


    public ParticleSystem explosionParticles;
    // Center object just Camera, for looking at / orienting players

        // Center object just Camera, for looking at / orienting players

    public Transform center;
    public void OnEnable()
    {

        players = new List<GameObject>();
        playerAvatars = new List<PlayerAvatar>();
        playerIDS = new List<int>();
        playerLastSeenTimestamp = new List<float>();
        playerSeenScaler = new List<float>();
        playerTargetPositions = new List<Vector3>();



        dots = new List<Transform>();

        for (int i = 0; i < numDots; i++)
        {
            GameObject dot = Instantiate(dotPrefab, Vector3.zero, Quaternion.identity);
            Vector3 pos = new Vector3(Random.Range(-1f, 1f), Mathf.Pow(Random.Range(0f, 1f), dotFlatnessPower), Random.Range(-1f, 1f));


            pos = pos.normalized * sphereSize;

            Dot dotAvatar = dot.GetComponent<Dot>();

            dotAvatar.controller = this;
            dotAvatar.id = i;
            dotAvatar.originalPosition = pos;
            dotAvatar.velocity = Vector3.zero;
            dotAvatar.targetPosition = pos;
            dotAvatar.collected = false;
            dotAvatar.SetData();


            dot.transform.position = pos;
            dot.transform.SetParent(dotHolder);
            dot.transform.localScale = Vector3.one * dotSize;


            dot.SetActive(dot);

            dots.Add(dot.transform);
            dotAvatars.Add(dotAvatar);


        }


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


    public void OnPlayersWithDotsCollided(PlayerAvatar p1, PlayerAvatar p2)
    {

        // Dont do it if they dont have enought
        if (p1.numDotsCollected < minNumDotsForCollision || p2.numDotsCollected < minNumDotsForCollision)
        {
            return;
        }


        for (int i = 0; i < dots.Count; i++)
        {
            if (dotAvatars[i].collector == p1.transform || dotAvatars[i].collector == p2.transform)
            {
                dotAvatars[i].collected = false;
                dotAvatars[i].collector = null;
            }
        }

        p1.numDotsCollected = 0;
        p2.numDotsCollected = 0;
        p1.transform.localScale = Vector3.one * startSize;
        p2.transform.localScale = Vector3.one * startSize;

        audioPlayer.Play(onExplodeClip);
        explosionParticles.transform.position = (p1.transform.position + p2.transform.position) / 2;
        explosionParticles.Play();
    }

    public void OnPlayerTrigger(GameObject player, GameObject collider)
    {
        //Debug.Log("Player Trigger");
        // Check if the collider is a dot
        if (collider.CompareTag("Dot"))
        {
            int index = dots.IndexOf(collider.transform);
            if (index != -1)
            {

                //   print("WE GOT A DOT");


                // Dont Recollect
                if (dots[index].GetComponent<Dot>().collected)
                {
                    return;
                }

                dotAvatars[index].collected = true;
                dotAvatars[index].collector = player.transform;

                player.GetComponent<PlayerAvatar>().numDotsCollected++;
                audioPlayer.Play(onDotCollectClip);
                // dots[index].gameObject.SetActive(false);
                // dots[index].position = dotOriginalPositions[index];
                // dots[index].gameObject.SetActive(true);
            }
        }
        else
        {
            print("NOT A DOT");
            print("COLLIDER TAG: " + collider.tag);
        }
    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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

            float fScale = (startSize + (float)playerAvatars[i].numDotsCollected * sizeIncrementOnCollect) * playerSeenScaler[i];


            players[i].transform.localScale = Vector3.one * fScale;

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
