using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StickTogether : MonoBehaviour
{
    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager

    public Controller controller;
    public SoundEventSender soundEventSender;

    [Tooltip("The radius of the area we need to stay inside.")]
    public float radiusForCollection;

    [Tooltip("The representation of the collection area.")]
    public GameObject collectionRepresentation;

    [Tooltip("List of LineRenderers used to visualize connections between players.")]
    public List<LineRenderer> connections;

    [Tooltip("Prefab used to create new connection lines.")]
    public GameObject connectionPrefab;

    [Tooltip("Total number of players inside the collection circle.")]
    public int totalInsideCircle = 0;

    [Tooltip("Speed at which the collection area moves.")]
    public Vector3 movementSpeed;



    [Tooltip("Size of the movement area for the collection.")]
    public Vector3 movementSize;

    [Tooltip("Reference to the floor GameObject")]
    public GameObject floor;

    [Tooltip("Minimum distance to keep from the floor")] // remove me?
    public float minDistanceFromFloor = 1f;

    [Tooltip("The minimum allowed Y position (bottom of the dome) to prevent the collection area from going underground.")]
    public float bottomOfDome = 0f;

    [Tooltip("Offset for the movement of the collection area.")]
    public Vector3 movementOffset;

    [Tooltip("Control points for the Bezier curve path.")]
    public Vector3[] bezierControlPoints = new Vector3[4];

    [Tooltip("Radius of the dome.")]
    private float domeRadius;

    [Tooltip("Temporary visual representation of the dome (for testing purposes).")]
    public GameObject tempDomeVisual;  // Assign a sphere or dome mesh for testing

    [Header("Audio Info")]
    public AudioClip happySound;
    public AudioClip sadSound;
    public AudioPlayer audioPlayer;

    [Header("Timer Info")]
    public TextMeshPro timerTextMesh;
    public Vector3 timerTextOffset;

    [Tooltip("Percentage of players required to be inside the circle to keep the timer running.")]
    public float requiredPercentage = 0.5f;

    private HashSet<GameObject> playersInside = new HashSet<GameObject>();
    private float timer = 0f;
    private bool majorityInsideStartTheParty = false;

    private Vector3 defaultSoundPosition = new Vector3(1f, 1f, 0.01f);

    public AudioSource loop;

    void Start()
    {
        InitializeConnections();      // Call this to initialize connections
        InitializeTimerDisplay();     // Call this to initialize timer display
        InitializeBezierControlPoints(); // Call this to initialize Bezier control points

        Controller controller = FindObjectOfType<Controller>();
        if (controller != null)
        {
            domeRadius = controller.sphereSize;
        }
        else
        {
            Debug.LogError("Controller not found in the scene!");
        }

        // Set bottomOfDome based on the floor's position
        if (floor != null)
        {
            bottomOfDome = floor.transform.position.y + minDistanceFromFloor;
        }
        else
        {
            Debug.LogWarning("Floor object not assigned!");
        }
    }

    // Initialize Bezier control points
    void InitializeBezierControlPoints()
    {
        bezierControlPoints[0] = controller.getFinalPosition(new Vector3(0, bottomOfDome + 5f, 0));
        bezierControlPoints[1] = controller.getFinalPosition(new Vector3(3f, bottomOfDome + 8f, 5f));
        bezierControlPoints[2] = controller.getFinalPosition(new Vector3(-3f, bottomOfDome + 8f, -5f));
        bezierControlPoints[3] = controller.getFinalPosition(new Vector3(0, bottomOfDome + 5f, -10f));
    }

    // Initialize connections
    void InitializeConnections()
    {
        if (connections == null)
        {
            connections = new List<LineRenderer>();
        }
    }

    // Initialize timer display
    void InitializeTimerDisplay()
    {
        if (timerTextMesh != null)
        {
            timerTextMesh.text = timer.ToString("F2");
        }
    }

    void Update()
    {
        UpdateCollectionAreaPosition();
        UpdateTimerDisplay();

        if (connections.Count != controller.activePlayers.Count)
        {
            UpdateConnections();
        }

        CheckPlayerPositions();
        UpdateTimerBasedOnPlayers();
        UpdateLoopVolume();

        // Check if the collection area intersects with the dome
        CheckIntersectionWithDome();
    }

    void UpdateCollectionAreaPosition()
{
    transform.localScale = Vector3.one * (radiusForCollection * 2);

    Vector3 position = CalculateNewPosition();
    Debug.Log($"[DEBUG] Initial calculated position: {position}");

    Vector3 projectedPosition = ProjectOntoDomeSurface(position);
    Debug.Log($"[DEBUG] Projected position: {projectedPosition}");

    Vector3 adjustedPosition = AdjustForFloorDistance(projectedPosition);
    Debug.Log($"[DEBUG] Adjusted position: {adjustedPosition}");

    bool isBlockedByNoGoZone = CheckNoGoZoneCollision(adjustedPosition);

    if (!isBlockedByNoGoZone)
    {
        Debug.Log($"[DEBUG] Position before getFinalPosition: {adjustedPosition}");
        Vector3 finalPosition = controller.getFinalPosition(adjustedPosition);
        Debug.Log($"[DEBUG] Position after getFinalPosition: {finalPosition}");

        // Ensure the final position is not below the floor
        if (finalPosition.y < bottomOfDome)
        {
            Debug.Log($"[DEBUG] Final position below floor, adjusting...");
            float distanceFromCenter = new Vector2(finalPosition.x, finalPosition.z).magnitude;
            float newY = Mathf.Max(bottomOfDome, finalPosition.y);
            
            // Calculate the maximum allowed distance from the center based on the dome's radius
            float maxAllowedDistance = Mathf.Sqrt(domeRadius * domeRadius - newY * newY);
            
            // If the current distance is greater than the maximum allowed, scale it down
            if (distanceFromCenter > maxAllowedDistance)
            {
                float scaleFactor = maxAllowedDistance / distanceFromCenter;
                finalPosition.x *= scaleFactor;
                finalPosition.z *= scaleFactor;
            }
            
            finalPosition.y = newY;
            
            Debug.Log($"[DEBUG] Adjusted final position: {finalPosition}");
        }

        // Ensure the position is within the dome
        float distanceFromOrigin = finalPosition.magnitude;
        if (distanceFromOrigin > domeRadius)
        {
            finalPosition = finalPosition.normalized * domeRadius;
            Debug.Log($"[DEBUG] Position outside dome, scaled to: {finalPosition}");
        }

        transform.position = finalPosition;
        Debug.Log($"[DEBUG] Final position set for collection area: {finalPosition}");
    }
    else
    {
        Debug.Log("[DEBUG] Position blocked by no-go zone. Keeping previous position.");
    }
}

    /// <summary>
    /// Updates the size of the collection area based on the radiusForCollection.
    /// </summary>
    void UpdateCollectionAreaSize()
    {
        transform.localScale = Vector3.one * (radiusForCollection * 2);
        Debug.Log($"[DEBUG] Updated collection area size: {transform.localScale}");
    }

    /// <summary>
    /// Calculates a new position for the collection area using a wavy motion.
    /// </summary>
    /// <returns>The calculated position.</returns>
    Vector3 CalculateNewPosition()
    {
        Vector3 newPosition = new Vector3(
            Mathf.Sin(Time.time * movementSpeed.x) * movementSize.x,
            Mathf.Sin(Time.time * movementSpeed.y) * movementSize.y,
            Mathf.Sin(Time.time * movementSpeed.z) * movementSize.z
        ) + movementOffset;

        Debug.Log($"[DEBUG] Calculated new position: {newPosition}");
        return newPosition;
    }

    /// <summary>
    /// Projects the given position onto the dome's surface.
    /// </summary>
    /// <param name="position">The position to project.</param>
    /// <returns>The projected position on the dome's surface.</returns>
    Vector3 ProjectOntoDomeSurface(Vector3 position)
    {
        Vector3 directionFromCenter = position.normalized;
        Vector3 projectedPosition = directionFromCenter * (domeRadius - radiusForCollection);
        Debug.Log($"[DEBUG] Projected position onto dome surface: {projectedPosition}");
        return projectedPosition;
    }

   Vector3 AdjustForFloorDistance(Vector3 position)
{
    if (position.y < bottomOfDome)
    {
        float distanceFromCenter = new Vector2(position.x, position.z).magnitude;
        float newY = Mathf.Max(bottomOfDome, position.y);
        
        // Calculate the maximum allowed distance from the center based on the dome's radius
        float maxAllowedDistance = Mathf.Sqrt(domeRadius * domeRadius - newY * newY);
        
        // If the current distance is greater than the maximum allowed, scale it down
        if (distanceFromCenter > maxAllowedDistance)
        {
            float scaleFactor = maxAllowedDistance / distanceFromCenter;
            position.x *= scaleFactor;
            position.z *= scaleFactor;
        }
        
        position.y = newY;
        
        Debug.Log($"[DEBUG] AdjustForFloorDistance: Input={position}, Output={position}");
    }
    return position;
}

    /// <summary>
    /// Checks if the given position collides with any no-go zones.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True if the position is blocked by a no-go zone, false otherwise.</returns>
    bool CheckNoGoZoneCollision(Vector3 position)
    {
        Ray ray = new Ray(Vector3.zero, position.normalized);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, position.magnitude))
        {
            bool isBlocked = hit.collider == noGoZoneManager.doorCollider ||
                             hit.collider == noGoZoneManager.soundBoothCollider ||
                             hit.collider == noGoZoneManager.stageCollider;

            if (isBlocked)
            {
                Debug.Log($"[DEBUG] Collection area blocked by {hit.collider.name} at {position}.");
            }

            return isBlocked;
        }

        Debug.Log($"[DEBUG] No collision detected at position: {position}");
        return false;
    }

    /// <summary>
    /// Sets the final position of the collection area.
    /// </summary>
    /// <param name="position">The position to set.</param>
    void SetFinalPosition(Vector3 position)
    {
        Vector3 finalPosition = controller.getFinalPosition(position);
        transform.position = finalPosition;
        Debug.Log($"[DEBUG] Final position set for collection area: {finalPosition}");
    }



    // Calculate a cubic Bezier curve point
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // First term
        p += 3 * uu * t * p1; // Second term
        p += 3 * u * tt * p2; // Third term
        p += ttt * p3; // Fourth term

        return p;
    }

    // Ensure the collection area stays outside the dome
    Vector3 ProjectOutsideDome(Vector3 position)
    {
        // Calculate the distance from the dome's center (assumed to be (0,0,0))
        float distanceFromCenter = Vector3.Distance(position, Vector3.zero);

        // Log distance information for debugging
        Debug.Log($"[DEBUG] Initial distance from center: {distanceFromCenter}, Dome radius: {domeRadius}");

        // If the position is inside or too close to the dome, push it outside
        if (distanceFromCenter < domeRadius + radiusForCollection)
        {
            // Get the direction from the center of the dome to the position
            Vector3 directionFromCenter = position.normalized;

            // Ensure it's placed outside the dome by moving it to the dome radius plus the collection area's radius
            Vector3 adjustedPosition = directionFromCenter * (domeRadius + radiusForCollection);

            // Log the adjustment process
            Debug.Log($"[DEBUG] Collection area pushed outside the dome. Adjusted position: {adjustedPosition}, Dome radius: {domeRadius}");

            return adjustedPosition;
        }

        // If the position is already outside the dome, return the original position
        Debug.Log($"[DEBUG] Collection area is already outside the dome. Position: {position}, Distance from center: {distanceFromCenter}");
        return position;
    }






    // Check if the collection area intersects with the dome
    void CheckIntersectionWithDome()
    {
        float distanceFromCenter = transform.position.magnitude;

        if (distanceFromCenter > domeRadius)
        {
            Debug.Log($"[DEBUG] Collection area is outside the dome. Distance: {distanceFromCenter}, Dome radius: {domeRadius}");
        }
        else if (distanceFromCenter < domeRadius)
        {
            Debug.Log($"[DEBUG] Collection area is inside the dome. Distance: {distanceFromCenter}, Dome radius: {domeRadius}");
        }

        // Visualize the temporary dome for testing purposes
        if (tempDomeVisual != null)
        {
            tempDomeVisual.transform.localScale = new Vector3(domeRadius * 2, domeRadius * 2, domeRadius * 2);
        }
    }

    // Move code for updating timer text
    void UpdateTimerDisplay()
    {
        if (timerTextMesh != null)
        {
            timerTextMesh.transform.position = transform.position + timerTextOffset;
            float scaleFactor = 7.1f;
            timerTextMesh.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            // Use LookAt to face the center of the dome
            timerTextMesh.transform.LookAt(Vector3.zero);
            timerTextMesh.transform.Rotate(0, 180, 0); // Flip to correct orientation
        }
    }

    // Handle connection line updates
    void UpdateConnections()
    {
        // Remove all existing connections
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Clear the list of connections
        connections.Clear();

        // Create new connection lines for each player
        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            GameObject newConnection = Instantiate(connectionPrefab, transform.position, Quaternion.identity);
            newConnection.transform.parent = transform;
            connections.Add(newConnection.GetComponent<LineRenderer>());
        }
    }

    // Check player positions relative to the collection area
    void CheckPlayerPositions()
    {
        totalInsideCircle = 0;

        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            Vector3 dir = controller.activePlayers[i].transform.position - transform.position;
            float dist = dir.magnitude;

            if (dist < radiusForCollection)
            {
                if (!playersInside.Contains(controller.activePlayers[i].gameObject))
                {
                    playersInside.Add(controller.activePlayers[i].gameObject);

                    if (Controller.enableOldSoundSystem && happySound != null)
                    {
                        audioPlayer.Play(happySound); // <-- deep for debugging
                    }

                    if (Controller.enableNewSoundSystem)

                    {
                        string soundID = $"p{controller.activePlayers[i].GetComponent<PlayerAvatar>().id}EffectsStickTogetherEntry";
                        Vector3 playerPosition = controller.activePlayers[i].transform.position;
                        soundEventSender.SendOneShotSound(soundID, playerPosition);
                    }
                }

                totalInsideCircle++;
                connections[i].positionCount = 2;
                connections[i].SetPosition(0, transform.position);
                connections[i].SetPosition(1, controller.activePlayers[i].transform.position);
            }
            else
            {
                connections[i].positionCount = 0;
                playersInside.Remove(controller.activePlayers[i].gameObject);
            }
        }
    }

    // Update the timer based on the number of players inside the circle
    void UpdateTimerBasedOnPlayers()
    {
        float requiredNumber = controller.activePlayers.Count * requiredPercentage;

        if (totalInsideCircle >= requiredNumber)
        {
            if (!majorityInsideStartTheParty)
            {
                majorityInsideStartTheParty = true;
                if (Controller.enableNewSoundSystem)

                {
                    string soundID = "StickTogetherBeat";
                    Vector3 centerOfCollectionArea = transform.position;
                    soundEventSender.SendOrUpdateContinuousSound(soundID, centerOfCollectionArea);
                }
            }
            timer += Time.deltaTime;
        }
        else
        {
            if (majorityInsideStartTheParty)
            {
                majorityInsideStartTheParty = false;
                timer = 0f;

                string soundID = "StickTogetherBeat";
                Vector3 centerOfCollectionArea = transform.position;
                soundEventSender.StopContinuousSound(soundID, defaultSoundPosition);

                if (Controller.enableOldSoundSystem && sadSound != null)
                {
                    audioPlayer.Play(sadSound); // <-- old sound keep for debugging

                }
                if (Controller.enableNewSoundSystem)
                {
                    // string soundID = $"p{player.id}EffectsWithMePointCollision";
                    // Vector3 pointPosition = player.transform.position;
                    // soundEventSender.SendOneShotSound(soundID, pointPosition);
                }
            }
        }
    }

    // Update the volume of the loop audio
    void UpdateLoopVolume()
    {
        loop.volume = Mathf.Lerp(loop.volume, Mathf.Clamp(timer, 0, 1), .03f);

        if (timerTextMesh != null)
        {
            timerTextMesh.text = timer.ToString("F2");
        }
    }
}
