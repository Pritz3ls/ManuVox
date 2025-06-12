using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContextualBase : MonoBehaviour{
    public static ContextualBase instance;
    [SerializeField] private TTSBase ttsEngine;
    [SerializeField] private List<Gesture> gestureHistory = new List<Gesture>();
    [SerializeField] private int maxHistory = 5;
    [SerializeField] private bool noContext = false;

    [Header("Context Aware")]
    [SerializeField] private int maxPauseDuplicateCount = 5; // Pause the system after several duplicates 
    [SerializeField] private int currentDuplicateCount = 5; // Pause the system after several duplicates 
    public Gesture temporaryGesture = null;

    private List<Gesture> dynamicGestures = new List<Gesture>();
    private void Start() {
        instance = this;
        dynamicGestures = GestureLibrary.instance.GetLoadedGestures().Where(g => g.type == GestureType.Dynamic).ToList();
    }

    // Update Gesture History
    public void UpdateGestureHistory(Gesture gesture){
        /*
            User pauses too long, this has double implementations, one here and on Gesture Recognizer,
            #Fix: The system tracks the number of duplicated gestures rapid succession by the user, if it
            exceeds, timeout the system, and notify the user with UI elements
        */
        // Some return checks if the given gesture is empty or a duplicate
        // If either, provide a UI feedback indicating, Continue
        // This can also help to stop double triggers of gestures, preventing TTS to speak twice the duplicates
        if(gesture == null){
            Debug.LogWarning("Continue..."); // The user is idling
            return;
        }
        if(gesture == gestureHistory.Last()){
            currentDuplicateCount++;
            if(currentDuplicateCount >= maxPauseDuplicateCount){
                Debug.LogWarning("User exceed a maximum duplicate, will pause the system...");
                // System set to pause, the user pause too long
                GestureRecognizer.instance.SetRecognizerState(false); // Pause/Stop the recognizer
                currentDuplicateCount = 0; // Reset the duplicate count to 0
            }
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

        // Set the temporary gesture to this current gesture
        temporaryGesture = gesture;

        // Detect any dynamic gesture building
        // if(gesture.type == GestureType.Dynamic){
        //     DetectDynamicGestureSequence();
        // }

        // if (gesture.type == GestureType.Static && gesture.canBeStandalone){
        //     Call_TextToSpeech(gesture);
        //     return;
        // }
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
}

#region Old-Code Base *BACKUP*
    // private void DetectDynamicGestureSequence(){
    //     // If dynamic gestures are null or empty, return
    //     if (dynamicGestures == null || dynamicGestures.Count == 0) return;
    //     // Iterates every dynamic gestures avaiable
    //     foreach (Gesture dynamicGesture in dynamicGestures){
    //         if (IsFlexibleSequenceMatch(gestureHistory, dynamicGesture.sequence)){
    //             // Debug.LogWarning($"System speaking : {dynamicGesture.phraseOrWord}");
    //             Debug.Log($"Dynamic Gesture is {dynamicGesture.name}");
    //             Call_TextToSpeech(dynamicGesture);
    //             return;
    //         }
    //     }
    // }

    // Is there a sequence matching in the history with the dynamic gestures
    // private bool IsFlexibleSequenceMatch(List<Gesture> history, Gesture[] sequenceSteps){
    //     if (sequenceSteps == null || sequenceSteps.Length == 0) return false; // If the sequence on the gesture data are null or empty, return
    //     if (history.Count < sequenceSteps.Length) return false; // If the length of the history are not enough to perform the sequence, return

    //     int historyIndex = history.Count - sequenceSteps.Length;
    //     // int matchCount = 0; // Track how many matches it counts

    //     for (int i = 0; i < sequenceSteps.Length; i++){
    //         // if (history[historyIndex + i] == sequenceSteps[i]){
    //         //     matchCount++; // Increment if the sequence find something matches in the history
    //         // }
    //         if (history[historyIndex + i] != sequenceSteps[i]){
    //             return false; // If any step is wrong, return false immediately
    //         }
    //     }

    //     // return matchCount >= sequenceSteps.Length - 1; // Allow slight variation
    //     return true; // Return true if everything goes right
    // }
    
#endregion