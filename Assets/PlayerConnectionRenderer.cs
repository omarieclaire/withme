using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

public class PlayerConnectionRenderer : MonoBehaviour
{
    public Material drawMaterial;

    public float distanceForConnection;


    public MainController controller;

    public int oNumPlayers;
    public int numPlayers;
    // Start is called before the first frame update
    void Start()
    {

    }


    public MaterialPropertyBlock mpb;
    // Update is called once per frame
    void Update()
    {



        numPlayers = controller.players.Count;


        if (numPlayers != oNumPlayers)
        {
            oNumPlayers = numPlayers;
            ResetPlayerBuffer();
        }

        if (numPlayers == 0)
        {
            return;
        }


        for (int i = 0; i < numPlayers; i++)
        {
            playerPositions[i] = controller.players[i].transform.position;
        }

        playerBuffer.SetData(playerPositions);


        if (controller.players.Count > 1 && playerBuffer != null)
        {

            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }


            mpb.SetBuffer("_VertBuffer", playerBuffer);
            mpb.SetInt("_Count", numPlayers);
            mpb.SetFloat("_ConnectionDistance", distanceForConnection);

            Graphics.DrawProcedural(drawMaterial, new Bounds(transform.position, Vector3.one * 5000), MeshTopology.Triangles, numPlayers * numPlayers * 3 * 2, 1, null, mpb, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Default"));


        }


    }

    public Vector3[] playerPositions;
    public ComputeBuffer playerBuffer;
    public void ResetPlayerBuffer()
    {


        if (playerBuffer != null)
        {
            playerBuffer.Release();
        }

        if (numPlayers == 0)
        {
            return;
        }

        playerBuffer = new ComputeBuffer(numPlayers, sizeof(float) * 3);
        playerPositions = new Vector3[numPlayers];

    }

    public void OnDestroy()
    {
        if (playerBuffer != null)
        {
            playerBuffer.Release();
        }
    }
}
