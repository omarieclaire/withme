using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hug : MonoBehaviour
{
    public NoGoZoneManager noGoZoneManager;  // Reference to the NoGoZoneManager

    [Tooltip("Prefab used to instantiate faces.")]
    public GameObject hugFacePrefab;

    [Tooltip("Size of each face.")]
    public float hugFaceSize;

    [Tooltip("List of all HugFace objects in the game.")]
    public List<HugFace> listOfHugFaceObjects = new List<HugFace>();  // List to store current HugFaces

    [Tooltip("The game controller managing the game state.")]
    public Controller controller;

    [Tooltip("Radius within which faces get activated.")]
    public float activationRadius;

    [Tooltip("Prefab used to create connection lines between faces.")]
    public GameObject connectionPrefab;

    public List<HugFace> listOfCompletedHugFaces = new List<HugFace>();  // Track completed HugFaces

    [Tooltip("Number textures, which also is total number of faces.")]
    public List<Texture> pensTrueFaces;
    public Texture pensNeutralFace;
    public Texture pensPostHugFace;

    [Tooltip("List of special sounds for each HugFace pair.")]
    public List<AudioClip> specialHugFaceSongs;  // List of unique sounds for HugFace pairs
    public int numColors;

    public float howFastWeHug = .01f;

     public   int soundIndex = 0;


    public AudioClip onHugClip;
    public AudioClip winSound;
    public ParticleSystem winParticleSystem;

    public AudioPlayer player;

    private int currentPairCount = 1;  // Start with 1 pair
    private int completedPairs = 0;    // Track completed pairs
    private int totalHugFaces = 0;     // Track total HugFaces created
    private bool gameIsOver = false;   // Control game flow

    // Delays for the first, second/third, and remaining pairs
    public float firstPairDelay = 15f;   // Slowest delay for the first pair
    public float slowPairDelay = 10f;    // Slow delay for second and third pairs
    public float minimumRoundDelay = 3f; // Minimum delay to avoid too fast spawning
    private float currentRoundDelay;     // Current delay that will change based on pair count

    private bool isSpawningPairs = false;  // Ensure delay is respected

    private void Start()
    {
        currentRoundDelay = firstPairDelay;  // Start with the slow delay for the first pair
        StartCoroutine(SpawnInitialPairs()); // Start the coroutine to spawn pairs
    }

    private void ClearExistingFaces()
    {
        foreach (var face in listOfHugFaceObjects)
        {
            Destroy(face.gameObject);  // Destroy all existing HugFaces
        }
        listOfHugFaceObjects.Clear();
    }

  private void SpawnPairs(int numberOfPairs)
{
    if (gameIsOver || isSpawningPairs) return;  // Stop if the game is over or we're waiting

    // Calculate how many more faces are needed to reach 20 total
    int facesNeeded = 20 - totalHugFaces;
    Debug.Log($"Total HugFaces: {totalHugFaces}, Faces Needed: {facesNeeded}");  // Logging to see the current face count

    // Adjust number of pairs to spawn based on remaining faces needed
    int pairsToSpawn = Mathf.Min(numberOfPairs, facesNeeded / 2);
    Debug.Log($"Pairs to spawn this round: {pairsToSpawn}");  // Logging to track the number of pairs being spawned

    // If no pairs need to be spawned or active face limit reached, return
    if (pairsToSpawn <= 0) 
    {
        Debug.Log("No more pairs needed to spawn.");  // Log if no pairs need to be spawned
        return;
    }


    for (int i = 0; i < pairsToSpawn; i++)
    {
        Color randomColor = GenerateRandomColor();

        HugFace face1 = CreateHugFace(totalHugFaces, randomColor, ref soundIndex);
        HugFace face2 = CreateHugFace(totalHugFaces, randomColor, ref soundIndex);

        AssignPartners(face1, face2);  // Ensure pairing
        PositionFaces(face1, face2, 0.75f);  // Adjust minDistance if needed
        AddFacesToList(face1, face2);  // Add faces to the list

        totalHugFaces += 2;  // Increment total HugFaces count
        Debug.Log($"Total HugFaces after spawning: {totalHugFaces}");  // Log after spawning each pair
    }

    ActivateAllFaces();  // Enable all HugFaces
    isSpawningPairs = false;  // Reset flag
}


    private Color GenerateRandomColor()
    {
        return Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);  // Random color
    }

    private HugFace CreateHugFace(int smileID, Color color, ref int soundIndex)
    {
        HugFace face = Instantiate(hugFacePrefab).GetComponent<HugFace>();

        if (soundIndex < specialHugFaceSongs.Count)
        {
            face.hugFaceSong = specialHugFaceSongs[soundIndex];  // Assign sound to both faces
        }
        soundIndex++;
        face.smileID = smileID;
        face.color = color;
        return face;
    }

    private void AssignPartners(HugFace face1, HugFace face2)
    {
        face1.partners = new List<HugFace> { face2 };  // Assign as partners
        face2.partners = new List<HugFace> { face1 };
    }

    private void PositionFaces(HugFace face1, HugFace face2, float minDistance)
    {
        Vector3 randomPos1, randomPos2;
        bool isBlocked1, isBlocked2;
        int retries1 = 0, retries2 = 0;

        // Position face1 while avoiding no-go zones
        do
        {
            randomPos1 = GetRandomPosition();
            isBlocked1 = CheckIfBlocked(randomPos1);
            retries1++;
            if (retries1 > 10) break;
        } while (isBlocked1);

        // Position face2 while avoiding no-go zones and ensuring no overlap with face1
        do
        {
            randomPos2 = GetRandomPosition();
            isBlocked2 = CheckIfBlocked(randomPos2) || Vector3.Distance(randomPos1, randomPos2) < minDistance;
            retries2++;
            if (retries2 > 10) break;
        } while (isBlocked2);

        face1.transform.position = randomPos1;
        face2.transform.position = randomPos2;
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
        face1.transform.localScale = Vector3.one * hugFaceSize * 2;
        face1.transform.parent = transform;
        listOfHugFaceObjects.Add(face1);

        face2.transform.localScale = Vector3.one * hugFaceSize * 2;
        face2.transform.parent = transform;
        listOfHugFaceObjects.Add(face2);
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
        // Handle interactions with players and HugFaces
        foreach (var player in controller.activePlayers)
        {
            player.GetComponent<LineRenderer>().enabled = false;
            int closestFaceID = -1;
            float closestDistance = float.MaxValue;

            for (int j = 0; j < listOfHugFaceObjects.Count; j++)
            {
                float distance = Vector3.Distance(player.transform.position, listOfHugFaceObjects[j].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFaceID = j;
                }
            }

            if (closestDistance < activationRadius)
            {
                listOfHugFaceObjects[closestFaceID].WhileInside(player, listOfHugFaceObjects[closestFaceID]);
            }
        }

        foreach (var completedFace in listOfCompletedHugFaces)
        {
            completedFace.WhileFinished();  // Handle completion logic
        }
    }

    public void HUG(HugFace hugFace, int smileID)
    {
        // Mark all partners of the given HugFace as fully completed
        foreach (HugFace partner in hugFace.partners)
        {
            partner.fullComplete = true;  // Mark partner as complete
            partner.OnFullComplete();  // Trigger completion for the partner
        }

        // Mark the original HugFace as complete as well
        hugFace.fullComplete = true;
        hugFace.OnFullComplete();

        // Play the sound for the hug event
        player.Play(onHugClip);

        completedPairs++;  // Increment completed pairs

        // Check if the game should end
        if (totalHugFaces >= 20)
        {
            GameOver();  // Trigger game over when 20 HugFaces have been created
        }
    }


    private IEnumerator SpawnInitialPairs()
    {
        // Loop until the game is over or 20 hugfaces are spawned
        while (!gameIsOver && totalHugFaces < 20)
        {
            Debug.Log("Spawning pair...");  // Log for tracking
            SpawnPairs(currentPairCount);   // Spawn pairs based on the current pair count

            // Apply specific delays for first, second/third, and subsequent pairs
            if (currentPairCount == 1)
            {
                yield return new WaitForSeconds(firstPairDelay);  // Slowest delay for the first pair
            }
            else if (currentPairCount == 2 || currentPairCount == 3)
            {
                yield return new WaitForSeconds(slowPairDelay);   // Slow delay for second and third pairs
            }
            else
            {
                // After the third pair, progressively decrease the delay
                currentRoundDelay = Mathf.Max(minimumRoundDelay, currentRoundDelay - 1f);
                yield return new WaitForSeconds(currentRoundDelay);  // Faster delay for remaining pairs
            }

            currentPairCount++;  // Increment the pair count after each round
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! You win!");

        // Mark the game as over
        gameIsOver = true;

        // Play the win sound
        if (player != null && winSound != null)
        {
            player.Play(winSound);
        }

        // Trigger the win particle system if assigned
        if (winParticleSystem != null)
        {
            winParticleSystem.Play();
        }

    }

}
