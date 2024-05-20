using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class oscMessages : MonoBehaviour
{
    Vector3 osc;
    public Vector3 myposition;

    void Start()
    {

    }

    void Update()
    {
        transform.position = osc;
    }

    public void ReceiveOSC(Vector3 incomingOSCValues)
    {
        Debug.Log($"Received OSC values: {incomingOSCValues.x}, {incomingOSCValues.y}");
        osc.x = MapToRange(incomingOSCValues.x, 0, 640, -14, 14);
        osc.z = MapToRange(incomingOSCValues.y, 0, 480, -14, 14); 
        osc.y = 0f; // ground plane

        // osc.y =  the camera perspective or height of the avatar
    }

    public float MapToRange(float numberInRange1, float start1, float end1, float start2, float end2)
    {
        float distance = numberInRange1 - start1;
        float distanceRatio = distance / (end1 - start1);
        float amountInRange2 = distanceRatio * (end2 - start2);
        float finalValue = start2 + amountInRange2;
        return finalValue;
    }
}
