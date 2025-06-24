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
    [Header("Core Component")]
    [SerializeField] private BaseRunner runner;
    [SerializeField] private SwitchCamera cameraSwitcher;
    // Set the default function mode to CameraMode 
    [SerializeField] private FunctionMode mode = FunctionMode.CameraMode;

    [Header("Master Mode")]
    [SerializeField] private GameObject modeCamera;
    [SerializeField] private GameObject modeReference;

    [Header("Camera Components")]
    [SerializeField] private TextMeshProUGUI screenText;

    [Header("Reference Components")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private GameObject emptySearchResultTextObj;
    [SerializeField] private GameObject searchParent;
    [SerializeField] private GameObject searchResultPrefab;

    // private StringBuilder sb = new StringBuilder();

    private void Start() {
        instance = this;
    }
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

        // Open Settings
        public void OpenAppSettings(){}
    #endregion

    // Camera Mode Functions only
    #region Camera-Mode-Functions
        public void Button_SwitchCamera(){
            cameraSwitcher.ChangeCamera();
        }
        public void Text_OnScreenText(string text){
            screenText.text += $"\n{text}";
        }
    #endregion

    // Reference Mode Functions only
    #region Reference-Mode-Functions
        public void SearchGesture(){
            List<Gesture> results = GestureLibrary.instance.SearchGestureByName(searchInputField.text);
            
            emptySearchResultTextObj.SetActive(false);
            // Return if the results were empty
            if(results.Count <= 0){
                // Change this to a text obj instead if there's no result
                emptySearchResultTextObj.SetActive(true);
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
                GameObject instance = Instantiate(searchResultPrefab, searchParent.transform.position, Quaternion.identity);

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

            // gestureViewObj.SetActive(true);
            // gestureViewText.text = gestureData.phraseOrWord;
            // gestureViewImage.sprite = gestureData.referenceImage;
        }
        public void CloseGestureView(){
            // gestureViewObj.SetActive(false);
        }
    #endregion

    #region Settings
        // Change On Screen Text Size base on 5 Size Presets
        public void ChangeOnScreenScreenText(OnScreenTextPresets textSizePresets){
            switch (textSizePresets){
                case OnScreenTextPresets.Tiny:
                    screenText.fontSize = 8.5f;
                break;
                case OnScreenTextPresets.Small:
                    screenText.fontSize = 13;
                break;
                case OnScreenTextPresets.Normal:
                    screenText.fontSize = 16;
                break;
                case OnScreenTextPresets.Big:
                    screenText.fontSize = 21;
                break;
                case OnScreenTextPresets.Large:
                    screenText.fontSize = 26;
                break;
            }
        }
        // Will add a popup option for this, but first i need to explore how to pass a function inside a parameter
        public void ExitApplication(){
            Application.Quit();
        }
    #endregion

    #region Miscellaneous
        public string GetBasicAppVersion(){
            return $"ManuVox-{Application.version}-{Application.buildGUID}";
        }
        public string GetFullAppVersionID(){
            return $"ManuVox-{Application.version}-{Application.companyName}-{Application.buildGUID}-Unity-{Application.unityVersion}";
        }
    #endregion
}
enum FunctionMode{
    CameraMode, ReferenceMode 
}
public enum OnScreenTextPresets{
    Tiny,
    Small,
    Normal,
    Big,
    Large
}