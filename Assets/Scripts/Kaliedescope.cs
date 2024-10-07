using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaliedescope : MonoBehaviour
{
    public float sizeMultiplier = .3f;
    [Tooltip("Number of iterations to create the kaleidoscope effect - how kaleidoscopey it is.")]
    public int numberOfIterations = 9; // Start at 9

    [Tooltip("Prefab used to instantiate each node of the kaleidoscope.")]
    public PlayerAvatar playerAvatar;

    public GameObject nodePrefab;
    public Controller controller;

    [Tooltip("List of instantiated nodes forming the kaleidoscope.")]
    public List<GameObject> nodes; // List to store instantiated nodes

    [Tooltip("How much the redrawings of the players move in and out.")]
    public float sphereOsscilationSize;
    [Tooltip("How fast the redrawings of the players move in and out.")]
    public float sphereOsscilationSpeed;

    // Variables for controlling the iterations over time
    public float iterationChangeDuration = 100f; // 2 minutes to go from 9 to 40
    public int minIterations = 9; // Start with 9 iterations
    public int maxIterations = 40; // Max number of iterations

    private float timeAtMinIterations = 20f; // Stay at 9 iterations for 15 seconds
    private float timeSinceChangeStart = 0f; // Track time since we started increasing the iterations
    private bool isChangingIterations = false; // Flag to indicate whether we are increasing iterations

    void Start()
    {
        // Use rainbow color logic for this specific game
        playerAvatar.useRainbowColorLogic = true;
    }

    void Update()
    {
        // Stay at 9 iterations for 15 seconds, then slowly increase over 2 minutes
        if (!isChangingIterations)
        {
            timeSinceChangeStart += Time.deltaTime;

            // After 15 seconds, start increasing iterations
            if (timeSinceChangeStart >= timeAtMinIterations)
            {
                isChangingIterations = true;
                timeSinceChangeStart = 0f; // Reset for the next phase
            }
        }
        else
        {
            // Slowly increase iterations from 9 to 40 over 2 minutes
            float t = Mathf.Clamp01(timeSinceChangeStart / iterationChangeDuration); // 't' will smoothly go from 0 to 1 over 2 minutes
            numberOfIterations = Mathf.FloorToInt(Mathf.Lerp(minIterations, maxIterations, t));

            timeSinceChangeStart += Time.deltaTime;

            // Optionally loop back (if you want to go back to 9 after reaching 40)
            if (timeSinceChangeStart >= iterationChangeDuration)
            {
                timeSinceChangeStart = 0f;
                isChangingIterations = false; // Reset and loop back
            }
        }

        float fRadius = controller.sphereSize + sphereOsscilationSize * Mathf.Sin(Time.time * sphereOsscilationSpeed);

        // Check if the number of nodes matches the required count
        if (nodes.Count != numberOfIterations * controller.activePlayers.Count)
        {
            // Destroy all existing nodes
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            nodes.Clear();

            // Instantiate new nodes based on numberOfIterations and player count
            for (int i = 0; i < numberOfIterations * controller.activePlayers.Count; i++)
            {
                GameObject node = Instantiate(nodePrefab, transform);
                node.SetActive(true);
                nodes.Add(node);

                // Assign the player's color to the node
                Color color = controller.activePlayers[i / numberOfIterations].color;
                TrailRenderer tr = node.GetComponent<TrailRenderer>();

                tr.material.SetColor("_Color", color);
                tr.material.SetColor("_BaseColor", color * .1f);
                tr.material.SetColor("_ReflectionColor", color);
                tr.material.SetColor("_CenterOrbColor", color * .1f);
                tr.material.SetColor("_NoiseColor", color * 2);

                // Set the color for the node's renderer
                node.transform.GetChild(0).transform.GetComponent<Renderer>().material.color = color;
            }
        }

        // Update the position, scale, and rotation of each node
        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            for (int j = 0; j < numberOfIterations; j++)
            {
                // Calculate rotation angle for the current iteration
                float rotation = 360.0f * (float)j / (float)numberOfIterations;
                int index = i * numberOfIterations + j;

                // Set node position to the player's position
                nodes[index].transform.position = controller.activePlayers[i].transform.position;
                nodes[index].transform.position = nodes[index].transform.position.normalized * fRadius;

                // Set node scale based on player's scale
                nodes[index].transform.localScale = controller.GetScale(i) * sizeMultiplier;

                // Rotate node around the origin (Vector3.zero)
                nodes[index].transform.RotateAround(Vector3.zero, Vector3.up, rotation);
            }
        }
    }
}
