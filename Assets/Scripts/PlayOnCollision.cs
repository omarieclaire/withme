using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnCollision : MonoBehaviour
{
    public ParticleSystem ps;
    public AudioPlayer audioPlayer;
    public AudioClip clip;

    public bool die;
    public bool changeColor;
    public float randomizePitch;

    public bool grower;
    public bool isSpike;

    public float maxSize;
    public float minSize = 0.05f;
    public float startSize;
    public float growSizePerHit;
    public float shrinkSizePerHit;

    public ParticleSystem growSuccessParticles;
    public AudioClip growSuccessClip;

    public bool changeColorOnHit;
    public float hueShiftPerHit;
    public float startHue;
    public float currentHue;

    public bool followOnColliderTarget;
    public bool followOnColliderSpike;

    public Transform followTransform;
    public float collideTime;
    public LineRenderer followLine;
    public float increaseCollider;

    public float followForce;
    public float dampening;
    public Vector3 vel;

    void OnCollisionEnter(Collision collision)
    {
        GameObject thisObject = gameObject;  // This is the object the script is attached to (FlorpTarget or FlorpSpike)
        GameObject otherObject = collision.gameObject;  // This is the object colliding with it (Florp)

        // Debug.Log("This object: " + thisObject.name);
        // examples: 
        // This object: FlorpTarget(Clone)
        // This object: FlorpSpike(Clone)



        // If the other object is FLORP, we proceed with shrinking this object (FlorpTarget or FlorpSpike)
        if (otherObject.tag == "FLORP")
        {
            Debug.Log("Collision detected between: " + thisObject.name + " and " + otherObject.name);

            // Shrink this object (FlorpTarget or FlorpSpike)
            ShrinkOtherObject(thisObject);

            // Handle other effects like particles and audio
            PlayParticleEffect(collision);
            PlayAudioClip();

            if (die)
            {
                Destroy(thisObject);
            }

            if (changeColorOnHit)
            {
                UpdateColor();
            }

            // Handle following logic
            if (followOnColliderTarget || followOnColliderSpike)
            {
                HandleFollowOnCollider(collision, followOnColliderTarget);
            }
        }
    }



    private void ShrinkOtherObject(GameObject obj)
    {
        // Shrink the object by a factor of 0.5 (50% of its current size) until a minimum scale is reached
        Vector3 newScale = obj.transform.localScale * 0.1f;  // Change shrink factor as needed
        if (newScale.x < minSize) newScale = Vector3.one * minSize;  // Prevent the object from shrinking below a minimum size
        obj.transform.localScale = newScale;

        Debug.Log("Shrinking object: " + obj.name + " to new scale: " + obj.transform.localScale);
    }




    private void PlayParticleEffect(Collision collision)
    {
        // Play the particle system at the collision position, if available
        if (ps != null)
        {
            ps.transform.position = collision.gameObject.transform.position;
            ps.Play();
        }
    }

    private void PlayAudioClip()
    {
        // Play the audio clip with randomized pitch, if audioPlayer and clip are available
        if (audioPlayer != null && clip != null)
        {
            float fPitch = 1.0f + Random.Range(-randomizePitch, randomizePitch);
            audioPlayer.Play(clip, fPitch);
        }
    }

    private void ResetGrowth()
    {
        // Handle growth reset when max size is reached
        // Debug.Log("333 entered condition -- transform.localScale.x >= maxSize");
        growSuccessParticles.Play();
        audioPlayer.Play(growSuccessClip);
        currentHue = startHue;
        // Debug.Log("333[GROW] Florp reached max size, resetting to start size.");
    }

    private void UpdateColor()
    {
        // Increment the hue by the specified amount and wrap around if necessary
        currentHue += hueShiftPerHit;
        currentHue %= 1;
        Color color = Color.HSVToRGB(currentHue, 1, 1);

        // Update material properties based on the new hue
        Material m = GetComponent<Renderer>().material;
        m.SetColor("_Color", color);
        m.SetColor("_BaseColor", color * .1f);
        m.SetColor("_ReflectionColor", color);
        m.SetColor("_CenterOrbColor", color * .1f);
        m.SetColor("_NoiseColor", color * 2);
    }

    private void HandleFollowOnCollider(Collision collision, bool isTarget)
    {
        // Debug.Log("in the collision");
        // Store the transform of the collided object and reset velocity
        // this is the "unity transform" - I found out by logging
        followTransform = collision.gameObject.transform;
        vel = Vector3.zero;
        collideTime = Time.time;

        // Adjust the scale of the object based on whether it's a target or spike
        float scaleChange = isTarget ? increaseCollider : -increaseCollider;

        collision.gameObject.transform.localScale += Vector3.one * scaleChange;

        // Disable the collider to prevent further collisions
        GetComponent<SphereCollider>().enabled = false;

        // If the object has grown beyond the maximum size, reset its growth
        if (transform.localScale.x >= maxSize)
        {
            ResetGrowth();
        }
    }

    public Vector3 randomDir(float collideTime)
    {
        // Generate a random direction based on sine and cosine functions
        return new Vector3(Mathf.Sin(collideTime), Mathf.Cos(collideTime), Mathf.Sin(collideTime * 2));
    }

    void Update()
    {
        // If following a transform, apply force and update positions
        if (followTransform != null)
        {
            Vector3 force = (followTransform.position - transform.position + randomDir(collideTime)) * followForce;
            vel += force * Time.deltaTime;
            transform.position += vel * Time.deltaTime;
            vel *= dampening;

            // Update the line renderer positions to visualize the connection
            followLine.SetPosition(0, transform.position);
            followLine.SetPosition(1, followTransform.position);
        }
    }

    void OnEnable()
    {
        // Initialize hue when the object is enabled
        currentHue = startHue;
    }
}
