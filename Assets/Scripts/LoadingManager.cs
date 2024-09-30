using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    void Update()
    {
        // Check if the Enter key is pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Start loading the main game scene
            StartGame();
        }
    }

    void StartGame()
    {
        // Load the main game scene (replace "MainScene" with the actual name of your main scene)
        SceneManager.LoadScene("MainScene");
    }
}
