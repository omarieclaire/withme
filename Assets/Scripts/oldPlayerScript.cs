// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class PlayerAvatar : MonoBehaviour
// {
//     public Controller controller; // Reference to the game controller

//     [Tooltip("TextMeshPro component for displaying player name.")]
//     public TextMeshPro text;

//     [Tooltip("Unique identifier for the player.")]
//     public int id;

//     [Tooltip("Number of dots collected by the player.")]
//     public int numDotsCollected;

//     [Tooltip("Renderer for the regular ring.")]
//     public Renderer regularRing;

//     [Tooltip("Renderer for the charged ring.")]
//     public Renderer chargedRing;

//     [Tooltip("Renderer for the maximum ring.")]
//     public Renderer maxRing;

//     [Tooltip("Color assigned to the player.")]
//     public Color color;

//     [Tooltip("Checkbox to toggle player name display.")]
//     public bool showPlayerName;

//     // Sets the player's data including name and color
//     public void SetData(string name)
//     {
//         if (showPlayerName)
//         {
//             text.text = name; // Set player name
//         }
//         else
//         {
//             text.text = ""; // Leave player name blank
//         }

//         color = Color.HSVToRGB((Mathf.Sin(id) + 1) / 2, 1, 1); // Generate unique color based on ID

//         // Set the color for all rings and text
//         regularRing.material.color = color;
//         chargedRing.material.color = color;
//         maxRing.material.color = color;
//         text.color = color;
//     }

//     // Handles dot collection and updates rings
//     public void OnDotCollect(bool chargeRingOn, bool maxRingOn)
//     {
//         if (chargeRingOn)
//         {
//             chargedRing.enabled = true; // Enable charged ring if condition met
//         }

//         if (maxRingOn)
//         {
//             maxRing.enabled = true; // Enable max ring if condition met
//         }

//         numDotsCollected++; // Increment the number of collected dots
//     }

//     // Resets the player's state
//     public void Reset()
//     {
//         numDotsCollected = 0; // Reset collected dots count
//         maxRing.enabled = false; // Disable max ring
//         chargedRing.enabled = false; // Disable charged ring
//         regularRing.enabled = true; // Enable regular ring
//     }

//     // Trigger event handler
//     public void OnTriggerEnter(Collider collider)
//     {
//         controller.OnPlayerTrigger(this, collider.gameObject); // Notify controller of the trigger event
//     }

//     // Updates player state each frame
//     public void Update()
//     {
//         transform.LookAt(controller.center); // Make the player face the center

//         // Check for collisions with other players
//         for (int i = 0; i < controller.players.Count; i++)
//         {
//             if (controller.players[i] != this.gameObject)
//             {
//                 float distance = Vector3.Distance(controller.players[i].transform.position, transform.position);

//                 // Adjust distance for player scales
//                 distance -= transform.localScale.x / 2;
//                 distance -= controller.players[i].transform.localScale.x / 2;

//                 if (distance < 0)
//                 {
//                     // Notify controller of player collision if close enough
//                     controller.OnPlayersWithDotsCollided(this, controller.playerAvatars[i]);
//                 }
//             }
//         }
//     }
// }
