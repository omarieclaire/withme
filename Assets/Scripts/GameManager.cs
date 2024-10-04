using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public GameObject TheOracleOfAll;
    public float gameDuration = 60f;
    public float waitTime = 3f;
    public int retryCount = 3;
    public float retryDelay = 0.5f;
    private float gameTimer;
    private bool gameEnded = false;
    private Controller controller;
    private bool musicPlayed = false;
     private  Vector3 defaultSoundPosition = new Vector3(1f, 1f, 0.01f); 

    public SoundEventSender soundEventSender;

    public string timeoutSoundID = "timeoutSound"; 
    public string winGameSoundID = "winGameSound"; 
    private bool winConditionMet = false;

    // List of all player sound IDs (can be dynamic based on your needs)
    private string[] playerSoundIDs = { "p0", "p1", "p2", "p3", "p4", "p5", "p6", "p7", "p8", "p9", "p10"  
    };

    void Start()
    {
        // Debug.Log("[INFO] GameManager started. Looking for Main Controller...");

        if (TheOracleOfAll != null)
        {
            controller = TheOracleOfAll.GetComponent<Controller>();
        }
        else
        {
            // Debug.LogError("[ERROR] Main Controller GameObject not found.");
        }

        if (controller == null)
        {
            // Debug.LogError("[ERROR] No Controller component found on Main Controller GameObject.");
        }
        else
        {
            // Debug.Log("[INFO] Starting background music.");
            StartCoroutine(TryStartBackgroundMusicWithRetries());
        }

        gameTimer = gameDuration;
        // Debug.Log("[INFO] Game timer initialized to " + gameDuration + " seconds.");
    }

    public void StopAllSoundsOnPlayExit()
    {
        // Stop background music for the current scene
        StopBackgroundMusic();

        // Stop all player sounds
        StopAllPlayerSounds();
    }

    public void StopBackgroundMusic()
    {
        string soundID = GetSoundIDForCurrentScene();  
        if (soundEventSender != null)
        {
            soundEventSender.StopContinuousSound(soundID, defaultSoundPosition);
            // Debug.Log($"[INFO] Stopped background music: {soundID}");
        }
        else
        {
            // Debug.LogError("[ERROR] No soundEventSender available to stop background music.");
        }
    }

    private void StopAllPlayerSounds()
{
    if (soundEventSender != null)
    {
        soundEventSender.StopContinuousSound("p0", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p1", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p2", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p3", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p4", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p5", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p6", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p7", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p8", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p9", defaultSoundPosition);
        soundEventSender.StopContinuousSound("p10", defaultSoundPosition);


        // foreach (string playerSoundID in playerSoundIDs)
        // {
        //     Debug.Log($"[DEBUG] Attempting to stop player sound: {playerSoundID}");
        //     soundEventSender.StopContinuousSound(playerSoundID);
        //     Debug.Log($"[INFO] Stopped player sound: {playerSoundID}");
        // }
    }
    else
    {
        // Debug.LogError("[ERROR] No soundEventSender available to stop player sounds.");
    }
}


    string GetSoundIDForCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        string soundID = $"{sceneName}BGMusic";
        // Debug.Log($"[INFO] Generated soundID for current scene: {soundID}");
        return soundID;
    }

    void Update()
    {
        if (!gameEnded)
        {
            gameTimer -= Time.deltaTime;

            if (gameTimer <= 0f && !winConditionMet)
            {
                // Debug.Log("[INFO] Time's up. Playing timeout sound and ending the game.");
                PlayTimeoutSound();
                EndGame();
            }
            else if (winConditionMet)
            {
                // Debug.Log("[INFO] Win condition met. Playing win game sound.");
                PlayWinGameSound();
                EndGame();
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            // Debug.Log("[INFO] Keypress detected: N. Loading next scene.");
            LoadNextScene();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            // Debug.Log("[INFO] Keypress detected: P. Loading previous scene.");
            LoadPreviousScene();
        }
    }

    IEnumerator TryStartBackgroundMusicWithRetries()
    {
        int attempts = 0;
        string soundID = GetSoundIDForCurrentScene();  

        while (attempts < retryCount && !musicPlayed)
        {
            attempts++;
            // Debug.Log($"[INFO] Attempt {attempts} to send background music OSC message.");

            if (soundEventSender != null)
            {
                Vector3 musicPosition = new Vector3(1f, 1f, 0.01f); 
                soundEventSender.SendOrUpdateContinuousSound(soundID, musicPosition);
                // Debug.Log($"Sending music data to soundEventSender.SendOrUpdateContinuousSound: {soundID} {musicPosition}");
            }
            else
            {
                // Debug.LogError("[ERROR] No soundEventSender available to send background music.");
            }

            yield return new WaitForSeconds(retryDelay);  
        }

        if (attempts >= retryCount)
        {
            // Debug.Log("[INFO] Background music send attempts exhausted. Stopping further retries.");
        }
        else
        {
            // Debug.Log("[INFO] Background music successfully sent within retry limit.");
            musicPlayed = true;  
        }
    }

    void EndGame()
    {
        if (controller != null)
        {
            // Debug.Log("[INFO] Stopping background music.");
            StopBackgroundMusic();
        }

        gameEnded = true;
        // Debug.Log("Game Over! Moving to the next scene in " + waitTime + " seconds.");
        Invoke("LoadNextScene", waitTime); 
    }

    void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        // Debug.Log("[INFO] Loading next scene. Current scene index: " + currentSceneIndex + ", Next scene index: " + nextSceneIndex);
        SceneManager.LoadScene(nextSceneIndex);
    }

    void LoadPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = (currentSceneIndex - 1 + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
        // Debug.Log("[INFO] Loading previous scene. Current scene index: " + currentSceneIndex + ", Previous scene index: " + previousSceneIndex);
        SceneManager.LoadScene(previousSceneIndex);
    }

    void PlayTimeoutSound()
    {
        if (soundEventSender != null)
        {
            Vector3 soundPosition = new Vector3(1f, 1f, 0.01f);
            soundEventSender.SendOrUpdateContinuousSound(timeoutSoundID, soundPosition);
            // Debug.Log($"[INFO] Playing timeout sound: {timeoutSoundID}");
        }
        else
        {
            // Debug.LogError("[ERROR] No soundEventSender available for timeout sound.");
        }
    }

    void PlayWinGameSound()
    {
        if (soundEventSender != null)
        {
            Vector3 soundPosition = new Vector3(1f, 1f, 0.01f);
            soundEventSender.SendOrUpdateContinuousSound(winGameSoundID, soundPosition);
            // Debug.Log($"[INFO] Playing win game sound: {winGameSoundID}");
        }
        else
        {
            // Debug.LogError("[ERROR] No soundEventSender available for win game sound.");
        }
    }

    public void SetWinConditionMet()
    {
        winConditionMet = true;
        // Debug.Log("[INFO] Win condition met has been set.");
    }
}
