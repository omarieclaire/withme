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



    public void OnCollisionEnter(Collision collision)
    {
        // print("dot collided");
        // controller.OnDotCollision(this.gameObject, collision.collider.gameObject);

    }

    public void OnTriggerEnter(Collider collider)
    {

        print("dot triggered");
        //print("dot triggered");
        //controller.OnDotCollision(this.gameObject, collider.gameObject);
    }

    public void SetData()
    {
        randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        randomDirection = randomDirection.normalized;
        Color c = Color.HSVToRGB((Mathf.Sin(id) + 1) / 2, 1, 1);
        GetComponent<Renderer>().material.color = c;
        lr.startColor = c;
        lr.endColor = c;
    }

    public Vector3 targetPosition;
    public void Update()
    {


        if (collected)
        {

            targetPosition = collector.position;
            targetPosition += collector.localScale.x * .6f * randomDirection.x * collector.right;
            targetPosition += collector.localScale.y * .6f * randomDirection.y * collector.up;

            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, collector.position);

        }
        else
        {
            lr.positionCount = 0;
            targetPosition = originalPosition;
        }

        Vector3 force = targetPosition - transform.position;

        velocity += force * Time.deltaTime * controller.dotForceTowardsPosition;

        transform.position += velocity * Time.deltaTime;
        velocity *= controller.dotDampening;

        transform.LookAt(controller.center);
        transform.Rotate(0, 180, 0);


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
