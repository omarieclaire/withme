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
    private Transform _transform;

    void Start()
    {
        _transform = transform;
    }

    public void OnCollisionEnter(Collision collision)
    {
        // Handle collision
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Tree")
        {
            controller.OnTreeCollect();
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

    public bool towardsTree;

    public void OnPlayersHoldingMeCollided()
    {
        collected = true;
        collector = controller.tree;
        towardsTree = true;
    }

    public void OnTreeFed()
    {
        collected = false;
        collector = null;
        towardsTree = false;
    }
}
