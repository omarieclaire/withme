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


    void OnEnable()
    {

        while (transform.childCount > 0)
        {
            Transform c = transform.GetChild(0);
            DestroyImmediate(c.gameObject);
        }

        miniFlorps = new List<PlayOnCollision>();

        for (int i = 0; i < numTargets; i++)
        {
            GameObject target = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);

            Vector3 randomPosition = new Vector3(Random.Range(-1f, 1f) * xzSpread, 0, Random.Range(-1f, 1f) * xzSpread);
            target.transform.parent = transform;
            target.transform.localScale = new Vector3(targetSize, targetSize, targetSize);
            target.transform.position = controller.getFinalPosition(randomPosition);
            // Debug.Log($"Target generated at position: {target.transform.position}");
            PlayOnCollision poc = target.GetComponent<PlayOnCollision>();
            miniFlorps.Add(poc);
        }

        // Generate spikes (negative targets)
        for (int i = 0; i < numSpikes; i++)
        {
            // Generate a random rotation for the spike
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Instantiate the spike with the random rotation
            GameObject spike = Instantiate(spikePrefab, Vector3.zero, randomRotation);

            Vector3 randomPosition = new Vector3(Random.Range(-1f, 1f) * xzSpread, 0, Random.Range(-1f, 1f) * xzSpread);
            spike.transform.parent = transform;
            spike.transform.localScale = new Vector3(spikeSize, spikeSize, spikeSize);
            spike.transform.position = controller.getFinalPosition(randomPosition);

            PlayOnCollision poc = spike.GetComponent<PlayOnCollision>();
            miniFlorps.Add(poc);
        }





    }

    public GameObject Florp;

    public ParticleSystem ps;
    public AudioClip clip;
    public AudioPlayer audioPlayer;

    public bool finished = false;

    // Update is called once per frame
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
