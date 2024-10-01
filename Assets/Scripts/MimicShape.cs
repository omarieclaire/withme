using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicShape : MonoBehaviour
{
    // Public variables to be set in the Unity Editor
    public int numSpheres;

    public SoundEventSender soundEventSender;  

    public List<GameObject> spheres;
    public GameObject spherePrefab;
    public Controller controller;
    public List<bool> spheresActive; // List to track the active state of each sphere
    public float sphereSize;
    public Timer timer;

    public Transform lineRendererHolder;
    public GameObject lineRendererPrefab;

    // Time-based variables
    public float timeBetweenChanges;
    public float lastTimeChange;
    public float activationRadius;

    private float oTime;
    private float nTime;

    public float tickRate = .1f;

    // Number of activated shapes
    public int numShapesActivated;

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

        // Play New Constellation Sound
        string soundID = "MimicShapeNewShapeGen";
        Vector3 pointPosition = new Vector3(0, 2, 0);  // Example position, adjust as needed
        soundEventSender.SendOneShotSound(soundID, pointPosition);

        // Comment out the old audio system call
        // audio.Play(newShapeSetClip);

        while (lineRendererHolder.childCount > 0)
        {
            DestroyImmediate(lineRendererHolder.GetChild(0).gameObject);
        }

        // Connect spheres with random lines
        for (int i = 0; i < numSpheres; i++)
        {
            int numRandomConnections = Random.Range(1, numSpheres - 2);
            for (int j = 0; j < numRandomConnections; j++)
            {
                GameObject lr = Instantiate(lineRendererPrefab);
                lr.transform.SetParent(lineRendererHolder);

                int randomConnection = Random.Range(0, numSpheres);

                if (randomConnection != i)
                {
                    lr.GetComponent<LineRenderer>().positionCount = 2;
                    lr.GetComponent<LineRenderer>().SetPosition(0, spheres[i].transform.position);
                    lr.GetComponent<LineRenderer>().SetPosition(1, spheres[randomConnection].transform.position);
                }
            }
        }
    }

    void Update()
    {
        oTime = nTime;
        nTime = (Time.time - lastTimeChange) / timeBetweenChanges;
        timer.SetHand(nTime);

        // Check if it's time to change the shape
        if (Time.time - lastTimeChange > timeBetweenChanges)
        {
            // Play Timeout Sound
            string soundID = "MimicShapeTimeout";
            Vector3 pointPosition = new Vector3(0, 2, 0);  // Example position
            soundEventSender.SendOneShotSound(soundID, pointPosition);

            // Reset timer and change shape
            lastTimeChange = Time.time;
            NewShapeSet();
        }
    

        // Check the activation state of each sphere
        for (int j = 0; j<numSpheres; j++)
        {
            bool sphereActive = false;

            // Check each active player's distance to the sphere
            for (int i = 0; i<controller.activePlayers.Count; i++)
            {
                GameObject player = controller.activePlayers[i].gameObject;
                float distance = Vector3.Distance(player.transform.position, spheres[j].transform.position);

                // Activate the sphere if a player is within the activation radius
                if (distance<activationRadius)
                {
                    sphereActive = true;
                    LineRenderer lr = spheres[j].GetComponent<LineRenderer>();
                    lr.positionCount = 2;
                    lr.SetPosition(0, spheres[j].transform.position);
                    lr.SetPosition(1, player.transform.position);

                    // Play sound if the sphere was previously inactive and is now activated
                    if (!spheresActive[j]) // Only play sound if the sphere wasn't active before
                    {
                        string soundID = $"p{controller.activePlayers[i].id}EffectsConstellationEntry";
                        Vector3 playerPosition = player.transform.position;
                        soundEventSender.SendOneShotSound(soundID, playerPosition);
                    }
                }
            }

            // Set the sphere's color and active state based on its activation status
            if (sphereActive)
            {
                Color color = Color.green;
                Renderer rend = spheres[j].GetComponent<Renderer>();
                rend.material.SetColor("_Color", color);
                spheresActive[j] = true;
            }
            else
            {
                LineRenderer lr = spheres[j].GetComponent<LineRenderer>();
                lr.positionCount = 0;

                Color color = Color.red;

                Renderer rend = spheres[j].GetComponent<Renderer>();
                rend.material.SetColor("_Color", color);
                spheresActive[j] = false;
            }
        }

        // Count the number of activated spheres
        int oNumActivated = numShapesActivated;
        numShapesActivated = 0;
        for (int i = 0; i < numSpheres; i++)
        {
            if (spheresActive[i])
            {
                numShapesActivated++;
            }
        }

        // Check if the number of activated spheres has changed
        if (oNumActivated != numShapesActivated)
        {
            if (oNumActivated < numShapesActivated)
            {
                // Comment out the old audio play and replace it with SoundEventSender for Leave sound
                // audio.Play(onLeaveClip);
                string soundID = "MimicShapeLeave";
                Vector3 pointPosition = new Vector3(0, 2, 0);  // Example position, adjust as needed
                soundEventSender.SendOneShotSound(soundID, pointPosition);
            }
            else
            {
                // Comment out the old audio play and replace it with SoundEventSender for Enter sound
                // audio.Play(onEnterClip);
                string soundID = "MimicShapeEnter";
                Vector3 pointPosition = new Vector3(0, 2, 0);  // Example position, adjust as needed
                soundEventSender.SendOneShotSound(soundID, pointPosition);
            }
        }

        // Trigger the shape complete event if all spheres are activated
        if (numShapesActivated == numSpheres)
        {
            OnShapeComplete();
        }
    }

    // References for audio and particle effects
    // public AudioPlayer audio;
    public AudioClip onShapeCompleteClip;
    public ParticleSystem onShapeCompleteParticles;
    public AudioClip newShapeSetClip;
    public AudioClip onEnterClip;
    public AudioClip onLeaveClip;

    // Called when the shape is complete
    void OnShapeComplete()
    {
        // Play Success Matching Shape Sound
        string soundID = "MimicShapeMatch";
        Vector3 pointPosition = new Vector3(0, 2, 0);  // Example position, adjust as needed
        soundEventSender.SendOneShotSound(soundID, pointPosition);

        // Comment out the old audio system call
        // audio.Play(onShapeCompleteClip);
        onShapeCompleteParticles.transform.position = transform.position;
        onShapeCompleteParticles.Play();

        // Set a new shape
        NewShapeSet();
    }
}
