using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    public DotGameController controller;
    public int dotId;
    public bool collectedDot;
    public Transform dotCollector;
    public Vector3 originalDotPosition;
    public Vector3 dotVelocity;
    public LineRenderer lineRenderer;
    public Vector2 randomDirection;
    public Vector3 targetPosition;

    // how much we spread out the collected dots
    public float collectedDotsSpreadRadius = 0.05f; // Radius for spreading dots around the tree when collected

    private Transform _transform; // Cached reference to this dot's transform for efficiency
    public bool dotMovesTowardsTree; // Flag indicating if the dot is moving toward the tree (when collected)
    public bool dotIsAtTree; // New variable to track if the dot has reached the tree



    void Start()
    {
        _transform = transform;
    }

    public void OnCollisionEnter(Collision collision)
    {
        // Handle collision
    }

    // Trigger event when the dot collides with the tree (goal object)
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Tree" && !dotIsAtTree)
        {
            // GetComponent<Renderer>().material.color = Color.white; // Change the dot's color to red
            transform.localScale *= 0.55f; // Reduce the size of the dot to 25% of its original size

            controller.OnTreeCollect(); // Notify the controller that a dot has reached the tree
            transform.position = collider.transform.position; // Set dot's position to the tree's position
            OnTreeFed(); // Mark the dot as being fed to the tree
        }
    }

    public void SetData()
    {
        randomDirection = Random.insideUnitCircle.normalized;
        Color c = Color.HSVToRGB((Mathf.Sin(dotId) + 1) / 2, 1, 1);
        GetComponent<Renderer>().material.color = c;
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }

    // Update is called once per frame, handles the dot's movement
    public void Update()
    {
        if (dotIsAtTree) return; // If the dot has reached the tree, do not update its position anymore

        float fForce = collectedDot ? controller.dotForceTowardsCollector : controller.dotForceTowardsPosition; // Determine which force to apply based on whether the dot is collected

        if (collectedDot)
        {
            float outMultiplier = dotMovesTowardsTree ? 0 : 0.6f; // Multiplier for the spread radius around the player
            if (dotMovesTowardsTree)
            {
                fForce = controller.dotForceTowardsTree; // Apply a stronger force if the dot is moving toward the tree
            }

            // Calculate the new target position of the dot relative to the player
            Vector3 newTargetPosition = dotCollector.position
                                        + dotCollector.localScale.x * outMultiplier * randomDirection.x * dotCollector.right
                                        + dotCollector.localScale.y * outMultiplier * randomDirection.y * dotCollector.up;

            if (targetPosition != newTargetPosition)
            {
                targetPosition = newTargetPosition; // Update the target position if it has changed
                lineRenderer.positionCount = 2; // Update LineRenderer to draw between the dot and the collector
                lineRenderer.SetPosition(0, _transform.position); // Set the start point of the LineRenderer
                lineRenderer.SetPosition(1, dotCollector.position); // Set the end point of the LineRenderer
            }
        }
        else
        {
            if (targetPosition != originalDotPosition)
            {
                targetPosition = originalDotPosition; // If not collected, the dot should move back to its original position
                lineRenderer.positionCount = 0; // Disable the LineRenderer
            }
        }

        // Calculate the force to move the dot toward the target position
        Vector3 force = targetPosition - _transform.position;
        dotVelocity += force * Time.deltaTime * fForce; // Apply the force to the dot's velocity

        dotVelocity = Vector3.ClampMagnitude(dotVelocity, 10f); // Clamp the velocity to prevent it from going too fast
        _transform.position += dotVelocity * Time.deltaTime; // Move the dot according to its velocity
        dotVelocity *= controller.dotDampening; // Apply dampening to slow down the dot's velocity

        _transform.LookAt(controller.center); // Make the dot look toward the center of the dome
        _transform.Rotate(0, 180, 0); // Rotate the dot 180 degrees to face outward
    }


    public bool towardsTree;

    public void OnPlayersHoldingMeCollided()
    {
        collectedDot = true;
        dotCollector = controller.tree;
        towardsTree = true;
    }


    // Mark the dot as fed to the tree, apply a spread effect

    public void OnTreeFed()
    {
        dotIsAtTree = true;
        collectedDot = false;
        dotCollector = null;
        dotMovesTowardsTree = false;
        dotVelocity = Vector3.zero;

        // Fixed Y position for visibility
        float fixedYOffset = -1.0f; // Fixed height above the ground

        // Spiral calculation: increasing angle and radius
        float angle = dotId * Mathf.PI / 6; // Increase angle for smoother spiral
        float radius = 0.002f * dotId; // Increase radius slightly for visible spacing between dots

        // Calculate X and Z using the angle and increasing radius
        float xOffset = radius * Mathf.Cos(angle);
        float zOffset = radius * Mathf.Sin(angle);

        // Set the new position with the spiral X, Z, and fixed Y
        Vector3 spreadPosition = controller.tree.position + new Vector3(xOffset, fixedYOffset, zOffset);

        transform.position = spreadPosition; // Set the dot's position

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0; // Disable the LineRenderer
        }
    }



}