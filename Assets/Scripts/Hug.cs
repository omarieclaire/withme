using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hug : MonoBehaviour
{
    [Header("References")]
    public NoGoZoneManager noGoZoneManager;
    public Controller controller;

    [Header("Prefabs")]
    public GameObject hugFacePrefab;
    public ParticleSystem matchParticlesPrefab;

    public GameObject connectionPrefab;

    [Header("HugFace Settings")]
    public float hugFaceSize = 1f;
    public float activationRadius = 2f;

    [Header("Game Settings")]
    public const int MAX_HUG_FACES = 20;
    public float howFastWeHug = 0.01f;

    [Header("Spawn Delays")]
    public float firstPairDelay = 15f;
    public float slowPairDelay = 10f;
    public float minimumRoundDelay = 3f;

    [Header("Assets")]
    public List<Texture> pensTrueFaces;
    public Texture pensNeutralFace;
    public Texture pensPostHugFace;
    public List<AudioClip> specialHugFaceSongs;
    public AudioClip onHugClip;
    public AudioClip winSound;
    public ParticleSystem winParticleSystem;

    [Header("Lists")]
    public List<HugFace> listOfHugFaceObjects = new List<HugFace>();
    public List<HugFace> listOfCompletedHugFaces = new List<HugFace>();

    private int currentPairCount = 1;
    private int completedPairs = 0;
    private int totalHugFaces = 0;
    private bool gameIsOver = false;
    private float currentRoundDelay;
    private bool isSpawningPairs = false;
    private Dictionary<PlayerAvatar, HugFace> currentInteractingFaces = new Dictionary<PlayerAvatar, HugFace>();
    public SoundEventSender soundEventSender;

    public AudioPlayer audioPlayer;
    public AudioClip spawnFacesClip;


    private void Start()
    {
        currentRoundDelay = firstPairDelay;
        StartCoroutine(SpawnInitialPairs());
    }

    private void ClearExistingFaces()
    {
        foreach (var face in listOfHugFaceObjects)
        {
            if (face != null && face.gameObject != null)
            {
                Destroy(face.gameObject);
            }
        }
        listOfHugFaceObjects.Clear();
    }


    private void PlaySound(PlayerAvatar player, string soundType)
    {
        string soundID = soundType;
        Vector3 randomPosition = new Vector3(0, 0, 0);
        soundEventSender.SendOneShotSound(soundID, randomPosition);
    }

    private void SpawnPairs(int numberOfPairs)
    {
        if (gameIsOver || isSpawningPairs) return;

        int facesNeeded = MAX_HUG_FACES - totalHugFaces;
        int pairsToSpawn = Mathf.Min(numberOfPairs, facesNeeded / 2);

        if (pairsToSpawn <= 0) return;

        for (int i = 0; i < pairsToSpawn; i++)
        {
            Color randomColor = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
            int smileID = totalHugFaces / 2;
            int soundIndex = (totalHugFaces / 2) % specialHugFaceSongs.Count;

            HugFace face1 = CreateHugFace(smileID, randomColor, soundIndex);
            HugFace face2 = CreateHugFace(smileID, randomColor, soundIndex);

            AssignPartners(face1, face2);
            PositionFaces(face1, face2, 0.75f);
            AddFacesToList(face1, face2);

            totalHugFaces += 2;

            if (Controller.enableOldSoundSystem && spawnFacesClip != null)
            {
                // audioPlayer.Play(spawnFacesClip);

            }
            if (Controller.enableNewSoundSystem)
            {
                // not using face1, just sending it to 0 0 0
                // reusing a sound
                // PlayHugSound(face1, "MimicShapeNewShapeGen");
                // PlayHugSound(face2, "MimicShapeNewShapeGen");

            }


        }

        ActivateAllFaces();
        isSpawningPairs = false;
    }

    private HugFace CreateHugFace(int smileID, Color color, int soundIndex)
    {
        HugFace face = Instantiate(hugFacePrefab).GetComponent<HugFace>();
        face.color = color;
        face.smileID = smileID;
        face.hugFaceSong = specialHugFaceSongs[soundIndex];
        face.matchParticlesPrefab = matchParticlesPrefab; // Set the particle prefab
        return face;
    }

    private void AssignPartners(HugFace face1, HugFace face2)
    {
        face1.partners = new List<HugFace> { face2 };
        face2.partners = new List<HugFace> { face1 };
    }

    private void PositionFaces(HugFace face1, HugFace face2, float minDistance)
{
    List<HugFace> existingFaces = new List<HugFace>(listOfHugFaceObjects);

    Vector3? position1 = GetValidPosition(existingFaces, minDistance);
    if (!position1.HasValue)
    {
        Debug.LogError("Failed to position first face. Using fallback position.");
        position1 = Vector3.up; // Fallback position
    }

    existingFaces.Add(face1);

    Vector3? position2 = GetValidPosition(existingFaces, minDistance);
    if (!position2.HasValue)
    {
        Debug.LogError("Failed to position second face. Using fallback position.");
        position2 = -Vector3.up; // Fallback position
    }

    face1.transform.position = position1.Value;
    face2.transform.position = position2.Value;

    listOfHugFaceObjects.Add(face1);
    listOfHugFaceObjects.Add(face2);
}
private bool IsOverlappingExistingFaces(Vector3 pos, List<HugFace> existingFaces, float minDistance)
{
    foreach (var face in existingFaces)
    {
        if (Vector3.Distance(pos, face.transform.position) < minDistance)
        {
            return true;
        }
    }
    return false;
}
    private Vector3? GetValidPosition(List<HugFace> existingFaces, float minDistance)
{
    const int maxAttempts = 1000; // Increase max attempts
    for (int attempts = 0; attempts < maxAttempts; attempts++)
    {
        Vector3 randomPos = GetRandomPosition();
        if (!CheckIfBlocked(randomPos) && !IsOverlappingExistingFaces(randomPos, existingFaces, minDistance))
        {
            return randomPos;
        }
    }
    Debug.LogWarning("Failed to find a valid position after " + maxAttempts + " attempts.");
    return null;
}
private Vector3 GetRandomPosition()
{
    // Use a uniform distribution on a sphere surface
    Vector3 randomDir = Random.onUnitSphere;
    float randomDistance = Random.Range(0.5f, 1f); // Adjust these values as needed
    Vector3 randomPos = randomDir * randomDistance;
    return controller.getFinalPosition(randomPos);
}

   private bool CheckIfBlocked(Vector3 pos)
{
    Ray ray = new Ray(Vector3.zero, pos.normalized);
    if (Physics.Raycast(ray, out RaycastHit hit, pos.magnitude))
    {
        return hit.collider == noGoZoneManager.doorCollider ||
               hit.collider == noGoZoneManager.soundBoothCollider ||
               hit.collider == noGoZoneManager.stageCollider;
    }
    return false;
}
    private void AddFacesToList(HugFace face1, HugFace face2)
    {
        AddFaceToList(face1);
        AddFaceToList(face2);
    }

    private void AddFaceToList(HugFace face)
    {
        face.transform.localScale = Vector3.one * hugFaceSize * 2;
        face.transform.parent = transform;
        listOfHugFaceObjects.Add(face);
    }

    private void ActivateAllFaces()
    {
        foreach (var face in listOfHugFaceObjects)
        {
            face.gameObject.SetActive(true);
        }
    }

    public void Update()
    {
        HandlePlayerInteractions();
        HandleCompletedHugFaces();
    }

    private void HandlePlayerInteractions()
    {
        foreach (var player in controller.activePlayers)
        {
            player.GetComponent<LineRenderer>().enabled = false;
            HugFace closestFace = FindClosestHugFace(player);

            if (closestFace != null)
            {
                closestFace.WhileInside(player, closestFace);
                currentInteractingFaces[player] = closestFace;
            }
            else if (currentInteractingFaces.TryGetValue(player, out HugFace interactingFace))
            {
                interactingFace.WhileOutside();
                currentInteractingFaces.Remove(player);
            }
        }
    }

    private HugFace FindClosestHugFace(PlayerAvatar player)
    {
        HugFace closestFace = null;
        float closestDistance = float.MaxValue;

        foreach (var face in listOfHugFaceObjects)
        {
            float distance = Vector3.Distance(player.transform.position, face.transform.position);
            if (distance < closestDistance && distance < activationRadius)
            {
                closestDistance = distance;
                closestFace = face;
            }
        }

        return closestFace;
    }

    private void HandleCompletedHugFaces()
    {
        foreach (var completedFace in listOfCompletedHugFaces)
        {
            completedFace.WhileFinished();
        }
    }

    public void HUG(HugFace hugFace, int smileID)
    {
        foreach (HugFace partner in hugFace.partners)
        {
            partner.fullComplete = true;
            partner.OnFullComplete();
            partner.PlayMatchParticles();

        }

        hugFace.fullComplete = true;
        hugFace.OnFullComplete();
        hugFace.PlayMatchParticles();


        // audioPlayer.Play(onHugClip);

        completedPairs++;

        if (totalHugFaces >= MAX_HUG_FACES)
        {
            GameOver();
        }
    }

    private IEnumerator SpawnInitialPairs()
    {
        while (!gameIsOver && totalHugFaces < MAX_HUG_FACES)
        {
            SpawnPairs(currentPairCount);

            if (currentPairCount == 1)
            {
                yield return new WaitForSeconds(firstPairDelay);
            }
            else if (currentPairCount == 2 || currentPairCount == 3)
            {
                yield return new WaitForSeconds(slowPairDelay);
            }
            else
            {
                currentRoundDelay = Mathf.Max(minimumRoundDelay, currentRoundDelay - 1f);
                yield return new WaitForSeconds(currentRoundDelay);
            }

            currentPairCount++;
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! You win!");
        gameIsOver = true;


        if (Controller.enableOldSoundSystem && winSound != null)
        {
            audioPlayer.Play(winSound);

        }
        if (Controller.enableNewSoundSystem)
        {
            // PlayHugSound(player, "Sigh");
        }

        if (winParticleSystem != null)
        {
            winParticleSystem.Play();
        }
    }
}