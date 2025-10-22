using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class TextToSign : MonoBehaviour, IPointerClickHandler {
    [Header("Text To Sign")]
    [SerializeField] private GameObject typeObj;
    [SerializeField] private GameObject resultObj;

    [Header("UI Element")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_InputField inputField; // Maximum of 50 characters in input field
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GestureViewerHandler viewerHandler;
    [SerializeField] private Camera cameraToUse;

    private Dictionary<string, List<Gesture>> result = new Dictionary<string, List<Gesture>>();
    private StringBuilder resultStringBuilder;
    public delegate void ClickOnLinkEvent(string word);
    public static event ClickOnLinkEvent OnClickedLinkEvent;

    // Start is called before the first frame update
    void Start() {
        // Subscribe the OnClickLinkEvent to opening the image viewer function
        OnClickedLinkEvent += OpenImageViewWithWord;
    }

    private void OnTextToSignEndEdit() {
        result.Clear(); // Clear the result dictionary to prevent duplication 
        // Start processing the sentence from the input field text
        StartCoroutine(ProcessSentence());
    }

    private IEnumerator ProcessSentence() {
        string[] words = inputField.text.Split(' '); // Segments words by white spaces
        foreach (var word in words) { // Iterate every word from the words
            List<Gesture> gestures = new List<Gesture>(); // Empty default gestures
            int range = 0; // Default to 0 tag range

            if (GestureLibrary.instance.DoesGestureExistByWord(word)) {
                // The gesture is available
                gestures.Add(GestureLibrary.instance.FindGestureByWord(word));
            } else {
                // The gesture is not available, fallback to fingerspelling
                gestures = FingerSpellByWord(word);
            }
            yield return null;
            result.Add(word, gestures); // Add it to the dictionary
            resultStringBuilder.Append(GetModifiedTextWithTag(word, range));
        }
        // Display the result by calling the method
        DisplayResult();
    }
    private List<Gesture> FingerSpellByWord(string word) {
        List<Gesture> gestures = new List<Gesture>(); // Empty list of gestures
        Gesture iGesture; // Default nan for Iterated Character
        foreach (char c in word) {
            iGesture = GestureLibrary.instance.FindGestureByWord(c.ToString());
            if(iGesture != null) continue; // Gesture Library returns nothing / null
            // Add the iterated gestures or found gestures from the library
            gestures.Add(iGesture);
        }
        return gestures; // After finger spelling, return the list of gestures
    }

    // Result
    // Display result after processing the sentence
    public void DisplayResult() {
        resultObj.SetActive(true);
    }

    // Public calls
    // Open Gesture Image Viewer with custom sprites built
    public void OpenImageViewWithWord(string keyword) {
        if (result.ContainsKey(keyword)) { // Check if the dictionary contains a key with comparison to keyword
            List<Sprite> sprites = new List<Sprite>(); // Empty list of sprites
            // Get the List of gestures
            if(result.TryGetValue(keyword, out List<Gesture> gestures)){
                foreach (var item in gestures){ // Foreach item in the gestures inside the dictionary in Key
                    sprites.Add(item.referenceImage); // Add the reference image to the list of sprites
                }
            }
            // Open the gesture image viewer with a custom data
            viewerHandler.SetupViewer(keyword, "Custom", sprites.ToArray());
        } else {
            // Keyword does not contain a result
            Debug.LogWarning("This keyword does not contain a result");
        }
    }

    public void OpenTextToSign() {
        resultObj.SetActive(false);
        typeObj.SetActive(true);
    }
    public void CloseTextToSign() {
        resultObj.SetActive(false);
        typeObj.SetActive(false);
    }

    // TODO: Change hyperlink tag to a custom tag same with text to speech
    public string GetModifiedTextWithTag(string word, int range) {
        StringBuilder tagBuilder = new StringBuilder();
        switch (range) {
            case 1:
                tagBuilder.Append($"{word}");
                break;
            case 2:
                tagBuilder.Append($"{word}");
                break;
            default:
                tagBuilder.Append($"{word}");
                break;
        }
        return tagBuilder.ToString();
    }

    public void OnPointerClick(PointerEventData eventData) {
        Vector3 mousePosition = new Vector3(eventData.position.x, eventData.position.y, 0);
        var linkTaggedText = TMP_TextUtilities.FindIntersectingLink(resultText, mousePosition, cameraToUse);

        if (linkTaggedText != -1) {
            TMP_LinkInfo linkInfo = resultText.textInfo.linkInfo[linkTaggedText];
            OnClickedLinkEvent?.Invoke(linkInfo.GetLinkID());
        }
    }
}
