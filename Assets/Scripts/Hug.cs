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

    public AudioPlayer player;

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
        return face;
    }

    private void AssignPartners(HugFace face1, HugFace face2)
    {
        face1.partners = new List<HugFace> { face2 };
        face2.partners = new List<HugFace> { face1 };
    }

    private void PositionFaces(HugFace face1, HugFace face2, float minDistance)
{
    List<HugFace> existingFaces = new List<HugFace>(listOfHugFaceObjects); // Copy existing faces

    Vector3 position1 = GetValidPosition(existingFaces, minDistance);
    existingFaces.Add(face1); // Add face1 to existing faces for checking face2 position

    Vector3 position2 = GetValidPosition(existingFaces, minDistance);

    face1.transform.position = position1;
    face2.transform.position = position2;

    // Add new faces to the list of existing faces
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

    private Vector3 GetValidPosition(List<HugFace> existingFaces, float minDistance)
{
    Vector3 randomPos;
    bool isValid;
    int maxAttempts = 100; // Prevent infinite loop
    int attempts = 0;

    do
    {
        randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 2f), Random.Range(-1f, 1f));
        randomPos = controller.getFinalPosition(randomPos);

        isValid = !CheckIfBlocked(randomPos) && !IsOverlappingExistingFaces(randomPos, existingFaces, minDistance);

        attempts++;
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("Failed to find a valid position after " + maxAttempts + " attempts. Using last generated position.");
            break;
        }
    } while (!isValid);

    return randomPos;
}

    private Vector3 GetRandomPosition()
    {
        return controller.getFinalPosition(new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f)));
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
        }

        hugFace.fullComplete = true;
        hugFace.OnFullComplete();

        player.Play(onHugClip);

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

        if (player != null && winSound != null)
        {
            player.Play(winSound);
        }

        if (winParticleSystem != null)
        {
            winParticleSystem.Play();
        }
    }
}