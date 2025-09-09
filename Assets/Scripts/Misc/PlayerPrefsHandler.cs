using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsHandler : MonoBehaviour{
    public static PlayerPrefsHandler instance; 

    /* Miscellenaous Preferences */
    private bool _OnboardingFinished = false;
    // Tutorials should be separated to two elements,
    /*
        0 - Null
        1 - Camera Tutorial Finished
        2 - Reference Tutorial Finished
    */
    private int _TutorialFinished = 0;

    /* Settings Preferences */
    private bool _TTSState = false;
    private int _OnScreenTextSize = 2;
    
    private void Awake() {
        // Set the instance to this gameobject, destroy if the scene already has it
        if(instance == null){
            instance = this;
        }else{
            Destroy(gameObject);
        }
        // Dont destory this library object, so we can use it anywhere on the scene
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start(){
        InitializeMisc();
        InitializeSettings();
    }

    /* 
        Global Naming Convention
        GLOBAL_USER_ONBOARDING_FINISHED
    */
    void InitializeSettings(){
        _OnScreenTextSize = PlayerPrefs.GetInt("GLOBAL_USER_PREFS_OSTEXT_SIZE", 2);
        _TTSState = PlayerPrefs.GetInt("GLOBAL_USER_PREFS_TTS_STATE", 1) == 1 ? true : false;
    }
    void InitializeMisc(){
        _OnboardingFinished = PlayerPrefs.GetInt("GLOBAL_USER_ONBOARDING_FINISHED", 0) == 0 ? false : true;
        _TutorialFinished = PlayerPrefs.GetInt("GLOBAL_USER_TUTORIAL_FINISHED", 0);
    }

    #region Save Preferences
        public void SavePref_TTS(bool value){
            PlayerPrefs.SetInt("GLOBAL_USER_PREFS_TTS_STATE", value ? 1 : 0);
            _TTSState = value;
        }
        public void SavePref_OSSize(int value){
            PlayerPrefs.SetInt("GLOBAL_USER_PREFS_OSTEXT_SIZE", value);
            _OnScreenTextSize = value;
        }
    #endregion
    #region Save Misc
        public void SaveMisc_Onboarding(bool value){
            PlayerPrefs.SetInt("GLOBAL_USER_ONBOARDING_FINISHED", value ? 1 : 0);
            _OnboardingFinished = value;
        }
        public void SaveMisc_Tutorial(int value){
            PlayerPrefs.SetInt("GLOBAL_USER_TUTORIAL_FINISHED", value);
            _TutorialFinished = value;
        }
    #endregion

    #region Public Functions
        public bool GetOnboardingState(){
            return _OnboardingFinished;
        }
        public int GetTutorialState(){
            return _TutorialFinished;
        }
        public bool GetTutorialFinishedCamera => _TutorialFinished >= 1 ? true : false;
        public bool GetTutorialFinishedReference => _TutorialFinished >= 2 ? true : false;

        public bool GetTTSState(){
            return _TTSState;
        }
        public int GetOnScreenTextSize(){
            return _OnScreenTextSize;
        }
        public void DeleteAllUserData(){
            PlayerPrefs.DeleteAll();
        }
    #endregion
}
