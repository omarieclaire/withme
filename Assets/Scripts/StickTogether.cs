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

    [Tooltip("The minimum allowed Y position (bottom of the dome) to prevent the collection area from going underground.")]
    public float bottomOfDome = 0f;

    [Tooltip("Offset for the movement of the collection area.")]
    public Vector3 movementOffset;

    [Tooltip("Control points for the Bezier curve path.")]
    public Vector3[] bezierControlPoints = new Vector3[4];

    [Tooltip("Radius of the dome.")]
    public float domeRadius = 20;  // Adjust the dome radius as needed

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
        // Make the collection area a sphere that grows/shrinks based on the radiusForCollection
        transform.localScale = new Vector3(radiusForCollection * 2, radiusForCollection * 2, radiusForCollection * 2);

        Vector3 position;
        bool isBlockedByNoGoZone;

        // Loop to ensure the collection area is not placed in forbidden zones
        do
        {
            // Calculate the new position for the collection area using a wavy motion/sine wave
            position = new Vector3(
                Mathf.Sin(Time.time * movementSpeed.x) * movementSize.x,
                Mathf.Sin(Time.time * movementSpeed.y) * movementSize.y,
                Mathf.Sin(Time.time * movementSpeed.z) * movementSize.z
            ) + movementOffset;

            // Ensure the collection area stays outside the dome
            Vector3 outsidePosition = ProjectOutsideDome(position);

            // Cast a ray from the dome center (assumed to be (0,0,0)) to the collection area position
            Ray ray = new Ray(Vector3.zero, outsidePosition.normalized);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, outsidePosition.magnitude))
            {
                // Check if the ray hits a forbidden zone
                isBlockedByNoGoZone = hit.collider == noGoZoneManager.doorCollider ||
                                      hit.collider == noGoZoneManager.soundBoothCollider ||
                                      hit.collider == noGoZoneManager.stageCollider;

                if (isBlockedByNoGoZone)
                {
                    Debug.Log($"Collection area blocked by {hit.collider.name} at {outsidePosition}.");
                }
            }
            else
            {
                isBlockedByNoGoZone = false;  // No collision, it's a valid position
                Debug.Log($"Collection area is not blocked at position {outsidePosition}.");
            }

        } while (isBlockedByNoGoZone);  // Repeat if the position is blocked by a forbidden zone

        // Set the final position of the collection area
        transform.position = controller.getFinalPosition(position);
        Debug.Log($"[DEBUG] Final position for collection area after dome projection and no-go zone checks: {transform.position}");
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
            timerTextMesh.transform.LookAt(controller.getFinalPosition(Vector3.zero));
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
                    // audioPlayer.Play(happySound); // <-- deep for debugging

                    string soundID = $"p{controller.activePlayers[i].GetComponent<PlayerAvatar>().id}EffectsStickTogetherEntry";
                    Vector3 playerPosition = controller.activePlayers[i].transform.position;
                    soundEventSender.SendOneShotSound(soundID, playerPosition);
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

                string soundID = "StickTogetherBeat";
                Vector3 centerOfCollectionArea = transform.position;
                soundEventSender.SendOrUpdateContinuousSound(soundID, centerOfCollectionArea);
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

                // audioPlayer.Play(sadSound); // <-- deep for debugging
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
