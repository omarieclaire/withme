using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishBye : MonoBehaviour
{

  private void OnTriggerEnter(Collider other)
{
    // Fish fish = other.GetComponent<Fish>();
    // if (fish != null && !fishCollisionStatus.ContainsKey(fish))
    // {
        Debug.Log("Fish entered the portal: "); // Debug message for collision detection
        // fishCollisionStatus[fish] = true; // Mark the fish as collided
        // TriggerportalColliderEffect(fish);
    }
}
// }
