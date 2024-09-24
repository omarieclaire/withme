// using System.Collections;
// using UnityEngine;

// public class SpikeBehavior : MonoBehaviour
// {
//     public float shiveringSpeed = 10f;
//     public float shiveringAmplitude = 0.1f;
//     public float rotationSpeed = 10f;

//     private Vector3 initialScale;

//     void Start()
//     {
//         initialScale = transform.localScale;
//         StartCoroutine(ShiveringMovement());
//     }

//     // Apply shivering-like movement and slow rotation
//     IEnumerator ShiveringMovement()
//     {
//         while (true)
//         {
//             float scale = 1 + Mathf.PerlinNoise(Time.time * shiveringSpeed, 0) * shiveringAmplitude;
//             transform.localScale = initialScale * scale;

//             transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
//             yield return null;
//         }
//     }
// }
