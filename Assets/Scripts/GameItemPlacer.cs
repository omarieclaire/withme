using UnityEngine;

public class GameItemPlacer : MonoBehaviour
{
    // Reference to NoGoZoneManager to handle no-go zones
    public NoGoZoneManager noGoZoneManager;

        public Controller controller;


    /// <summary>
    /// Places a game item in a random position, avoiding no-go zones, and maps the position with getFinalPositionObjects.
    /// </summary>
    public Vector3 PlaceItem(Transform itemHolder)
    {
        // Ensure NoGoZoneManager and colliders are set
        if (noGoZoneManager == null || noGoZoneManager.doorCollider == null ||
            noGoZoneManager.soundBoothCollider == null || noGoZoneManager.stageCollider == null)
        {
            Debug.LogError("NoGoZoneManager or one of its colliders is not assigned!");
            return Vector3.zero; // Return an invalid position
        }

        Vector3 randomPos;
        bool isBlockedByNoGoZone;

        // Loop to ensure the item is not placed inside any no-go zones
        do
        {
            // Generate random position
            randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 2f), Random.Range(-1f, 1f));
            randomPos = controller.getFinalPositionObjects(randomPos);  // Map the random position

            // Cast a ray from the center of the dome to the random position
            Ray ray = new Ray(Vector3.zero, randomPos.normalized); // Ensure direction is normalized
            RaycastHit hit;

            // Perform raycast to check if anything is between the center and the random position
            if (Physics.Raycast(ray, out hit, randomPos.magnitude))
            {
                // Check if the ray hits a forbidden zone
                isBlockedByNoGoZone = hit.collider == noGoZoneManager.doorCollider ||
                                      hit.collider == noGoZoneManager.soundBoothCollider ||
                                      hit.collider == noGoZoneManager.stageCollider;

                if (isBlockedByNoGoZone)
                {
                    Debug.Log($"Item placement blocked by {hit.collider.name} at position {randomPos}.");
                }
            }
            else
            {
                isBlockedByNoGoZone = false; // No collision, it's a valid position
            }

        } while (isBlockedByNoGoZone);  // Repeat if the position is blocked by a forbidden zone

        return randomPos; // Return the valid position after no-go zone checks
    }
}
