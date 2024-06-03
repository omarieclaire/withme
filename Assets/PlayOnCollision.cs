using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer.Internal;
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

    public float maxSize;

    public float startSize;
    public float growSizePerHit;

    public ParticleSystem growSuccessParticles;
    public AudioClip growSuccessClip;

    public bool changeColorOnHit;
    public float hueShiftPerHit;
    public float startHue;

    public float currentHue;

    public bool followOnCollider;
    public Transform followTransform;
    public float collideTime;
    public LineRenderer followLine;
    public float increaseCollider;


    void OnCollisionEnter(Collision collision)
    {



        print(collision.gameObject.tag);
        if (collision.gameObject.tag == "FLORP")
        {

            if (ps != null)
            {
                ps.transform.position = collision.gameObject.transform.position;
                ps.Play();

                float fPitch = 1.0f;
                fPitch += Random.Range(-randomizePitch, randomizePitch);
                audioPlayer.Play(clip, fPitch);

            }


            if (audioPlayer == null && clip != null)
            {
                print("AUDS");
                audioPlayer.Play(clip);
            }

            if (die)
            {
                Destroy(gameObject);
            }

            if (grower)
            {
                transform.localScale += Vector3.one * growSizePerHit;
                if (transform.localScale.x >= maxSize)
                {
                    transform.localScale = Vector3.one * startSize;
                    growSuccessParticles.Play();
                    audioPlayer.Play(growSuccessClip);

                    currentHue = startHue;
                }
            }

            if (changeColorOnHit)
            {

                currentHue += hueShiftPerHit;
                currentHue = currentHue % 1;
                Color color = Color.HSVToRGB(currentHue, 1, 1);

                Material m = GetComponent<Renderer>().material;


                m.SetColor("_Color", color);
                m.SetColor("_BaseColor", color * .1f);
                m.SetColor("_ReflectionColor", color);
                m.SetColor("_CenterOrbColor", color * .1f);
                m.SetColor("_NoiseColor", color * 2);

            }

            if (followOnCollider)
            {
                followTransform = collision.gameObject.transform;
                vel = Vector3.zero;
                collideTime = Time.time;
                collision.gameObject.transform.localScale += Vector3.one * increaseCollider;
                GetComponent<SphereCollider>().enabled = false;
            }

        }



    }


    public float followForce;
    public float dampening;

    public Vector3 vel;

    public Vector3 randomDir(float collideTime)
    {
        return new Vector3(Mathf.Sin(collideTime), Mathf.Cos(collideTime), Mathf.Sin(collideTime * 2));

    }
    public void Update()
    {
        if (followTransform)
        {

            Vector3 force = Vector3.zero;

            force += (followTransform.position - transform.position + randomDir(collideTime)) * followForce;

            vel += force * Time.deltaTime;
            transform.position += vel * Time.deltaTime;

            vel *= dampening;

            followLine.SetPosition(0, transform.position);
            followLine.SetPosition(1, followTransform.position);

        }
    }

    public void OnEnable()
    {
        currentHue = startHue;
    }
}
