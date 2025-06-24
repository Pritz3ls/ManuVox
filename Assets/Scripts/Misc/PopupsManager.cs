using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupsManager : MonoBehaviour
{
    public static PopupsManager instance;
    [Header("UI Elements")]
    public GameObject popupObj;
    public Image popupContainerImage;
    public TextMeshProUGUI popupTitleTMP;
    public TextMeshProUGUI popupMessageTMP;
    public Image popupButtonImage;
    public TextMeshProUGUI popupButtonTMP;


    [Header("Components")]
    public Color popupPrimaryColor;
    public Color popupSecondaryColor;
    public PopupDesign popInfoDesign;
    public PopupDesign popWarningDesign;
    public PopupDesign popErrorDesign;

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
            Popup(PopupType.Info, "This is a info popup message");
            Debug.LogWarning("Test");
        }else if(Input.GetKeyDown(KeyCode.W)){
            Popup(PopupType.Warning, "This is a warning popup message");
        }else if(Input.GetKeyDown(KeyCode.E)){
            Popup(PopupType.Error, "This is a error popup message,\n::ERR_NO_CAMERA_ACS");
        }
    }

    // Popup events, this will open up a popup window
    public void Popup(PopupType type, string message){
        // Set the popup text color to the predetermined design choice
        popupTitleTMP.color = type == PopupType.Info ? popupPrimaryColor : popupSecondaryColor;
        popupMessageTMP.color = type == PopupType.Info ? popupPrimaryColor : popupSecondaryColor;

        // Change the container image sprite using the popup type
        switch (type){
            case PopupType.Info:
                popupContainerImage.sprite = popInfoDesign.containerSprite;
                popupButtonImage.sprite = popInfoDesign.buttonSprite;
                popupButtonTMP.color = popupSecondaryColor;

                // Will add different buttons
                // actionButton.onClick.AddListener(() => popevent);
            break;
            case PopupType.Warning:
                popupContainerImage.sprite = popWarningDesign.containerSprite;
                popupButtonImage.sprite = popWarningDesign.buttonSprite;
                popupButtonTMP.color = popupPrimaryColor;
            break;
            case PopupType.Error:
                popupContainerImage.sprite = popErrorDesign.containerSprite;
                popupButtonImage.sprite = popErrorDesign.buttonSprite;
                popupButtonTMP.color = popupPrimaryColor;
            break;
        }

        // Set the text message to the given value on the parameter
        popupTitleTMP.text = type.ToString();
        popupMessageTMP.text = message;
        popupObj.SetActive(true);
    }
    // Close the popup window
    public void ClosePopup(){
        popupObj.SetActive(false);
    }
}
[System.Serializable]
public class PopupDesign{
    public Sprite containerSprite;
    public Sprite buttonSprite;
}
public enum PopupType{
    Info, Warning, Error
}
