using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContextualBase : MonoBehaviour{
    public static ContextualBase instance;
    [SerializeField] private List<Gesture> gestureHistory = new List<Gesture>();
    [SerializeField] private List<Gesture> dynamicGestures = new List<Gesture>();
    [SerializeField] private int maxHistory = 5;
    [SerializeField] private bool noContext = false;

    private void Start() {
        instance = this;
    }

    // Update Gesture History
    public void UpdateGestureHistory(Gesture gesture){
        // Add a new gesture
        gestureHistory.Add(gesture);

        // If the gesture history exceed the max length, remove the first one
        if(gestureHistory.Count > maxHistory){
            gestureHistory.RemoveAt(0);
        }

        // Detect any dynamic gesture building
        if(gestureHistory.Last().type == GestureType.Static && gestureHistory.Last().canBeStandalone){
            // Debug.LogWarning($"System speaking : {gestureHistory.Last().phraseOrWord}");
            return;
        }
        DetectDynamicGestureSequence();
    }
    // Context Based Detection
    private void DetectDynamicGestureSequence(){
        // If dynamic gestures are null or empty, return
        if (dynamicGestures == null || dynamicGestures.Count == 0) return;
        
        // Iterates every dynamic gestures avaiable
        foreach (Gesture dynamicGesture in dynamicGestures){
            if (IsFlexibleSequenceMatch(gestureHistory, dynamicGesture.sequence)){
                // Speak(dynamicGesture.phrase);
                Debug.LogWarning($"System speaking : {dynamicGesture.phraseOrWord}"); // Call TTS Base to speak the phrase or word
                FlushHistory();
                return;
            }
        }
    }

    // Is there a sequence matching in the history with the dynamic gestures
    private bool IsFlexibleSequenceMatch(List<Gesture> history, Gesture[] sequenceSteps){
        if (sequenceSteps == null || sequenceSteps.Length == 0) return false; // If the sequence on the gesture data are null or empty, return
        if (history.Count < sequenceSteps.Length) return false; // If the length of the history are not enough to perform the sequence, return

        int historyIndex = history.Count - sequenceSteps.Length;
        // int matchCount = 0; // Track how many matches it counts

        for (int i = 0; i < sequenceSteps.Length; i++){
            // if (history[historyIndex + i] == sequenceSteps[i]){
            //     matchCount++; // Increment if the sequence find something matches in the history
            // }
            if (history[historyIndex + i] != sequenceSteps[i]){
                return false; // If any step is wrong, return false immediately
            }
        }

        // return matchCount >= sequenceSteps.Length - 1; // Allow slight variation
        return true; // Return true if everything goes right
    }

    // Flush the history
    private void FlushHistory(){
        gestureHistory.Clear();
    }
}
