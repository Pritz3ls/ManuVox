using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using System.Linq;

public class TextToSign : MonoBehaviour{
    [Header("Text To Sign")]
    [SerializeField] private GameObject mainObj;
    [SerializeField] private GameObject typeObj;
    [SerializeField] private GameObject resultObj;
    [SerializeField] private GameObject textToSignButtonObj;

    [Header("UI Element")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_InputField inputField; // Maximum of 50 characters in input field
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GestureViewerHandler viewerHandler;
    [SerializeField] private Camera cameraToUse;

    public delegate void OnOpenEvent();
    public delegate void OnClosedEvent();

    private Dictionary<string, List<Gesture>> result = new Dictionary<string, List<Gesture>>();
    private StringBuilder resultStringBuilder;
    public delegate void ClickOnLinkEvent(string word);
    public static event ClickOnLinkEvent OnClickedLinkEvent;

    // Start is called before the first frame update
    void Start() {
        // Subscribe the OnClickLinkEvent to opening the image viewer function
        OnClickedLinkEvent += OpenImageViewWithWord;
        resultStringBuilder = new StringBuilder();
    }

    void OnEnable() {
        LinkHandlerTMP.OnClickedLinkEvent += OpenImageViewWithWord;
    }

    void OnDisable() {
        LinkHandlerTMP.OnClickedLinkEvent -= OpenImageViewWithWord;
    }

    public void OnTextToSignEndEdit(){
        if(inputField.text == string.Empty || string.IsNullOrWhiteSpace(inputField.text)){
            // PopupsManager.instance.Popup(PopupType.Warning, "Sentence is empty! Try again.");
            Debug.LogWarning("Sentence is empty! Try again.");
            return;
        }
        result.Clear(); // Clear the result dictionary to prevent duplication 
        resultStringBuilder.Clear(); // Clear any result string before
        // Start processing the sentence from the input field text
        StartCoroutine(ProcessSentence());
    }

    private IEnumerator ProcessSentence() {
        var words = inputField.text.Split(' ', System.StringSplitOptions.RemoveEmptyEntries); // Split text by space
        int i = 0; // Word index tracker

        while (i < words.Length) {
            string phrase = null; // Default null phrase
            // Try lengths from 5 down to 1 — but CLEAN preserving spaces when testing multi-word phrases
            for (int len = Mathf.Min(5, words.Length - i); len > 0; len--) {
                string testRaw = string.Join(" ", words, i, len); // raw phrase with spaces
                string test = CleanPhrase(testRaw); // <-- keep spaces for phrase matching
                if (GestureLibrary.instance.DoesGestureExistByWord(test)) {
                    phrase = test;
                    break;
                }
            }

            List<Gesture> gestures = new(); // Temporary gesture holder
            int range = 0; // Default tag range

            if (phrase != null) {
                // phrase is already cleaned by CleanPhrase above
                Gesture gesture = GestureLibrary.instance.FindGestureByWord(phrase);

                if (gesture != null) {
                    if (gesture.type == GestureType.Dynamic && gesture.sequence != null && gesture.sequence.Length > 0) {
                        gestures.AddRange(gesture.sequence);
                        gestures.Add(gesture);
                    } else {
                        gestures.Add(gesture);
                    }
                }

                range = 1; // Word gesture tag
                i += phrase.Split(' ').Length; // Move word index forward (phrase has spaces)
            } else {
                // fallback use CleanWordExcess
                phrase = CleanWordExcess(words[i]);
                gestures = FingerSpellByWord(phrase); // Fallback to finger spelling
                range = 2; // embed a Fingerspell tag
                i++;
            }

            result[phrase] = new List<Gesture>(gestures); // Add gesture to result dictionary
            resultStringBuilder.Append($"{GetModifiedTextWithTag(phrase, range)} "); // Add tagged word to text
            yield return null;
        }

        DisplayResult();
    }


    private List<Gesture> FingerSpellByWord(string word) {
        List<Gesture> gestures = new List<Gesture>(); // Empty list of gestures
        Gesture iGesture; // Default nan for Iterated Character
        foreach (char c in word) {
            iGesture = GestureLibrary.instance.FindGestureByWord(c.ToString());
            if(iGesture == null) continue; // Gesture Library returns nothing / null
            if(iGesture.type == GestureType.Dynamic) {
                gestures.AddRange(iGesture.sequence);
            }
            Debug.Log($"Found {iGesture.name}!");
            // Add the iterated gestures or found gestures from the library
            gestures.Add(iGesture);
        }
        return gestures; // After finger spelling, return the list of gestures
    }

    // Result
    // Display result after processing the sentence
    public void DisplayResult() {
        typeObj.SetActive(false);
        resultText.SetText(resultStringBuilder.ToString());
        resultObj.SetActive(true);

        foreach (string key in result.Keys){
            if(result.TryGetValue(key, out List<Gesture> gestures)){
                Debug.Log($"{key} containing {gestures.Count}");
            }
        }
    }
    public void StartNewSentence() {
        inputField.text = string.Empty;
        typeObj.SetActive(true);
        resultObj.SetActive(false);
    }

    // Public calls
    // Open Gesture Image Viewer with custom sprites built
    public void OpenImageViewWithWord(string keyword) {
        if (result.ContainsKey(keyword)) { // Check if the dictionary contains a key with comparison to keyword
            List<Sprite> sprites = new List<Sprite>(); // Empty list of sprites
            // Get the List of gestures
            if(result.TryGetValue(keyword, out List<Gesture> gestures)){
                foreach (Gesture item in gestures){ // Foreach item in the gestures inside the dictionary in Key
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
        mainObj.SetActive(true);
        textToSignButtonObj.SetActive(false);
        resultObj.SetActive(false);
        typeObj.SetActive(true);
    }
    public void CloseTextToSign() {
        inputField.text = string.Empty;
        textToSignButtonObj.SetActive(true);
        mainObj.SetActive(false);
        resultObj.SetActive(false);
        typeObj.SetActive(false);
    }

    // TODO: Change hyperlink tag to a custom tag same with text to speech
    public string GetModifiedTextWithTag(string word, int range) {
        StringBuilder tagBuilder = new StringBuilder();
        switch (range) {
            case 1:
                // Green
                tagBuilder.Append($"<link=\"{word}\"><mark=#00FF85>{word}<color=#00000000></link></mark></color>");
                break;
            case 2:
                // Blue
                // tagBuilder.Append($"<mark=#FF0099>{word}<color=#00000000>");
                tagBuilder.Append($"<link=\"{word}\"><mark=#1E90FF>{word}<color=#00000000></link></mark></color>");
                break;
            default:
                tagBuilder.Append($"{word}");
                break;
        }
        return tagBuilder.ToString();
    }
    private string CleanPhrase(string phrase) {
        if (string.IsNullOrWhiteSpace(phrase)) return string.Empty;
        // Keep letters, digits and spaces — remove punctuation
        var chars = phrase.Trim()
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
            .ToArray();
        // Collapse multiple spaces to a single space
        string cleaned = new string(chars);
        var parts = cleaned.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts);
    }

    private string CleanWordExcess(string word) {
        if (string.IsNullOrWhiteSpace(word)) return string.Empty;
        // Keep only letters/digits, symbol bad >:(
        return new string(word.Trim().Where(char.IsLetterOrDigit).ToArray());
    }
}
