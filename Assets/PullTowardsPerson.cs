using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullTowardsPerson : MonoBehaviour
{

    public Controller controller;
    public float pullForce = 1.0f;

    public float pullDampening = 0.1f;
    public float nonPullDampening = 0.1f;

    public float pullPower = 1.0f;

    public Rigidbody rb;

    public Transform target;




    // Update is called once per frame
    void Update()
    {


        if (controller.numActivePlayers > 0)
        {

            rb.AddForce((target.position - transform.position) * pullForce);

        }





    }


}
