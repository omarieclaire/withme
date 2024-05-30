
// Kali: People collectively draw on the dome, kaleidoscope style, by moving their bodies
// TODO: Unique colour for each player 
// TODO: Sound? I don't know when a sound could play without sounding like a cacophony.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaliedescope : MonoBehaviour
{

    public float sizeMultiplier = .3f;
    public int numberOfIterations = 10;

    public GameObject nodePrefab;
    public Controller controller;

    public List<GameObject> nodes; // List to store instantiated nodes

    void Start()
    {
        // Initialization if needed
    }

    void Update()
    {
        // Check if the number of nodes matches the required count
        if (nodes.Count != numberOfIterations * controller.activePlayers.Count)
        {

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

                Color color = controller.activePlayers[i / numberOfIterations].color;
                TrailRenderer tr = node.GetComponent<TrailRenderer>();

                tr.material.SetColor("_Color", color);
                tr.material.SetColor("_BaseColor", color * .1f);
                tr.material.SetColor("_ReflectionColor", color);
                tr.material.SetColor("_CenterOrbColor", color * .1f);
                tr.material.SetColor("_NoiseColor", color * 2);
                node.transform.GetChild(0).transform.GetComponent<Renderer>().material.color = controller.activePlayers[i / numberOfIterations].color;

            }
        }

        // Update the position, scale, and rotation of each node
        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            for (int j = 0; j < numberOfIterations; j++)
            {
                // Calculate rotation angle for current iteration
                float rotation = 360.0f * (float)j / (float)numberOfIterations;
                int index = i * numberOfIterations + j;

                // Set node position to the player's position
                nodes[index].transform.position = controller.activePlayers[i].transform.position;

                // Set node scale based on player's scale
                nodes[index].transform.localScale = controller.GetScale(i) * sizeMultiplier;

                // Rotate node around the origin (Vector3.zero)
                nodes[index].transform.RotateAround(Vector3.zero, Vector3.up, rotation);
            }
        }
    }
}
