using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.Sample;
using TMPro;
using System.Text;

public class CameraManager : MonoBehaviour{
    public static CameraManager instance;

    [Header("Core Component")]
    [SerializeField] private UI_CalibrationEvent calibrationEvent;
    [SerializeField] private BaseRunner runner;
    [SerializeField] private SwitchCamera cameraSwitcher;

    [Header("Camera Components")]
    [SerializeField] private Toggle ttsToggle;
    [SerializeField] private TextMeshProUGUI contextText;
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

        if(!PlayerPrefsHandler.instance.GetTutorialFinishedCamera) return;
        calibrationEvent.SetupSpeedCalibration();
    }

    #region Camera
        public void LoadReferenceScene(){
            CustomSceneManager.instance.StartLoadScene("Android-Shipping-Reference", 1, CustomFillOrigin.Left);
        }
        public void Text_OnScreenText(string text){
            StringBuilder sb = new StringBuilder();
            sb.Append($"{text} ");
            screenText.text += sb.ToString();
        }
        private bool TextContainSymbols(string text){
            foreach(char c in text){
                return char.IsSymbol(c);
            }
            return false;
        }
        public void Text_ContextScreenText(string text){
            if(!contextText.isActiveAndEnabled){
                contextText.gameObject.SetActive(true);
            }
            contextText.text = $"<mark=#000000A0>{text}<color=#00000000>";
        }
        public void Text_ClearContextText(){
            contextText.gameObject.SetActive(false);
            contextText.text = string.Empty;
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
            int desiredFontSize = 0;
            switch (preset){
                case OnScreenTextPresets.Tiny:
                    desiredFontSize = 36;
                break;
                case OnScreenTextPresets.Small:
                    desiredFontSize = 48;
                break;
                case OnScreenTextPresets.Normal:
                    desiredFontSize = 52;
                break;
                case OnScreenTextPresets.Big:
                    desiredFontSize = 64;
                break;
                case OnScreenTextPresets.Large:
                    desiredFontSize = 72;
                break;
            }

            // Set the font size on both onscreen text using the selected font size
            screenText.fontSize = desiredFontSize;
            contextText.fontSize = desiredFontSize / 1.25f;
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
