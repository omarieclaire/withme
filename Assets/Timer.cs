using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{

    public LineRenderer lr;

    public void SetHand(float v)
    {

        float angle = v * Mathf.PI * 2;
        Vector3 pos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 0.5f;
        lr.SetPosition(0, new Vector3(0, 0, 1) * .5f);
        lr.SetPosition(1, Vector3.zero);
        lr.SetPosition(2, pos);
    }
}
