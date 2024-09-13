// using System.Collections.Generic;
// using UnityEngine;

// public class FlockManager : MonoBehaviour
// {
//     public int numFish = 100;
//     public GameObject fishPrefab;
//     public List<Fish> fishes = new List<Fish>();

//     // Forces for flocking behavior
//     public float separationForce = 1.5f;
//     public float alignmentForce = 1.0f;
//     public float cohesionForce = 1.0f;

//     // Radii for flocking behaviors
//     public float separationRadius = 1.5f;
//     public float alignmentRadius = 2.0f;
//     public float cohesionRadius = 2.5f;

//     // Fish movement parameters
//     public float baseSpeed = 3.0f;
//     public float minSpeed = 1.0f;
//     public float maxSpeed = 6.0f;
//     public float minNoise = 0.1f;
//     public float maxNoise = 1.0f;
//     public float fishSize = 0.1f;

//     // Portal reference to calculate proximity
//     public Transform portal;

//     // Fish trail settings
//     public float minTrailWidth = 0.05f;
//     public float maxTrailWidth = 0.2f;
//     public Color farColor = Color.blue;
//     public Color nearColor = Color.red;

// void Start()
// {
//     // Instantiate all the fish at random positions
//     for (int i = 0; i < numFish; i++)
//     {
//         GameObject fishObject = Instantiate(fishPrefab, Random.insideUnitSphere * 10, Quaternion.identity);
//         fishObject.transform.localScale = Vector3.one * fishSize;
        
//         Fish fish = fishObject.GetComponent<Fish>();

//         // Assign the FlockManager, not the GameObject itself
//         fish.flock = this;  // 'this' refers to the FlockManager component

//         fishes.Add(fish);
//     }
// }




//     void Update()
//     {
//         foreach (Fish fish in fishes)
//         {
//             // Update flocking behavior forces
//             Vector3 separation = CalculateSeparation(fish);
//             Vector3 alignment = CalculateAlignment(fish);
//             Vector3 cohesion = CalculateCohesion(fish);

//             // Apply forces to fish
//             Vector3 force = separation * separationForce + alignment * alignmentForce + cohesion * cohesionForce;
//             fish.ApplyForce(force);

//             // Update visual effects
//             UpdateFishTrail(fish);
//         }
//     }

//     // Separation: Avoid crowding local flockmates
//     Vector3 CalculateSeparation(Fish fish)
//     {
//         Vector3 separation = Vector3.zero;
//         int nearbyFishCount = 0;

//         foreach (Fish otherFish in fishes)
//         {
//             if (otherFish != fish)
//             {
//                 float distance = Vector3.Distance(fish.transform.position, otherFish.transform.position);
//                 if (distance < separationRadius)
//                 {
//                     separation -= (otherFish.transform.position - fish.transform.position).normalized / distance;
//                     nearbyFishCount++;
//                 }
//             }
//         }

//         return nearbyFishCount > 0 ? separation / nearbyFishCount : separation;
//     }

//     // Alignment: Align velocity with nearby flockmates
//     Vector3 CalculateAlignment(Fish fish)
//     {
//         Vector3 alignment = Vector3.zero;
//         int nearbyFishCount = 0;

//         foreach (Fish otherFish in fishes)
//         {
//             if (otherFish != fish)
//             {
//                 float distance = Vector3.Distance(fish.transform.position, otherFish.transform.position);
//                 if (distance < alignmentRadius)
//                 {
//                     alignment += otherFish.GetVelocity();
//                     nearbyFishCount++;
//                 }
//             }
//         }

//         return nearbyFishCount > 0 ? alignment / nearbyFishCount : alignment;
//     }

//     // Cohesion: Move toward the center of mass of nearby flockmates
//     Vector3 CalculateCohesion(Fish fish)
//     {
//         Vector3 cohesion = Vector3.zero;
//         int nearbyFishCount = 0;

//         foreach (Fish otherFish in fishes)
//         {
//             if (otherFish != fish)
//             {
//                 float distance = Vector3.Distance(fish.transform.position, otherFish.transform.position);
//                 if (distance < cohesionRadius)
//                 {
//                     cohesion += otherFish.transform.position;
//                     nearbyFishCount++;
//                 }
//             }
//         }

//         return nearbyFishCount > 0 ? (cohesion / nearbyFishCount - fish.transform.position) : cohesion;
//     }

//     // Update trail visuals based on speed and proximity to the portal
//     void UpdateFishTrail(Fish fish)
//     {
//         TrailRenderer trail = fish.GetComponent<TrailRenderer>();
//         float proximityToPortal = Vector3.Distance(fish.transform.position, portal.position);

//         // Update trail width based on proximity
//         float width = Mathf.Lerp(minTrailWidth, maxTrailWidth, proximityToPortal / 10.0f);
//         trail.startWidth = width;
//         trail.endWidth = width * 0.5f;

//         // Update trail color based on proximity
//         Color trailColor = Color.Lerp(farColor, nearColor, proximityToPortal / 10.0f);
//         trail.material.SetColor("_Color", trailColor);
//     }
// }
