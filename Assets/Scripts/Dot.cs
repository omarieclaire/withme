using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    public DotGameController controller;
    public int id;
    public bool collected;
    public Transform collector;
    public Vector3 originalPosition;
    public Vector3 velocity;
    public LineRenderer lr;
    public Vector2 randomDirection;
    public Vector3 targetPosition;

    // how much we spread out the collected dots
    public float spreadRadius = 0.01f; // Radius for spreading dots at the tree


    private Transform _transform;
    public bool towardsTree;
    public bool atTree; // New variable to track if the dot is at the tree

    void Start()
    {
        _transform = transform;
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Tree" && !atTree)
        {
            controller.OnTreeCollect();
            // Set the dot's position to the tree's position
            transform.position = collider.transform.position;
            OnTreeFed();
        }
    }

    public void SetData()
    {
        randomDirection = Random.insideUnitCircle.normalized;
        Color c = Color.HSVToRGB((Mathf.Sin(id) + 1) / 2, 1, 1);
        GetComponent<Renderer>().material.color = c;
        lr.startColor = c;
        lr.endColor = c;
    }

    public void Update()
    {
        if (atTree) return; // If at the tree, don't update position

        float fForce = collected ? controller.dotForceTowardsCollector : controller.dotForceTowardsPosition;

        if (collected)
        {
            float outMultiplier = towardsTree ? 0 : 0.6f;
            if (towardsTree)
            {
                fForce = controller.dotForceTowardsTree;
            }

            Vector3 newTargetPosition = collector.position
                                        + collector.localScale.x * outMultiplier * randomDirection.x * collector.right
                                        + collector.localScale.y * outMultiplier * randomDirection.y * collector.up;

            if (targetPosition != newTargetPosition)
            {
                targetPosition = newTargetPosition;
                lr.positionCount = 2;
                lr.SetPosition(0, _transform.position);
                lr.SetPosition(1, collector.position);
            }
        }
        else
        {
            if (targetPosition != originalPosition)
            {
                targetPosition = originalPosition;
                lr.positionCount = 0;
            }
        }

        Vector3 force = targetPosition - _transform.position;
        velocity += force * Time.deltaTime * fForce;

        velocity = Vector3.ClampMagnitude(velocity, 10f);
        _transform.position += velocity * Time.deltaTime;
        velocity *= controller.dotDampening;

        _transform.LookAt(controller.center);
        _transform.Rotate(0, 180, 0);
    }

    public void OnPlayersHoldingMeCollided()
    {
        collected = true;
        collector = controller.tree;
        towardsTree = true;
    }
    public void OnTreeFed()
    {
        atTree = true;
        collected = false;
        collector = null;
        towardsTree = false;
        velocity = Vector3.zero;

        // Increase the spread radius for a larger spread, but still keep it within control
        Vector2 randomOffset = Random.insideUnitCircle * spreadRadius;  // Adjust spreadRadius if needed

        // Introduce a small random value for the Y axis to give height variation
        float yOffset = Random.Range(0.1f, 0.5f);  // Adjust the range as needed for vertical spread

        Vector3 spreadPosition = controller.tree.position + new Vector3(randomOffset.x, yOffset, randomOffset.y);

        // Allow a bit more room for spread while keeping it constrained
        float maxSpreadDistance = 0.5f;  // Increase this for a bit more spread
        Vector3 offsetFromTree = spreadPosition - controller.tree.position;

        // Clamp the spread to ensure it doesn't go too far, but still gives some room for variation
        if (offsetFromTree.magnitude > maxSpreadDistance)
        {
            spreadPosition = controller.tree.position + offsetFromTree.normalized * maxSpreadDistance;
        }

        // Apply the spread position
        transform.position = spreadPosition;

        if (lr != null)
        {
            lr.positionCount = 0;
        }
    }


}