using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTargets : MonoBehaviour
{

    public GameObject targetPrefab;

    public int numTargets;

    public Controller controller;

    public float targetSize;


    public List<PlayOnCollision> miniFlorps;


    void OnEnable()
    {

        while (transform.childCount > 0)
        {
            Transform c = transform.GetChild(0);
            Destroy(c.gameObject);
        }

        miniFlorps = new List<PlayOnCollision>();

        for (int i = 0; i < numTargets; i++)
        {
            GameObject target = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);

            Vector3 randomPosition = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            target.transform.parent = transform;
            target.transform.localScale = new Vector3(targetSize, targetSize, targetSize);
            target.transform.position = controller.getFinalPosition(randomPosition);

            PlayOnCollision poc = target.GetComponent<PlayOnCollision>();
            miniFlorps.Add(poc);



        }



    }

    public GameObject Florp;

    public ParticleSystem ps;
    public AudioClip clip;
    public AudioPlayer audioPlayer;

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

        if (done == true)
        {

            audioPlayer.Play(clip);
            ps.transform.position = Florp.transform.position;
            ps.Play();

        }
    }
}
