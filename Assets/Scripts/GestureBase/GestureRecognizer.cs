using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using TMPro;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public class GestureRecognizer : GestureBase{
    [Header("General")]
    [SerializeField] private TextMeshProUGUI gestureText;
    [SerializeField] private TextMeshProUGUI recognitionInfoText;
    [SerializeField] private TextMeshProUGUI posInfoText;
    [SerializeField] private HandLandmarkerRunner runner;
    [SerializeField] private int numHands;

    [SerializeField] private Gesture[] gestures;
    [SerializeField] private float threshold = 0.5f; 
    void Start(){
        runner.config.NumHands = numHands;
        Application.targetFrameRate = 60;
        // handLandmarkList.GetComponentInChildren
        InvokeRepeating("Recognize", 1f, 1f);
    }
    void Recognize(){
        if(FindObjectOfType<HandLandmarkListAnnotation>() == null) return;

        Vector2[] firstAvailableHand = GetLandMarks(0);
        Vector2[] secondAvailableHand = IsTwoHandsActive() ? GetLandMarks(1) : new Vector2[0];

        Gesture recognizedGesture = RecognizeGesture(firstAvailableHand, secondAvailableHand);

        if(recognizedGesture != null){
            gestureText.text = $"Recognized Gesture: {recognizedGesture.name}";
            ContextualBase.instance.UpdateGestureHistory(recognizedGesture);
        }else if(recognizedGesture == null){
            gestureText.text = "Recognized Gesture: Unknown";
        }
        
        // posInfoText.text = $"Hand Positions : \n";
        // for (int i = 0; i < firstAvailableHand.Length; i++){
        //     posInfoText.text += $"First Available LandMarks{i}: {firstAvailableHand[i]}\n";
        // }
        // if(secondAvailableHand.Length >= 1){
        //     for (int i = 0; i < secondAvailableHand.Length; i++){
        //         posInfoText.text += $"Second Available LandMarks {i}: {secondAvailableHand[i]}\n";
        //     }
        // }
    }

    Gesture RecognizeGesture(Vector2[] firstLandmarks, Vector2[] secondLandmarks){
        Gesture bestMatch = null;
        float bestDifference = float.MaxValue; // Start with a large value

        foreach (Gesture gesture in gestures){
            if (gesture.handRequirement == HandRequirement.OneHand){
                bool handMatch = false;
                float difference = float.MaxValue;

                Vector2[] handedness = IsRightHanded() ? gesture.rightHandPositions : gesture.leftHandPositions;
                difference = GetHandDifference(firstLandmarks, handedness);
                handMatch = difference < threshold;

                if (handMatch && difference < bestDifference){
                    bestDifference = difference;
                    bestMatch = gesture;
                }
            }else if (gesture.handRequirement == HandRequirement.TwoHands){
                float normalDifference = GetHandDifference(firstLandmarks, gesture.leftHandPositions) + GetHandDifference(secondLandmarks, gesture.rightHandPositions);

                float swappedDifference = GetHandDifference(secondLandmarks, gesture.leftHandPositions) + GetHandDifference(firstLandmarks, gesture.rightHandPositions);

                if (normalDifference < threshold && normalDifference < bestDifference){
                    bestDifference = normalDifference;
                    bestMatch = gesture;
                }

                if (swappedDifference < threshold && swappedDifference < bestDifference){
                    bestDifference = swappedDifference;
                    bestMatch = gesture;
                }
            }

            recognitionInfoText.text = $"Current match {bestMatch} with Best Difference of {bestDifference}";
        }

        return bestMatch;
    }

    // Deprecated, might use later
    // private bool CompareHand(Vector2[] detectedHand, Vector2[] storedHand){
    //     if (storedHand == null || detectedHand == null) return false;
    //     if (detectedHand.Length != storedHand.Length) return false;

    //     float totalDifference = 0f;

    //     for (int i = 0; i < storedHand.Length; i++){
    //         totalDifference += Vector2.Distance(detectedHand[i], storedHand[i]);
    //     }

    //     float avgDifference = totalDifference / storedHand.Length;
    //     recognitionInfoText.text = $"Current difference {avgDifference} < {threshold}";
    //     return avgDifference < threshold;  // positionThreshold is now the similarity threshold
    // }

    private float GetHandDifference(Vector2[] detectedHand, Vector2[] storedHand){
        if (storedHand == null || detectedHand == null || detectedHand.Length != storedHand.Length)
            return float.MaxValue; // Treat mismatch as maximum difference
        float totalDifference = 0f;
        for (int i = 0; i < storedHand.Length; i++){
            totalDifference += Vector2.Distance(detectedHand[i], storedHand[i]);
        }
        return totalDifference / storedHand.Length;
    }
}
