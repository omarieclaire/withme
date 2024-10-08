using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicShape : MonoBehaviour
{
    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager

    // Public variables to be set in the Unity Editor
    public int numSpheres;

    public SoundEventSender soundEventSender;

    public List<GameObject> spheres;
    public GameObject spherePrefab;
    public Controller controller;
    public List<bool> spheresActive; // List to track the active state of each sphere
    public float sphereSize;
    public Timer timer;
    public Timer timerPrefab;

    public Transform lineRendererHolder;
    public GameObject lineRendererPrefab;

    // Time-based variables
    public float timeBetweenChanges;
    public float lastTimeChange;
    public float activationRadius;

    public AudioClip onTickClip;  // Keeping old audio references for fallback or debugging


    // References for audio and particle effects
    public AudioPlayer audio;
    public AudioClip onShapeCompleteClip;
    public AudioClip newShapeSetClip;
    public AudioClip onEnterClip;
    public AudioClip onLeaveClip;

    public AudioClip mimicShapeTimeoutClip;
    private float oTime;
    private float nTime;

    public float tickRate = .1f;

    // Number of activated shapes
    public int numShapesActivated;

    public ParticleSystem onShapeCompleteParticles; // Reference for the particle system on shape completion

    // Called when the script is enabled
    public void OnEnable()
    {
        // Check for required components
        if (spherePrefab == null)
        {
            Debug.LogError("[ERROR] Sphere Prefab is not assigned.");
            return;
        }

        if (soundEventSender == null)
        {
            Debug.LogError("[ERROR] SoundEventSender is not assigned.");
        }

        if (controller == null)
        {
            Debug.LogError("[ERROR] Controller reference is missing.");
        }

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
            try
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
            catch (System.Exception ex)
            {
                Debug.LogError($"[ERROR] Failed to instantiate sphere {i}: {ex.Message}");
            }
        }

        for (int i = 0; i < numSpheres; i++)
        {
            try
            {
                // Instantiate a new sphere and set its properties
                GameObject sphere = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
                sphere.transform.position = Vector3.zero;
                sphere.transform.localScale = Vector3.one * sphereSize;
                sphere.transform.SetParent(transform);
                sphere.name = $"Sphere_{i}";
                spheres.Add(sphere);
                spheresActive.Add(false);

                // Instantiate the timer and set it as a child of the sphere
                Timer newTimer = Instantiate(timerPrefab, sphere.transform);
                newTimer.transform.localPosition = Vector3.zero; // Center the timer inside the sphere
                newTimer.SetHand(0); // Start the timer at 0, or configure it as needed
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ERROR] Failed to instantiate sphere {i}: {ex.Message}");
            }
        }

        // Initialize a new shape set
        NewShapeSet();
    }

    

    public void NewShapeSet()
    {
        if (noGoZoneManager == null)
    {
        Debug.LogError("[ERROR] NoGoZoneManager is not assigned.");
        return;
    }

    numShapesActivated = 0;
    lastTimeChange = Time.time;

        // Set each sphere to a new random position
        for (int i = 0; i < numSpheres; i++)
        {
            spheresActive[i] = false;

            Vector3 randomPos;
            bool isBlockedByNoGoZone;
            int retryCount = 0; // Track the number of retries for generating a valid position

            // Retry generating random positions until one is found outside no-go zones
            do
            {
                randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 2f), Random.Range(-1f, 1f));
                Debug.Log($"xxx Sphere_{i}: Trying Random Position = {randomPos}");

                if (controller != null)
                {
                    randomPos = controller.getFinalPosition(randomPos);
                    Debug.Log($"xxx Sphere_{i}: Position after getFinalPosition = {randomPos}");
                }

                // Cast a ray from the center of the dome to the random sphere position
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
                        Debug.Log($"Sphere {i} placement blocked by {hit.collider.name} at position {randomPos}.");
                    }
                    else
                    {
                        Debug.Log($"Ray hit {hit.collider.name} but not a forbidden zone.");
                    }
                }
                else
                {
                    isBlockedByNoGoZone = false; // No collision, it's a valid position
                    Debug.Log($"No hit for sphere {i} at position {randomPos}.");
                }

                retryCount++;
                if (retryCount > 10) // Add a safety limit to prevent infinite loops
                {
                    Debug.LogWarning($"[WARNING] Sphere_{i} failed to generate a valid position after 10 tries. Using last position: {randomPos}");
                    break;
                }

            } while (isBlockedByNoGoZone); // Repeat if position is in a no-go zone

            // Log and set sphere's color based on final valid position
            if (noGoZoneManager.IsInNoGoZone(randomPos))
            {
                Debug.Log($"Sphere_{i} is still generated in a no-go zone at: {randomPos}, using last valid position.");
                spheres[i].transform.localScale = Vector3.one * sphereSize * 20; // Scale it up to highlight it
            }
            else
            {
                Debug.Log($"Sphere_{i} is generated at: {randomPos}");
                spheres[i].transform.localScale = Vector3.one * sphereSize * 2; // Scale it up to highlight it

                spheres[i].GetComponent<Renderer>().material.color = Color.white;
            }

            // Set the sphere's position
            spheres[i].transform.position = randomPos;
        }



        // timout place?
        if (Controller.enableOldSoundSystem)
        {
            audio.Play(mimicShapeTimeoutClip); // <-- old sound keep for debugging
        }


        // Play New Constellation Sound via SoundEventSender
        if (soundEventSender != null)
        {
            if (Controller.enableNewSoundSystem)

            {

                // string soundID = "MimicShapeNewShapeGen";
                // Vector3 pointPosition = new Vector3(0, 2, 0);  // Example position, adjust as needed
                // soundEventSender.SendOneShotSound(soundID, pointPosition);
            }
        }
        else
        {
            Debug.LogWarning("[WARNING] SoundEventSender is null. Cannot play sound.");
        }

        // Clear existing lines
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
                try
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
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ERROR] Failed to create line between spheres: {ex.Message}");
                }
            }
        }
    }

    // Declare a flag to track whether ticking is currently playing
    private bool isTickingPlaying = false;

    void Update()
    {
        oTime = nTime;
        nTime = Time.time - lastTimeChange;
        nTime /= timeBetweenChanges;

        // Check if it's time to start or continue the ticking sound

        if (nTime % tickRate < oTime % tickRate)
        {
            if (Controller.enableOldSoundSystem)
            {
                audio.Play(onTickClip, .5f + nTime * .5f);

            }
        }


        // if (Controller.enableNewSoundSystem)
        // {
        //  string soundID = "MimicShapeTick";
        //     soundEventSender.SendOrUpdateContinuousSound(soundID, transform.position);
        //     isTickingPlaying = true;
        // }
        // else
        // {
        //     if (isTickingPlaying)
        //     {
        //         if (Controller.enableNewSoundSystem)
        //         {
        //             string soundID = "MimicShapeTick";
        //             soundEventSender.StopContinuousSound(soundID, transform.position);
        //             isTickingPlaying = false;
        //         }
        //     }
        // }

        // Set hand for the central timer 


        timer.SetHand(nTime);

        // Now update each sphere's timer as well


        for (int i = 0; i < spheres.Count; i++)
        {
            Timer sphereTimer = spheres[i].GetComponentInChildren<Timer>();
            if (sphereTimer != null)
            {
                sphereTimer.SetHand(nTime); // Update each sphere's timer
            }
        }

        // Check if it's time to change the shape
        if (Time.time - lastTimeChange > timeBetweenChanges)
        {
            // if (isTickingPlaying && soundEventSender != null)
            // {
            //     if (Controller.enableNewSoundSystem)
            //     {
            //         string soundID = "MimicShapeTick";
            //         soundEventSender.StopContinuousSound(soundID, transform.position);
            //         isTickingPlaying = false;
            //     }
            // }

            // if (soundEventSender != null)
            // {
            //     if (Controller.enableNewSoundSystem)
            //     {
            //         string timeoutSoundID = "MimicShapeTimeout";
            //         soundEventSender.SendOneShotSound(timeoutSoundID, transform.position);
            //     }
            // }

            lastTimeChange = Time.time;
            NewShapeSet();
        }

        for (int j = 0; j < numSpheres; j++)
        {
            bool sphereActive = false;

            for (int i = 0; i < controller.activePlayers.Count; i++)
            {
                GameObject player = controller.activePlayers[i].gameObject;
                float distance = Vector3.Distance(player.transform.position, spheres[j].transform.position);

                if (distance < activationRadius)
                {
                    sphereActive = true;
                    LineRenderer lr = spheres[j].GetComponent<LineRenderer>();
                    lr.positionCount = 2;
                    lr.SetPosition(0, spheres[j].transform.position);
                    lr.SetPosition(1, player.transform.position);

                }
            }

            // if (!spheresActive[j])
            // {
            //     if (soundEventSender != null)
            //     {
            //         if (Controller.enableNewSoundSystem)

            //         {
            //             string soundID = $"p{controller.activePlayers[i].id}EffectsConstellationEntry";
            //             soundEventSender.SendOneShotSound(soundID, player.transform.position);
            //         }

            if (sphereActive)
            {


                Color color = Color.green;

                Renderer rend = spheres[j].GetComponent<Renderer>();
                rend.material.SetColor("_Color", color);
                rend.material.SetColor("_BaseColor", color * .1f);
                rend.material.SetColor("_ReflectionColor", color);
                rend.material.SetColor("_CenterOrbColor", color * .1f);
                rend.material.SetColor("_NoiseColor", color * 2);


                spheresActive[j] = true;
            }
            else
            {
                LineRenderer lr = spheres[j].GetComponent<LineRenderer>();
                lr.positionCount = 0;

                Color color = Color.yellow;

                Renderer rend = spheres[j].GetComponent<Renderer>();
                rend.material.SetColor("_Color", color);
                rend.material.SetColor("_BaseColor", color * .1f);
                rend.material.SetColor("_ReflectionColor", color);
                rend.material.SetColor("_CenterOrbColor", color * .1f);
                rend.material.SetColor("_NoiseColor", color * 2);

                spheresActive[j] = false;
            }
        }

        // Count activated spheres and trigger shape complete if all are active
        int oNumActivated = numShapesActivated;
        numShapesActivated = 0;
        for (int i = 0; i < numSpheres; i++)
        {
            if (spheresActive[i])
            {
                numShapesActivated++;
            }
        }

        if (oNumActivated != numShapesActivated)
        {
            if (oNumActivated < numShapesActivated)
            {
                if (Controller.enableOldSoundSystem)
                {
                    audio.Play(onLeaveClip);
                }
                if (Controller.enableNewSoundSystem)
                {
                    // string soundID = $"p{player.id}EffectsWithMePointCollision";
                    // Vector3 pointPosition = player.transform.position;
                    // soundEventSender.SendOneShotSound(soundID, pointPosition);
                }
            }
            else
            {
                if (Controller.enableOldSoundSystem)
                {
                    audio.Play(onEnterClip);
                }
                if (Controller.enableNewSoundSystem)
                {
                    // string soundID = $"p{player.id}EffectsWithMePointCollision";
                    // Vector3 pointPosition = player.transform.position;
                    // soundEventSender.SendOneShotSound(soundID, pointPosition);
                }
            }
        }

        if (numShapesActivated == numSpheres)
        {
            OnShapeComplete();
        }
    }

    // Called when the shape is complete
    void OnShapeComplete()
    {

        if (Controller.enableOldSoundSystem)
        {
            audio.Play(onShapeCompleteClip);

        }
        if (Controller.enableNewSoundSystem)
        {
            // string soundID = $"p{player.id}EffectsWithMePointCollision";
            // Vector3 pointPosition = player.transform.position;
            // soundEventSender.SendOneShotSound(soundID, pointPosition);
        }



        if (onShapeCompleteParticles != null)
        {
            onShapeCompleteParticles.transform.position = transform.position;
            onShapeCompleteParticles.Play();
        }
        else
        {
            Debug.LogWarning("[WARNING] Shape Complete Particles not assigned.");
        }

        NewShapeSet();

    }
}

