using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaliedescope : MonoBehaviour
{
    public CameraAndPlayAreaSettings cameraAndPlayAreaSettings;

    [Tooltip("Number of iterations to create the kaleidoscope effect - how kaleidoscopey it is.")]
    public int numberOfIterations = 10;

    [Tooltip("Prefab used to instantiate each node of the kaleidoscope.")]
    public GameObject nodePrefab;

    public Controller controller;

    [Tooltip("List of instantiated nodes forming the kaleidoscope.")]
    public List<GameObject> nodes; // List to store instantiated nodes

    [Tooltip("How much the redrawings of the players move in and out.")]
    public float sphereOsscilationSize;

    [Tooltip("How fast the redrawings of the players move in and out.")]
    public float sphereOsscilationSpeed;

    [Tooltip("Multiplier for node sizes.")]
    public float sizeMultiplier = 0.3f;

    void Update()
    {
        float fRadius = cameraAndPlayAreaSettings.playAreaSphereSize + sphereOsscilationSize * Mathf.Sin(Time.time * sphereOsscilationSpeed);

        // Get active players
        var activePlayers = controller.playerSetupManager.GetActivePlayers();
        int activePlayerCount = activePlayers.Count;

        // Check if the number of nodes matches the required count
        if (nodes.Count != numberOfIterations * activePlayerCount)
        {
            // Destroy all existing nodes
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            nodes.Clear();

            // Instantiate new nodes based on numberOfIterations and player count
            for (int i = 0; i < numberOfIterations * activePlayerCount; i++)
            {
                GameObject node = Instantiate(nodePrefab, transform);
                node.SetActive(true);
                nodes.Add(node);

                // Get the corresponding player info
                var playerInfo = activePlayers[i / numberOfIterations];
                GameObject playerObject = playerInfo.PlayerObject;

                // Access the player's color
                var avatar = playerObject.GetComponent<PlayerAvatar>();
                Color color = avatar != null ? avatar.color : Color.white;

                // Configure the TrailRenderer's materials with the player's color
                TrailRenderer tr = node.GetComponent<TrailRenderer>();
                if (tr != null)
                {
                    tr.material.SetColor("_Color", color);
                    tr.material.SetColor("_BaseColor", color * 0.1f);
                    tr.material.SetColor("_ReflectionColor", color);
                    tr.material.SetColor("_CenterOrbColor", color * 0.1f);
                    tr.material.SetColor("_NoiseColor", color * 2);
                }

                // Set the child object's material color
                Renderer renderer = node.transform.GetChild(0).GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }

        // Update the position, scale, and rotation of each node
        for (int i = 0; i < activePlayerCount; i++)
        {
            for (int j = 0; j < numberOfIterations; j++)
            {
                // Calculate rotation angle for the current iteration
                float rotation = 360.0f * j / numberOfIterations;
                int index = i * numberOfIterations + j;

                // Get the player's position and scale
                var playerInfo = activePlayers[i];
                GameObject playerObject = playerInfo.PlayerObject;

                // Set node position to the player's position
                nodes[index].transform.position = playerObject.transform.position;
                nodes[index].transform.position = nodes[index].transform.position.normalized * fRadius;

                // Set node scale using a helper function for scaling
float scale = controller.playerSetupManager.GetPlayerScale(playerInfo.PlayerID).x;
                nodes[index].transform.localScale = Vector3.one * scale * sizeMultiplier;

                // Rotate node around the origin (Vector3.zero)
                nodes[index].transform.RotateAround(Vector3.zero, Vector3.up, rotation);
            }
        }
    }
}
