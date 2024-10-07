// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class HugFace : MonoBehaviour
// {
//     [Header("References")]
//     public NoGoZoneManager noGoZoneManager;
//     public Controller controller;
//     public Hug hug;
//     public SoundEventSender soundEventSender;
//     public AudioPlayer audioPlayer;

//     [Header("Face States")]
//     public GameObject preDiscovered;
//     public GameObject discovered;
//     public GameObject finished;

//     [Header("Effects")]
//     public ParticleSystem matchParticlesPrefab;
//     private ParticleSystem instantiatedParticles; 



//     [Header("Audio")]
//     public AudioClip HugFaceSongSoundClip;
//     public AudioClip HugFaceFlipSoundClip;
//     public AudioClip HugFaceSighClip;

//     [Header("Properties")]
//     public List<HugFace> partners;
//     public int smileID;
//     public Color color;
//     public bool fullComplete;

//     private bool wasFlipped = false;
//     private bool soundsPlayed = false;
//     private int state;
//     private bool inside;
//     private bool flipped;
//     private Renderer faceRenderer;

//     private void Start()
//     {
//         OnEnable();
//                 SetupParticles();

//     }

//     public void OnEnable()
//     {
//         ResetFaceState();
//         ApplyTextures();
//     }

//     public void OnDisable()
//     {
//         SetAllStatesInactive();
//     }

//     public void WhileInside(PlayerAvatar player, HugFace face)
//     {
//         if (fullComplete) return;

//         if (!wasFlipped && !flipped)
//         {
//             HandleFirstFlip(player);
//         }

//         UpdateVisualState();
//         CheckForFullHug(player);
//     }

//     public void Update()
//     {
//         transform.LookAt(Vector3.zero);

//         if (fullComplete)
//         {
//             WhileFinished();
//         }

//         if (flipped && preDiscovered.activeSelf)
//         {
//             WhileOutside();
//         }
//     }

//     public void OnFullComplete()
//     {
//         // Debug.Log($"{name} - Full completion of HugFace.");
//     }

//     public void WhileOutside()
//     {
//         ResetToNeutralState();
//     }

//     public void WhileFinished()
//     {
//         SetFinishedStateActive();
//         MoveToTopOfDome();
//     }

//     public void ApplyColor(Color newColor)
//     {
//         if (faceRenderer != null && faceRenderer.material != null)
//         {
//             faceRenderer.material.color = newColor;
//         }
//     }

//     private void SetupParticles()
//     {
//         if (matchParticlesPrefab != null && instantiatedParticles == null)
//         {
//             instantiatedParticles = Instantiate(matchParticlesPrefab, transform.position, Quaternion.identity, transform);
//             instantiatedParticles.Stop(); // Ensure it's not playing by default
//         }
//     }

//     public void PlayMatchParticles()
//     {
//         if (instantiatedParticles != null)
//         {
//             instantiatedParticles.Play();
//         }
//         else
//         {
//             Debug.LogWarning("Match particles not set up for " + gameObject.name);
//         }
//     }


//     private void ResetFaceState()
//     {
//         state = 0;
//         flipped = false;
//         preDiscovered.SetActive(true);
//         discovered.SetActive(false);
//         finished.SetActive(false);
//     }

//     private void ApplyTextures()
//     {
//         ApplyTextureToState(preDiscovered, hug.pensNeutralFace, Color.white);
//         ApplyTextureToState(discovered, GetDiscoveredTexture(), color);
//         ApplyTextureToState(finished, hug.pensPostHugFace, color);
//     }

//     private void ApplyTextureToState(GameObject stateObject, Texture texture, Color stateColor)
//     {
//         MeshRenderer meshRenderer = stateObject.GetComponent<MeshRenderer>();
//         if (meshRenderer != null)
//         {
//             meshRenderer.material.SetTexture("_MainTex", texture);
//             meshRenderer.material.SetColor("_Color", stateColor);
//         }
//     }

//     private Texture GetDiscoveredTexture()
//     {
//         int textureCount = hug.pensTrueFaces.Count;
//         return (smileID >= 0 && smileID < textureCount) ? hug.pensTrueFaces[smileID] : null;
//     }

//     private void SetAllStatesInactive()
//     {
//         preDiscovered.SetActive(false);
//         discovered.SetActive(false);
//         finished.SetActive(false);
//     }

//     private void PlayHugSound(PlayerAvatar player, string soundType)
//     {
//         string soundID = $"p{player.id}EffectsHugFace{soundType}";
//         Vector3 pointPosition = player.transform.position;
//         Debug.Log($"Step 1: Playing hug {soundType.ToLower()} sound: {soundID} at {pointPosition}");
//         soundEventSender.SendOneShotSound(soundID, pointPosition);
//     }

//     private void HandleFirstFlip(PlayerAvatar player)
//     {

//         if (HugFaceSongSoundClip == null)
// {
//     Debug.LogError($"HugFaceSongSoundClip is null for HugFace {name}");
// }
// else
// {
//     Debug.Log($"Playing HugFaceSongSoundClip for HugFace {name}, Clip: {HugFaceSongSoundClip.name}");
// }

//         if (!soundsPlayed)
//         {
//             if (Controller.enableOldSoundSystem && HugFaceSongSoundClip != null && HugFaceFlipSoundClip != null)
//             {
//                 audioPlayer.Play(HugFaceFlipSoundClip);
//                 audioPlayer.Play(HugFaceSongSoundClip);
//                 soundsPlayed = true;
//             }
//             if (Controller.enableNewSoundSystem)
//             {
//                 // PlayHugSound(player, "FlipSound");
//                 PlayHugSound(player, "SongSound");
//                 soundsPlayed = true;

//             }
//         }
//         flipped = true;
//         wasFlipped = true;
//         inside = true;
//     }

//     private void UpdateVisualState()
//     {
//         discovered.SetActive(true);
//         preDiscovered.SetActive(false);
//         finished.SetActive(false);
//     }

//     private void CheckForFullHug(PlayerAvatar player)
//     {
//         if (partners.TrueForAll(partner => partner.inside))
//         {
//             if (Controller.enableOldSoundSystem)
//             {
//                 // audioPlayer.Play(HugFaceSighClip);

//             }
//             if (Controller.enableNewSoundSystem)
//             {
//                 // PlayHugSound(player, "Sigh");
//             }

//             hug.HUG(this, smileID);
//             partners.ForEach(partner => hug.HUG(partner, partner.smileID));
//         }
//     }


//     private void ResetToNeutralState()
//     {
//         discovered.SetActive(false);
//         preDiscovered.SetActive(true);
//         finished.SetActive(false);
//         inside = false;

//         if (flipped)
//         {
//             flipped = false;
//             wasFlipped = false;
//             soundsPlayed = false;
//         }
//     }

//     private void SetFinishedStateActive()
//     {
//         discovered.SetActive(false);
//         preDiscovered.SetActive(false);
//         finished.SetActive(true);
//     }

//     private void MoveToTopOfDome()
//     {
//         Vector3 topOfDomePosition = new Vector3(0, 1.3f, 0);
//         float offsetRange = 0.01f;
//         Vector3 randomOffset = new Vector3(Random.Range(-offsetRange, offsetRange), 0, Random.Range(-offsetRange, offsetRange));
//         Vector3 finalPosition = topOfDomePosition + randomOffset;

//         transform.position = Vector3.Lerp(transform.position, finalPosition, hug.howFastWeHug);
//         transform.LookAt(Vector3.zero);
//     }
// }


// // 