using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoGoZoneManager : MonoBehaviour
{
    // List of no-go zone colliders
    public Collider doorCollider;
    public Collider soundBoothCollider;
    public Collider stageCollider;

    public Controller controller;

    // Method to check no-go zones
    public bool IsInNoGoZone(Vector3 position)
    {
        // Ensure the position passed in is valid
        if (position == null)
        {
            Debug.LogError("Position is null! Unable to check no-go zones.");
            return false;
        }

        // Check if no-go colliders are assigned and warn if any are missing
        if (doorCollider == null)
        {
            Debug.LogWarning("Door collider is not assigned in the NoGoZoneManager.");
        }
        if (soundBoothCollider == null)
        {
            Debug.LogWarning("Sound booth collider is not assigned in the NoGoZoneManager.");
        }
        if (stageCollider == null)
        {
            Debug.LogWarning("Stage collider is not assigned in the NoGoZoneManager.");
        }

        // Check if the position is in the Door no-go zone
        if (doorCollider != null && doorCollider.bounds.Contains(position))
        {
            Debug.Log("Position is inside the Door no-go zone.");
            return true;
        }

        // Check if the position is in the SoundBooth no-go zone
        if (soundBoothCollider != null && soundBoothCollider.bounds.Contains(position))
        {
            Debug.Log("Position is inside the SoundBooth no-go zone.");
            return true;
        }

        // Check if the position is in the Stage no-go zone
        if (stageCollider != null && stageCollider.bounds.Contains(position))
        {
            Debug.Log("Position is inside the Stage no-go zone.");
            return true;
        }

        // If the position is not inside any no-go zones
        Debug.Log("Position is not inside any no-go zones.");
        return false;
    }
}
