using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class OnboardingManager : MonoBehaviour
{
    [Header("Global Components")]
    [SerializeField] private GameObject[] onboardingScreenObjects;
    [SerializeField] private GameObject onboardingAlreadyFinishedObject;
    
    [Header("T&C and Privacy Policy Components")]
    [SerializeField] private GameObject TAC_PPContainer;
    private TMP_Text _TACPPTMP;
    [SerializeField] private GameObject termsConditionObject;
    [SerializeField] private GameObject privacyPolicyObject;
    [SerializeField] private TextMeshProUGUI versionIDText;
    bool isFinalizing = false;

    // Private variables
    private int _onboardingCurrentSteps = 0;

    private void Awake() {
        _TACPPTMP = TAC_PPContainer.GetComponentInChildren<TMP_Text>();
        isFinalizing = false;
        InitializeVersionIDText();
    }

    #region Terms and Condtions and Privacy Policy Part
        private void OnEnable() {
            LinkHandlerTMP.OnClickedLinkEvent += OpenLinkObject;
        }
        void OnDisable(){
            LinkHandlerTMP.OnClickedLinkEvent -= OpenLinkObject;
        }
    #endregion

    private void Update() {
        // if() return;
        if(Input.GetKeyDown(KeyCode.Escape) && !isFinalizing){
            Debug.LogWarning("Escape entered.");
            if(termsConditionObject.activeInHierarchy || privacyPolicyObject.activeInHierarchy){
                termsConditionObject.SetActive(false);
                privacyPolicyObject.SetActive(false);
                return;
            }
            PreviousOnboardingStep();
        }
    }

    private void OpenLinkObject(string keyword){
        Debug.LogWarning(keyword);
        switch (keyword){
            case "terms": termsConditionObject.SetActive(true); break;
            case "privacy": privacyPolicyObject.SetActive(true); break;
        }
    }

    public void RequestCameraAccess(){
        #if PLATFORM_ANDROID
            if(!IsAccessGranted_Camera()){
                Permission.RequestUserPermission(Permission.Camera);
                return;
            }
        #endif
        NextOnboardingStep();
    }
    public void PreviousOnboardingStep(){
        // Decrement the progress
        if(_onboardingCurrentSteps != 0){
            _onboardingCurrentSteps--;
        }
        ActivateProcessObject(_onboardingCurrentSteps);
    }
    public void NextOnboardingStep(){
        if(PlayerPrefsHandler.instance.GetOnboardingState){
            Debug.LogWarning("The user has already finished onboarding process.");
            onboardingAlreadyFinishedObject.SetActive(true);
            StartCoroutine(Finished());
            return;
        }
        // Increment the progress
        if(_onboardingCurrentSteps < onboardingScreenObjects.Length){
            _onboardingCurrentSteps++;
        }
        ActivateProcessObject(_onboardingCurrentSteps);
    }
    private void ActivateProcessObject(int step){
        // Disable all screen objects first
        foreach (GameObject obj in onboardingScreenObjects){
            obj.SetActive(false);
        }
        // Enable only the specific screen with the current onboarding step
        onboardingScreenObjects[step].SetActive(true);
        
        // Object Related Functionality
        /*
            0 - Welcome Screen
            1 - Introductory
            2 - Permission
            3 - Finalization
        */
        if(step >= onboardingScreenObjects.Length-1){
            FinalizeApp(); // Finalized the app once the user reached the last step
        }
    }
    private void FinalizeApp(){
        // Playerprefs saving the onbaording process
        // Remember with naming convention strict, booleans to int
        /*
            Naming Convention
            GLOBAL_USER_ONBOARDING_FINISHED
            1 = means finished
            0 = means not finished
        */
        isFinalizing = true;
        PlayerPrefsHandler.instance.SaveMisc_Onboarding(true);
        StartCoroutine(Finished());
    }
    private IEnumerator Finished(){
        yield return new WaitForSecondsRealtime(1f); // Delay the load process to give time for saving playerprefs
        CustomSceneManager.instance.StartLoadScene("Android-Shipping-Recognition", 2, CustomFillOrigin.Right); // Load up the next scene, but will be changing this to a Global Scene Management
        // Debug.LogWarning("Player has finsished onboarding process, loading main scene");
    }
    private bool IsAccessGranted_Camera(){
        return Permission.HasUserAuthorizedPermission(Permission.Camera);
    }
    // Initialize the app verion ID text
    private void InitializeVersionIDText(){
        versionIDText.SetText($"{VersionController.GetBasicAppVersion()}");
    }
}
