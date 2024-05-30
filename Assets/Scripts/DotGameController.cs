// With me: Players “eat” dots to grow bigger. When players collide with each other, those dots go to regenerate the world!
// TODO: regenerating the earth


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DotGameController : Controller
{


    public Transform tree;

    [Header("Collectable Info")]
    [Tooltip("Number of dots in the game.")]
    public int numDots;

    [Tooltip("Prefab used to instantiate dots.")]
    public GameObject dotPrefab;

    [Tooltip("Actual size of the dot.")]
    public float dotSize;

    [Tooltip("Lower value spawns near the top of the dome, a higher value towards the ring of the dome.")]
    public float dotFlatnessPower;

    [Tooltip("List of transforms for each dot in the game.")]
    public List<Transform> dots;

    [Tooltip("List of Dot objects representing the dot avatars.")]
    public List<Dot> dotAvatars;

    [Tooltip("Transform that holds all the dot objects.")]
    public Transform dotHolder;
    [Tooltip("How slowly the dot moves to the new position; 1 means it will move exactly, 0 means it will never move.")]
    public float dotForceTowardsPosition;

    [Tooltip("Dampening factor for the dot's movement.")]
    public float dotDampening;

    [Header("Game Info")]
    [Tooltip("Minimum number of dots required for the possibility of a player collision.")]
    public int minNumDotsForCollision;

    [Tooltip("Maximum number of dots a player can collect.")]
    public int maxDotsPerPlayer;

    [Tooltip("Size increment of the player when a dot is collected.")]
    public float sizeIncrementOnCollect;


    [Header("Audio Info")]

    // the sound when the dot is collected
    [Tooltip("Sound played when a dot is collected.")]
    public AudioClip onDotCollectClip;

    // the sound when player collide
    [Tooltip("Sound played when players collide.")]
    public AudioClip onExplodeClip;

    public AudioPlayer audioPlayer;

    public ParticleSystem explosionParticles;
    public ParticleSystem playerCollectDotParticleSystem;



    public int totalDotsCollected;
    public int totalDotsCollectedForCompletion;



    public float dotForceTowardsCollector;
    public float dotForceTowardsTree;




    public override void SetUp()
    {
        _SetUp();


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

    public override Vector3 GetScale(int i)
    {
        float fScale = (startSize + (float)playerAvatars[i].numDotsCollected * sizeIncrementOnCollect) * playerSeenScaler[i];


        return Vector3.one * fScale;
    }

    public override void OnPlayersWithDotsCollided(PlayerAvatar p1, PlayerAvatar p2)
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
                dotAvatars[i].OnPlayersHoldingMeCollided();
            }
        }



        p1.Reset();
        p2.Reset();

        audioPlayer.Play(onExplodeClip);
        explosionParticles.transform.position = (p1.transform.position + p2.transform.position) / 2;
        explosionParticles.Play();
    }

    public override void OnPlayerTrigger(PlayerAvatar player, GameObject collider)
    {
        //Debug.Log("Player Trigger");
        // Check if the collider is a dot
        if (collider.CompareTag("Dot"))
        {
            int index = dots.IndexOf(collider.transform);
            if (index != -1)
            {

                // Dont collect if we have reached the max
                if (player.numDotsCollected > maxDotsPerPlayer)
                {
                    return;
                }

                // Dont Recollect
                if (dots[index].GetComponent<Dot>().collected)
                {
                    return;
                }

                dotAvatars[index].collected = true;
                dotAvatars[index].collector = player.transform;
                player.OnDotCollect(player.numDotsCollected >= minNumDotsForCollision, player.numDotsCollected >= maxDotsPerPlayer);

                audioPlayer.Play(onDotCollectClip);
                playerCollectDotParticleSystem.transform.position = collider.transform.position;
                playerCollectDotParticleSystem.Play();
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

    public void OnTreeCollect()
    {
        totalDotsCollected++;
        if (totalDotsCollected >= totalDotsCollectedForCompletion)
        {
            OnLevelComplete();
        }
    }
    public void OnLevelComplete()
    {
        print("LEVEL COMPLETE");
    }



}
