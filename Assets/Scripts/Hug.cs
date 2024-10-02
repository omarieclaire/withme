using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hug : MonoBehaviour
{

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
                    face1.transform.position = controller.getFinalPosition(new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)));
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


