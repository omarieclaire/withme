using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hug : MonoBehaviour
{
    [Tooltip("Number of face pairs in the game.")]
    public int numFacePairs;

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
        for (int i = 0; i < numFacePairs; i++)
        {
            HugFace face1 = Instantiate(facePrefab).GetComponent<HugFace>();
            face1.transform.position = controller.getFinalPosition(new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)));
            face1.transform.localScale = Vector3.one * faceSize * 2;
            face1.transform.parent = transform;
            Debug.Log($"Hug - OnEnable: Instantiated face1 with position {face1.transform.position} and scale {face1.transform.localScale}.");

            HugFace face2 = Instantiate(facePrefab).GetComponent<HugFace>();
            face2.transform.position = controller.getFinalPosition(new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)));
            face2.transform.localScale = Vector3.one * faceSize * 2;
            face2.transform.parent = transform;
            Debug.Log($"Hug - OnEnable: Instantiated face2 with position {face2.transform.position} and scale {face2.transform.localScale}.");

            face1.hug = this;
            face2.hug = this;

            face1.smileID = i;
            face2.smileID = i;

            face1.partner = face2;
            face2.partner = face1;

            face1.gameObject.SetActive(true);
            face2.gameObject.SetActive(true);

            faces.Add(face1);
            faces.Add(face2);
            Debug.Log($"Hug - OnEnable: Face pair {i} added with smileID {i}.");
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
                faces[closestFaceID].WhileInside();
                Debug.Log($"Hug - Update: Player {i} activated face {faces[closestFaceID].name} at distance {closestFaceDistance}.");
            }
        }

        if (completedFaces != null)
        {
            for (int i = 0; i < completedFaces.Count; i++)
            {
                completedFaces[i].WhileFinished();
                Debug.Log($"Hug - Update: Face {completedFaces[i].name} in completedFaces set to finished.");
            }
        }
    }

    public void PairDiscover(GameObject f1, GameObject f2, int smileID)
    {
        if (smilesDiscovered == null)
        {
            smilesDiscovered = new List<int>();
        }

        if (connections == null)
        {
            connections = new List<LineRenderer>();
        }

        if (smilesDiscovered.Contains(smileID))
        {
            return;
        }

        if (completedFaces == null)
        {
            completedFaces = new List<HugFace>();
        }

        HugFace face1 = f1.GetComponent<HugFace>();
        HugFace face2 = f2.GetComponent<HugFace>();

        if (face1.TrueTexture == face2.TrueTexture) // Use the public getter
        {
            face1.ApplyColor(Color.red);
            face2.ApplyColor(Color.red);
            Debug.Log($"Hug - PairDiscover: Matching faces {face1.name} and {face2.name} colored red.");
        }

        completedFaces.Add(face1);
        completedFaces.Add(face2);

        smilesDiscovered.Add(smileID);

        Debug.Log($"Hug - PairDiscover: Discovered pair with smileID {smileID}. Total discovered: {smilesDiscovered.Count}");

        LineRenderer connection = Instantiate(connectionPrefab).GetComponent<LineRenderer>();
        connection.gameObject.transform.parent = transform;
        connection.SetPosition(0, f1.transform.position);
        connection.SetPosition(1, f2.transform.position);
        connections.Add(connection);
        Debug.Log($"Hug - PairDiscover: Connection created between {face1.name} and {face2.name}.");
    }
}