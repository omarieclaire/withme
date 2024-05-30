using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickTogether : MonoBehaviour
{

    public Controller controller;
    public float radiusForCollection = 1;

    public GameObject collectionRepresentation;
    public List<LineRenderer> connections;
    public GameObject connectionPrefab;

    public int totalInsideCircle = 0;


    public Vector3 movementSpeed;
    public Vector3 movementSize;

    public Vector3 movementOffset;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        transform.localScale = new Vector3(radiusForCollection * 2, radiusForCollection * 2, radiusForCollection * 2);


        Vector3 position = new Vector3(
            Mathf.Sin(Time.time * movementSpeed.x) * movementSize.x,
            Mathf.Sin(Time.time * movementSpeed.y) * movementSize.y,
            Mathf.Sin(Time.time * movementSpeed.z) * movementSize.z
        );

        position += movementOffset;

        transform.position = controller.getFinalPosition(position);


        if (connections.Count != controller.activePlayers.Count)
        {

            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            connections.Clear();
            for (int i = 0; i < controller.activePlayers.Count; i++)
            {
                GameObject newConnection = Instantiate(connectionPrefab, transform.position, Quaternion.identity);
                newConnection.transform.parent = transform;
                connections.Add(newConnection.GetComponent<LineRenderer>());
            }
        }


        totalInsideCircle = 0;
        for (int i = 0; i < controller.activePlayers.Count; i++)
        {
            Vector3 dir = controller.activePlayers[i].transform.position - transform.position;
            float dist = dir.magnitude;

            if (dist < radiusForCollection)
            {
                totalInsideCircle++;
                connections[i].positionCount = 2;
                connections[i].SetPosition(0, transform.position);
                connections[i].SetPosition(1, controller.activePlayers[i].transform.position);
            }
            else
            {
                connections[i].positionCount = 0;
            }




        }


    }


}
