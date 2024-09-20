using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Tooltip("Duration of the game before switching scenes (in seconds)")]
    public float gameDuration = 60f;  // Time before the scene switches

    [Tooltip("Time to wait after win condition or timer runs out before switching scenes")]
    public float waitTime = 3f;  // You can set this in the Inspector

    private float gameTimer;
    private bool gameEnded = false;

    void Start()
    {
        gameTimer = gameDuration;
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
                EndGame();
            }
        }

        // Keypress to switch scenes manually
        if (Input.GetKeyDown(KeyCode.N)) LoadNextScene();
        if (Input.GetKeyDown(KeyCode.P)) LoadPreviousScene();
    }

    void EndGame()
    {
        gameEnded = true;
        Debug.Log("Game Over! Moving to the next scene in " + waitTime + " seconds.");
        Invoke("LoadNextScene", waitTime); // Wait for `waitTime` seconds before loading the next scene
    }

    void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextSceneIndex);
    }

    void LoadPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = (currentSceneIndex - 1 + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(previousSceneIndex);
    }
}
