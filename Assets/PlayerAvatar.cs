using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class PlayerAvatar : MonoBehaviour
{

    public MainController controller;

    public TextMeshPro text;
    public int id;

    public int numDotsCollected;




    public void SetData(string name)
    {
        text.text = name;
        GetComponent<Renderer>().material.color = Color.HSVToRGB((Mathf.Sin(id) + 1) / 2, 1, 1);
        text.color = Color.HSVToRGB((Mathf.Sin(id) + 1) / 2, 1, 1);

    }




    public void OnTriggerEnter(Collider collider)
    {
        controller.OnPlayerTrigger(this.gameObject, collider.gameObject);
    }

    public void Update()
    {

        //        print(controller.center);
        transform.LookAt(controller.center);
        for (int i = 0; i < controller.players.Count; i++)
        {
            if (controller.players[i] != this.gameObject)
            {
                float distance = Vector3.Distance(controller.players[i].transform.position, transform.position);

                distance -= transform.localScale.x / 2;
                distance -= controller.players[i].transform.localScale.x / 2;

                if (distance < 0)
                {
                    print("PLAYER COLLISION");

                    print("Other Player dots: " + controller.playerAvatars[i].numDotsCollected);

                    // check to see if both me and other player have collected dots

                    if (numDotsCollected > 0 || controller.playerAvatars[i].numDotsCollected > 0)
                    {
                        print("COLLISION");
                        // if so, destroy both players
                        controller.OnPlayersWithDotsCollided(this, controller.playerAvatars[i]);
                    }


                }
            }
        }

    }
}
