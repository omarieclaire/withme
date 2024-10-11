// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class getFinalPositionObjects : MonoBehaviour
// {
//     [Tooltip("The minimum allowed Y position (floor level) to prevent objects from going underground.")]
//     public float floorY = 0f; // Set this in the inspector or initialize it here.

//     public Vector3 getFinalPositionObjects(Vector3 position)
//     {
//         Vector3 finalPosition = position;

//         // Normalize the x and z components based on the max size
//         float nX = finalPosition.x / maxSize.x;
//         float nZ = finalPosition.z / maxSize.z;

//         Vector2 nXZ = new Vector2(nX, nZ);
//         float l = nXZ.magnitude;
//         l = Mathf.Pow(l, pushTowardsBottom);
//         float angleAround = Mathf.Atan2(nZ, nX);
//         float angleDown = l * (maxDegrees / 360) * 2 * Mathf.PI;

//         // Convert spherical coordinates to cartesian
//         Vector3 fPosition = SphericalToCartesian(sphereSize, angleAround, Mathf.PI - angleDown);

//         // Transform the position to the object's local space
//         fPosition = transform.TransformPoint(fPosition);

//         // Ensure the Y position does not go below the defined floor level
//         fPosition.y = Mathf.Max(fPosition.y, floorY);

//         return fPosition;
//     }
// }

