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


        }



    }

    public void OnEnable()
    {
        currentHue = startHue;
    }
}
