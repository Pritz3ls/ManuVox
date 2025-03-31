using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using TMPro;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using Unity.VisualScripting;

public class GestureRecognizer : GestureBase{
    public static GestureRecognizer instance;

    [Header("Core Components")]
    [SerializeField] private HandLandmarkerRunner runner;
    
    [Header("Recognizer Components")]
    [SerializeField] private int numHands;
    [SerializeField] private float threshold = 0.5f; 
    [SerializeField] private float recognizerTickRate;
    
    [Header("Debug Components")]
    [SerializeField] private TextMeshProUGUI gestureText;
    [SerializeField] private TextMeshProUGUI recognitionInfoText;
    [SerializeField] private TextMeshProUGUI posInfoText;

    private List<Gesture> gestures;

    private bool recognizerState = true;

    private void Awake() {
        instance = this;
    }

    void Start(){
        runner.config.NumHands = numHands;
        Application.targetFrameRate = 60;

        gestures = GestureLibrary.instance.GetLoadedGestures();

        InitializeGestureRecognizer();
    }

    public void SetRecognizerState(bool value) {
        recognizerState = value;
    }

    private void InitializeGestureRecognizer(){
        InvokeRepeating("Recognize", recognizerTickRate, recognizerTickRate);
    }

    void Recognize(){
        if(!recognizerState) return;

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