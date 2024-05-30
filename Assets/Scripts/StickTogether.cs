using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // Include the TextMeshPro namespace


public class StickTogether : MonoBehaviour
{
    public Controller controller;

    [Tooltip("The radius within which players need to stay.")]
    public float radiusForCollection = 1;

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

    [Tooltip("Offset for the movement of the collection area.")]
    public Vector3 movementOffset;

    [Header("Audio Info")]
    [Tooltip("Sound played when a player gets inside the circle.")]
    public AudioClip onPlayerGetsInside;

    [Tooltip("Sound played when the majority of players fall outside the circle.")]
    public AudioClip onMajorityOutside;

    public AudioPlayer audioPlayer;

    [Header("Timer Info")]
    [Tooltip("3D TextMesh component to display the timer.")]
    public TextMeshPro timerTextMesh;

    [Tooltip("Percentage of players required to be inside the circle to keep the timer running.")]
    public float requiredPercentage = 0.5f;

    private HashSet<GameObject> playersInside = new HashSet<GameObject>();
    private float timer = 0f;
    private bool majorityInside = false;

    void Start()
    {
        // Initialize connections list if null
        if (connections == null)
        {
            connections = new List<LineRenderer>();
        }

        // Initialize timer display
        if (timerTextMesh != null)
        {
            timerTextMesh.text = timer.ToString("F2");
        }
    }

    void Update()
    {
        // Make the collection area a sphere that grows/shrinks based on the radiusForCollection
        transform.localScale = new Vector3(radiusForCollection * 2, radiusForCollection * 2, radiusForCollection * 2);

        // Calculate the new position for the collection area using a wavy motion/sine wave
        Vector3 position = new Vector3(
            Mathf.Sin(Time.time * movementSpeed.x) * movementSize.x,
            Mathf.Sin(Time.time * movementSpeed.y) * movementSize.y,
            Mathf.Sin(Time.time * movementSpeed.z) * movementSize.z
        );

        // Add the starting point (offset) to the position
        position += movementOffset;

        // Set the position of the collection area
        transform.position = controller.getFinalPosition(position);

        // If the number of connections doesn't match the number of players
        if (connections.Count != controller.activePlayers.Count)
        {
            // Remove all existing connections (lines)
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            // Clear the list of connections
            connections.Clear();

            // Create new connection lines for each player
            for (int i = 0; i < controller.activePlayers.Count; i++)
            {
                // Make a new connection line at the collection area's position
                GameObject newConnection = Instantiate(connectionPrefab, transform.position, Quaternion.identity);
                newConnection.transform.parent = transform;
                // Add the new connection to the list
                connections.Add(newConnection.GetComponent<LineRenderer>());
            }
        }

        // Reset the count of players inside the circle
        totalInsideCircle = 0;

        // Check each player to see if they are inside the circle
        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            // Find the direction and distance from the player to the center of the collection area
            Vector3 dir = controller.activePlayers[i].transform.position - transform.position;
            float dist = dir.magnitude;

            // If the player is inside the circle
            if (dist < radiusForCollection)
            {
                // If the player wasn't inside before, play the sound
                if (!playersInside.Contains(controller.activePlayers[i].gameObject))
                {
                    playersInside.Add(controller.activePlayers[i].gameObject);
                    audioPlayer.Play(onPlayerGetsInside);
                }

                // Count this player as inside
                totalInsideCircle++;
                // Draw a line between the collection area and the player
                connections[i].positionCount = 2;
                connections[i].SetPosition(0, transform.position);
                connections[i].SetPosition(1, controller.activePlayers[i].transform.position);
            }
            else
            {
                // If the player is outside, don't draw a line and remove from the inside set
                connections[i].positionCount = 0;
                playersInside.Remove(controller.activePlayers[i].gameObject);
            }
        }

        // Check if the majority of players are inside the circle
        float requiredNumber = controller.activePlayers.Count * requiredPercentage;
        if (totalInsideCircle >= requiredNumber)
        {
            if (!majorityInside)
            {
                // Timer starts or continues incrementing
                majorityInside = true;
                collectionRepresentation.GetComponent<Renderer>().material.color = Color.green; // Change color to green
            }
            timer += Time.deltaTime;
        }
        else
        {
            if (majorityInside)
            {
                // Timer resets
                majorityInside = false;
                timer = 0f;
                collectionRepresentation.GetComponent<Renderer>().material.color = Color.red; // Change color to red
                audioPlayer.Play(onMajorityOutside); // Play sad sound
            }
        }

        // Update timer display
        if (timerTextMesh != null)
        {
            timerTextMesh.text = timer.ToString("F2");
        }
    }
}
