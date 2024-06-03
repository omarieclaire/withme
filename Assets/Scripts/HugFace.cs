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

                hug.HUG(this, smileID);// 

            }
        }


    }

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
