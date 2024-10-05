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
    private Vector3 defaultSoundPosition = new Vector3(1f, 1f, 0.01f);

    public SoundEventSender soundEventSender;
    public AudioSource BGMusicSource;
    public AudioClip BGMusicClip;
    public AudioClip timeoutClip;
    public AudioClip winGameClip;




    public string timeoutSoundID = "timeoutSound";
    public string winGameSoundID = "winGameSound";
    private bool winConditionMet = false;

    // List of all player sound IDs (can be dynamic based on your needs)
    private string[] playerSoundIDs = { "p0", "p1", "p2", "p3", "p4", "p5", "p6", "p7", "p8", "p9", "p10"
    };

    void Start()
    {



        if (TheOracleOfAll != null)
        {
            controller = TheOracleOfAll.GetComponent<Controller>();
        }

        else
        {
            Debug.Log("[INFO] Starting background music.");

            if (Controller.enableOldSoundSystem)
            {
                if (BGMusicSource != null && BGMusicClip != null)
                {
                    BGMusicSource.clip = BGMusicClip;
                    BGMusicSource.loop = true; // Ensure the clip loops
                    BGMusicSource.Play(); // Play the background music
                }
                else
                {
                    Debug.LogError("AudioSource or BGMusicClip is missing.");
                }

            }

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
            if (Controller.enableNewSoundSystem)
            {
                soundEventSender.StopContinuousSound(soundID, defaultSoundPosition);

            }

        }
    }

    private void StopAllPlayerSounds()
    {
        if (soundEventSender != null)
        {
            if (Controller.enableNewSoundSystem)
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
            }
        }

    }


    string GetSoundIDForCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        string soundID = $"{sceneName}BGMusic";
        Debug.Log($"[INFO] Generated soundID for current scene: {soundID}");
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
                if (Controller.enableNewSoundSystem)
                {
                    Debug.Log("sendng bg to soundeventsender");
                    Vector3 musicPosition = new Vector3(1f, 1f, 0.01f);
                    soundEventSender.SendOrUpdateContinuousSound(soundID, musicPosition);
                }


            }

            yield return new WaitForSeconds(retryDelay);
        }

        if (attempts >= retryCount)
        {
            Debug.Log("[INFO] Background music send attempts exhausted. Stopping further retries.");
        }
        else
        {
            Debug.Log("[INFO] Background music successfully sent within retry limit.");
            musicPlayed = true;
        }
    }

    void EndGame()
    {
        if (controller != null)
        {
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
            if (Controller.enableNewSoundSystem)
            {
                Vector3 soundPosition = new Vector3(1f, 1f, 0.01f);
                soundEventSender.SendOrUpdateContinuousSound(timeoutSoundID, soundPosition);
            }


        }

    }

    void PlayWinGameSound()
    {
        if (soundEventSender != null)
        {
            if (Controller.enableNewSoundSystem)
            {
                Vector3 soundPosition = new Vector3(1f, 1f, 0.01f);
                soundEventSender.SendOrUpdateContinuousSound(winGameSoundID, soundPosition);
            }

        }
    }

    public void SetWinConditionMet()
    {
        winConditionMet = true;
        // Debug.Log("[INFO] Win condition met has been set.");
    }
}
