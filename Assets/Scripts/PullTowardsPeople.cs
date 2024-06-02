using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullTowardsPeople : MonoBehaviour
{

    public Controller controller;
    public float pullForce = 1.0f;

    public Rigidbody rb;


    public float distancePitchFalloff = .3f;


    // Update is called once per frame
    void Update()
    {


        if (controller.numActivePlayers > 0)
        {
            rb.AddForce((controller.averagePosition - transform.position) * pullForce);

            for (int i = 0; i < controller.activePlayers.Count; i++)
            {
                GameObject player = controller.activePlayers[i].gameObject;
                LineRenderer lr = player.GetComponent<LineRenderer>();
                AudioSource audio = player.GetComponent<AudioSource>();

                float distance = Vector3.Distance(player.transform.position, transform.position);
                lr.SetPosition(0, player.transform.position);
                lr.SetPosition(1, transform.position);

                audio.pitch = Mathf.Clamp(5f - distance * distancePitchFalloff, 0, 10);
                //  audio.volume = 3f - distance / 10f;



            }
        }




    }


}
