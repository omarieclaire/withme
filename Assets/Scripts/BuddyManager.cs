using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlorpManager : MonoBehaviour
{


    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager

    public Controller controller;


    public List<GameObject> florps;

    public GameObject florpPrefab;
    public float florpSize;



    // Update is called once per frame
    void Update()
    {

        if (transform.childCount != controller.playerSetupManager.GetActivePlayerCount())
        {
            Reset();
        }
    }

    public void Reset()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        florps = new List<GameObject>();

        for (int i = 0; i < controller.playerSetupManager.GetActivePlayerCount(); i++)
        {
            GameObject florp = Instantiate(florpPrefab, Vector3.zero, Quaternion.identity);
            florp.transform.position = Vector3.zero;
            florp.transform.localScale = Vector3.one * florpSize;
            florp.transform.SetParent(transform);
            florp.name = $"Florp_{i}";




            florp.GetComponent<PullTowardsPerson>().playerID = i;
            florp.GetComponent<PullTowardsPerson>().controller = controller;
var activePlayers = controller.playerSetupManager.GetActivePlayers();
if (activePlayers != null && activePlayers.Count > i) // Ensure index is valid
{
    florp.transform.position = activePlayers[i].PlayerObject.transform.position;
}

            florp.SetActive(true);

            florps.Add(florp);

        }

    }

}