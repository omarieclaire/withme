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
    // Ensure there are active players before proceeding
    var activePlayers = controller.playerSetupManager.GetActivePlayers();
    if (activePlayers.Count > 0)
    {
        // Apply force based on average player position
        rb.AddForce((controller.averagePosition - transform.position) * pullForce);

        foreach (var playerInfo in activePlayers)
        {
            GameObject player = playerInfo.PlayerObject;
            LineRenderer lr = player.GetComponent<LineRenderer>();
            AudioSource audio = player.GetComponent<AudioSource>();

            // Calculate the distance between the player and the object
            float distance = Vector3.Distance(player.transform.position, transform.position);

            // Update the line renderer positions
            if (lr != null)
            {
                lr.SetPosition(0, player.transform.position);
                lr.SetPosition(1, transform.position);

                // Optionally set line thickness
                // lr.startWidth = lineStartWidth;
                // lr.endWidth = lineEndWidth;
            }

            // Adjust audio pitch based on distance (if needed)
            if (audio != null)
            {
                // Uncomment and adjust as needed
                // audio.pitch = Mathf.Clamp(5f - distance * distancePitchFalloff, 0, 10);
            }
        }
    }
}

}
