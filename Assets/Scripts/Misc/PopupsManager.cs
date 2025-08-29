using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupsManager : MonoBehaviour
{
    public static PopupsManager instance;

    [Header("UI Elements")]
    [SerializeField] private GameObject popupObj;
    [SerializeField] private Image popupContainerImage;
    [SerializeField] private TextMeshProUGUI popupTitleTMP;
    [SerializeField] private TextMeshProUGUI popupMessageTMP;
    [SerializeField] private TextMeshProUGUI popupButtonTMP;
    [SerializeField] private Button firstButton;
    [SerializeField] private Button secondButton;

    private System.Action FirstActionEvent;
    private System.Action SecondActionEvent;


    [Header("Components")]
    [SerializeField] private Color popupPrimaryColor;
    [SerializeField] private Color popupSecondaryColor;
    [SerializeField] private PopupDesign popInfoDesign;
    [SerializeField] private PopupDesign popWarningDesign;
    [SerializeField] private PopupDesign popErrorDesign;

    /*
        This is a test for registering pass functions
        public delegate void DelegateMethod();
        public DelegateMethod callMethod;
        public UnityEvent uevent;
        private void Start() {
            callMethod += Test;
            callMethod += Call;
            uevent.AddListener(() => callMethod());
            uevent.Invoke();
        }
        void Call(){
            Debug.LogWarning("This is a Call Method");
        }
        void Test(){
            Debug.LogWarning("This is a Test Method");
        }
    */

    // Run first to set the instance
    private void Awake() {
        if(instance != null){
            Destroy(gameObject);
        }

        // Set the instance to this
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Q)){
            System.Action sub = () => {
                Debug.LogWarning("Testing of the passing");
            };
            Popup(PopupType.Info, "This is a info popup message", new PopupEvent("Okay", sub));
        }else if(Input.GetKeyDown(KeyCode.W)){
            System.Action sub1 = () => {
                Debug.LogWarning("First button invocation: Hello");
            };
            System.Action sub2 = () => {
                Debug.LogWarning("Second button invocation: WORLD");
            };
            Popup(PopupType.Warning, "This is a warning popup message", new PopupEvent(
                "Hello", sub1,
                "World", sub2
            ));
        }else if(Input.GetKeyDown(KeyCode.E)){
            Popup(PopupType.Error, "This is a error popup message,\n::ERR_NO_CAMERA_ACS");
        }
    }

    // Popup events, this will open up a popup window
    public void Popup(PopupType type, string message, PopupEvent popupEvent = null){
        // Close the popup if there was
        ClosePopup();

        // Set the popup text color to the predetermined design choice
        popupTitleTMP.color = type == PopupType.Info ? popupPrimaryColor : popupSecondaryColor;
        popupMessageTMP.color = type == PopupType.Info ? popupPrimaryColor : popupSecondaryColor;

        // Change the container image sprite using the popup type
        switch (type){
            case PopupType.Info:
                popupContainerImage.sprite = popInfoDesign.containerSprite;
                popupButtonTMP.color = popupSecondaryColor;
                
                // Buttons
                firstButton.image.sprite = popInfoDesign.buttonSprite;
                secondButton.image.sprite = popInfoDesign.buttonSprite;
                firstButton.GetComponentInChildren<TextMeshProUGUI>().color = popupSecondaryColor;
                secondButton.GetComponentInChildren<TextMeshProUGUI>().color = popupSecondaryColor;
            break;
            case PopupType.Warning:
                popupContainerImage.sprite = popWarningDesign.containerSprite;
                popupButtonTMP.color = popupPrimaryColor;
                
                // Buttons
                firstButton.image.sprite = popWarningDesign.buttonSprite;
                secondButton.image.sprite = popWarningDesign.buttonSprite;
                firstButton.GetComponentInChildren<TextMeshProUGUI>().color = popupPrimaryColor;
                secondButton.GetComponentInChildren<TextMeshProUGUI>().color = popupPrimaryColor;
            break;
            case PopupType.Error:
                popupContainerImage.sprite = popErrorDesign.containerSprite;
                popupButtonTMP.color = popupPrimaryColor;

                // Buttons
                firstButton.image.sprite = popErrorDesign.buttonSprite;
                secondButton.image.sprite = popErrorDesign.buttonSprite;
                firstButton.GetComponentInChildren<TextMeshProUGUI>().color = popupPrimaryColor;
                secondButton.GetComponentInChildren<TextMeshProUGUI>().color = popupPrimaryColor;
            break;
        }

        if(popupEvent != null){
            // Register if there's a passed first action in the parameter but not the second
            if(popupEvent.firstEvent != null && popupEvent.secondEvent == null){
                SubscribeFirstAction(popupEvent.firstEvent);

                // Add Event Listeners
                firstButton.onClick.AddListener(CallFirstAction);
                firstButton.GetComponentInChildren<TextMeshProUGUI>().SetText(popupEvent.firstButtonText);
                // Fallback for the second button
                secondButton.onClick.AddListener(ClosePopup);
                secondButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Close");
                
                firstButton.gameObject.SetActive(true);
                secondButton.gameObject.SetActive(true);
            // Register if there's a passed both action in the parameter
            }else if(popupEvent.firstEvent != null && popupEvent.secondEvent != null){
                SubscribeFirstAction(popupEvent.firstEvent);
                SubscribeSecondAction(popupEvent.secondEvent);
                
                // Add Event Listener for the first button with the first action
                firstButton.onClick.AddListener(CallFirstAction);
                firstButton.GetComponentInChildren<TextMeshProUGUI>().SetText(popupEvent.firstButtonText);
                // Add Event Listener for the second button with the first action
                secondButton.onClick.AddListener(CallSecondAction);
                secondButton.GetComponentInChildren<TextMeshProUGUI>().SetText(popupEvent.secondButtonText);
                
                firstButton.gameObject.SetActive(true);
                secondButton.gameObject.SetActive(true);
            }
        }else{
            // Add Event Listeners
            firstButton.onClick.AddListener(ClosePopup);
            firstButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Close");
            
            firstButton.gameObject.SetActive(true);
        }

        // Set the text message to the given value on the parameter
        popupTitleTMP.text = type.ToString();
        popupMessageTMP.text = message;
        popupObj.SetActive(true);
    }
    // Close the popup window
    public void ClosePopup(){
        popupObj?.SetActive(false);

        firstButton.gameObject.SetActive(false);
        secondButton.gameObject.SetActive(false);
        
        UnsubscribeAll();
    }

    #region Popup Events
        private void CallFirstAction(){
            FirstActionEvent?.Invoke();
            ClosePopup();
        }
        private void CallSecondAction(){
            SecondActionEvent?.Invoke();
            ClosePopup();
        }

        private void SubscribeFirstAction(System.Action sub){
            FirstActionEvent += sub;
        }
        private void SubscribeSecondAction(System.Action sub){
            SecondActionEvent += sub;
        }

        private void UnsubscribeAll(){
            FirstActionEvent = null;
            SecondActionEvent = null;

            firstButton.onClick.RemoveAllListeners();
            secondButton.onClick.RemoveAllListeners();
        }
    #endregion
}
[System.Serializable]
public class PopupDesign{
    public Sprite containerSprite;
    public Sprite buttonSprite;
}
public enum PopupType{
    Info, Warning, Error
}

public class PopupEvent{
    public string firstButtonText; 
    public System.Action firstEvent; 
    public string secondButtonText; 
    public System.Action secondEvent;

    public PopupEvent(string _fButText, System.Action _fEvent, string _sButText = "", System.Action _sEvent = null){
        this.firstButtonText = _fButText;
        this.firstEvent = _fEvent;
        this.secondButtonText = _sButText;
        this.secondEvent = _sEvent;
    }
}