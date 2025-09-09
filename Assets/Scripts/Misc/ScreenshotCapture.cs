using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenshotCapture : MonoBehaviour {
    void Update(){
        if (Input.GetKeyDown(KeyCode.S)) { // Or your desired key
            // Takes a screenshot and saves it to the project folder
            ScreenCapture.CaptureScreenshot($"{SceneManager.GetActiveScene().name}.png"); 
            Debug.Log("Screenshot taken!");
        }
    }
}