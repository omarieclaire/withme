using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hug : MonoBehaviour
{
    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager

    [Tooltip("Prefab used to instantiate faces.")]
    public GameObject facePrefab;

    [Tooltip("Size of each face.")]
    public float faceSize;


    [Tooltip("List of all HugFace objects in the game.")]
    public List<HugFace> faces;

    [Tooltip("The game controller managing the game state.")]
    public Controller controller;

    [Tooltip("Radius within which faces get activated.")]
    public float activationRadius;

    [Tooltip("Prefab used to create connection lines between faces.")]
    public GameObject connectionPrefab;

    [Tooltip("List of LineRenderers to visualize connections between faces.")]
    public List<LineRenderer> connections;

    [Tooltip("List of discovered smile IDs.")]
    public List<int> smilesDiscovered;

    [Tooltip("List of HugFace objects that have completed the hug.")]
    public List<HugFace> completedFaces;

    [Tooltip("number textures, which also is total number of faces")]
    public List<Texture> trueTextures;
    public Texture neutralTexture;

    public Texture finalTexture;

    [Tooltip("List of special sounds for each HugFace pair.")]
    public List<AudioClip> hugFaceSounds;  // List of unique sounds for HugFace pairs

    public int numFacesPerGroup;
    public int numColors;

    public float hugSpeed = .01f;


    public AudioClip onHugClip;
    public ParticleSystem onHugParticleSystem;

    public AudioPlayer player;



    private void Start()
    {
        // Debug.Log("Hug - Start: Hug initialization complete.");
    }

    public void OnEnable()
{
    // Debug.Log("Hug - OnEnable: Initializing and resetting faces.");

    // Clear any previously existing faces
    while (faces.Count > 0)
    {
        HugFace face = faces[0];
        faces.RemoveAt(0);
        Destroy(face.gameObject);
        // Debug.Log($"Hug - OnEnable: Destroyed face {face.name}.");
    }

    faces.Clear();  // Ensure the faces list is empty before adding new faces

    int soundIndex = 0;  // Tracks the sound index for unique sounds

    // Loop through textures to create face pairs
    for (int i = 0; i < trueTextures.Count; i++)
    {
        for (int k = 0; k < numColors; k++)
        {
            // Generate a random color for each face pair
            Color c = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);

            // Create only two faces per group to ensure one-to-one matching
            List<HugFace> partners = new List<HugFace>();

            // Instantiate the two faces that will form a pair
            HugFace face1 = Instantiate(facePrefab).GetComponent<HugFace>();
            HugFace face2 = Instantiate(facePrefab).GetComponent<HugFace>();

            // Assign a unique sound to both faces in the pair (ensuring they get the same sound)
            if (soundIndex < hugFaceSounds.Count)
            {
                AudioClip pairSound = hugFaceSounds[soundIndex]; // Store the sound to assign to both faces
                face1.hugFaceSong = pairSound;
                face2.hugFaceSong = pairSound;
                // Debug.Log($"Hug - OnEnable: Assigned sound to face pair, sound index {soundIndex}.");
            }
            else
            {
                // Debug.LogWarning("Not enough sounds provided for each HugFace pair.");
            }

            // Increment soundIndex for the next pair
            soundIndex++;

            // Assign a unique smileID (True face) to the pair
            face1.smileID = i;
            face2.smileID = i;

            // Set the same random color for both faces in the pair
            face1.color = c;
            face2.color = c;

            // Assign each other as partners
            face1.partners = new List<HugFace> { face2 };
            face2.partners = new List<HugFace> { face1 };

            // Generate random positions while avoiding no-go zones
            Vector3 randomPos1, randomPos2;
            bool isBlockedByNoGoZone1, isBlockedByNoGoZone2;
            int retryCount1 = 0, retryCount2 = 0;

            // Position face1 while avoiding no-go zones
            do
            {
                randomPos1 = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f));
                randomPos1 = controller.getFinalPosition(randomPos1);  // Get final mapped position

                Ray ray1 = new Ray(Vector3.zero, randomPos1.normalized);  // Ensure direction is normalized
                RaycastHit hit1;

                if (Physics.Raycast(ray1, out hit1, randomPos1.magnitude))
                {
                    isBlockedByNoGoZone1 = hit1.collider == noGoZoneManager.doorCollider ||
                                           hit1.collider == noGoZoneManager.soundBoothCollider ||
                                           hit1.collider == noGoZoneManager.stageCollider;
                    if (isBlockedByNoGoZone1)
                    {
                        // Debug.Log($"Hug - OnEnable: Face placement blocked by no-go zone at position {randomPos1}. Retrying...");
                    }
                }
                else
                {
                    isBlockedByNoGoZone1 = false;
                }

                retryCount1++;
                if (retryCount1 > 10) break;  // Safety limit to prevent infinite loops
            } while (isBlockedByNoGoZone1);

            // Position face2 while avoiding no-go zones
            do
            {
                randomPos2 = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f));
                randomPos2 = controller.getFinalPosition(randomPos2);  // Get final mapped position

                Ray ray2 = new Ray(Vector3.zero, randomPos2.normalized);  // Ensure direction is normalized
                RaycastHit hit2;

                if (Physics.Raycast(ray2, out hit2, randomPos2.magnitude))
                {
                    isBlockedByNoGoZone2 = hit2.collider == noGoZoneManager.doorCollider ||
                                           hit2.collider == noGoZoneManager.soundBoothCollider ||
                                           hit2.collider == noGoZoneManager.stageCollider;
                    if (isBlockedByNoGoZone2)
                    {
                        // Debug.Log($"Hug - OnEnable: Face placement blocked by no-go zone at position {randomPos2}. Retrying...");
                    }
                }
                else
                {
                    isBlockedByNoGoZone2 = false;
                }

                retryCount2++;
                if (retryCount2 > 10) break;  // Safety limit to prevent infinite loops
            } while (isBlockedByNoGoZone2);

            // Set final positions and add the HugFaces to the list
            face1.transform.position = randomPos1;
            face1.transform.localScale = Vector3.one * faceSize * 2;  // Set the size of the HugFace
            face1.transform.parent = transform;  // Set this Hug object as the parent of the face
            face1.color = c;  // Set the random color for the HugFace
            face1.hug = this;  // Assign this Hug instance to the HugFace
            face1.smileID = i;  // Assign a smileID for the textures
            faces.Add(face1);  // Add the face to the list of faces
            partners.Add(face1);  // Add the face to the partners list

            face2.transform.position = randomPos2;
            face2.transform.localScale = Vector3.one * faceSize * 2;  // Set the size of the HugFace
            face2.transform.parent = transform;  // Set this Hug object as the parent of the face
            face2.color = c;  // Set the random color for the HugFace
            face2.hug = this;  // Assign this Hug instance to the HugFace
            face2.smileID = i;  // Assign a smileID for the textures
            faces.Add(face2);  // Add the face to the list of faces
            partners.Add(face2);  // Add the face to the partners list
        }
    }

    // Activate all faces
    for (int i = 0; i < faces.Count; i++)
    {
        // Debug.Log($"Hug - OnEnable: Activating face {faces[i].name}.");
        faces[i].gameObject.SetActive(true);
    }
}



    public void Update()
    {
        // Handle faces that are outside the hug zone
        for (int j = 0; j < faces.Count; j++)
        {
            faces[j].WhileOutside();  // Reset faces that are not currently being interacted with
        }

        // Check for player interactions with HugFaces
        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            controller.activePlayers[i].GetComponent<LineRenderer>().enabled = false;  // Disable line renderer for players
            int closestFaceID = -1;
            float closestFaceDistance = 1000000;  // Set an initially large distance

            // Find the closest face to the player
            for (int j = 0; j < faces.Count; j++)
            {
                float distance = Vector3.Distance(controller.activePlayers[i].transform.position, faces[j].transform.position);
                if (distance < closestFaceDistance)
                {
                    closestFaceDistance = distance;
                    closestFaceID = j;
                }
            }

            // If a face is close enough, trigger interaction
            if (closestFaceDistance < activationRadius)
            {
                faces[closestFaceID].WhileInside(controller.activePlayers[i], faces[closestFaceID]);
                // Debug.Log($"Hug - Update: Player {i} activated face {faces[closestFaceID].name} at distance {closestFaceDistance}.");
            }

        }

        // Handle completed HugFaces
        if (completedFaces != null)
        {
            for (int i = 0; i < completedFaces.Count; i++)
            {
                completedFaces[i].WhileFinished();  // Mark finished faces
                // Debug.Log($"Hug - Update: Completed face {completedFaces[i].name} set to finished.");
            }
        }
    }
    public void HUG(HugFace hugFace, int smileID)
    {
        // Mark all partners of the given HugFace as fully completed
        for (int i = 0; i < hugFace.partners.Count; i++)
        {
            HugFace hf = hugFace.partners[i];
            hf.fullComplete = true;
            hf.OnFullComplete();  // Trigger the full complete state
            // Debug.Log($"Hug - HUG: Marked HugFace {hf.name} as fully complete.");
        }

        // Play a particle effect and sound for the hug event (currently commented out)
        // onHugParticleSystem.transform.position = hugFace.transform.position;
        // onHugParticleSystem.Play();
        player.Play(onHugClip);
        // Debug.Log($"Hug - HUG: Hug event completed, played sound and particle effect for {hugFace.name}.");
    }
}
