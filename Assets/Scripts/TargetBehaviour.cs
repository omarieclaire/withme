// using UnityEngine;

// public class TargetBehavior : MonoBehaviour
// {
//     public float breathingSpeed;  // Speed of breathing effect
//     public float breathingAmplitude;  // Amplitude of breathing effect
//     public float rotationSpeed;  // Speed of rotation

//     private Vector3 initialScale;
//     private Vector3 initialPosition;

//     void Start()
//     {
//         initialScale = transform.localScale;
//         initialPosition = transform.position;

//         // Randomize breathing speed, amplitude, and rotation speed for unique variation
//         breathingSpeed = Random.Range(0.05f, 0.15f);  // Random speed between 0.05 and 0.15
//         breathingAmplitude = Random.Range(0.03f, 0.07f);  // Random amplitude between 0.03 and 0.07
//         rotationSpeed = Random.Range(0.5f, 2f);  // Random rotation speed between 0.5 and 2
//     }

//     void Update()
//     {
//         // Subtle breathing effect (scaling up and down)
//         float scale = 1 + Mathf.Sin(Time.time * breathingSpeed) * breathingAmplitude;
//         transform.localScale = initialScale * scale;

//         // Subtle floating effect (small vertical movement)
//         float yOffset = Mathf.Sin(Time.time * breathingSpeed) * breathingAmplitude;
//         transform.position = initialPosition + new Vector3(0, yOffset, 0);

//         // Rotate around the target's own local Y-axis
//         transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
//     }
// }
