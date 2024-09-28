// using for buddy

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnCollision : MonoBehaviour
{
    public AudioPlayer audioPlayer;

    public SoundEventSender soundEventSender;  

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

    public ParticleSystem growParticles;

    public ParticleSystem spikeParticles;

    public ParticleSystem winGameParticles;
    public AudioClip growSuccessClip;
    public AudioClip spikeClip;
    public AudioClip growClip;


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

    private bool hasCollided = false;  // Add this flag to prevent multiple triggers

void OnCollisionEnter(Collision collision)
{
    if (hasCollided) return;  // Prevent re-triggering the collision logic
    hasCollided = true;  // Set the flag when collision happens

    GameObject thisObject = gameObject;  // This is the object the script is attached to (FlorpTarget or FlorpSpike)
    GameObject otherObject = collision.gameObject;  // This is the FLORP object colliding with it

    if (otherObject.tag == "FLORP")
    {
        // Shrink this object (FlorpTarget or FlorpSpike)
        ShrinkOtherObject(thisObject);
        PlayParticleEffect(collision, thisObject);
        
        // PlayOldAudioClip(thisObject); // <-- keep old audio for debugging
        SendAudioSounds(thisObject, otherObject);


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
        // Shrink the object by a factor of 0.1 (10% of its current size) until a minimum scale is reached
        Vector3 newScale = obj.transform.localScale * 0.1f;  // Change shrink factor as needed
        if (newScale.x < minSize) newScale = Vector3.one * minSize;  // Prevent the object from shrinking below a minimum size
        obj.transform.localScale = newScale;

        Debug.Log("Shrinking object: " + obj.name + " to new scale: " + obj.transform.localScale);
    }


    private void PlayParticleEffect(Collision collision, GameObject thisObject)
    {
        Debug.Log("PlayParticleEffect triggered for: " + thisObject.name);

        // Check the type of object and play corresponding particle effects
        if (thisObject.name.Contains("FlorpTarget"))
        {
            if (growParticles != null)
            {
                Debug.Log("Playing grow particles for FlorpTarget");
                growParticles.transform.position = collision.contacts[0].point;  // Use collision point for accuracy
                growParticles.Play();
            }
            else
            {
                Debug.LogError("growParticles not assigned!");
            }
        }
        else if (thisObject.name.Contains("FlorpSpike"))
        {
            if (spikeParticles != null)  // Use the spike-specific particle system
            {
                Debug.Log("Playing spike particles for FlorpSpike");
                spikeParticles.transform.position = collision.contacts[0].point;
                spikeParticles.Play();
            }
            else
            {
                Debug.LogError("spikeParticles not assigned!");
            }
        }
        else
        {
            Debug.LogWarning("Object is neither FlorpTarget nor FlorpSpike. Check object names.");
        }
    }



    // private void PlayOldAudioClip(GameObject thisObject) // <--- keep for debugging
    // {
    //     // Play different audio clips based on the object type
    //     if (thisObject.name.Contains("FlorpTarget"))
    //     {
    //         if (audioPlayer != null && growSuccessClip != null)
    //         {
    //             float fPitch = 1.0f + Random.Range(-randomizePitch, randomizePitch);
    //             audioPlayer.Play(growSuccessClip, fPitch);  // Play success clip for FlorpTarget
    //         }
    //     }
    //     else if (thisObject.name.Contains("FlorpSpike"))
    //     {
    //         if (audioPlayer != null && spikeClip != null)
    //         {
    //             float fPitch = 1.0f + Random.Range(-randomizePitch, randomizePitch);
    //             audioPlayer.Play(spikeClip, fPitch);  // Play spike clip for FlorpSpike
    //         }
    //     }
    // }


    private void SendAudioSounds(GameObject targetOrSpike, GameObject player)
    {
        string soundID;
        Vector3 pointPosition = targetOrSpike.transform.position;  // Position of the FLORP (colliding object)

        if (targetOrSpike.name.Contains("FlorpTarget"))
        {
            soundID = $"p{player.GetInstanceID()}EffectsSharePoint";  // I'm using player's spat channel because I don't want to manage distributing spike locations for spat
            soundEventSender.SendOneShotSound(soundID, pointPosition);
        }
        else if (targetOrSpike.name.Contains("FlorpSpike"))
        {
            soundID = $"p{player.GetInstanceID()}EffectsShareSpikes";  
            soundEventSender.SendOneShotSound(soundID, pointPosition);
        }
    }

    // haven't tested this!
    private void ResetGrowth()
    {
        // Handle growth reset when max size is reached
        // winGameParticles.Play();
        audioPlayer.Play(growSuccessClip);
        currentHue = startHue;
    }


    // haven't tested this!
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
        // Store the transform of the collided object and reset velocity
        followTransform = collision.gameObject.transform;
        vel = Vector3.zero;
        collideTime = Time.time;

        // Adjust the scale of the object based on whether it's a target or spike
        float scaleChange = isTarget ? increaseCollider : -increaseCollider;

        collision.gameObject.transform.localScale += Vector3.one * scaleChange;

        // haven't tested this --> Disable the collider to prevent further collisions
        GetComponent<SphereCollider>().enabled = false;

        // haven't tested this --> If the object has grown beyond the maximum size, reset its growth
        if (transform.localScale.x >= maxSize)
        {
            ResetGrowth();
        }
    }

    public Vector3 randomDir(float collideTime)
    {
        // haven't tested this --> Generate a random direction based on sine and cosine functions
        return new Vector3(Mathf.Sin(collideTime), Mathf.Cos(collideTime), Mathf.Sin(collideTime * 2));
    }

    void Update()
    {
        // haven't tested this --> If following a transform, apply force and update positions
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
        // haven't tested this --> Initialize hue when the object is enabled
        currentHue = startHue;
    }
}
