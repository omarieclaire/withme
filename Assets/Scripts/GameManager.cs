using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Tooltip("Duration of the game before switching scenes (in seconds)")]
    public float gameDuration = 60f;  // Time before the scene switches

    [Tooltip("Time to wait after win condition or timer runs out before switching scenes")]
    public float waitTime = 3f;  // You can set this in the Inspector

    [Tooltip("Number of retries to send the background music message")]
    public int retryCount = 3;  // Number of times to retry sending the message

    [Tooltip("Delay between retries (in seconds)")]
    public float retryDelay = 0.5f;  // Delay between retries

    private float gameTimer;
    private bool gameEnded = false;
    private Controller controller;
    private bool musicPlayed = false; // Flag to ensure music is played only once

    public SoundEventSender soundEventSender;

    void Start()
    {
        Debug.Log("[INFO] GameManager started. Looking for Main Controller...");

        // Find the GameObject named "Main Controller" and get its Controller component
        GameObject mainControllerObj = GameObject.Find("Main Controller");

        if (mainControllerObj != null)
        {
            Debug.Log("[INFO] Main Controller GameObject found.");
            controller = mainControllerObj.GetComponent<Controller>();
        }
        else
        {
            Debug.LogError("[ERROR] Main Controller GameObject not found.");
        }

        if (controller == null)
        {
            Debug.LogError("[ERROR] No Controller component found on Main Controller GameObject.");
        }
        else
        {
            // Start the background music when the game starts
            Debug.Log("[INFO] Starting background music.");
            StartCoroutine(TryStartBackgroundMusicWithRetries());
        }

        gameTimer = gameDuration;
        Debug.Log("[INFO] Game timer initialized to " + gameDuration + " seconds.");
    }

    void Update()
    {
        // Decrease timer if the game hasn't ended
        if (!gameEnded)
        {
            gameTimer -= Time.deltaTime;

            // Check if time has run out or a win condition has been met
            if (gameTimer <= 0f)
            {
                Debug.Log("[INFO] Time's up. Ending the game.");
                EndGame();
            }
        }

        // Keypress to switch scenes manually
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("[INFO] Keypress detected: N. Loading next scene.");
            LoadNextScene();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[INFO] Keypress detected: P. Loading previous scene.");
            LoadPreviousScene();
        }
    }

    IEnumerator TryStartBackgroundMusicWithRetries()
    {
        int attempts = 0;
        // string soundID = "music";
        string soundID = GetSoundIDForCurrentScene();  // Get the unique soundID for the current scene

        while (attempts < retryCount && !musicPlayed)
        {
            attempts++;
            Debug.Log($"[INFO] Attempt {attempts} to send background music OSC message.");

            if (soundEventSender != null)
            {
                Vector3 musicPosition = new Vector3(1f, 1f, 0.01f);  // Fixed position for background music
                soundEventSender.SendOrUpdateContinuousSound(soundID, musicPosition);
                Debug.Log($"Sending music data to soundEventSender.SendOrUpdateContinuousSound: {soundID} {musicPosition}");
            }
            else
            {
                Debug.LogError("[ERROR] No soundEventSender available to send background music.");
            }

            yield return new WaitForSeconds(retryDelay);  // Wait before retrying
        }

        if (attempts >= retryCount)
        {
            Debug.Log("[INFO] Background music send attempts exhausted. Stopping further retries.");
        }
        else
        {
            Debug.Log("[INFO] Background music successfully sent within retry limit.");
            musicPlayed = true;  // Mark music as played to stop further attempts
        }
    }

    string GetSoundIDForCurrentScene()
{
    // Get the current scene's name
    Scene currentScene = SceneManager.GetActiveScene();
    string sceneName = currentScene.name;

    // Format the soundID as "SceneNameBGMusic"
    string soundID = $"{sceneName}BGMusic";
    Debug.Log($"[INFO] Generated soundID for current scene: {soundID}");
    return soundID;
}


public void StopBackgroundMusic()
{
    string soundID = GetSoundIDForCurrentScene();  // Get the unique soundID for the current scene
    
    if (soundEventSender != null)
    {
        soundEventSender.StopContinuousSound(soundID);
        Debug.Log($"[INFO] Stopped background music: {soundID}");
    }
    else
    {
        Debug.LogError("[ERROR] No soundEventSender available to stop background music.");
    }
}


    void EndGame()
    {
        if (controller != null)
        {
            Debug.Log("[INFO] Stopping background music.");
            StopBackgroundMusic();  // Stop the background music when the game ends
        }

        gameEnded = true;
        Debug.Log("Game Over! Moving to the next scene in " + waitTime + " seconds.");
        Invoke("LoadNextScene", waitTime); // Wait for `waitTime` seconds before loading the next scene
    }

    void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        Debug.Log("[INFO] Loading next scene. Current scene index: " + currentSceneIndex + ", Next scene index: " + nextSceneIndex);
        SceneManager.LoadScene(nextSceneIndex);
    }

    void LoadPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = (currentSceneIndex - 1 + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
        Debug.Log("[INFO] Loading previous scene. Current scene index: " + currentSceneIndex + ", Previous scene index: " + previousSceneIndex);
        SceneManager.LoadScene(previousSceneIndex);
    }
}
