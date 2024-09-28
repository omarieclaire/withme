// florp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTargets : MonoBehaviour
{
    public GameObject targetPrefab;
    public GameObject spikePrefab;

    public int numTargets;
    public int numSpikes;

    public Controller controller;

    public float targetSize;
    public float spikeSize;

    public List<PlayOnCollision> miniFlorps;

    public float xzSpread;

    // List of materials for targets and spikes
    public List<Material> targetMaterials;
    public List<Material> spikeMaterials;

    // List to store used positions to avoid overlap
    private List<Vector3> usedPositions;

    void OnEnable()
    {
        // Clear old children
        while (transform.childCount > 0)
        {
            Transform c = transform.GetChild(0);
            DestroyImmediate(c.gameObject);
        }

        miniFlorps = new List<PlayOnCollision>();
        usedPositions = new List<Vector3>();

        // Generate targets
        for (int i = 0; i < numTargets; i++)
        {
            GenerateObject(targetPrefab, targetSize, targetMaterials);
        }

        // Generate spikes (negative targets)
        for (int i = 0; i < numSpikes; i++)
        {
            GenerateObject(spikePrefab, spikeSize, spikeMaterials);
        }
    }

    // Simplified object generation
    private void GenerateObject(GameObject prefab, float objectSize, List<Material> materials)
    {
        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        Vector3 randomPosition = GetValidPosition(objectSize);

        if (randomPosition == Vector3.zero)
        {
            // Could not find a valid position, skip this object
            Destroy(obj);
            return;
        }

        obj.transform.parent = transform;
        obj.transform.localScale = new Vector3(objectSize, objectSize, objectSize);
        obj.transform.position = controller.getFinalPosition(randomPosition);

        // Randomly assign a material
        if (materials.Count > 0)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material randomMaterial = materials[Random.Range(0, materials.Count)];
                renderer.material = randomMaterial;
            }
        }

        PlayOnCollision poc = obj.GetComponent<PlayOnCollision>();
        miniFlorps.Add(poc);

        // Store the object's position to avoid overlap
        usedPositions.Add(randomPosition);
    }

    // Function to get a valid random position that does not overlap
    private Vector3 GetValidPosition(float objectSize)
    {
        int maxAttempts = 200;  // Increase the number of attempts to find a valid position
        int attempt = 0;
        Vector3 randomPosition;

        // Try finding a valid position
        do
        {
            randomPosition = new Vector3(Random.Range(-1f, 1f) * xzSpread, 0, Random.Range(-1f, 1f) * xzSpread);
            bool isValid = true;

            foreach (Vector3 usedPos in usedPositions)
            {
                // Ensure distance between objects is at least objectSize * 1.2 to avoid overlap
                if (Vector3.Distance(randomPosition, usedPos) < objectSize * .1f)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid) return randomPosition;

            attempt++;
        } while (attempt < maxAttempts);

        // If no valid position found within the max attempts, return Vector3.zero
        return Vector3.zero;
    }

    public GameObject Florp;

    public ParticleSystem ps;
    public AudioClip clip;
    public AudioPlayer audioPlayer;

    public bool finished = false;

    void Update()
    {
        bool done = true;

        for (int i = 0; i < miniFlorps.Count; i++)
        {
            if (miniFlorps[i].followTransform == null)
            {
                done = false;
                break;
            }
        }

        if (done == true && !finished)
        {
            audioPlayer.Play(clip);
            ps.transform.position = Florp.transform.position;
            ps.Play();
            finished = true;
        }
    }
}
