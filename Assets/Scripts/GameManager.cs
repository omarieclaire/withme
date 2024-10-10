using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject TheOracleOfAll;
    public float gameDuration = 60f;
    public float waitTime = 3f;
    public int retryCount = 3;
    public float retryDelay = 0.5f;
    private float gameTimer;
    private bool gameEnded = false;

    private bool isFading = false;  // Track if a fade is in progress

    private float fadeInSpeed = 10f;

    private float fadeOutSpeed = 10f;

    public Controller controller;

    // public DotGameController dotGameController;  

    private bool musicPlayed = false;
    private Vector3 defaultSoundPosition = new Vector3(1f, 1f, 0.01f);

    public GameObject sphereObject;
    public Material sphereMaterial;
    public AudioPlayer audioPlayer;
    public SoundEventSender soundEventSender;
    public AudioSource BGMusicSource;
    public AudioClip BGMusicClip;
    public AudioClip timeoutClip;
    public AudioClip winGameClip;

    public string timeoutSoundID = "timeoutSound";
    public string winGameSoundID = "winGameSound";
    private bool winConditionMet = false;

    public bool isMusicMuted = false;

    private string[] playerSoundIDs = { "p0", "p1", "p2", "p3", "p4", "p5", "p6", "p7", "p8", "p9", "p10" };

    void Start()
    {
        if (sphereObject != null)
        {
            sphereMaterial = sphereObject.GetComponent<MeshRenderer>().material;
            // Debug.Log("[INFO] Sphere material assigned successfully.");
        }
        else
        {
            // Debug.LogError("[ERROR] Sphere object is not assigned!");
        }

        if (TheOracleOfAll != null)
        {
            controller = TheOracleOfAll.GetComponent<Controller>();
        }

        if (Controller.enableOldSoundSystem && BGMusicSource != null && BGMusicClip != null)
        {
            BGMusicSource.clip = BGMusicClip;
            BGMusicSource.loop = true;
            if (!isMusicMuted)
            {
                BGMusicSource.Play();
            }
        }
        else
        {
            // Debug.LogError("AudioSource or BGMusicClip is missing.");
        }
        StartCoroutine(TryStartBackgroundMusicWithRetries());
        gameTimer = gameDuration;
        StartCoroutine(FadeIn(fadeInSpeed));
    }


    public void StopAllSoundsOnPlayExit()
    {
        StopBackgroundMusic();
        StopAllPlayerSounds();
    }

    public void StopBackgroundMusic()
    {
        if (soundEventSender != null)
        {
            if (Controller.enableNewSoundSystem)
            {
                string soundID = GetSoundIDForCurrentScene();
                soundEventSender.StopContinuousSound(soundID, defaultSoundPosition);
            }
        }
        if (BGMusicSource != null)
        {
            BGMusicSource.Stop(); // Stop background music
        }
    }

    private void StopAllPlayerSounds()
    {
        if (soundEventSender != null)
        {
            if (Controller.enableNewSoundSystem)
            {
                foreach (var playerSoundID in playerSoundIDs)
                {
                    soundEventSender.StopContinuousSound(playerSoundID, defaultSoundPosition);
                }
            }
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
                EndGame(false);  // Timeout scenario
            }
            else if (winConditionMet)
            {
                EndGame(true);  // Win scenario
            }
        }

        // Scene change inputs using number keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) { LoadSceneByIndex(1); }  // withme
        if (Input.GetKeyDown(KeyCode.Alpha2)) { LoadSceneByIndex(2); }  // steer
        if (Input.GetKeyDown(KeyCode.Alpha3)) { LoadSceneByIndex(3); }  // hug
        if (Input.GetKeyDown(KeyCode.Alpha4)) { LoadSceneByIndex(4); }  // kali
        if (Input.GetKeyDown(KeyCode.Alpha5)) { LoadSceneByIndex(5); }  // MimicShape
        if (Input.GetKeyDown(KeyCode.Alpha6)) { LoadSceneByIndex(6); }  // share
        if (Input.GetKeyDown(KeyCode.Alpha7)) { LoadSceneByIndex(7); }  // stickTogether
        if (Input.GetKeyDown(KeyCode.Alpha8)) { LoadSceneByIndex(8); }  // Herd
        if (Input.GetKeyDown(KeyCode.Alpha9)) { LoadSceneByIndex(9); }  // buddy

        // Manual fade and scene switching controls
        if (Input.GetKeyDown(KeyCode.F)) // Fade in manually
        {
            StartCoroutine(FadeIn(fadeInSpeed));
        }

        if (Input.GetKeyDown(KeyCode.G)) // Fade out manually
        {
            StartCoroutine(FadeOut(fadeOutSpeed));
        }

        if (Input.GetKeyDown(KeyCode.N)) // Manually switch to next scene
        {
            StartCoroutine(FadeOutAndLoadNextScene(fadeOutSpeed));
        }

        if (Input.GetKeyDown(KeyCode.P)) // Manually load previous scene
        {
            LoadPreviousScene();
        }

        if (Input.GetKeyDown(KeyCode.L)) // Manually reload current scene
        {
            ReloadCurrentScene();
        }
    }
    public void ReloadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            StopAllCoroutines();  // Stop any ongoing fade effect
            SceneManager.LoadScene(sceneIndex);
            StartCoroutine(FadeIn(fadeInSpeed));  // Optionally add fade-in for the new scene
        }
        else
        {
            // Debug.LogWarning($"[INFO] Scene index {sceneIndex} is out of range.");
        }
    }

    IEnumerator TryStartBackgroundMusicWithRetries()
    {
        int attempts = 0;
        string soundID = GetSoundIDForCurrentScene();

        while (attempts < retryCount && !musicPlayed)
        {
            attempts++;

            if (soundEventSender != null)
            {
                if (Controller.enableNewSoundSystem)
                {
                    Vector3 musicPosition = new Vector3(1f, 1f, 0.01f);
                    soundEventSender.SendOrUpdateContinuousSound(soundID, musicPosition);
                }
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

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;

        if (isMusicMuted)
        {
            BGMusicSource.Pause(); // Pause the music if muted
            // Debug.Log("[INFO] Background music muted.");
        }
        else
        {
            BGMusicSource.Play(); // Resume music if unmuted
            // Debug.Log("[INFO] Background music unmuted.");
        }
    }
    public void EndGame(bool isWinConditionMet)
    {
        if (isWinConditionMet)
        {
            HandleWinScenario();  // Call win logic
        }
        else
        {
            HandleTimeoutScenario();  // Call timeout logic
        }
    }

    public IEnumerator FadeOutAndLoadNextScene(float fadeOutDuration)
    {
        // Wait for the fade-out to complete
        yield return StartCoroutine(FadeOut(fadeOutDuration));

        // Once the fade-out is complete, load the next scene
        LoadNextScene();
    }


    public IEnumerator FadeOut(float duration)
    {
        if (isFading) yield break;  // Prevent multiple fades from starting at the same time

        // Debug.Log("[INFO] Starting FadeOut with duration: " + duration);
        isFading = true;
        float currentTime = 0;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float fadeValue = Mathf.Lerp(0, 1, currentTime / duration);  // Fade from open (0) to closed (1)
            sphereMaterial.SetFloat("_Fade", fadeValue);  // Update the fade value in the material
            // Debug.Log("[INFO] FadeOut in progress. Fade value: " + fadeValue);
            yield return null;
        }

        // Ensure it's fully faded out
        sphereMaterial.SetFloat("_Fade", 1);
        // Debug.Log("[INFO] FadeOut complete. Fully faded.");

        isFading = false;  // Allow future fade actions
    }


    public IEnumerator FadeIn(float duration)
    {
        if (isFading) yield break;  // Don't allow new fade while one is in progress

        // Debug.Log("[INFO] Starting FadeIn (FadeUV_Pulse) with duration: " + duration);
        isFading = true;  // Set fade flag
        float currentTime = 0;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float fadeValue = Mathf.Lerp(1, 0, currentTime / duration);  // Fade from closed (1) to open (0)
            sphereMaterial.SetFloat("_Fade", fadeValue);  // Set the fade property

            // Debug.Log("[INFO] FadeIn in progress. Fade value: " + fadeValue);
            yield return null;
        }

        // Ensure it's fully faded in at the end
        sphereMaterial.SetFloat("_Fade", 0);
        // Debug.Log("[INFO] FadeIn complete. Fully visible.");

        isFading = false;  // Reset fade flag
    }



    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextSceneIndex);

        // Fade in the new scene over 5 seconds
        StartCoroutine(FadeIn(fadeInSpeed));
    }

    public void LoadPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = (currentSceneIndex - 1 + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(previousSceneIndex);

        // Fade in the previous scene over 5 seconds
        StartCoroutine(FadeIn(fadeInSpeed));
    }


    void PlayTimeoutSound()
    {
        if (soundEventSender != null)
        {
            if (Controller.enableOldSoundSystem && timeoutClip != null)
            {
                audioPlayer.Play(timeoutClip);
            }
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
            if (Controller.enableOldSoundSystem && winGameClip != null)
            {
                audioPlayer.Play(winGameClip);
            }
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
    }









    public void HandleWinScenario()
{
    if (!gameEnded)
    {
        gameEnded = true;

        // Play the win sound
        PlayWinGameSound();

        // Check if the controller has a treeController (for games like DotGameController)
        if (controller && controller.treeController != null)
        {
            controller.treeController.StartGrowingTree(10f, 20f, 0.5f, 3f);
        }
        else
        {
            Debug.LogWarning("TreeController is not present in this game or controller is not DotGameController.");
        }

        // Start the scaling and fading routine
        StartCoroutine(ScaleTreeAndFadeRoutine(10f, BGMusicSource, false));
    }
}



   public void HandleTimeoutScenario()
{
    if (!gameEnded)
    {
        gameEnded = true;

        // Play the timeout sound
        PlayTimeoutSound();

        // Check if the controller has a treeController (for games like DotGameController)
        if (controller && controller.treeController != null)
        {
            controller.treeController.ChangeMaterialToTimeout();
            controller.treeController.StartGrowingTree(10f, 50f, 0.5f, 3f);  // Adjust as necessary
        }
        else
        {
            Debug.LogWarning("TreeController is not present in this game or controller is not DotGameController.");
        }

        // Start the scaling and fading routine
        StartCoroutine(ScaleTreeAndFadeRoutine(10f, BGMusicSource, true));
    }
}


   public IEnumerator ScaleTreeAndFadeRoutine(float scaleDuration, AudioSource backgroundMusic, bool isTimeout)
{
    float currentTime = 0;
    float fadeOutStartTime = scaleDuration - fadeOutSpeed;  // Start the fade-out right before the scale finishes

    // Start scaling the tree (handled by DotGameController)
    while (currentTime < scaleDuration)
    {
        currentTime += Time.deltaTime;

        // Start fade-out when the scaleDuration is nearly complete
        if (currentTime >= fadeOutStartTime && !isFading)
        {
            StartCoroutine(FadeOut(fadeOutSpeed));  // Start visual fade-out
        }

        yield return null;
    }

    // Ensure the fade-out finishes before proceeding
    yield return new WaitForSeconds(fadeOutSpeed);  // Wait for the fade-out duration to complete

    // Stop background music after fade-out
    if (backgroundMusic != null)
    {
        backgroundMusic.Stop();
    }

    // Once the fade-out and music stop, load the next scene
    LoadNextScene();
}


}
