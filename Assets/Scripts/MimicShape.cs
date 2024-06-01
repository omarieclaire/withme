using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicShape : MonoBehaviour
{
    // Public variables to be set in the Unity Editor
    public int numSpheres;
    public List<GameObject> spheres;
    public GameObject spherePrefab;
    public Controller controller;
    public List<bool> spheresActive; // List to track the active state of each sphere
    public float sphereSize;

    // Called when the script is enabled
    public void OnEnable()
    {
        // Destroy any existing child GameObjects
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Initialize the lists
        spheres = new List<GameObject>();
        spheresActive = new List<bool>();

        // Create the specified number of spheres
        for (int i = 0; i < numSpheres; i++)
        {
            // Instantiate a new sphere and set its properties
            GameObject sphere = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
            sphere.transform.position = Vector3.zero;
            sphere.transform.localScale = Vector3.one * sphereSize;
            sphere.transform.SetParent(transform);
            sphere.name = $"Sphere_{i}";
            spheres.Add(sphere);
            spheresActive.Add(false);
        }

        // Initialize a new shape set
        NewShapeSet();
    }

    // Variables to control shape activation
    public int numShapesActivated; // Number of activated shapes

    // Sets a new shape by randomizing sphere positions
    public void NewShapeSet()
    {
        numShapesActivated = 0;
        lastTimeChange = Time.time;

        // Set each sphere to a new random position
        for (int i = 0; i < numSpheres; i++)
        {
            spheresActive[i] = false;

            Vector3 randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 2f), Random.Range(-1f, 1f));
            randomPos = controller.getFinalPosition(randomPos);

            spheres[i].transform.position = randomPos;
        }
    }

    // Variables for time-based shape changes
    [Tooltip("Time interval between shape changes")]

    public float timeBetweenChanges;

    [Tooltip("Time of the last shape change")]

    public float lastTimeChange;

    [Tooltip("Radius within which a sphere gets activated")]

    public float activationRadius;

    // Update is called once per frame
    void Update()
    {
        // Check if it's time to change the shape
        if (Time.time - lastTimeChange > timeBetweenChanges)
        {
            lastTimeChange = Time.time;
            NewShapeSet();
        }

        // Check the activation state of each sphere
        for (int j = 0; j < numSpheres; j++)
        {
            bool sphereActive = false;

            // Check each active player's distance to the sphere
            for (int i = 0; i < controller.activePlayers.Count; i++)
            {
                GameObject player = controller.activePlayers[i].gameObject;
                float distance = Vector3.Distance(player.transform.position, spheres[j].transform.position);

                // Activate the sphere if a player is within the activation radius
                if (distance < activationRadius)
                {
                    sphereActive = true;
                }
            }

            // Set the sphere's color and active state based on its activation status
            if (sphereActive)
            {
                spheres[j].GetComponent<Renderer>().material.color = Color.green;
                spheresActive[j] = true;
            }
            else
            {
                spheres[j].GetComponent<Renderer>().material.color = Color.red;
                spheresActive[j] = false;
            }
        }

        // Count the number of activated spheres
        numShapesActivated = 0;
        for (int i = 0; i < numSpheres; i++)
        {
            if (spheresActive[i])
            {
                numShapesActivated++;
            }
        }

        // Trigger the shape complete event if all spheres are activated
        if (numShapesActivated == numSpheres)
        {
            OnShapeComplete();
        }
    }

    // References for audio and particle effects
    public AudioPlayer audio; 
    public AudioClip onShapeCompleteClip; 
    public ParticleSystem onShapeCompleteParticles; 

    // Called when the shape is complete
    void OnShapeComplete()
    {
        // Play audio and particle effects
        audio.Play(onShapeCompleteClip);
        onShapeCompleteParticles.transform.position = transform.position;
        onShapeCompleteParticles.Play();

        // Set a new shape
        NewShapeSet();
    }
}
