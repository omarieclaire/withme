using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{

    public Flock flock;

    public LineRenderer attractLR;
    public LineRenderer repelLR;

    public void OnTriggerEnter(Collider other)
    {
        // flock.OnFishEnter(this, other);
    }


}
