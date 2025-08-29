using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mediapipe.Unity.Sample;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    // private StringBuilder sb = new StringBuilder();

    private void Start() {
        instance = this;
    }
    // Global Functions only
    #region Global
        // public void SwitchMode(){
        //     // // Switch the current mode to the other mode
        //     // mode = mode == FunctionMode.CameraMode ? FunctionMode.ReferenceMode : FunctionMode.CameraMode;
            
        //     // // Disable or Enable Mode GameObjects depending on the switchMode
        //     // modeCamera.SetActive(mode == FunctionMode.CameraMode ? true : false);
        //     // modeReference.SetActive(mode == FunctionMode.ReferenceMode ? true : false);

        //     // GestureRecognizer.instance.SetRecognizerState(mode == FunctionMode.CameraMode ? true : false);

        //     // // Re-Initialize the BaseRunner of MediaPipe depending on the Mode
        //     // if(mode == FunctionMode.CameraMode){
        //     //     runner.Play();
        //     // }else{
        //     //     runner.Stop();
        //     // }
        // }
    #endregion

    // Reference Mode Functions only
    #region Reference-Mode-Functions
        
    #endregion
}