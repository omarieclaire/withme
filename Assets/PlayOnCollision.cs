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

    void OnCollisionEnter(Collision collision)
    {



        print(collision.gameObject.tag);
        if (collision.gameObject.tag == "FLORP")
        {

            if (ps != null)
            {
                ps.transform.position = collision.gameObject.transform.position;
                ps.Play();
                audioPlayer.Play(clip);

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

        }
    }
}
