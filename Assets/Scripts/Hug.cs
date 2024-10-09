using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hug : MonoBehaviour
{
    // References to various managers and controllers for gameplay logic
    [Header("References")]
    public NoGoZoneManager noGoZoneManager; // Manages areas where HugFaces should not spawn
    public Controller controller; // Handles player control and interactions
    public TreeController treeController;
    public GameManager gameManager;

    // Prefabs for creating HugFaces, particles, and connections between objects
    [Header("Prefabs")]
    public GameObject hugFacePrefab; // Prefab for HugFaces
    public ParticleSystem matchParticlesPrefab; // Particles for when HugFaces match
    public GameObject connectionPrefab; // Prefab for connections between HugFaces (unused in this snippet)

    // Settings related to HugFaces like size and interaction radius
    [Header("HugFace Settings")]
    public float hugFaceSize = 1f; // Size of each HugFace
    public float activationRadius = 2f; // Radius within which a player can interact with a HugFace

    // Minimum distances for placing HugFaces
    public float partnerMinDistance = 1.5f; // Minimum distance between paired HugFaces
    public float otherFacesMinDistance = 2f; // Minimum distance from other HugFaces

    // General game settings including maximum HugFaces and speed of animations
    [Header("Game Settings")]
    public const int MAX_HUG_FACES = 10; // Maximum number of HugFaces that can spawn
    public float howFastWeHug = 0.01f; // Speed at which HugFaces move or change states

    // Delays between spawning pairs of HugFaces
    [Header("Spawn Delays")]
    public float firstPairDelay = 35f; // Delay before spawning the first pair of HugFaces
    public float slowPairDelay = 30f; // Delay for subsequent slower pair spawns
    public float minimumRoundDelay = 10f; // Minimum delay between rounds of spawns

    // Textures and sound clips used for HugFaces and game events
    [Header("Assets")]
    public List<Texture> pensTrueFaces; // List of textures for HugFaces in their true state
    public Texture pensNeutralFace; // Neutral face texture before interaction
    public Texture pensPostHugFace; // Post-hug face texture
    public List<AudioClip> HugFaceSongSoundClips; // Sound clips for each HugFace

    public AudioClip onHugClip; // Sound clip for when HugFaces match
    public AudioClip winSound; // Sound clip for when the game ends
    public ParticleSystem winParticleSystem; // Particles for when the game ends

    [Header("Lists")]
    public List<HugFace> listOfHugFaceObjects = new List<HugFace>(); // All active HugFaces
    public List<HugFace> listOfCompletedHugFaces = new List<HugFace>(); // Completed HugFaces

    private int currentPairCount = 1; // Number of pairs spawned so far
    private int completedPairs = 0; // Number of completed HugFace pairs
    private int totalHugFaces = 0; // Total number of HugFaces spawned
    private bool gameIsOver = false; // Whether the game has ended
    private float currentRoundDelay; // Delay between rounds of HugFace spawns
    private bool isSpawningPairs = false; // Flag to check if pairs are currently being spawned
    private Dictionary<PlayerAvatar, HugFace> currentInteractingFaces = new Dictionary<PlayerAvatar, HugFace>(); // Tracks player interactions with HugFaces
    public SoundEventSender soundEventSender; // Sends sound events during the game

    public AudioPlayer audioPlayer; // Manages audio playback
    public AudioClip spawnFacesClip; // Audio clip for when HugFaces are spawned

    private void Start()
    {
        Debug.Log($"Starting game. Total possible HugFaces to generate: {MAX_HUG_FACES}");

        currentRoundDelay = firstPairDelay;
        StartCoroutine(SpawnInitialPairs());

        if (treeController == null)
        {
            treeController = FindObjectOfType<TreeController>();  // Find TreeController if not set
        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();  // Find GameManager if not set
        }
    }

    private void ClearExistingFaces()
    {
        foreach (var face in listOfHugFaceObjects)
        {
            if (face != null && face.gameObject != null)
            {
                Destroy(face.gameObject); // Destroy each HugFace GameObject
            }
        }
        listOfHugFaceObjects.Clear(); // Clear the list of HugFaces
    }

    private List<Color> usedColors = new List<Color>();

    private void SpawnPairs(int numberOfPairs)
    {
        if (gameIsOver || isSpawningPairs) return;

        int facesNeeded = MAX_HUG_FACES - totalHugFaces;
        int pairsToSpawn = Mathf.Min(numberOfPairs, facesNeeded / 2);

        if (pairsToSpawn <= 0) return;

        for (int i = 0; i < pairsToSpawn; i++)
        {
            Color distinctColor = GenerateDistinctColor();

            int smileID = totalHugFaces / 2;
            int soundIndex = (totalHugFaces / 2) % HugFaceSongSoundClips.Count;

            HugFace face1 = CreateHugFace(smileID, distinctColor, soundIndex);
            HugFace face2 = CreateHugFace(smileID, distinctColor, soundIndex);
            AssignPartners(face1, face2);

            PositionFaces(face1, face2, partnerMinDistance, otherFacesMinDistance);
            AddFacesToList(face1, face2);

            totalHugFaces += 2;

            if (Controller.enableOldSoundSystem && spawnFacesClip != null)
            {
                audioPlayer.Play(spawnFacesClip);
            }

            if (Controller.enableNewSoundSystem)
            {
                // PlayHugSound(face1, "MimicShapeNewShapeGen");
                // PlayHugSound(face2, "MimicShapeNewShapeGen");
            }
        }

    lastSpawnTime = Time.time;
        ActivateAllFaces();
        isSpawningPairs = false;
    }

    private Color GenerateDistinctColor()
    {
        const int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Color newColor = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);

            bool isDistinct = true;
            foreach (Color usedColor in usedColors)
            {
                if (Vector3.Distance(new Vector3(newColor.r, newColor.g, newColor.b),
                                     new Vector3(usedColor.r, usedColor.g, usedColor.b)) < 0.5f)
                {
                    isDistinct = false;
                    break;
                }
            }

            if (isDistinct)
            {
                usedColors.Add(newColor);
                return newColor;
            }
        }

        Debug.LogWarning("Failed to generate distinct color. Reusing existing colors.");
        return Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
    }

    private HugFace CreateHugFace(int smileID, Color color, int soundIndex)
    {
        HugFace face = Instantiate(hugFacePrefab).GetComponent<HugFace>();
        face.color = color;
        face.smileID = smileID;
        face.HugFaceSongSoundClip = HugFaceSongSoundClips[soundIndex];
        face.matchParticlesPrefab = matchParticlesPrefab;
        return face;
    }

    private void AssignPartners(HugFace face1, HugFace face2)
    {
        face1.partners = new List<HugFace> { face2 };
        face2.partners = new List<HugFace> { face1 };
    }

   private void PositionFaces(HugFace face1, HugFace face2, float partnerMinDistance, float otherFacesMinDistance)
{
    List<HugFace> existingFaces = new List<HugFace>(listOfHugFaceObjects);

    Vector3? position1 = GetValidPosition(existingFaces, otherFacesMinDistance);
    if (!position1.HasValue)
    {
        Debug.LogError("Failed to position first face. Using fallback position.");
        position1 = Vector3.up;
    }
    Debug.Log($"[INFO] Placing HugFace 1 at {position1.Value}");

    existingFaces.Add(face1);

    Vector3? position2 = GetValidPartnerPosition(face1, existingFaces, partnerMinDistance, otherFacesMinDistance);
    if (!position2.HasValue)
    {
        Debug.LogError("Failed to position second face. Using fallback position.");
        position2 = -Vector3.up;
    }
    Debug.Log($"[INFO] Placing HugFace 2 at {position2.Value}");

    face1.transform.position = position1.Value;
    face2.transform.position = position2.Value;

    listOfHugFaceObjects.Add(face1);
    listOfHugFaceObjects.Add(face2);
}


    private Vector3? GetValidPosition(List<HugFace> existingFaces, float minDistance)
    {
        const int maxAttempts = 1000;
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
        Vector3 randomDir = Random.onUnitSphere;
        float randomDistance = Random.Range(0.5f, 1f);
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

    private bool IsOverlappingExistingFaces(Vector3 pos, List<HugFace> existingFaces, float minDistance)
{
    foreach (var face in existingFaces)
    {
        float distance = Vector3.Distance(pos, face.transform.position);
        if (distance < minDistance)
        {
            Debug.LogWarning($"[WARNING] HugFace at {face.transform.position} is too close to position {pos}. Distance: {distance}, Minimum allowed: {minDistance}");
            return true;
        }
    }
    return false;
}


    private Vector3? GetValidPartnerPosition(HugFace partner, List<HugFace> existingFaces, float partnerMinDistance, float otherFacesMinDistance)
    {
        const int maxAttempts = 1000;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomPos = GetRandomPosition();

            if (Vector3.Distance(randomPos, partner.transform.position) >= partnerMinDistance &&
                !IsOverlappingExistingFaces(randomPos, existingFaces, otherFacesMinDistance))
            {
                return randomPos;
            }
        }
        Debug.LogWarning("Failed to find a valid position for partner after " + maxAttempts + " attempts.");
        return null;
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
    }

    private void HandlePlayerInteractions()
    {
        List<PlayerAvatar> playersToRemove = new List<PlayerAvatar>();

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
                playersToRemove.Add(player);
            }
        }

        foreach (var player in playersToRemove)
        {
            currentInteractingFaces.Remove(player);
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

    // HUG Function
    public void HUG(HugFace hugFace, int smileID)
    {
        // Check if this smileID is already marked as completed in any HugFace
        if (listOfCompletedHugFaces.Exists(hf => hf.smileID == smileID))
        {
            Debug.Log($"HugFace with smileID {smileID} is already completed. Skipping completion.");
            return;
        }

        // Mark the main HugFace and its partner as full complete
        foreach (HugFace partner in hugFace.partners)
        {
            // Ensure we're not double-counting the completion for partners
            if (!partner.fullComplete)
            {
                partner.fullComplete = true;
                partner.OnFullComplete();
                partner.PlayMatchParticles();
            }
        }

        // Only proceed with logging and marking the main HugFace as complete if it wasn't already completed
        if (!hugFace.fullComplete)
        {
            hugFace.fullComplete = true;
            hugFace.OnFullComplete();
            hugFace.PlayMatchParticles();

            // Add the completed HugFace to the list
            listOfCompletedHugFaces.Add(hugFace);

            // Log completed pairs and current status
            completedPairs++;
            Debug.Log($"HugFace matched! Total completed pairs: {completedPairs}, Total completed HugFaces: {completedPairs * 2}.");
        }

        // Play sound if applicable
        if (Controller.enableOldSoundSystem && onHugClip != null)
        {
            Debug.Log($"{onHugClip} - playing hug sound.");
            audioPlayer.Play(onHugClip);
        }

        if (Controller.enableNewSoundSystem)
        {
            // Send sound event for new sound system
            // string soundID = $"p{player.id}EffectsWithMePointCollision";
            // Vector3 pointPosition = player.transform.position;
            // soundEventSender.SendOneShotSound(soundID, pointPosition);
        }

        // Log before game over check
        Debug.Log($"Total HugFaces: {totalHugFaces}, MAX_HUG_FACES: {MAX_HUG_FACES}, Completed HugFaces: {completedPairs * 2}");

        // Check if game is over
        if (totalHugFaces >= MAX_HUG_FACES && completedPairs * 2 >= MAX_HUG_FACES)
        {
            GameOver();
        }
    }

    float lastSpawnTime;

    private IEnumerator SpawnInitialPairs()
    {
        while (!gameIsOver && totalHugFaces < MAX_HUG_FACES)
        {
            // Spawn fewer pairs
            int pairsToSpawn = Mathf.Min(currentPairCount, MAX_HUG_FACES - totalHugFaces);

            // Reduce number of pairs spawned in the first round
            if (totalHugFaces == 0)
            {
                pairsToSpawn = Mathf.Clamp(pairsToSpawn, 1, 2); // Spawn at most 2 pairs at the start
            }

            SpawnPairs(pairsToSpawn);

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
        if (gameIsOver) return;

        gameIsOver = true;
        Debug.Log("Game Over! You win!");

        treeController.EnableTree();
        treeController.StartGrowingTree(10f, 20f, 0.5f, 3f);

        if (Controller.enableOldSoundSystem && winSound != null)
        {
            audioPlayer.Play(winSound); // Play win sound in old system
        }
        if (Controller.enableNewSoundSystem)
        {
            // PlayHugSound(player, "Sigh");  // Trigger a sound in the new sound system if required
        }

        if (winParticleSystem != null)
        {
            winParticleSystem.Play();
        }

        OnLevelComplete();
    }

    public void OnLevelComplete()
    {
        if (gameManager != null)
        {
            gameManager.HandleWinScenario();
        }
        else
        {
            Debug.LogError("GameManager reference is missing.");
        }
    }
}
