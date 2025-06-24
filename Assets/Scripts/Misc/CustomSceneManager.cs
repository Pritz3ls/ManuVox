using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour{
    public static CustomSceneManager instance;
    [SerializeField] private Animator sceneLoaderAnimator;
    private void Awake() {
        if(instance == null){
            instance = this;
        }else{Destroy(gameObject);}

        // Don't destroy 
        DontDestroyOnLoad(gameObject);
    }

    public void StartLoadingScene(string sceneName, float delay = 0){
        // LoadScene
        StartCoroutine(LoadSceneAsync(sceneName, delay));
    }
    IEnumerator LoadSceneAsync(string value, float delay = 0){
        // Create a delay before starting to load the next scene
        yield return new WaitForSeconds(delay);
        
        // Async operation, load the scene sync to the CPU ticks
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(value);
        // Play the loader animation for transitioning in
        sceneLoaderAnimator.Play("In");

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
