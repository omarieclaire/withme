using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class HugFace : MonoBehaviour
{
    public Hug hug;
    public List<HugFace> partners;
    public int smileID;
    public int state;

    public SoundEventSender soundEventSender;


    public GameObject preDiscovered;
    public GameObject discovered;
    public GameObject finished;

    public bool fullComplete;

    public Color color;


    private Renderer renderer;

    public bool inside;

    public LineRenderer lineRenderer;


    void Start()
    {
        renderer = GetComponent<Renderer>();
        Debug.Log($"{name} - Start: Initial neutral texture applied.");
        OnEnable();
    }

    public void OnEnable()
    {
        state = 0;
        preDiscovered.SetActive(true);
        discovered.SetActive(false);
        finished.SetActive(false);

        preDiscovered.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
        discovered.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        finished.GetComponent<MeshRenderer>().material.SetColor("_Color", color);


        preDiscovered.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", hug.neutralTexture);
        discovered.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", hug.trueTextures[smileID]);
        finished.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", hug.finalTexture);

        Debug.Log($"{name} - OnEnable: State set to {state}, preDiscovered active, discovered inactive.");
    }

    public void OnDisable()
    {
        preDiscovered.SetActive(false);
        discovered.SetActive(false);
        finished.SetActive(false);
        Debug.Log($"{name} - OnDisable: preDiscovered, discovered, and finished set to inactive.");
    }

    public void WhileInside(PlayerAvatar player)
    {
        // Track whether sounds have been played
        if (!soundsPlayed)
        {
            Debug.Log("hug:inside for now");

            // Play both sounds (face flip and face song)
            PlayHugSound(player, "FlipSound");
            PlayHugSound(player, "SongSound");
            soundsPlayed = true;



            // // Play the first sound when face flips
            // string soundID = $"p{player.id}EffectsHugFaceFlipSound";
            // Vector3 pointPosition = player.transform.position;
            // Debug.Log($"Playing hug sound: {soundID} at {pointPosition}");
            // soundEventSender.SendOneShotSound(soundID, pointPosition);

            // // Play the second sound for the face song
            // soundID = $"p{player.id}EffectsHugFaceSongSound";
            // pointPosition = player.transform.position;
            // Debug.Log($"Playing hug sound: {soundID} at {pointPosition}");
            // soundEventSender.SendOneShotSound(soundID, pointPosition);

            // // Mark sounds as played
            // soundsPlayed = true;
        }

        discovered.SetActive(true);
        preDiscovered.SetActive(false);
        finished.SetActive(false);
        inside = true;

        bool allInside = true;

        if (fullComplete == false)
        {
            LineRenderer lr = player.gameObject.GetComponent<LineRenderer>();
            lr.SetPosition(0, player.transform.position);
            lr.SetPosition(1, transform.position);
            lr.enabled = true;

            for (int i = 0; i < partners.Count; i++)
            {
                if (!partners[i].inside)
                {
                    allInside = false;
                    break;
                }
            }

            if (allInside)
            {
                Debug.Log("hug:inside");

                // // Play the sigh sound when all players are inside (matched)
                // string soundID = $"p{player.id}EffectsHugFaceSigh";
                // Vector3 pointPosition = player.transform.position;
                // Debug.Log($"Playing hug sigh sound: {soundID} at {pointPosition}");
                // soundEventSender.SendOneShotSound(soundID, pointPosition);

                // PlayHugSound(player, "Sigh");


                hug.HUG(this, smileID);
            }
        }
    }

    private void PlayHugSound(PlayerAvatar player, string soundType)
    {
        // audioPlayer.Play(soundType); // <-- old code to keep, maybe not accurate

        string soundID = $"p{player.id}EffectsHugFace{soundType}";
        Vector3 pointPosition = player.transform.position;
        Debug.Log($"Playing hug {soundType.ToLower()} sound: {soundID} at {pointPosition}");
        soundEventSender.SendOneShotSound(soundID, pointPosition);
    }

    // Add a flag at the class level to ensure the sounds are only played once
    private bool soundsPlayed = false;

    public void Update()
    {
        transform.LookAt(Vector3.zero);
        if (fullComplete)
        {
            WhileFinished();
        }

    }

    public void OnFullComplete()
    {
        print("YAY");
    }
    public void WhileOutside()
    {
        discovered.SetActive(false);
        preDiscovered.SetActive(true);
        finished.SetActive(false);
        inside = false;

        // Reset the soundsPlayed flag so sounds can play again next time
        soundsPlayed = false;

        Debug.Log($"{name} - WhileOutside: Inside set to false, preDiscovered active, neutral texture applied.");
    }

    public void WhileFinished()
    {
        discovered.SetActive(false);
        preDiscovered.SetActive(false);
        finished.SetActive(true);


        Vector3 averagePosition = Vector3.zero;
        int totalCounted = 0;
        for (int i = 0; i < partners.Count; i++)
        {
            if (partners[i].transform.position != transform.position)
            {
                totalCounted++;
                averagePosition += partners[i].transform.position;
            }
        }

        if (totalCounted > 0)
        {
            averagePosition /= totalCounted;
            transform.position = Vector3.Lerp(transform.position, averagePosition, hug.hugSpeed);
        }


        Debug.Log($"{name} - WhileFinished: Finished state active.");
    }

    private void ApplyTexture(Texture texture)
    {
        if (renderer != null && renderer.material != null)
        {
            renderer.material.mainTexture = texture;
            Debug.Log($"{name} - ApplyTexture: Texture {texture.name} applied.");
        }
    }

    public void ApplyColor(Color color) // Change to public
    {
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = color;
            Debug.Log($"{name} - ApplyColor: Color {color} applied.");
        }
    }


}
