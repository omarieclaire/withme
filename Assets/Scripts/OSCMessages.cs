using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// monobehaviour means that it will expose public variables and methods
public class OSCMessages : MonoBehaviour
{
    Vector3 osc;
    public Vector3 myposition;

    void Start()
    {

    }

    void Update()
    {
        transform.position = osc;
        // Debug.Log(osc);
    }

    public void ReceiveOSC(Vector3 incomingOSCValues){
        osc.x = MapToRange(incomingOSCValues.x, 0, 640, -4, 4);
        osc.y = MapToRange(incomingOSCValues.y, 0, 480, 8, -4);
        osc.z = 0f;
    }

    public float MapToRange(float numberInRange1, float start1, float end1, float start2, float end2) {
        float distance = numberInRange1 - start1;
        float distanceRatio = distance / (end1 - start1);
        float amountInRange2 = distanceRatio * (end2 - start2);
        float finalValue = start2 + amountInRange2;
        return finalValue;
    }
}
