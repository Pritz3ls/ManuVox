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
    [SerializeField] private Animator[] imageAnim; // only 5 slots in the hierarchy
    [SerializeField] private float transitionTime = 1.5f;

    private Coroutine transitionCoroutine;
    private int sequenceLength = 0;
    private int curSlide = 0;

    private Sprite[] spriteSequence; // store all sprites (could be 34+)
    private const int MAX_VISIBLE_SLOTS = 5;

    public void SetupViewer(string name, string category, Sprite[] sequence)
    {
        anim.gameObject.SetActive(true);

        spriteSequence = sequence;
        sequenceLength = spriteSequence.Length;
        curSlide = 0;

        foreach (var item in imageAnim)
        {
            item.gameObject.SetActive(false);
        }

        nameText.SetText($"'{name}'");
        categoryText.SetText($"{category} Category");

        // Load initial 5 or fewer sprites
        int loadCount = Mathf.Min(MAX_VISIBLE_SLOTS, sequenceLength);
        for (int i = 0; i < loadCount; i++)
        {
            imageAnim[i].GetComponent<Image>().sprite = spriteSequence[i];
        }

        StartSlideSequence();
    }

    private void StartSlideSequence()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        footerSlideText.SetText(GetFooterText(curSlide));

        int slotIndex = curSlide % MAX_VISIBLE_SLOTS;
        imageAnim[slotIndex].gameObject.SetActive(true);
        imageAnim[slotIndex].Play("Slide-In", 0, 0);

        if (sequenceLength > 1)
        {
            transitionCoroutine = StartCoroutine(SlideTransition());
        }
    }

    IEnumerator SlideTransition()
    {
        while (true)
        {
            yield return new WaitForSeconds(transitionTime);

            int oldSlot = curSlide % MAX_VISIBLE_SLOTS;
            imageAnim[oldSlot].Play("Slide-Out", 0, 0);
            yield return new WaitForSeconds(0.5f);
            imageAnim[oldSlot].gameObject.SetActive(false);

            // Move to next sprite
            curSlide = (curSlide + 1) % sequenceLength;

            int newSlot = curSlide % MAX_VISIBLE_SLOTS;
            var imgComp = imageAnim[newSlot].GetComponent<Image>();

            // Update the sprite for this slot before showing
            imgComp.sprite = spriteSequence[curSlide];

            imageAnim[newSlot].gameObject.SetActive(true);
            imageAnim[newSlot].Play("Slide-In", 0, 0);

            footerSlideText.SetText(GetFooterText(curSlide));
        }
    }

    public void CloseViewer()
    {
        StartCoroutine(Close());
    }

    IEnumerator Close()
    {
        anim.Play("DragDown", 0, 0);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        anim.gameObject.SetActive(false);
    }

    private string GetFooterText(int st)
    {
        string value = String.Empty;
        for (int i = 0; i < sequenceLength; i++)
        {
            if (i == st)
                value += "<b><font-weight=900>.</font-weight></b>";
            else
                value += "<font-weight=300>.</font-weight>";
        }
        return value;
    }

    public bool IsViewerActive => anim.gameObject.activeInHierarchy;
}
