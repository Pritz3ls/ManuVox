using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.Sample;
using TMPro;

public class CameraManager : MonoBehaviour{
    public static CameraManager instance;

    [Header("Core Component")]
    [SerializeField] private UI_CalibrationEvent calibrationEvent;
    [SerializeField] private BaseRunner runner;
    [SerializeField] private SwitchCamera cameraSwitcher;

    [Header("Camera Components")]
    [SerializeField] private Toggle ttsToggle;
    [SerializeField] private TextMeshProUGUI screenText;
    [SerializeField] private TMP_Dropdown onScreenDropdown;
    [SerializeField] private TextMeshProUGUI versionIDText;
    [SerializeField] private GameObject settingsPopup;
    [SerializeField] private GameObject speedPopup;
    
    // Start is called before the first frame update
    void Start(){
        instance = this;
        InitializeCameraScene();
        InitializeVersionIDText();
    }

    void InitializeCameraScene(){
        ChangeOnScreenScreenText();
        ChangeTTSToggle();

        if(!CheckTutorialFinishedCamera()) return;
        calibrationEvent.SetupSpeedCalibration();
    }

    bool CheckTutorialFinishedCamera() => PlayerPrefsHandler.instance.GetTutorialState() == 1 ? true : false;

    #region Camera
        public void LoadReferenceScene(){
            CustomSceneManager.instance.StartLoadScene("Android-Shipping-Reference", 1, CustomFillOrigin.Left);
        }
        public void Text_OnScreenText(string text){
            screenText.text += $"\n{text}";
        }
        public void StartCalibrationAgain(){
            calibrationEvent.SetupSpeedCalibration();
        }
        public void SwitchCamera(){
            cameraSwitcher.ChangeCamera();
        }
        // Open Settings
        public void OpenAppSettings(){
            settingsPopup.SetActive(true);
            GestureRecognizer.instance.SetRecognizerState(false);
        }
        public void CloseAppSettings(){
            settingsPopup.SetActive(false);
            GestureRecognizer.instance.SetRecognizerState(true);
        }
    #endregion


    #region Settings
        // Change On Screen Text Size base on 5 Size Presets
        public void ChangeOnScreenScreenText(){
            OnScreenTextPresets preset = (OnScreenTextPresets)onScreenDropdown.value;
            switch (preset){
                case OnScreenTextPresets.Tiny:
                    screenText.fontSize = 36;
                break;
                case OnScreenTextPresets.Small:
                    screenText.fontSize = 48;
                break;
                case OnScreenTextPresets.Normal:
                    screenText.fontSize = 52;
                break;
                case OnScreenTextPresets.Big:
                    screenText.fontSize = 64;
                break;
                case OnScreenTextPresets.Large:
                    screenText.fontSize = 72;
                break;
            }
            PlayerPrefsHandler.instance.SavePref_OSSize(onScreenDropdown.value);
        }
        private void ChangeTTSToggle(){
            ttsToggle.isOn = PlayerPrefsHandler.instance.GetTTSState();
        }
        public void SaveTTSToggle(){
            PlayerPrefsHandler.instance.SavePref_TTS(ttsToggle.isOn);
        }
        // Check for application updates
        public void CheckUpdate(){
            // Call GestureLibrary to check for any new updates
            GestureLibrary.instance.CheckForUpdatesAsync();
        }
        // Reset App Data
        public void ResetApplicationData(){
            System.Action sub = () => {
                Debug.LogWarning("Reset the application data");
                PlayerPrefsHandler.instance?.DeleteAllUserData();
            };
            PopupsManager.instance.Popup(PopupType.Warning, "Are you sure you want to reset your app data?", new PopupEvent(
                "Yes", sub,
                "No", null
            ));
        }
        // Will add a popup option for this, but first i need to explore how to pass a function inside a parameter
        public void ExitApplication(){
            System.Action sub = () => {
                Debug.LogWarning("Application has been quit");
                Application.Quit();
            };
            PopupsManager.instance.Popup(PopupType.Warning, "Are you sure you want to quit?", new PopupEvent(
                "Yes", sub,
                "No", null
            ));
        }
        // Initialize the app verion ID text
        private void InitializeVersionIDText(){
            versionIDText.SetText($"App Version\n{VersionController.GetBasicAppVersion()}");
        }
    #endregion
}
public enum OnScreenTextPresets{
    Tiny,
    Small,
    Normal,
    Big,
    Large
}
