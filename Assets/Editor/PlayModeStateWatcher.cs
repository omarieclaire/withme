using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class PlayModeStateWatcher
{
    static PlayModeStateWatcher()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("Play mode started");
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Exiting Play mode. Stopping background music and player sounds...");

            // Fetch the instance of your GameManager and stop background music and player sounds
            GameManager gameManager = Object.FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.StopAllSoundsOnPlayExit(); // This will stop both background and player sounds
            }
        }
    }
}
