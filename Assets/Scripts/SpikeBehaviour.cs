// using UnityEngine;

// public class SpikeBehavior : MonoBehaviour
// {
//     public float shiveringSpeed = 10f;        // Speed of the shivering effect
//     public float shiveringAmplitude = 0.1f;   // Amplitude of the shivering effect
//     public float rotationSpeed = 10f;         // Speed of rotation

//     private Vector3 initialScale;

//     void Start()
//     {
//         initialScale = transform.localScale;
//     }

//     void Update()
//     {
//         // Apply a shivering-like movement using Sin function for a smooth jitter effect
//         float scale = 1 + Mathf.Sin(Time.time * shiveringSpeed) * shiveringAmplitude;
//         transform.localScale = initialScale * scale;

//         // Rotate the object around its own Y-axis
//         transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
//     }
// }
