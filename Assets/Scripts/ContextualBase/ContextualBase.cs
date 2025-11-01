using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ContextualBase : GestureBase{
    public static ContextualBase instance;
    [SerializeField] private TTSBase ttsEngine;
    [SerializeField] private List<Gesture> gestureHistory = new List<Gesture>();
    [SerializeField] private int maxHistory = 5;
    // [SerializeField] private bool noContext = false;

    [Header("Context Aware")]
    [SerializeField] private float maxContextTimeOut = 2; 
    private float currentContextTimeOut = 0; 
    [SerializeField] private GestureContext currentContext = GestureContext.None;
    public Gesture temporaryGesture = null;
    public StringBuilder stringBuilder = new StringBuilder();
    private List<Gesture> dynamicGestures = new List<Gesture>();

    private List<Gesture> prediction = new List<Gesture>();
    private StringBuilder predictionBuilder = new StringBuilder();

    private void Start() {
        instance = this;

        var allGestures = GestureLibrary.instance.GetLoadedGestures();
        foreach (var g in allGestures)
            if (g.type == GestureType.Dynamic)
                dynamicGestures.Add(g);
    }

    // Update Gesture History
    public void UpdateGestureHistory(Gesture gesture){
        if(gesture == null){
            // Debug.LogWarning("Continue..."); // The user is idling
            return;
        }
        // Reset the context time out to 0
        currentContextTimeOut = 0;

        // Add a new gesture
        gestureHistory.Add(gesture);

        // If the gesture history exceed the max length, remove the first one
        if(gestureHistory.Count > maxHistory){
            gestureHistory.RemoveAt(0);
        }

        /*
            Subsequent frames are called in even though user intents to do just one, example I and J.
            #Fix: Store the latest detected gesture into temporary then check if it's the same as temporary to conclude
            if it's a dynamic gesture or static  
        */
        // The system detects the same gesture as last temporary gesture, display and call TTS on it
        if(gesture == temporaryGesture){
            // #if UNITY_EDITOR
            //     Debug.LogWarning("Same gesture as last time, calling feedback");
            // #endif
            if (gesture.type == GestureType.Static && gesture.canBeStandalone){
                BuildSentence(gesture);
                temporaryGesture = null;
                return;
            }
        }else{ // The system detects it's not a same gesture as before, Detect any dynamic gesture sequence
            DetectDynamicGestureSequence();
        }


        // Set the current context to the new context
        SetContext(gesture.context);

        // Set the temporary gesture to this current gesture
        temporaryGesture = gesture;
    }

    // Context Based Detection
    private void DetectDynamicGestureSequence(){
        // If dynamic gestures are null or empty, return
        if (dynamicGestures == null || dynamicGestures.Count == 0) return;

        bool partialMatchFound = false;

        foreach (Gesture dynamicGesture in dynamicGestures){
            if (DynamicMatch(gestureHistory, dynamicGesture)){
                // #if UNITY_EDITOR
                //     Debug.LogWarning($"Detected Dynamic Gesture is {dynamicGesture.name}");
                // #endif

                BuildSentence(dynamicGesture);
                FlushHistory();
                ClearPrediction();
                return; // Stop checking and predicting if a full match is found
            }

            // If there's no dynamic match first, check if it's a dynamic partial match
            if (DynamicPartialMatch(gestureHistory, dynamicGesture.sequence) > 0){
                partialMatchFound = true;
            }
        }

        // No partial match found, clear prediction
        if (!partialMatchFound) {
            ClearPrediction();
        }

        DynamicPrediction(gestureHistory);
    }
    private bool DynamicMatch(List<Gesture> history, Gesture dynamic) {
        List<Gesture> sequence = new List<Gesture>();
        sequence.AddRange(dynamic.sequence);
        sequence.Add(dynamic);
        
        if (sequence == null || sequence.Count == 0) return false;
        if (history.Count < sequence.Count) return false;

        int seqIndex = 0;
        for (int i = 0; i < history.Count && seqIndex < sequence.Count; i++) {
            if (history[i] == sequence[seqIndex])
                seqIndex++;
        }
        return seqIndex == sequence.Count;
    }

    private void DynamicPrediction(List<Gesture> history) {
        if (history.Count == 0) return;

        // Clear the prediction first, to get a new prediction out of the first one
        ClearPrediction();

        // Iterate through all dynamic gestures to check for partial matches
        foreach (Gesture dynamicGesture in dynamicGestures) {
            if (dynamicGesture.sequence == null || dynamicGesture.sequence.Length == 0) continue;

            // Use the updated function to find the length of the longest matching suffix
            int matchLength = DynamicPartialMatch(history, dynamicGesture.sequence); // Modified
            if (matchLength > 0 && matchLength < dynamicGesture.sequence.Length+1) {
                #if UNITY_EDITOR
                    // Modified prediction message for clarity on shared prefixes :D
                    Debug.LogWarning($"--- Dynamic Gesture Prediction: User's latest sequence partially matches '{dynamicGesture.name}' (Matched: {matchLength}/{dynamicGesture.sequence.Length+1}).");
                #endif

                // Add this gesture to the prediction list
                if (!prediction.Contains(dynamicGesture) == prediction.Count < 1) {
                    prediction.Add(dynamicGesture);
                    predictionBuilder.Append($"'{dynamicGesture.phraseOrWord}'");

                    CameraManager.instance.Text_ContextScreenText($"{stringBuilder.ToString()} {predictionBuilder.ToString()}");
                }
            }
        }
    }

    private void ClearPrediction() {
        prediction.Clear();
        predictionBuilder.Clear();
    }

    private int DynamicPartialMatch(List<Gesture> history, Gesture[] sequence) {
        if (sequence == null || sequence.Length == 0 || history.Count == 0) return 0; // Modified condition
        int longestMatch = 0;

        int seqIndex = 0;
        for (int i = 0; i < history.Count && seqIndex < sequence.Length; i++) {
            if (history[i] == sequence[seqIndex])
                longestMatch++;
        }
        return longestMatch;
    }

    private void LateUpdate() {
        if (GestureRecognizer.instance.IsHandsHiddenFromFrame) {
            PostBuildSentence();
        }
    }

    // Sentence Building
    public void PostBuildSentence(){
        if(stringBuilder.Length <= 0) return;
        /*
            Check if the context timeout has reached he maximum hold, if yes then
            release the sentence and call the text to speech output
        */
        // Increase the context time out by tick speed
        currentContextTimeOut += Time.deltaTime;
        if(currentContextTimeOut >= maxContextTimeOut && stringBuilder.Length > 0){
            // Release the sentence
            Call_TextToSpeech();
        }
    }
    private void Call_TextToSpeech(){
        currentContextTimeOut = 0;
        SetContext(GestureContext.None);
        temporaryGesture = null;

        // On Screen Text
        CameraManager.instance.Text_OnScreenText(stringBuilder.ToString());
        
        // Call TTS Base to speak the phrase or word
        // Check if it's turned on
        if(PlayerPrefsHandler.instance.GetTTSState){
            ttsEngine.Speak(stringBuilder.ToString());
        }

        CameraManager.instance.Text_ClearContextText();
        stringBuilder.Clear();
    }
    public void BuildSentence(Gesture gesture){
        // The gesture is a special gesture containing action
        if (IsSpecialGesture(gesture)) {
            return;
        }
            
        switch (gesture.context){
            // This is for spelling out words or names using alphabet
            case GestureContext.Number:
            case GestureContext.Letter:
                stringBuilder.Append($"{gesture.phraseOrWord}");
            break;

            // Default callback to words and others
            case GestureContext.None:
            default:
                stringBuilder.Append($"{gesture.phraseOrWord} ");
            break;
        }

        // #if UNITY_EDITOR
        //     Debug.LogWarning($"Phrase or word here for {gesture} is {gesture.phraseOrWord}");
        // #endif
        CameraManager.instance.Text_ContextScreenText(stringBuilder.ToString());
    }

    private bool IsSpecialGesture(Gesture gesture) {
        if(gesture == null || string.IsNullOrEmpty(gesture.phraseOrWord)) return false;
        string command = gesture.phraseOrWord.Trim().ToLower();
        switch (command) {
            case "delete":
                DeleteLastWord();
            return true;
            case "space":
                stringBuilder.Append(" ");
                CameraManager.instance.Text_OnScreenText(stringBuilder.ToString());
            return true;
        }
        return false;
    }

    // Flush the history
    private void FlushHistory(){
        if(gestureHistory.Count != 0){
            gestureHistory.Clear();
        }
    }

    public GestureContext GetCurrentContext(){
        return currentContext;
    }
    public void SetContext(GestureContext newContext){
        currentContext = newContext;
    }
    private void DeleteLastWord() {
        if (stringBuilder.Length == 0) return;
        stringBuilder.Remove(stringBuilder.Length - 1, 1);
        CameraManager.instance.Text_ContextScreenText(stringBuilder.ToString());
    }
}

/* Deprecated */
