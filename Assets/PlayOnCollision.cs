using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnCollision : MonoBehaviour
{

    public ParticleSystem ps;

    public AudioPlayer audioPlayer;

    public AudioClip clip;

    public bool die;

    void OnCollisionEnter(Collision collision)
    {

        print("HMMM");
        print(collision.gameObject.tag);
        if (collision.gameObject.tag == "FLORP")
        {

            print("HI");
            if (ps != null)
            {

                print("PARITSL:");
                ps.transform.position = collision.gameObject.transform.position;
                ps.Play();
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
