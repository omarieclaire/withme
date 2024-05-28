using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaliedescope : MonoBehaviour
{

    public int numberOfIterations = 10;

    public GameObject nodePrefab;
    public Controller controller;


    public List<GameObject> nodes;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (nodes.Count != numberOfIterations * controller.players.Count)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Destroy(nodes[i]);
            }
            nodes.Clear();

            for (int i = 0; i < numberOfIterations * controller.players.Count; i++)
            {
                GameObject node = Instantiate(nodePrefab, transform);
                node.SetActive(true);
                nodes.Add(node);
            }

        }




        for (int i = 0; i < controller.players.Count; i++)
        {
            for (int j = 0; j < numberOfIterations; j++)
            {

                float rotation = 360.0f * (float)j / (float)numberOfIterations;
                int index = i * numberOfIterations + j;
                nodes[index].transform.position = controller.players[i].transform.position;
                nodes[index].transform.localScale = controller.GetScale(i);
                nodes[index].transform.RotateAround(Vector3.zero, Vector3.up, rotation);
            }
        }







    }
}
