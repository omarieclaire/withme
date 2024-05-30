using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicShape : MonoBehaviour
{

    public int numSpheres;

    public List<GameObject> spheres;
    public GameObject spherePrefab;

    public Controller controller;

    public List<bool> spheresActive;

    public float sphereSize;

    public void OnEnable()
    {

        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        spheres = new List<GameObject>();
        spheresActive = new List<bool>();

        for (int i = 0; i < numSpheres; i++)
        {
            GameObject sphere = Instantiate(spherePrefab, Vector3.zero, Quaternion.identity);
            sphere.transform.position = Vector3.zero;
            sphere.transform.localScale = Vector3.one * sphereSize;
            sphere.transform.SetParent(transform);
            sphere.name = $"Sphere_{i}";
            spheres.Add(sphere);
            spheresActive.Add(false);
        }

        NewShapeSet();
    }


    public int numShapesActivated;

    public void NewShapeSet()
    {

        numShapesActivated = 0;
        lastTimeChange = Time.time;



        for (int i = 0; i < numSpheres; i++)
        {
            spheresActive[i] = false;

            Vector3 randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 2f), Random.Range(-1f, 1f));
            randomPos = controller.getFinalPosition(randomPos);

            spheres[i].transform.position = randomPos;

        }

    }


    public float timeBetweenChanges;
    public float lastTimeChange;

    public float activationRadius;




    // Update is called once per frame
    void Update()
    {

        if (Time.time - lastTimeChange > timeBetweenChanges)
        {
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
                }

            }

            if (sphereActive)
            {

                spheres[j].GetComponent<Renderer>().material.color = Color.green;
                spheresActive[j] = true;
            }
            else
            {
                spheres[j].GetComponent<Renderer>().material.color = Color.red;
                spheresActive[j] = false;
            }

        }


        numShapesActivated = 0;
        for (int i = 0; i < numSpheres; i++)
        {
            if (spheresActive[i])
            {
                numShapesActivated++;
            }
        }

        if (numShapesActivated == numSpheres)
        {
            OnShapeComplete();
        }


    }


    public AudioPlayer audio;
    public AudioClip onShapeCompleteClip;

    public ParticleSystem onShapeCompleteParticles;


    void OnShapeComplete()
    {

        audio.Play(onShapeCompleteClip);
        onShapeCompleteParticles.transform.position = transform.position;
        onShapeCompleteParticles.Play();

        NewShapeSet();


    }




}
