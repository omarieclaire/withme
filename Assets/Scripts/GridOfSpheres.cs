using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOfSpheres : MonoBehaviour
{

    public GameObject spherePrefab;
    public int num;
    public float size;
    public float scale;


    // Start is called before the first frame update
    void OnEnable()
    {

        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < num; j++)
            {
                for (int k = 0; k < num; k++)
                {
                    GameObject sphere = Instantiate(spherePrefab);

                    sphere.transform.position = new Vector3(i * size, j * size, k * size) - Vector3.one * size * num / 2 + Vector3.up * size * num / 3;
                    sphere.transform.localScale = Vector3.one * scale;
                    sphere.transform.parent = transform;

                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
