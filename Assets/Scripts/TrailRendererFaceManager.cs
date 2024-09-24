// using UnityEngine;

// public class TrailRendererFaceManager : MonoBehaviour
// {
//     public TrailRenderer trailRenderer;
//     public float faceSwitchInterval = 0.1f; // Time in seconds for each face switch
//     public int activeFaceIndex = 0; // The cubemap face index where trails should be active

//     private float timer = 0.0f;
//     private int currentFace = 0;

//     void Start()
//     {
//         if (trailRenderer == null)
//         {
//             trailRenderer = GetComponent<TrailRenderer>();

//             if (trailRenderer == null)
//             {
//                 Debug.LogError("TrailRenderer component is missing from " + gameObject.name);
//                 return;
//             }
//         }

//         // Start with the trail renderer disabled
//         trailRenderer.emitting = false;
//         Debug.Log(gameObject.name + " TrailRenderer started with emitting = false");
//     }

//     void Update()
//     {
//         // Update the timer to simulate cubemap face switching
//         timer += Time.deltaTime;

//         if (timer >= faceSwitchInterval)
//         {
//             // Reset the timer and switch to the next cubemap face
//             timer = 0.0f;
//             currentFace = (currentFace + 1) % 6; // There are 6 cubemap faces (PositiveX, NegativeX, PositiveY, etc.)

//             Debug.Log(gameObject.name + " switched to cubemap face " + currentFace);

//             // Only enable the trail renderer for the selected face
//             if (currentFace == activeFaceIndex)
//             {
//                 if (!trailRenderer.emitting)
//                 {
//                     trailRenderer.emitting = true;
//                     Debug.Log(gameObject.name + " TrailRenderer emitting enabled on face " + activeFaceIndex);
//                 }
//             }
//             else
//             {
//                 if (trailRenderer.emitting)
//                 {
//                     trailRenderer.emitting = false;
//                     Debug.Log(gameObject.name + " TrailRenderer emitting disabled on face " + currentFace);
//                 }
//             }
//         }
//     }
// }
