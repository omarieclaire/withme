using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugFace : MonoBehaviour
{

    public Hug hug;

    public GameObject partner;

    public int smileID;
    public int state;

    public GameObject preDiscovered;
    public GameObject discovered;
    public GameObject completed;
    public MeshRenderer highlighted;



    public void OnEnable()
    {
        state = 0;
        preDiscovered.SetActive(true);
        discovered.SetActive(false);
        completed.SetActive(false);
    }

    public void OnDisable()
    {
        preDiscovered.SetActive(false);
        discovered.SetActive(false);
        completed.SetActive(false);
    }

    public void OnDiscover()
    {
        state = 1;
        preDiscovered.SetActive(false);
        discovered.SetActive(true);
        completed.SetActive(false);
    }

    public void OnComplete()
    {
        state = 2;
        preDiscovered.SetActive(false);
        discovered.SetActive(false);
        completed.SetActive(true);
    }

    public void WhileInside()
    {
        highlighted.enabled = true;

        if (state == 0)
        {
            OnDiscover();
        }

        if (state == 1)
        {
            OnComplete();
        }
    }

    public void Update()
    {
        highlighted.enabled = false;
    }


}
