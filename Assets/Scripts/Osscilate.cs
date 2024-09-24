using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Osscilate : MonoBehaviour
{

    public Vector3 size;
    public Vector3 speed;

    public Vector3 center;
    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;

    }

    // Update is called once per frame
    void Update()
    {

        transform.position = center + new Vector3(Mathf.Sin(Time.time * speed.x) * size.x, Mathf.Sin(Time.time * speed.y) * size.y, Mathf.Sin(Time.time * speed.z) * size.z);

    }
}
