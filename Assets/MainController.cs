using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{

    public float verticalValue;
    public float sphereSize;

    public Vector3 maxSize;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 getFinalPosition(Vector3 position)
    {
        Vector3 finalPosition = position;



        float nX = finalPosition.x / maxSize.x;
        float nZ = finalPosition.z / maxSize.z;

        Vector2 nXZ = new Vector2(nX, nZ);
        float l = nXZ.magnitude;    

        float fVerticalValue = verticalValue * (1 - Mathf.Clamp(l, 0, 1));

        finalPosition.y = fVerticalValue; ;
        finalPosition = finalPosition.normalized * sphereSize;

        finalPosition = transform.TransformPoint(finalPosition);

        return finalPosition;
    }
}
