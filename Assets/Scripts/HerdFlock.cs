// Flock: Players herd little spheres to and through a portal. The little spheres then come out ofthe top of the dome, transformed. (Maybe after 100 spheres go through the portal something happens?)

// Player Repel Radius (1.29): This defines how close a player needs to be to repel the fish. A lower value means the fish will only be repelled when the player is very close. This looks reasonable depending on the size of the play area and the dome. You can start at 1.0 and adjust based on testing.
// Player Repel Force (0.66): This controls how strongly the fish are pushed away from the player when within the repel radius. You might want to start with a value between 0.5 and 1 and adjust based on how aggressive you want the fish to move away from players.
// Player Attract Radius (0.78): This is the radius where fish start moving toward the player, but only if they're outside the repel radius. A value close to the repel radius means fish will quickly switch from being repelled to being attracted, which can create jittery behavior. Try a slightly larger attract radius—around 1.5—so fish have a smoother transition.
// Player Attract Force (0.93): This is how strongly fish are attracted to players. A value of 0.5 to 1.5 should be good depending on how responsive you want the fish to be. Your current value of 0.93 is reasonable, but you can test a slightly higher value for more aggressive attraction.
// Noise Force (0.66): This adds a random force to each fish to make them look more natural and not move in straight lines. Too much noise will make the fish appear erratic, while too little will make them feel robotic. A value between 0.3 and 0.6 is a good starting point for natural movement. Your current value of 0.66 is on the higher side; you could reduce it slightly for smoother movement.
// Force Towards Dome (0.48): This is the force that pulls fish toward the dome walls. The value depends on how strong the dome’s "gravity" effect is. Try a value between 0.3 and 0.5 for a gentle pull. If you want fish to more strongly stick to the dome walls, you can increase this value, but start small.
// Force to Above Horizon (0.39): This is meant to push fish back up if they dip below a certain height. A value of 0.3 to 0.5 is usually good to keep fish from sinking too low while still giving them room to explore the lower areas.
// Fish Speed (0.39): This determines how fast the fish move overall. Start with a value of 0.5 to 1. If you want slower, more relaxed fish, keep it below 1. Faster, more frenetic fish should have a value of 1 to 2.
// Noise Speed (0.24): This controls how quickly the random noise changes over time. A lower value will make the fish movements more predictable and less chaotic. Start with something around 0.1 to 0.3 if you want smooth, natural-looking movement.
// Noise Size (1.78): This affects the range of the random noise. Too large and fish may appear to jerk around too much. A value between 1 and 2 should give some randomness without being too extreme. You can try reducing this if fish appear too jittery.


// Player Repel Radius: 1.0
// Player Repel Force: 0.75
// Player Attract Radius: 1.5
// Player Attract Force: 1.0
// Noise Force: 0.5
// Force Towards Dome: 0.4
// Force To Above Horizon: 0.35
// Fish Speed: 0.8
// Noise Speed: 0.2
// Noise Size: 1.5



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public Controller controller;
    [Tooltip("Number of fish in the flock.")]
    public int numFish;

    [Tooltip("Size of each fish in the flock.")]
    public float fishSize = .1f;

    [Tooltip("Prefab used to create fish.")]
    public GameObject fishPrefab;

    [Tooltip("List of all fish in the flock.")]
    public List<Fish> fishes;

    [Tooltip("List of velocities for each fish.")]
    public List<Vector3> velocities;

    [Tooltip("LineRenderers to visualize attraction forces.")]
    public List<LineRenderer> attractLR;

    [Tooltip("LineRenderers to visualize repulsion forces.")]
    public List<LineRenderer> repelLR;

    [Tooltip("If the fish are within this radium you are pushing them away from you")]
    public float playerRepelRadius;

    [Tooltip("How hard/fast you push the fish away.")]
    public float playerRepelForce;

    [Tooltip("Radius within which players attract the fish. If the fish are outside the repel radius and inside the attract radius, they move towards me")]
    public float playerAttractRadius;

    [Tooltip("Force with which players attract the fish. How hard they move towards you!")]
    public float playerAttractForce;

    [Tooltip("Random noise force applied to the fish to separate the fish from each other. But the dream would be flocking")]
    public float noiseForce;

    [Tooltip("Force pulling the fish towards the dome, basically trapping them in the dome wall instead of outerspace or innerspace")]
    public float forceTowardsDome;

    [Tooltip("Force pushing the fish above the horizon.")]
    public float forceToAboveHorizon;

    [Tooltip("How fast the fish move.")]
    public float fishSpeed;

    [Tooltip("Speed of the noise affecting the fish. How fast the noise changes!")]
    public float noiseSpeed = 0;

    [Tooltip("Size of the noise affecting the fish. How big the noise is!")]
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
