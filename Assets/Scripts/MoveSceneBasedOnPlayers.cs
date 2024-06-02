using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSceneBasedOnPlayers : MonoBehaviour
{

    public Controller controller;

    [Tooltip("Horizontal speed")]
    public float xzSpeed;

    [Tooltip("Up/Down speed")]
    public float ySpeed;

    [Tooltip("Sets the line of when we start to move up or down, if the avg position is above the line we’ll move up")]

    public float verticalOffset;
    [Tooltip("How much the velocity slows down each frame. Lower=> more responsive, more vomit! Higher => more momentum, harder controls. Max 1. 0 is like high friction. Lower values need more force.")]
    public float dampening;
    [Tooltip("How fast it moves")]
    public float forceMultiplier;
    [Tooltip("How quickly people's movements affect the steering")]

    public Vector3 force;
    public Vector3 velocity;

    public Vector3 size;

    public void Reset()
    {
        velocity = Vector3.zero;
        transform.position = Vector3.zero;
    }




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

        // Sum the positions of all active players
        for (int i = 0; i < numPlayers; i++)
        {
            if (controller.players[i].activeSelf)
            {
                numActive++;
                averagePosition += controller.players[i].transform.position;
            }
        }

        // Calculate the average position if there are active players
        if (numActive > 0)
        {
            averagePosition /= numActive;
        }
        else
        {
            averagePosition = Vector3.zero;
        }

        // Find target position based on average position and offset
        Vector3 targetPosition = new Vector3(averagePosition.x, averagePosition.y + verticalOffset, averagePosition.z);

        // Apply speed factors to target position
        targetPosition.x *= xzSpeed;
        targetPosition.z *= xzSpeed;
        targetPosition.y *= ySpeed; // speed up and down

        // Calculate force and update velocity
        force = targetPosition * forceMultiplier;
        velocity += force * Time.deltaTime;
        velocity *= dampening;


        // Update position based on velocity
        transform.position += velocity * Time.deltaTime;

        Vector3 offset = Vector3.zero;

        // Let those beautiful people loop through the universe
        if (transform.position.x < -size.x)
        {
            offset += size.x * 2 * Vector3.right;
        }

        if (transform.position.x > size.x)
        {
            offset -= size.x * 2 * Vector3.right;
        }

        if (transform.position.y < -size.y)
        {
            offset += size.y * 2 * Vector3.up;
        }

        if (transform.position.y > size.y)
        {
            offset -= size.y * 2 * Vector3.up;
        }

        if (transform.position.z < -size.z)
        {
            offset += size.z * 2 * Vector3.forward;
        }

        if (transform.position.z > size.z)
        {
            offset -= size.z * 2 * Vector3.forward;
        }


        transform.position += offset;

    }
}
