// // Flock: Players herd little spheres to and through a portal. The little spheres then come out ofthe top of the dome, transformed. (Maybe after 100 spheres go through the portal something happens?)

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Flock : MonoBehaviour
// {
//     public Controller controller;
//     [Tooltip("Number of fish in the flock.")]
//     public int numFish;

//     [Tooltip("Size of each fish in the flock.")]
//     public float fishSize = .1f;

//     [Tooltip("Prefab used to create fish.")]
//     public GameObject fishPrefab;

//     [Tooltip("List of all fish in the flock.")]
//     public List<Fish> fishes;

//     [Tooltip("List of velocities for each fish.")]
//     public List<Vector3> velocities;

//     [Tooltip("LineRenderers to visualize attraction forces.")]
//     public List<LineRenderer> attractLR;

//     [Tooltip("LineRenderers to visualize repulsion forces.")]
//     public List<LineRenderer> repelLR;

//     [Tooltip("If the fish are within this radium you are pushing them away from you")]
//     public float playerRepelRadius;

//     [Tooltip("How hard/fast you push the fish away.")]
//     public float playerRepelForce;

//     [Tooltip("Radius within which players attract the fish. If the fish are outside the repel radius and inside the attract radius, they move towards me")]
//     public float playerAttractRadius;

//     [Tooltip("Force with which players attract the fish. How hard they move towards you!")]
//     public float playerAttractForce;

//     [Tooltip("Random noise force applied to the fish to separate the fish from each other. But the dream would be flocking")]
//     public float noiseForce;

//     [Tooltip("Force pulling the fish towards the dome, basically trapping them in the dome wall instead of outerspace or innerspace")]
//     public float forceTowardsDome;

//     [Tooltip("Force pushing the fish above the horizon.")]
//     public float forceToAboveHorizon;

//     [Tooltip("How fast the fish move.")]
//     public float fishSpeed;

//     [Tooltip("Speed of the noise affecting the fish. How fast the noise changes!")]
//     public float noiseSpeed = 0;

//     [Tooltip("Size of the noise affecting the fish. How big the noise is!")]
//     public float noiseSize = 1;

//     public void OnEnable()
//     {

//         while (fishes.Count > 0)
//         {
//             Fish fish = fishes[0];
//             fishes.RemoveAt(0);
//             Destroy(fish.gameObject);
//         }

//         fishes.Clear();
//         velocities.Clear();

//         for (int i = 0; i < numFish; i++)
//         {
//             Fish fish = Instantiate(fishPrefab).GetComponent<Fish>();
//             fish.transform.position = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f));
//             fish.transform.localScale = Vector3.one * fishSize;

//             fish.transform.position = controller.getFinalPositionObjects(fish.transform.position);
//             fish.transform.parent = this.transform;
//             fish.flock = this;

//             fish.gameObject.SetActive(true);

//             fishes.Add(fish);
//             attractLR.Add(fish.attractLR);
//             repelLR.Add(fish.repelLR);
//             velocities.Add(Vector3.zero);

//         }

//     }

//     // Update is called once per frame
//     void Update()
//     {

//         for (int i = 0; i < fishes.Count; i++)
//         {
//             Fish fish = fishes[i];
//             Vector3 pos = fish.transform.position;

//             Vector3 vel = velocities[i];
//             Vector3 force = Vector3.zero;

//             float closestPlayerDist = 10000;
//             int closestPlayerID = -1;

//             for (int j = 0; j < controller.activePlayers.Count; j++)
//             {

//                 Vector3 dif = controller.activePlayers[j].transform.position - pos;
//                 float dist = dif.magnitude;

//                 if (dist < closestPlayerDist)
//                 {
//                     closestPlayerDist = dist;
//                     closestPlayerID = j;
//                 }

//                 if (dist < playerRepelRadius)
//                 {
//                     force -= dif.normalized * playerRepelForce;
//                 }

//                 if (dist < playerAttractRadius && dist > playerRepelRadius)
//                 {
//                     force += dif.normalized * playerAttractForce;
//                 }


//             }

//             if (closestPlayerDist < playerRepelRadius)
//             {
//                 repelLR[i].positionCount = 2;
//                 repelLR[i].SetPosition(0, pos);
//                 repelLR[i].SetPosition(1, controller.activePlayers[closestPlayerID].transform.position);
//             }
//             else
//             {
//                 repelLR[i].positionCount = 0;
//             }

//             if (closestPlayerDist < playerAttractRadius && closestPlayerDist > playerRepelRadius)
//             {
//                 attractLR[i].positionCount = 2;
//                 attractLR[i].SetPosition(1, pos);
//                 attractLR[i].SetPosition(0, controller.activePlayers[closestPlayerID].transform.position);
//             }
//             else
//             {
//                 attractLR[i].positionCount = 0;
//             }



//             Vector3 randomForce = new Vector3(
//                 Mathf.Sin(Time.time * noiseSpeed * 4 + pos.x * noiseSize + i * 11),
//                 Mathf.Sin(Time.time * noiseSpeed * 2.3f + pos.y * noiseSize + i * 30),
//                 Mathf.Sin(Time.time * noiseSpeed * 3.7f + pos.z * noiseSize + i * 60)
//             );

//             force += randomForce * noiseForce;

//             force += (pos.normalized * controller.sphereSize - pos) * forceTowardsDome;

//             if (pos.y < 0)
//             {
//                 force += Vector3.up * forceToAboveHorizon;
//             }


//             vel += force;
//             vel = vel.normalized * fishSpeed;

//             pos += vel * Time.deltaTime;

//             fish.transform.position = pos; //controller.getFinalPositionObjects(pos);

//             velocities[i] = vel;

//         }

//     }

//     public void OnFishEnter(Fish fish, Collider other)
//     {
//         print("Fish entered");
//     }


// }
