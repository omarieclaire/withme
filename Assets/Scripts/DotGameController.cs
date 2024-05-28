using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DotGameController : Controller
{




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

    public float dotForceTowardsPosition;
    public float dotDampening;


    [Header("Game Info")]
    public int minNumDotsForCollision;
    public int maxDotsPerPlayer;


    [Header("Audio Info")]

    // the sound when the dot is collected

    public AudioClip onDotCollectClip;

    // the sound when player collide

    public AudioClip onExplodeClip;

    public AudioPlayer audioPlayer;


    public ParticleSystem explosionParticles;
    public ParticleSystem playerCollectDotParticleSystem;
    // Center object just Camera, for looking at / orienting players

    // Center object just Camera, for looking at / orienting players


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



}
