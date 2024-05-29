using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public Controller controller;
    public int numFish;
    public float fishSize = .1f;
    public GameObject fishPrefab;

    public List<Fish> fishes;

    public List<Vector3> velocities;

    public List<LineRenderer> attractLR;
    public List<LineRenderer> repelLR;

    public float playerRepelRadius;
    public float playerRepelForce;

    public float playerAttractRadius;
    public float playerAttractForce;

    public float noiseForce;
    public float forceTowardsDome;

    public float forceToAboveHorizon;

    public float fishSpeed;

    public float noiseSpeed = 0;
    public float noiseSize = 1;

    public void OnEnable()
    {

        while (fishes.Count > 0)
        {
            Fish fish = fishes[0];
            fishes.RemoveAt(0);
            Destroy(fish.gameObject);
        }

        fishes.Clear();
        velocities.Clear();

        for (int i = 0; i < numFish; i++)
        {
            Fish fish = Instantiate(fishPrefab).GetComponent<Fish>();
            fish.transform.position = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f));
            fish.transform.localScale = Vector3.one * fishSize;

            fish.transform.position = controller.getFinalPosition(fish.transform.position);
            fish.transform.parent = this.transform;
            fish.flock = this;

            fish.gameObject.SetActive(true);

            fishes.Add(fish);
            attractLR.Add(fish.attractLR);
            repelLR.Add(fish.repelLR);
            velocities.Add(Vector3.zero);

        }

    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < fishes.Count; i++)
        {
            Fish fish = fishes[i];
            Vector3 pos = fish.transform.position;

            Vector3 vel = velocities[i];
            Vector3 force = Vector3.zero;

            float closestPlayerDist = 10000;
            int closestPlayerID = -1;

            for (int j = 0; j < controller.activePlayers.Count; j++)
            {

                Vector3 dif = controller.activePlayers[j].transform.position - pos;
                float dist = dif.magnitude;

                if (dist < closestPlayerDist)
                {
                    closestPlayerDist = dist;
                    closestPlayerID = j;
                }

                if (dist < playerRepelRadius)
                {
                    force -= dif.normalized * playerRepelForce;
                }

                if (dist < playerAttractRadius && dist > playerRepelRadius)
                {
                    force += dif.normalized * playerAttractForce;
                }


            }

            if (closestPlayerDist < playerRepelRadius)
            {
                repelLR[i].positionCount = 2;
                repelLR[i].SetPosition(0, pos);
                repelLR[i].SetPosition(1, controller.activePlayers[closestPlayerID].transform.position);
            }
            else
            {
                repelLR[i].positionCount = 0;
            }

            if (closestPlayerDist < playerAttractRadius && closestPlayerDist > playerRepelRadius)
            {
                attractLR[i].positionCount = 2;
                attractLR[i].SetPosition(1, pos);
                attractLR[i].SetPosition(0, controller.activePlayers[closestPlayerID].transform.position);
            }
            else
            {
                attractLR[i].positionCount = 0;
            }



            Vector3 randomForce = new Vector3(
                Mathf.Sin(Time.time * noiseSpeed * 4 + pos.x * noiseSize + i * 11),
                Mathf.Sin(Time.time * noiseSpeed * 2.3f + pos.y * noiseSize + i * 30),
                Mathf.Sin(Time.time * noiseSpeed * 3.7f + pos.z * noiseSize + i * 60)
            );

            force += randomForce * noiseForce;

            force += (pos.normalized * controller.sphereSize - pos) * forceTowardsDome;

            if (pos.y < 0)
            {
                force += Vector3.up * forceToAboveHorizon;
            }


            vel += force;
            vel = vel.normalized * fishSpeed;

            pos += vel * Time.deltaTime;

            fish.transform.position = pos; //controller.getFinalPosition(pos);

            velocities[i] = vel;

        }

    }

    public void OnFishEnter(Fish fish, Collider other)
    {
        print("Fish entered");
    }


}
