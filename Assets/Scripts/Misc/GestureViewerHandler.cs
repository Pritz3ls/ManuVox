using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GestureViewerHandler : MonoBehaviour
{
    [Header("Gesture Viewer Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private TextMeshProUGUI footerSlideText;
    [SerializeField] private Animator[] imageAnim;
    [SerializeField] private float transitionTime;

    private Coroutine transitionCoroutine;
    private int sequenceLength = 0;
    private int curSlide = 0;

    public void SetupViewer(string name, string category, Sprite[] sequence){
        anim.gameObject.SetActive(true);
        curSlide = 0;
        foreach (var item in imageAnim){
            item.gameObject.SetActive(false);
        }
        
        nameText.SetText($"' {name} '");
        categoryText.SetText($"{category} Category");

        sequenceLength = sequence.Length;
        for (int i = 0; i < sequenceLength; i++){
            imageAnim[i].GetComponent<Image>().sprite = sequence[i];
        }

        StartSlideSequence();
    }

    private void StartSlideSequence(){
        if(transitionCoroutine != null){
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        footerSlideText.SetText(GetFooterText(curSlide));
        imageAnim[curSlide].gameObject.SetActive(true);      
        imageAnim[curSlide].Play("Slide-In",0,0);

        if(sequenceLength > 1){
            transitionCoroutine = StartCoroutine(SlideTransition());
        }
    }

    IEnumerator SlideTransition(){
        while (true){
            yield return new WaitForSeconds(transitionTime);
            imageAnim[curSlide].Play("Slide-Out",0,0);
            yield return new WaitForSeconds(.5f);
            imageAnim[curSlide].gameObject.SetActive(false);

            if(curSlide != sequenceLength-1){
                curSlide++;
            }else{
                curSlide = 0;
            }

            imageAnim[curSlide].gameObject.SetActive(true);
            imageAnim[curSlide].Play("Slide-In",0,0);
            footerSlideText.SetText(GetFooterText(curSlide));
        }
    }

    public void CloseViewer(){
        StartCoroutine(Close());
    }
    IEnumerator Close(){
        anim.Play("DragDown",0,0);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        
        if(transitionCoroutine != null){
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
        anim.gameObject.SetActive(false);
    }
    private string GetFooterText(int st){
        string value = String.Empty;
        for (int i = 0; i < sequenceLength; i++){
            if(i == st){
                value += "<b><font-weight=900>.</font-weight></b>";
                continue;
            }
            value += "<font-weight=300>.</font-weight>";
        }
        return value;
    }

    public bool IsViewerActive => anim.gameObject.activeInHierarchy;
}
