using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{

    public MainController controller;

    public int id;

    public bool collected;
    public Transform collector;

    public Vector3 originalPosition;
    public Vector3 velocity;




    public void OnCollisionEnter(Collision collision)
    {
        // print("dot collided");
        // controller.OnDotCollision(this.gameObject, collision.collider.gameObject);

    }

    public void OnTriggerEnter(Collider collider)
    {
        //print("dot triggered");
        //controller.OnDotCollision(this.gameObject, collider.gameObject);
    }

    public void SetData()
    {
        GetComponent<Renderer>().material.color = Color.HSVToRGB((Mathf.Sin(id) + 1) / 2, 1, 1);
    }

    public Vector3 targetPosition;
    public void Update()
    {


        if (collected)
        {
            targetPosition = collector.position;
        }
        else
        {
            targetPosition = originalPosition;
        }

        Vector3 force = targetPosition - transform.position;

        velocity += force * Time.deltaTime * controller.dotForceTowardsPosition;

        transform.position += velocity * Time.deltaTime;
        velocity *= controller.dotDampening;

    }

}
