using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSceneBasedOnPlayers : MonoBehaviour
{

    public Controller controller;

    public float xzSpeed;
    public float ySpeed;

    public float verticalOffset;
    public float dampening;
    public float forceMultiplier;

    public Vector3 force;
    public Vector3 velocity;




    // Start is called before the first frame update
    void OnEnable()
    {

        // Reset position
        transform.position = Vector3.zero;

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 averagePosition = Vector3.zero;
        int numPlayers = controller.players.Count;

        int numActive = 0;
        for (int i = 0; i < numPlayers; i++)
        {
            if (controller.players[i].activeSelf)
            {
                numActive++;
                averagePosition += controller.players[i].transform.position;
            }
        }

        if (numActive > 0)
        {
            averagePosition /= numActive;
        }
        else
        {
            averagePosition = Vector3.zero;
        }

        Vector3 targetPosition = new Vector3(averagePosition.x, averagePosition.y + verticalOffset, averagePosition.z);


        targetPosition.x *= xzSpeed;
        targetPosition.z *= xzSpeed;
        targetPosition.y *= ySpeed; // speed up and down


        force = targetPosition * forceMultiplier;

        velocity += force * Time.deltaTime;
        velocity *= dampening;



        transform.position += velocity * Time.deltaTime;




    }
}
