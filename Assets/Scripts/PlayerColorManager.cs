// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerColorManager : MonoBehaviour
// {
//     public LineRenderer lineRendererPrefab; // Assign in the Unity Inspector
//     private Dictionary<int, Color> playerColors = new Dictionary<int, Color>();

//     // This method is called when a new player appears or when an action happens for the player
//     public void SetLineRendererColor(int playerId, LineRenderer lineRenderer)
//     {
//         Color playerColor;
        
//         // Check if the player already has an assigned color
//         if (!playerColors.TryGetValue(playerId, out playerColor))
//         {
//             // Assign a new random color for the new player and store it
//             playerColor = new Color(Random.value, Random.value, Random.value);
//             playerColors[playerId] = playerColor;
//         }

//         // Set the LineRenderer's color to the player's color
//         lineRenderer.startColor = playerColor;
//         lineRenderer.endColor = playerColor;
//     }

//     // Example to create a new LineRenderer for a player
//     public void CreatePlayerLineRenderer(int playerId)
//     {
//         LineRenderer newLineRenderer = Instantiate(lineRendererPrefab);
//         SetLineRendererColor(playerId, newLineRenderer);
//     }
// }
