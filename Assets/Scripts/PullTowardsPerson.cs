// Buddy: We each have a little sphere that we can control. It follows us around. 


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullTowardsPerson : MonoBehaviour
{
    public Controller controller;

    [Tooltip("The force used to pull the buddy towards the player.")]
    public float pullForce = 1.0f;

    [Tooltip("The Rigidbody component of the buddy.")]
    public Rigidbody rb;

    [Tooltip("The target transform the buddy is pulled towards.")]
    public Transform target;

    [Tooltip("The ID of the player being pulled towards.")]
    public int playerID;

    [Tooltip("The LineRenderer component to visualize the pull line.")]
    public LineRenderer lr;


    public float forceTowardsSphere = 1.0f;

    // Update is called once per frame
    void Update()
    {
        if (controller.numActivePlayers > playerID)
        {

            rb.AddForce((controller.players[playerID].transform.position - transform.position) * pullForce);

            rb.position = Vector3.MoveTowards(rb.position, controller.getFinalPosition(rb.position), forceTowardsSphere * Time.deltaTime);
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, controller.players[playerID].transform.position);

        }

    }
}
