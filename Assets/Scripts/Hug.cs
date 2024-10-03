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

    [Tooltip("Vertical alignment factor for the faces.")]
    public float faceVerticality;

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
    public int numFacesPerGroup;

    public int numColors;


    public float hugSpeed = .01f;
    private void Start()
    {
    }

    public void OnEnable()
    {
        Debug.Log("Hug - OnEnable: Initializing and resetting faces.");

        while (faces.Count > 0)
        {
            HugFace face = faces[0];
            faces.RemoveAt(0);
            Destroy(face.gameObject);
            Debug.Log($"Hug - OnEnable: Destroyed face {face.name}.");
        }

        faces.Clear();

        for (int i = 0; i < trueTextures.Count; i++)
        {
            for (int k = 0; k < numColors; k++)
            {
                Color c = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);

                List<HugFace> partners = new List<HugFace>();
                for (int j = 0; j < numFacesPerGroup; j++)
                {
                    HugFace face1 = Instantiate(facePrefab).GetComponent<HugFace>();
                    Vector3 randomPos;
                    bool isBlockedByNoGoZone;
                    int retryCount = 0;

                    // Loop to ensure that the face is not placed in a forbidden zone or blocked by a no-go zone
                    do
                    {
                        randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f));
                        randomPos = controller.getFinalPosition(randomPos);  // Get final mapped position

                        // Cast a ray from the center of the dome to the random face position
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
                                Debug.Log($"Face placement blocked by {hit.collider.name} at position {randomPos}.");
                            }
                            else
                            {
                                Debug.Log($"Ray hit {hit.collider.name} but not a forbidden zone.");
                            }
                        }
                        else
                        {
                            isBlockedByNoGoZone = false; // No collision, it's a valid position
                            Debug.Log($"No hit for face at position {randomPos}.");
                        }

                        retryCount++;
                        if (retryCount > 10) // Safety limit to prevent infinite loops
                        {
                            Debug.LogWarning($"[WARNING] Failed to generate a valid position for face after 10 tries. Using last position: {randomPos}");
                            break;
                        }

                    } while (isBlockedByNoGoZone);  // Repeat if the position is blocked by a forbidden zone

                    face1.transform.position = randomPos; // Set final valid position
                    face1.transform.localScale = Vector3.one * faceSize * 2;
                    face1.transform.parent = transform;
                    face1.color = c;

                    face1.hug = this;
                    face1.smileID = i;
                    faces.Add(face1);
                    partners.Add(face1);
                }

                for (int j = 0; j < partners.Count; j++)
                {
                    partners[j].partners = partners;
                }
            }
        }

        for (int i = 0; i < faces.Count; i++)
        {
            print("SACTIVATE");
            faces[i].gameObject.SetActive(true);
        }
    }


    public void Update()
    {

        for (int j = 0; j < faces.Count; j++)
        {
            faces[j].WhileOutside();
        }

        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            controller.activePlayers[i].GetComponent<LineRenderer>().enabled = false;
            int closestFaceID = -1;
            float closestFaceDistance = 1000000;
            for (int j = 0; j < faces.Count; j++)
            {
                float distance = Vector3.Distance(controller.activePlayers[i].transform.position, faces[j].transform.position);
                if (distance < closestFaceDistance)
                {
                    closestFaceDistance = distance;
                    closestFaceID = j;
                }
            }

            if (closestFaceDistance < activationRadius)
            {
                faces[closestFaceID].WhileInside(controller.activePlayers[i]);
                //                Debug.Log($"Hug - Update: Player {i} activated face {faces[closestFaceID].name} at distance {closestFaceDistance}.");
            }
        }

        if (completedFaces != null)
        {
            for (int i = 0; i < completedFaces.Count; i++)
            {
                completedFaces[i].WhileFinished();
                // Debug.Log($"Hug - Update: Face {completedFaces[i].name} in completedFaces set to finished.");
            }
        }
    }


    public AudioClip onHugClip;
    public ParticleSystem onHugParticleSystem;

    public AudioPlayer player;


    public void HUG(HugFace hugFace, int smileID)
    {

        for (int i = 0; i < hugFace.partners.Count; i++)
        {
            HugFace hf = hugFace.partners[i];

            hf.fullComplete = true;
            hf.OnFullComplete();


        }

        //onHugParticleSystem.transform.position = hugFace.transform.position;
        //onHugParticleSystem.Play();
        player.Play(onHugClip);


    }



    [Tooltip("number textures, which also is total number of faces")]
    public List<Texture> trueTextures;
    public Texture neutralTexture;

    public Texture finalTexture;



}


