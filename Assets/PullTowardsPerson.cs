using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullTowardsPerson : MonoBehaviour
{

    public Controller controller;
    public float pullForce = 1.0f;

    public Rigidbody rb;

    public Transform target;

    public int playerID;

    public LineRenderer lr;




    // Update is called once per frame
    void Update()
    {


        if (controller.numActivePlayers > playerID)
        {

            rb.AddForce((controller.players[playerID].transform.position - transform.position) * pullForce);
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, controller.players[playerID].transform.position);

        }





    }


}
