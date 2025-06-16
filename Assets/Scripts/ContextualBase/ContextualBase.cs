using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContextualBase : GestureBase{
    public static ContextualBase instance;
    [SerializeField] private TTSBase ttsEngine;
    [SerializeField] private List<Gesture> gestureHistory = new List<Gesture>();
    [SerializeField] private int maxHistory = 5;
    [SerializeField] private bool noContext = false;

    [Header("Context Aware")]
    [SerializeField] private int maxContextTimeOut = 3; 
    private int currentContextTimeOut = 0; 
    [SerializeField] private GestureContext currentContext = GestureContext.None;
    public Gesture temporaryGesture = null;

    private List<Gesture> dynamicGestures = new List<Gesture>();
    private void Start() {
        instance = this;
        dynamicGestures = GestureLibrary.instance.GetLoadedGestures().Where(g => g.type == GestureType.Dynamic).ToList();
    }

    // Update Gesture History
    public void UpdateGestureHistory(Gesture gesture){
        if(gesture == null){
            Debug.LogWarning("Continue..."); // The user is idling
            return;
        }

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
            Debug.LogWarning("Same gesture as last time, calling feedback");
            if (gesture.type == GestureType.Static && gesture.canBeStandalone){
                Call_TextToSpeech(gesture);
                temporaryGesture = null;
                return;
            }
        }else{ // The system detects it's not a same gesture as before, Detect any dynamic gesture sequence
            if(gesture.type == GestureType.Dynamic){
                // idk if i should reset the temporary here, will test tomorrow
                DetectDynamicGestureSequence();
            }
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
        Debug.LogWarning("Detecting Dynamic Gestures");
        // Iterates every dynamic gestures avaiable
        foreach (Gesture dynamicGesture in dynamicGestures){
            if (IsFlexibleSequenceMatch(gestureHistory, dynamicGesture.sequence)){
                // Debug.LogWarning($"System speaking : {dynamicGesture.phraseOrWord}");
                Debug.LogWarning($"Detected Dynamic Gesture is {dynamicGesture.name}");
                Call_TextToSpeech(dynamicGesture);
                FlushHistory();
                return;
            }
        }
    }

    private void Call_TextToSpeech(Gesture gesture){
        // On Screen Text
        UIManager.instance.Text_OnScreenText(gesture.phraseOrWord);
        
        // Call TTS Base to speak the phrase or word
        ttsEngine.Speak(gesture.phraseOrWord);
    }

    private bool IsFlexibleSequenceMatch(List<Gesture> history, Gesture[] sequenceSteps){
        if (sequenceSteps == null || sequenceSteps.Length == 0) return false; // If the sequence on the gesture data are null or empty, return
        if (history.Count < sequenceSteps.Length) return false; // If the length of the history are not enough to perform the sequence, return

        int historyIndex = history.Count - sequenceSteps.Length - 1;;
        int requiredMatchCount = sequenceSteps.Length <= 1 ? 1 : sequenceSteps.Length - 1;
        int matchCount = 0; // Track how many matches it counts

        for (int i = 0; i < sequenceSteps.Length; i++){
            if (history[historyIndex-i] == sequenceSteps[i]){
                matchCount++; // Increment if the sequence find something matches in the history
            }
        }
        return matchCount >= requiredMatchCount; // Allow slight variation
        // return true; // Return true if everything goes right
    }

    // Flush the history
    private void FlushHistory(){
        if(gestureHistory.Count != 0){
            gestureHistory.Clear();
        }
    }

    private bool ContextTimeOut(){
        currentContextTimeOut++;
        return currentContextTimeOut >= maxContextTimeOut;
    }
    public GestureContext GetCurrentContext(){
        return currentContext;
    }
    public void SetContext(GestureContext newContext){
        currentContextTimeOut = 0;
        currentContext = newContext;
    }
}
