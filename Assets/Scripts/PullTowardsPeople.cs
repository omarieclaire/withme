using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullTowardsPeople : MonoBehaviour
{

    public Controller controller;
    public float pullForce = 1.0f;

    public Rigidbody rb;



    // Update is called once per frame
    void Update()
    {


        if (controller.numActivePlayers > 0)
        {
            rb.AddForce((controller.averagePosition - transform.position) * pullForce);
        }




    }


}