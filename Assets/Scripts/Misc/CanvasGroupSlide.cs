using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CanvasGroupSlide : MonoBehaviour{
    [Header("Introductory Components")]
    [SerializeField] private float transitionTime; 
    [SerializeField] private float slidesTime = .5f; 
    [SerializeField] private Animator[] objectAnimator;
    [SerializeField] private TextMeshProUGUI footerText;
    [SerializeField] private UnityEvent slideFinishedCustomEvent;
    private int _cur = 0;
    
    private void OnEnable() {
        StartIntroductory();
    }
    void OnDisable(){
        _cur = 0;
        foreach (var obj in objectAnimator){
            obj.gameObject.SetActive(false);
        }
    }

    private void StartIntroductory(){
        footerText.SetText(GetFooterText(_cur));
        objectAnimator[_cur].gameObject.SetActive(true);
        objectAnimator[_cur].Play("CanvasGroup-FadeIn",0,0);

        // Start the coroutine the process to keep looping
        StartCoroutine(HandleTransition());
    }
    private IEnumerator HandleTransition(){
        while (true){
            yield return new WaitForSeconds(transitionTime);

            objectAnimator[_cur].Play("CanvasGroup-FadeOut",0,0);
            yield return new WaitForSeconds(slidesTime);
            objectAnimator[_cur].gameObject.SetActive(false);
            
            if(_cur != objectAnimator.Length-1){
                _cur++;
            }else{
                _cur = 0;
                // The slide has finished, invoke a custom event
                slideFinishedCustomEvent?.Invoke();
            }

            objectAnimator[_cur].gameObject.SetActive(true);
            objectAnimator[_cur].Play("CanvasGroup-FadeIn",0,0);

            // Update the footer text to the current slide
            footerText.SetText(GetFooterText(_cur));
        }
    }

    private string GetFooterText(int st){
        string value = String.Empty;
        for (int i = 0; i < objectAnimator.Length; i++){
            if(i == st){
                value += "<b><font-weight=900>.</font-weight></b>";
                continue;
            }
            value += "<font-weight=300>.</font-weight>";
        }
        return value;
    }
}
