using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hug : MonoBehaviour
{
    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager

    [Tooltip("Prefab used to instantiate faces.")]
    public GameObject hugFacePrefab;

    [Tooltip("Size of each face.")]
    public float hugFaceSize;

    [Tooltip("List of all HugFace objects in the game.")]
    public List<HugFace> listOfHugFaceObjects;

    [Tooltip("The game controller managing the game state.")]
    public Controller controller;

    [Tooltip("Radius within which faces get activated.")]
    public float activationRadius;

    [Tooltip("Prefab used to create connection lines between faces.")]
    public GameObject connectionPrefab;

    [Tooltip("List of LineRenderers to visualize connections between faces.")]
    public List<LineRenderer> connections;

    [Tooltip("List of HugFace objects that have completed the hug.")]
    public List<HugFace> listOfCompletedHugFaces;

    [Tooltip("number textures, which also is total number of faces")]
    public List<Texture> pensTrueFaces;
    public Texture pensNeutralFace;

    public Texture pensPostHugFace;

    [Tooltip("List of special sounds for each HugFace pair.")]
    public List<AudioClip> specialHugFaceSongs;  // List of unique sounds for HugFace pairs
    public int numColors;

    public float howFastWeHug = .01f;

    public AudioClip onHugClip;
    public AudioClip winSound;  // Sound for winning a round
    public ParticleSystem winParticleSystem;  // Particles for winning a round

    public AudioPlayer player;

    private int currentPairCount = 1;  // Start with 1 pair
    private int completedPairs = 0;    // Track completed pairs
    private bool gameIsOver = false;   // To stop the game after 3 rounds

    private void Start()
    {
        // Start by spawning the first pair
        SpawnPairs(currentPairCount);
    }
    
    private void ClearExistingFaces()
    {
        while (listOfHugFaceObjects.Count > 0)
        {
            HugFace face = listOfHugFaceObjects[0];
            listOfHugFaceObjects.RemoveAt(0);
            Destroy(face.gameObject);
        }

        listOfHugFaceObjects.Clear();  // Ensure the faces list is empty before adding new faces
    }

    private void SpawnPairs(int numberOfPairs)
    {
        int soundIndex = 0;  // Tracks the sound index for unique sounds

        for (int i = 0; i < numberOfPairs; i++)
        {
            for (int k = 0; k < numColors; k++)
            {
                Color randomColor = GenerateRandomColor();  // Generate a random color for each face pair

                // Create a pair of HugFaces
                HugFace face1 = CreateHugFace(i, randomColor, ref soundIndex);
                HugFace face2 = CreateHugFace(i, randomColor, ref soundIndex);

                // Assign each other as partners
                AssignPartners(face1, face2);

                // Position the faces while avoiding no-go zones
                PositionFaces(face1, face2);

                // Add the HugFaces to the list
                AddFacesToList(face1, face2);
            }
        }

        // Activate all faces
        ActivateAllFaces();
    }

    private Color GenerateRandomColor()
    {
        // Generate a random color for each face pair
        return Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
    }

    private HugFace CreateHugFace(int smileID, Color color, ref int soundIndex)
    {
        // Instantiate a HugFace from prefab
        HugFace face = Instantiate(hugFacePrefab).GetComponent<HugFace>();

        // Assign unique sound to the face
        if (soundIndex < specialHugFaceSongs.Count)
        {
            face.hugFaceSong = specialHugFaceSongs[soundIndex]; // Store the sound to assign to both faces
        }

        // Increment soundIndex for the next pair
        soundIndex++;

        // Assign a unique smileID (True face) and color to the face
        face.smileID = smileID;
        face.color = color;

        return face;
    }

    private void AssignPartners(HugFace face1, HugFace face2)
    {
        face1.partners = new List<HugFace> { face2 };
        face2.partners = new List<HugFace> { face1 };
    }

    private void PositionFaces(HugFace face1, HugFace face2)
    {
        Vector3 randomPos1, randomPos2;
        bool isBlockedByNoGoZone1, isBlockedByNoGoZone2;
        int retryCount1 = 0, retryCount2 = 0;
        float minimumDistance = 0.5f;  // Adjust this based on the size of your faces

        // Position face1 while avoiding no-go zones
        do
        {
            randomPos1 = GetRandomPosition();
            isBlockedByNoGoZone1 = CheckIfBlockedByNoGoZone(randomPos1);

            retryCount1++;
            if (retryCount1 > 10) break;  // Safety limit to prevent infinite loops
        } while (isBlockedByNoGoZone1);

        // Position face2 while avoiding no-go zones and ensuring no overlap with face1
        do
        {
            randomPos2 = GetRandomPosition();
            isBlockedByNoGoZone2 = CheckIfBlockedByNoGoZone(randomPos2) || Vector3.Distance(randomPos1, randomPos2) < minimumDistance;

            retryCount2++;
            if (retryCount2 > 10) break;  // Safety limit to prevent infinite loops
        } while (isBlockedByNoGoZone2);

        // Set positions for face1 and face2
        face1.transform.position = randomPos1;
        face2.transform.position = randomPos2;
    }

    private Vector3 GetRandomPosition()
    {
        // Generate a random position within certain bounds
        return controller.getFinalPosition(new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)));
    }

    private bool CheckIfBlockedByNoGoZone(Vector3 position)
    {
        Ray ray = new Ray(Vector3.zero, position.normalized);  // Ensure direction is normalized
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, position.magnitude))
        {
            return hit.collider == noGoZoneManager.doorCollider ||
                   hit.collider == noGoZoneManager.soundBoothCollider ||
                   hit.collider == noGoZoneManager.stageCollider;
        }
        return false;
    }

    private void AddFacesToList(HugFace face1, HugFace face2)
    {
        face1.transform.localScale = Vector3.one * hugFaceSize * 2;  // Set the size of the HugFace
        face1.transform.parent = transform;  // Set this Hug object as the parent of the face
        listOfHugFaceObjects.Add(face1);  // Add face1 to the list of faces

        face2.transform.localScale = Vector3.one * hugFaceSize * 2;  // Set the size of the HugFace
        face2.transform.parent = transform;  // Set this Hug object as the parent of the face
        listOfHugFaceObjects.Add(face2);  // Add face2 to the list of faces
    }

    private void ActivateAllFaces()
    {
        for (int i = 0; i < listOfHugFaceObjects.Count; i++)
        {
            listOfHugFaceObjects[i].gameObject.SetActive(true);
        }
    }

    public void Update()
    {
        // Handle faces that are outside the hug zone
        for (int j = 0; j < listOfHugFaceObjects.Count; j++)
        {
            listOfHugFaceObjects[j].WhileOutside();  // Reset faces that are not currently being interacted with
        }

        // Check for player interactions with HugFaces
        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            controller.activePlayers[i].GetComponent<LineRenderer>().enabled = false;  // Disable line renderer for players
            int closestFaceID = -1;
            float closestFaceDistance = 1000000;  // Set an initially large distance

            // Find the closest face to the player
            for (int j = 0; j < listOfHugFaceObjects.Count; j++)
            {
                float distance = Vector3.Distance(controller.activePlayers[i].transform.position, listOfHugFaceObjects[j].transform.position);
                if (distance < closestFaceDistance)
                {
                    closestFaceDistance = distance;
                    closestFaceID = j;
                }
            }

            // If a face is close enough, trigger interaction
            if (closestFaceDistance < activationRadius)
            {
                listOfHugFaceObjects[closestFaceID].WhileInside(controller.activePlayers[i], listOfHugFaceObjects[closestFaceID]);
            }
        }

        // Handle completed HugFaces
        if (listOfCompletedHugFaces != null)
        {
            for (int i = 0; i < listOfCompletedHugFaces.Count; i++)
            {
                listOfCompletedHugFaces[i].WhileFinished();  // Mark finished faces
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
        }

        // Play a particle effect and sound for the hug event
        player.Play(onHugClip);

        completedPairs++;  // Increment completed pairs

        // If all current pairs are completed, spawn more pairs
        if (completedPairs == currentPairCount)
        {
            currentPairCount++;  // Increase the number of pairs
            completedPairs = 0;  // Reset completed pairs
            SpawnPairs(currentPairCount);  // Spawn the next set of pairs
        }
    }
}

//     public void HUG(HugFace hugFace, int smileID)
//     {
//         // Mark all partners of the given HugFace as fully completed
//         for (int i = 0; i < hugFace.partners.Count; i++)
//         {
//             HugFace hf = hugFace.partners[i];
//             hf.fullComplete = true;
//             hf.OnFullComplete();  // Trigger the full complete state
//             // Debug.Log($"Hug - HUG: Marked HugFace {hf.name} as fully complete.");
//         }

//         // Play a particle effect and sound for the hug event (currently commented out)
//         // onHugParticleSystem.transform.position = hugFace.transform.position;
//         // onHugParticleSystem.Play();
//         player.Play(onHugClip);
//         // Debug.Log($"Hug - HUG: Hug event completed, played sound and particle effect for {hugFace.name}.");
//     }
// }
