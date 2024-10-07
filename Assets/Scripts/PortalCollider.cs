using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCollider : MonoBehaviour
{
    public Flock flock; // Reference to the Flock script to access fish

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the portal is a fish
        Fish fish = other.GetComponent<Fish>();
        if (fish != null)
        {
            // Trigger the transformation or destruction of the fish
            flock.TriggerportalColliderEffect(fish);
        }
    }
}
