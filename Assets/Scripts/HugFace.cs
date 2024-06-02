using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HugFace : MonoBehaviour
{
    public Hug hug;
    public HugFace partner;
    public int smileID;
    public int state;

    public GameObject preDiscovered;
    public GameObject discovered;
    public GameObject finished;

    public Texture neutralTexture;
    public Texture[] trueTextures; // Array of textures

    private Texture trueTexture;

    public Texture TrueTexture // Public getter
    {
        get { return trueTexture; }
    }

    private Renderer renderer;

    public bool inside;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        ApplyTexture(neutralTexture);
        Debug.Log($"{name} - Start: Initial neutral texture applied.");
        OnEnable();
    }

    public void OnEnable()
    {
        state = 0;
        preDiscovered.SetActive(true);
        discovered.SetActive(false);
        finished.SetActive(false);
        Debug.Log($"{name} - OnEnable: State set to {state}, preDiscovered active, discovered inactive.");
    }

    public void OnDisable()
    {
        preDiscovered.SetActive(false);
        discovered.SetActive(false);
        finished.SetActive(false);
        Debug.Log($"{name} - OnDisable: preDiscovered, discovered, and finished set to inactive.");
    }

    public void WhileInside()
    {
        discovered.SetActive(true);
        preDiscovered.SetActive(false);
        finished.SetActive(false);
        inside = true;
        ApplyTexture(trueTexture);
        Debug.Log($"{name} - WhileInside: Inside set to true, discovered active, true texture applied.");

        if (partner.inside && partner.TrueTexture == this.TrueTexture) // Use public getter
        {
            ApplyColor(Color.red);
            partner.ApplyColor(Color.red);
            Debug.Log($"{name} - WhileInside: Partner inside and matching texture, both colored red.");
        }
        else
        {
            hug.PairDiscover(this.gameObject, partner.gameObject, smileID);
            Debug.Log($"{name} - WhileInside: Partner not inside or textures do not match, pair discovery triggered.");
        }
    }

    public void WhileOutside()
    {
        discovered.SetActive(false);
        preDiscovered.SetActive(true);
        finished.SetActive(false);
        inside = false;
        ApplyTexture(neutralTexture);
        Debug.Log($"{name} - WhileOutside: Inside set to false, preDiscovered active, neutral texture applied.");
    }

    public void WhileFinished()
    {
        discovered.SetActive(false);
        preDiscovered.SetActive(false);
        finished.SetActive(true);
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

    public void AssignTrueTexture(int index)
    {
        trueTexture = trueTextures[index % trueTextures.Length];
        Debug.Log($"{name} - AssignTrueTexture: True texture {trueTexture.name} assigned based on index {index}.");
    }
}
