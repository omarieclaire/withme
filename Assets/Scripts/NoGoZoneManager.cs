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

    // Store original positions and scales
    private Vector3 originalDoorPosition;
    private Vector3 originalSoundBoothPosition;
    private Vector3 originalStagePosition;

    private Vector3 originalDoorScale;
    private Vector3 originalSoundBoothScale;
    private Vector3 originalStageScale;

    void Start()
    {
        // Store the original positions and scales for each collider
        if (doorCollider != null)
        {
            originalDoorPosition = doorCollider.transform.position;
            originalDoorScale = doorCollider.transform.localScale;
        }
        if (soundBoothCollider != null)
        {
            originalSoundBoothPosition = soundBoothCollider.transform.position;
            originalSoundBoothScale = soundBoothCollider.transform.localScale;
        }
        if (stageCollider != null)
        {
            originalStagePosition = stageCollider.transform.position;
            originalStageScale = stageCollider.transform.localScale;
        }

        // Adjust the no-go zones as soon as the scene starts
        AdjustNoGoZones();
    }

    // Adjusts the no-go zones based on the sphere size
    public void AdjustNoGoZones()
    {
        if (controller == null)
        {
            // Debug.LogError("Controller reference is missing. Cannot adjust no-go zones based on sphere size.");
            return;
        }

        // Calculate scale factor based on the sphereSize (relative to the default sphereSize of 2)
        float scaleFactor = controller.sphereSize / 2f;

        // Scale and move the door collider
        if (doorCollider != null)
        {
            doorCollider.transform.localScale = originalDoorScale * scaleFactor;
            doorCollider.transform.position = originalDoorPosition * scaleFactor;
            // Debug.Log($"Door scaled and moved with scale factor {scaleFactor}. New position: {doorCollider.transform.position}");
        }

        // Scale and move the soundbooth collider
        if (soundBoothCollider != null)
        {
            soundBoothCollider.transform.localScale = originalSoundBoothScale * scaleFactor;
            soundBoothCollider.transform.position = originalSoundBoothPosition * scaleFactor;
            // Debug.Log($"SoundBooth scaled and moved with scale factor {scaleFactor}. New position: {soundBoothCollider.transform.position}");
        }

        // Scale and move the stage collider
        if (stageCollider != null)
        {
            stageCollider.transform.localScale = originalStageScale * scaleFactor;
            stageCollider.transform.position = originalStagePosition * scaleFactor;
            // Debug.Log($"Stage scaled and moved with scale factor {scaleFactor}. New position: {stageCollider.transform.position}");
        }
    }

    // Method to check no-go zones
    public bool IsInNoGoZone(Vector3 position)
    {
        if (position == null)
        {
            // Debug.LogError("Position is null! Unable to check no-go zones.");
            return false;
        }

        if (doorCollider != null && doorCollider.bounds.Contains(position))
        {
            // Debug.Log("Position is inside the Door no-go zone.");
            return true;
        }

        if (soundBoothCollider != null && soundBoothCollider.bounds.Contains(position))
        {
            // Debug.Log("Position is inside the SoundBooth no-go zone.");
            return true;
        }

        if (stageCollider != null && stageCollider.bounds.Contains(position))
        {
            // Debug.Log("Position is inside the Stage no-go zone.");
            return true;
        }

        // Debug.Log("Position is not inside any no-go zones.");
        return false;
    }
}
