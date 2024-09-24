using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryTreeManager : MonoBehaviour
{

    public List<StoryNode> nodes;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public AudioPlayer player;
    public AudioClip nodeEnteredClip;

    public ParticleSystem nodeEnteredParticleSystem;


    public MoveSceneBasedOnPlayers moveSceneBasedOnPlayers;

    public AudioClip storyFinishedClip;
    public ParticleSystem storyFinishedParticleSystem;

    public void OnStoryEntered(StoryNode node)
    {

        if (node.nextNodes.Count > 0)
        {
            for (int i = 0; i < node.nextNodes.Count; i++)
            {
                node.nextNodes[i].gameObject.SetActive(true);
            }

            player.Play(nodeEnteredClip);

            nodeEnteredParticleSystem.transform.position = node.transform.position;
            nodeEnteredParticleSystem.Play();
        }
        else
        {
            moveSceneBasedOnPlayers.Reset();


            player.Play(storyFinishedClip);
            storyFinishedParticleSystem.transform.position = Vector3.zero;
            storyFinishedParticleSystem.Play();
        }


    }


    public void OnStoryLeft(StoryNode node)
    {

    }


}
