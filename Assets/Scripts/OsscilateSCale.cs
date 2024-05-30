using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OsscilateSCale : MonoBehaviour
{

    public float minScale;
    public float maxScale;

    public float speed;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.one * (Mathf.Sin(Time.time * speed) * 0.5f + 0.5f) * (maxScale - minScale) + Vector3.one * minScale;
    }
}
