using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerConnectionRenderer : MonoBehaviour
{
    public PlayerSetupManager playerSetupManager;
    public Material drawMaterial;
    public float distanceForConnection;
    public DotGameController controller;

    public int oNumPlayers;
    public int numPlayers;

    public MaterialPropertyBlock mpb;

    public Vector3[] playerPositions;
    public Vector4[] playerData;
    public ComputeBuffer playerBuffer;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update()
    {
        // Use the helper function to get the count of active players
        numPlayers = controller.playerSetupManager.GetActivePlayerCount();

        if (numPlayers != oNumPlayers)
        {
            oNumPlayers = numPlayers;
            ResetPlayerBuffer();
        }

        if (numPlayers == 0)
        {
            return;
        }

        // Use GetActivePlayers() to fetch active player data
        var activePlayers = controller.playerSetupManager.GetActivePlayers();
        for (int i = 0; i < numPlayers; i++)
        {
            var playerInfo = activePlayers[i];
            GameObject playerObject = playerInfo.PlayerObject;

            // Use GetPlayerScale(playerID) for the scaler
            float playerScale = controller.playerSetupManager.GetPlayerScale(playerInfo.PlayerID).x;


            playerData[i] = new Vector4(
                playerObject.transform.position.x,
                playerObject.transform.position.y,
                playerObject.transform.position.z,
                playerScale
            );
        }

        playerBuffer.SetData(playerData);

        if (numPlayers > 1 && playerBuffer != null)
        {
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }

            mpb.SetBuffer("_VertBuffer", playerBuffer);
            mpb.SetInt("_Count", numPlayers);
            mpb.SetFloat("_ConnectionDistance", distanceForConnection);

            Graphics.DrawProcedural(
                drawMaterial,
                new Bounds(transform.position, Vector3.one * 5000),
                MeshTopology.Triangles,
                numPlayers * numPlayers * 3 * 2,
                1,
                null,
                mpb,
                ShadowCastingMode.Off,
                true,
                LayerMask.NameToLayer("Default")
            );
        }
    }

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

        playerBuffer = new ComputeBuffer(numPlayers, sizeof(float) * 4);
        playerPositions = new Vector3[numPlayers];
        playerData = new Vector4[numPlayers];
    }

    public void OnDestroy()
    {
        if (playerBuffer != null)
        {
            playerBuffer.Release();
        }
    }
}
