using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour{
    [Header("UI Components")]
    [SerializeField] private Animator parentComponent;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private RectTransform highlightMask;
    
    [Header("Core Components")]
    [SerializeField] private TutorialElement[] _tutorialElements;

    int curElementIndex = 0;
    bool cooldown = false;
    // bool ignoringTap = false;

    // Special for this project only
    [System.Serializable]
    enum SceneType{
        Camera, Reference
    }
    [SerializeField] private SceneType type;

    // Start is called before the first frame update
    void Awake(){
        if(CheckTutorialFinishedCamera() || CheckTutorialFinishedReference()){
            gameObject.SetActive(false);
            enabled = false;
            return;
        }

        // Fire up the tutorial
        curElementIndex = 0;
        LoadTutorialElement(curElementIndex);
    }

    bool CheckTutorialFinishedCamera() => PlayerPrefsHandler.instance.GetTutorialState() == 1 ? true : false;
    bool CheckTutorialFinishedReference() => PlayerPrefsHandler.instance.GetTutorialState() == 2 ? true : false;
    void SaveFinishedTutorialState() => PlayerPrefsHandler.instance.SaveMisc_Tutorial(type == SceneType.Camera ? 1 : 2);

    // Update is called once per frame
    void Update(){
        if(cooldown) return;

        if(Input.GetMouseButtonDown(0)){
            ProgressTutorial();
        }
    }

    public void ProgressTutorial(){
        if(curElementIndex < _tutorialElements.Length-1){
            curElementIndex++;
            LoadTutorialElement(curElementIndex);
        }else{
            SaveFinishedTutorialState();
            gameObject.SetActive(false);
        }
    }

    private void LoadTutorialElement(int index){
        if(_tutorialElements[index].includeHighlight){
            highlightMask.GetComponent<Image>().color = new Color(1,1,1,0.003f);
            UpdateHighlight(_tutorialElements[index].targetElement);
        }else{
            highlightMask.GetComponent<Image>().color = Color.clear;
        }

        infoText.SetText(_tutorialElements[index].tutorialInfo);
        parentComponent.Play("CanvasGroup-FadeIn",0,0);

        StartCoroutine(Cooldown());
    }
    private void UpdateHighlight(RectTransform targetElement){
        if (targetElement == null || highlightMask == null){
            return;
        }
        float padding = 15f;

        // Get the world corners of the target UI element
        Vector3[] corners = new Vector3[4];
        targetElement.GetWorldCorners(corners);

        // Calculate the center and size of the target element in screen space
        Vector3 center = (corners[0] + corners[2]) / 2;
        float width = corners[2].x - corners[0].x;
        float height = corners[1].y - corners[0].y;
        
        float paddedWidth = width + padding * 2;
        float paddedHeight = height + padding * 2;

        // Scale the highlight mask to match the size of the target
        highlightMask.sizeDelta = new Vector2(paddedWidth, paddedHeight);
        
        // Position the highlight mask to the center of the target element
        highlightMask.position = center;
    }
    IEnumerator Cooldown(){
        cooldown = true;
        yield return new WaitForSecondsRealtime(2f);
        cooldown = false;
    }
}
[System.Serializable]
public class TutorialElement{
    [TextArea(5,10)]
    public string tutorialInfo;
    public RectTransform targetElement;
    public bool includeHighlight = true;
} 