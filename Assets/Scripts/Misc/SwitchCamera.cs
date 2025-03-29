using System.Collections;
using System.Collections.Generic;
using Mediapipe.Unity.Sample;
using TMPro;
using UnityEngine;

public class SwitchCamera : MonoBehaviour{
    List<string> options = new List<string>();
    [SerializeField] private BaseRunner _baseRunner;
    [SerializeField] private TextMeshProUGUI sourceNamesText;
    [SerializeField] private TextMeshProUGUI logText;
    void OnEnable(){
        Start();
    }
    // Start is called before the first frame update
    void Start(){
        InitilizeOptions();
        InitializeSourceText();
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
    private void InitializeSourceText(){
        var imageSource = ImageSourceProvider.ImageSource;
        sourceNamesText.text = $"CurrentSource: {imageSource.sourceName}'\n";
        sourceNamesText.text += $"Out of {options.Count}";
    }

    
    public void ChangeCamera(){
        var imageSource = ImageSourceProvider.ImageSource;
        if(options.Count == 0) return;

        logText.text += $"\nAvailable Sources: {options.Count}";
        logText.text += $"\nCurrent Source: {imageSource.sourceName}";

        for (int i = 0; i < options.Count; i++){
            if(imageSource.sourceName != options[i]){
                logText.text += $"\nSwitched to {options[i]}";
                
                imageSource.SelectSource(i);
                RestartBaseRunnger(true);
                
                break;
            }
        }

        InitializeSourceText();
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
