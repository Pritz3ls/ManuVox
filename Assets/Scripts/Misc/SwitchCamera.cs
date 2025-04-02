using System.Collections;
using System.Collections.Generic;
using Mediapipe.Unity.Sample;
using TMPro;
using UnityEngine;

public class SwitchCamera : MonoBehaviour{
    List<string> options = new List<string>();
    [SerializeField] private BaseRunner _baseRunner;
    void OnEnable(){
        Start();
    }
    // Start is called before the first frame update
    void Start(){
        InitilizeOptions();
    }
    private void InitilizeOptions(){
        options.Clear();

        var imageSource = ImageSourceProvider.ImageSource;
        var sourceNames = imageSource.sourceCandidateNames;
        

        if (sourceNames == null){
            return;
        }

        options.AddRange(sourceNames);
    }
    public void ChangeCamera(){
        var imageSource = ImageSourceProvider.ImageSource;
        if(options.Count == 0) return;
        for (int i = 0; i < options.Count; i++){
            if(imageSource.sourceName != options[i]){
                
                imageSource.SelectSource(i);
                RestartBaseRunnger(true);
                
                break;
            }
        }
    }

    public void RestartBaseRunnger(bool forceRestart = false){
        if (_baseRunner == null){
          return;
        }

        if (forceRestart){
            if (_baseRunner != null){
              _baseRunner.Play();
            }
        }else{
            if (_baseRunner != null){
              _baseRunner.Resume();
            }
        }
    }
}
