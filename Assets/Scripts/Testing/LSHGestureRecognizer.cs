using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public class LSHGestureRecognizer : GestureRecognizer
{
    private LSHGestureStorage lshStorage;

    public override void InitializeGestureRecognizer(){
        // lshStorage = new LSHGestureStorage(5); // Assuming 5 key points
        // foreach (Gesture gesture in gestures) {
        //     lshStorage.AddGesture(gesture);
        // }

        base.InitializeGestureRecognizer();
    }

    public override void Recognize(){
        if(!recognizerState) return;
        if(FindObjectOfType<HandLandmarkListAnnotation>() == null) return;

        Vector2[] firstAvailableHand = GetLandMarks(0);
        Vector2[] secondAvailableHand = IsTwoHandsActive() ? GetLandMarks(1) : new Vector2[0];

        Gesture recognizedGesture = RecognizeGesture(firstAvailableHand, secondAvailableHand);

        if(recognizedGesture != null){
            Debug.LogWarning($"Recognized Gesture: {recognizedGesture.name}");
            ContextualBase.instance.UpdateGestureHistory(recognizedGesture);
        }
    }

    public override Gesture RecognizeGesture(Vector2[] firstLandmarks, Vector2[] secondLandmarks){
        Gesture inputGesture = ScriptableObject.CreateInstance<Gesture>();
        inputGesture.rightHandPositions = firstLandmarks;

        List<Gesture> candidates = lshStorage.GetCandidates(inputGesture);
        Gesture bestMatch = null;
        float bestDifference = float.MaxValue;

        foreach (Gesture gesture in candidates) {
            float difference = GetHandDifference(firstLandmarks, gesture.rightHandPositions);
            if (difference < bestDifference) {
                bestDifference = difference;
                bestMatch = gesture;
            }
        }
        Debug.LogWarning(bestMatch != null ? $"{bestMatch.name} with {bestDifference} out of {candidates.Count} candidates" : $"Unknown, out of {candidates.Count} candidates");
        return bestMatch;
    }

    public override float GetHandDifference(Vector2[] detectedHand, Vector2[] storedHand) {
        float totalDifference = 0;
        for (int i = 0; i < detectedHand.Length; i++) {
            totalDifference += Vector2.Distance(detectedHand[i], storedHand[i]);
        }
        return totalDifference / detectedHand.Length;
    }
}
