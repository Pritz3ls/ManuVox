using System.Collections;
using System.Collections.Generic;
using Mediapipe.Unity.Sample;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Core Component")]
    [SerializeField] private BaseRunner runner;
    // Set the default function mode to CameraMode 
    [SerializeField] private FunctionMode mode = FunctionMode.CameraMode;

    [Header("Master Mode")]
    [SerializeField] private GameObject modeCamera;
    [SerializeField] private GameObject modeReference;

    [Header("Camera Components")]
    [SerializeField] private TextMeshProUGUI screenText;

    [Header("Reference Components")]
    [SerializeField] public TMP_InputField searchInputField;
    [SerializeField] public GameObject searchParent;
    [SerializeField] public GameObject searchResult;

    // Global Functions only
    #region Global
        public void SwitchMode(){
            // Switch the current mode to the other mode
            mode = mode == FunctionMode.CameraMode ? FunctionMode.ReferenceMode : FunctionMode.CameraMode;
            
            // Disable or Enable Mode GameObjects depending on the switchMode
            modeCamera.SetActive(mode == FunctionMode.CameraMode ? true : false);
            modeReference.SetActive(mode == FunctionMode.ReferenceMode ? true : false);

            GestureRecognizer.instance.SetRecognizerState(mode == FunctionMode.CameraMode ? true : false);

            // Re-Initialize the BaseRunner of MediaPipe depending on the Mode
            if(mode == FunctionMode.CameraMode){
                runner.Play();
            }else{
                runner.Stop();
            }
        }
    #endregion

    // Camera Mode Functions only
    #region Reference-Camera-Functions
        
    #endregion

    // Reference Mode Functions only
    #region Reference-Mode-Functions
        public void SearchGesture(){
            List<Gesture> results = GestureLibrary.instance.SearchGestureByName(searchInputField.text);
            
            // Return if the results were empty
            if(results.Count <= 0){
                Debug.Log("Sorry but there's no results.");
                return;
            }
            
            // Clear the current search results
            for (int i = 0; i < searchParent.transform.childCount; i++){
                Destroy(searchParent.transform.GetChild(i).gameObject);
            }

            // Iterate every results
            for (int i = 0; i < results.Count; i++){
                // Create an instance of the UI Object
                GameObject instance = Instantiate(searchResult, searchParent.transform.position, Quaternion.identity);

                // Set the instance parent to the UI Object
                instance.transform.SetParent(searchParent.transform);
                instance.transform.localScale = Vector3.one;
                
                // Set the name of the instance to the result name
                instance.transform.name = results[i].name;
                // Set the name inside the instance child component of TextMeshProUGUI
                instance.GetComponentInChildren<TextMeshProUGUI>().text = results[i].name;

                Gesture funcGesture = results[i];

                // Assign a listener for onClick event to view the gesture with the results (gesture) data
                instance.GetComponent<Button>().onClick.AddListener(() => ViewGesture(funcGesture));
            }
        }

        // For now, let's debug the option
        public void ViewGesture(Gesture gestureData){
            Debug.Log($"Viewing {gestureData.name}");
        }
    #endregion
}
public enum FunctionMode{
    CameraMode, ReferenceMode 
}
