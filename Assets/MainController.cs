using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{



    [Header("Camera Live Pose Feed Info")]
    public int cameraResolution = 640;
    public Vector2 RemapValues = new Vector2(10, -10);



    [Header("Positioning Info")]
    public float verticalValue;
    public float sphereSize;
    public Vector3 maxSize;




    [Header("Player Info")]

    public List<GameObject> players;
    public List<PlayerAvatar> playerAvatars;
    public List<int> playerIDS;

    // Speed at which players lerp towards their target position,
    // smaller value means slower speed

    public GameObject playerPrefab;
    public float startSize;
    public float sizeIncrementOnCollect;

    public Transform playerHolder;


    [Header("Collectable Info")]
    public int numDots;

    public GameObject dotPrefab;

    public float dotSize;
    public float dotFlatnessPower;

    public List<Transform> dots;
    public List<Dot> dotAvatars;

    public Transform dotHolder;

    [Header("Movement Info")]

    public float playerLerpSpeed;
    public float dotForceTowardsPosition;
    public float dotDampening;


    [Header("Game Info")]
    public int minNumDotsForCollision;

    [Header("Audio Info")]

    public AudioClip onDotCollectClip;
    public AudioClip onExplodeClip;

    public AudioPlayer audioPlayer;


    public ParticleSystem explosionParticles;
    // Center object just Camera, for looking at / orienting players
    public Transform center;
    public void OnEnable()
    {

        players = new List<GameObject>();
        playerAvatars = new List<PlayerAvatar>();
        playerIDS = new List<int>();



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
        int index = playerIDS.IndexOf(playerID);
        if (index != -1)
        {
            players[index].transform.position = Vector3.Lerp(players[index].transform.position, getFinalPosition(remappedPosition), playerLerpSpeed);
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

                player.transform.localScale += Vector3.one * sizeIncrementOnCollect;
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
