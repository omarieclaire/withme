using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugFace : MonoBehaviour
{

    public Hug hug;

    public HugFace partner;

    public int smileID;
    public int state;

    public GameObject preDiscovered;
    public GameObject discovered;
    public GameObject finished;



    public void OnEnable()
    {
        state = 0;
        preDiscovered.SetActive(true);
        discovered.SetActive(false);
    }

    public void OnDisable()
    {
        preDiscovered.SetActive(false);
        discovered.SetActive(false);
    }

    public bool inside;

    public void WhileInside()
    {
        discovered.SetActive(true);
        preDiscovered.SetActive(false);
        finished.SetActive(false);
        inside = true;

        if (partner.inside == true)
        {
            hug.PairDiscover(this.gameObject, partner.gameObject, smileID);
        }


    }

    public void WhileOutside()
    {
        discovered.SetActive(false);
        preDiscovered.SetActive(true);
        finished.SetActive(false);
        inside = false;
    }

    public void WhileFinished()
    {
        discovered.SetActive(false);
        preDiscovered.SetActive(false);
        finished.SetActive(true);
    }



}
