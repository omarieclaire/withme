using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullTowardsPeople : MonoBehaviour
{
    public Controller controller;
    public float pullForce = 1.0f;
    public Rigidbody rb;
    public float distancePitchFalloff = .3f;
    
    // Public variables to control line width
    // public float lineStartWidth = 0.1f; // Adjust as needed
    // public float lineEndWidth = 0.1f;   // Adjust as needed

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

                // Set line thickness
                // lr.startWidth = lineStartWidth;
                // lr.endWidth = lineEndWidth;

                // Adjust audio pitch based on distance
                audio.pitch = Mathf.Clamp(5f - distance * distancePitchFalloff, 0, 10);
            }
        }
    }
}
