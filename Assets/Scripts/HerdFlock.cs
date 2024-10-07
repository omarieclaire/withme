
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
    public GameObject portal;
    public GameObject portalCollider;



    public GameObject portalColliderParticlesPrefab;

    public float portalRadius = 0.5f; // How close the fish need to be to trigger the portal
    public float disappearTime = 3f; // Time to disappear after transformation

    public NoGoZoneManager noGoZoneManager;

    [Tooltip("Number of fish in the flock.")]
    public int numFish;

    private Dictionary<Fish, bool> fishCollisionStatus = new Dictionary<Fish, bool>();


    [Tooltip("Size of each fish in the flock.")]
    public float fishSize = .1f;

    [Tooltip("Prefab used to create fish.")]
    public GameObject fishPrefab;

    [Tooltip("List of all fish in the flock.")]
    public List<Fish> fishes;

    [Tooltip("List of velocities for each fish.")]
    public List<Vector3> velocities;

    [Tooltip("Dome radius - the distance from the center where fish should stay.")]
    public float domeRadius = 10.0f;

    [Tooltip("Flocking parameters.")]
    public float separationRadius = 1.0f;
    public float alignmentRadius = 1.5f;
    public float cohesionRadius = 2.0f;
    public float maxSpeed = 2.0f;
    public float maxForce = 0.03f;

    public float PlayerRepelRadius = 1.0f; // How close a player needs to be to repel fish
    public float PlayerRepelForce = 0.75f; // The strength of the repel force


    public float noiseForce = 0.5f;
    public float fishSpeed = 1.0f;

    public float noiseSpeed = 0.2f;
    public float noiseSize = 1.5f;

    public float forceToAboveHorizon = 0.35f; // Force to push fish back up if they go below the horizon


    private void OnEnable()
    {
        // Clear existing fish and velocities
        while (fishes.Count > 0)
        {
            Fish fish = fishes[0];
            fishes.RemoveAt(0);
            Destroy(fish.gameObject);
        }

        fishes.Clear();
        velocities.Clear();

        // Instantiate fish outside of no-go zones
        for (int i = 0; i < numFish; i++)
        {
            Vector3 position;
            do
            {
                position = Random.onUnitSphere * domeRadius; // Position on dome surface
            } while (noGoZoneManager.IsInNoGoZone(position)); // Ensure it's outside a no-go zone

            Fish fish = Instantiate(fishPrefab).GetComponent<Fish>();
            fish.transform.position = position;
            fish.transform.localScale = Vector3.one * fishSize;

            Rigidbody rb = fish.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;  // Disable gravity
            rb.isKinematic = true;  // Make the Rigidbody kinematic


            // Ensure each fish has a collider
            if (fish.GetComponent<Collider>() == null)
            {
                fish.gameObject.AddComponent<SphereCollider>();  // Adds a SphereCollider if none exists
            }

            fish.transform.parent = this.transform;
            fish.flock = this;

            fish.gameObject.SetActive(true);

            fishes.Add(fish);
            velocities.Add(Vector3.zero);
        }

        // Set portal to the top of the dome
        portal.transform.position = new Vector3(0, domeRadius, 0); // Top of the dome
    }





    void Update()
    {
        for (int i = 0; i < fishes.Count; i++)
        {
            Fish fish = fishes[i];
            Vector3 pos = fish.transform.position;
            Vector3 vel = velocities[i];
            Vector3 force = Vector3.zero;

            Vector3 separation = Separate(i);
            Vector3 alignment = Align(i);
            Vector3 cohesion = Cohere(i);

            // Combine flocking forces
            force += separation * 1.5f + alignment * 1.0f + cohesion * 1.0f;

            // Apply player repel force
            force += ApplyPlayerRepel(fish.transform.position);

            // Apply noise for natural movement
            force += ApplyNoise(pos, i);

            // Apply dome surface constraint
            force += ApplyDomeSurfaceConstraint(pos);

            // Limit force to avoid too abrupt movements
            force = Vector3.ClampMagnitude(force, maxForce);

            vel += force;
            vel = Vector3.ClampMagnitude(vel, maxSpeed);

            pos += vel * Time.deltaTime;
            pos = pos.normalized * (domeRadius + (fishSize / 2)); // Keep fish on the dome surface

            // Prevent fish from entering no-go zones
            if (noGoZoneManager.IsInNoGoZone(pos))
            {
                vel = -vel; // Reverse the direction if entering a no-go zone
            }

            fish.transform.position = pos;
            fish.transform.forward = vel.normalized;
            velocities[i] = vel;

            // You no longer need to check portalCollider direction here, as the collision will handle it
        }
    }

//   private void OnTriggerEnter(Collider other)
// {
//     Fish fish = other.GetComponent<Fish>();
//     if (fish != null && !fishCollisionStatus.ContainsKey(fish))
//     {
//         Debug.Log("Fish entered the portal: " + fish.name); // Debug message for collision detection
//         fishCollisionStatus[fish] = true; // Mark the fish as collided
//         TriggerportalColliderEffect(fish);
//     }
// }


    // Method to trigger particle effects and handle fish disappearance
    public void TriggerportalColliderEffect(Fish fish)
    {
        // Instantiate particles at the fish's position
        if (portalColliderParticlesPrefab != null)
        {
            Debug.Log("Playing particles for fish " + fish.name);
            Instantiate(portalColliderParticlesPrefab, fish.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("portalCollider particles prefab is not assigned!");
        }

        // Shrink and destroy the fish
        StartCoroutine(TransformAndDisappear(fish));
    }

   private IEnumerator TransformAndDisappear(Fish fish)
{
    Renderer fishRenderer = fish.GetComponent<Renderer>();
    TrailRenderer trailRenderer = fish.GetComponent<TrailRenderer>();

    if (fishRenderer == null)
    {
        yield break;  // If there's no renderer, exit the coroutine
    }

    // For testing: Scale the fish up by 5x for visibility
    fish.transform.localScale *= 5f;

    if (trailRenderer != null)
    {
        trailRenderer.enabled = false;  // Disable the TrailRenderer
    }

    fishRenderer.material.color = Color.cyan;  // Change color to cyan for visual feedback

    Vector3 originalScale = fish.transform.localScale;
    float timer = 0f;

    while (timer < disappearTime)
    {
        if (fish == null || fish.gameObject == null)
        {
            Debug.Log("Fish already destroyed during the coroutine.");
            yield break;
        }

        timer += Time.deltaTime;
        float scale = Mathf.Lerp(1f, 0f, timer / disappearTime);
        fish.transform.localScale = originalScale * scale;

        Color color = fishRenderer.material.color;
        color.a = Mathf.Lerp(1f, 0f, timer / disappearTime);  // Fade out
        fishRenderer.material.color = color;

        yield return null;
    }

    if (fish != null && fish.gameObject != null)
    {
        Debug.Log("Fish " + fish.name + " is being destroyed.");
        fishes.Remove(fish);
        Destroy(fish.gameObject);  // Destroy the fish after it disappears
    }
    else
    {
        Debug.LogWarning("Fish has already been destroyed before reaching this point.");
    }
}




    // Method to repel fish from players
    Vector3 ApplyPlayerRepel(Vector3 fishPos)
    {
        Vector3 repelForce = Vector3.zero;

        foreach (var player in controller.players) // Assuming you have access to the players
        {
            float distance = Vector3.Distance(fishPos, player.transform.position);
            if (distance < PlayerRepelRadius)
            {
                Vector3 direction = (fishPos - player.transform.position).normalized;
                repelForce += direction * PlayerRepelForce / distance;
            }
        }

        return repelForce;
    }




    // Separate: steer away from nearby flockmates to avoid crowding
    Vector3 Separate(int fishIndex)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;

        for (int i = 0; i < fishes.Count; i++)
        {
            if (i == fishIndex) continue; // Skip self

            float distance = Vector3.Distance(fishes[fishIndex].transform.position, fishes[i].transform.position);
            if (distance < separationRadius && distance > 0)
            {
                Vector3 diff = fishes[fishIndex].transform.position - fishes[i].transform.position;
                diff.Normalize();
                diff /= distance; // Weight by distance
                steer += diff;
                count++;
            }
        }

        if (count > 0)
        {
            steer /= count; // Average
        }

        if (steer.magnitude > 0)
        {
            steer.Normalize();
            steer *= maxSpeed;
            steer -= velocities[fishIndex];
            steer = Vector3.ClampMagnitude(steer, maxForce); // Limit force
        }

        return steer;
    }

    // Align: steer towards the average heading of nearby flockmates
    Vector3 Align(int fishIndex)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < fishes.Count; i++)
        {
            if (i == fishIndex) continue; // Skip self

            float distance = Vector3.Distance(fishes[fishIndex].transform.position, fishes[i].transform.position);
            if (distance < alignmentRadius)
            {
                sum += velocities[i];
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count; // Average
            sum.Normalize();
            sum *= maxSpeed;

            Vector3 steer = sum - velocities[fishIndex];
            steer = Vector3.ClampMagnitude(steer, maxForce); // Limit force
            return steer;
        }

        return Vector3.zero;
    }

    // Cohere: steer towards the average position of nearby flockmates
    Vector3 Cohere(int fishIndex)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < fishes.Count; i++)
        {
            if (i == fishIndex) continue; // Skip self

            float distance = Vector3.Distance(fishes[fishIndex].transform.position, fishes[i].transform.position);
            if (distance < cohesionRadius)
            {
                sum += fishes[i].transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count; // Average position
            return Seek(sum, fishIndex); // Steer towards average position
        }

        return Vector3.zero;
    }

    // Seek: helper function to steer towards a target position
    Vector3 Seek(Vector3 target, int fishIndex)
    {
        Vector3 desired = target - fishes[fishIndex].transform.position;
        desired.Normalize();
        desired *= maxSpeed;

        Vector3 steer = desired - velocities[fishIndex];
        steer = Vector3.ClampMagnitude(steer, maxForce); // Limit force
        return steer;
    }

    // Apply noise for more natural movement
    Vector3 ApplyNoise(Vector3 pos, int fishIndex)
    {
        return new Vector3(
            Mathf.Sin(Time.time * noiseSpeed * 4 + pos.x * noiseSize + fishIndex * 11),
            Mathf.Sin(Time.time * noiseSpeed * 2.3f + pos.y * noiseSize + fishIndex * 30),
            Mathf.Sin(Time.time * noiseSpeed * 3.7f + pos.z * noiseSize + fishIndex * 60)
        ) * noiseForce;
    }

    // Apply a force that keeps fish on the dome surface and above the horizon
    Vector3 ApplyDomeSurfaceConstraint(Vector3 pos)
    {
        Vector3 toCenter = -pos.normalized; // Direction toward the center of the dome
        float distanceFromSurface = (domeRadius - fishSize / 2) - pos.magnitude; // How far the fish is from the dome surface

        // Base force to keep fish on the dome surface
        Vector3 force = toCenter * distanceFromSurface * maxForce;

        // Check if the fish is below the horizon (y < 0) and apply an upward force
        if (pos.y < 0)
        {
            force += Vector3.up * forceToAboveHorizon; // Push fish upward if below the horizon
        }

        return force;
    }
}
