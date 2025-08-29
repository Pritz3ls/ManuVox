using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomSceneManager : MonoBehaviour{
    public static CustomSceneManager instance;
    [SerializeField] private Animator sceneLoaderAnimator;
    [SerializeField] private Image loaderImage;
    private void Awake() {
        if(instance == null){
            instance = this;
        }else{Destroy(gameObject);}

        // Don't destroy 
        DontDestroyOnLoad(gameObject);
    }

    public void StartLoadScene(string sceneName, float delay = 0, CustomFillOrigin newOrigin = CustomFillOrigin.Left){
        // LoadScene
        StartCoroutine(LoadSceneAsync(sceneName, delay, newOrigin));
    }
    IEnumerator LoadSceneAsync(string value, float delay = 0, CustomFillOrigin origin = CustomFillOrigin.Left){
        loaderImage.fillOrigin = origin == CustomFillOrigin.Left ? 0 : 1;
        sceneLoaderAnimator.Play("In");
        // Create a delay before starting to load the next scene
        yield return new WaitForSeconds(delay);
        
        // Async operation, load the scene sync to the CPU ticks
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(value);
        // Play the loader animation for transitioning in

        // Check the progress of the scene loading
        while (!loadOperation.isDone){
            yield return null;
        }
        
        // Give breathing room after loading the scene async
        yield return new WaitForSeconds(1f);
        // Transition the loader animator
        sceneLoaderAnimator.Play("Out");
    }
}
public enum CustomFillOrigin{
    Left,
    Right
}