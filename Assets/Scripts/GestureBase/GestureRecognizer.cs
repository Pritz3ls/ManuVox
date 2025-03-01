using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using TMPro;

public class GestureRecognizer : GestureBase{
    [Header("General")]
    [SerializeField] private TextMeshProUGUI gestureText;
    [SerializeField] private TextMeshProUGUI recognitionInfoText;
    [SerializeField] private TextMeshProUGUI posInfoText;

    [SerializeField] private Gesture[] gestures;
    [SerializeField] private float threshold = 0.5f; 
    void Start(){
        Application.targetFrameRate = 60;
        // handLandmarkList.GetComponentInChildren
        InvokeRepeating("Recognize", 1f, 1f);
    }

    void Recognize(){
        if(FindObjectOfType<HandLandmarkListAnnotation>() == null) return;

        Vector2[] landMark = GetLandMarks();
        landMark = NormalizedLandMarks(landMark);

        Gesture recognizedGesture = RecognizeGesture(landMark);
        if(recognizedGesture != null){
            gestureText.text = $"Recognized Gesture: {recognizedGesture.gestureName}";
            Debug.LogWarning($"Recognized Gesture : {recognizedGesture.gestureName}");
        }else if(recognizedGesture == null){
            gestureText.text = "Recognized Gesture: Unknown";
        }

        posInfoText.text = "Finger Index Positions:\n";
        for (int i = 0; i < landMark.Length; i++){
            posInfoText.text += $"Finger Index {i*4}: {landMark[i]}\n";
        }
    }

    Gesture RecognizeGesture(Vector2[] normalizedLandMark){
        Gesture bestMatch = null;
        float bestDifference = float.MaxValue;

        foreach (Gesture gesture in gestures){    
            float totalDifference = 0; 
            for (int i = 0; i < normalizedLandMark.Length; i++){
                totalDifference += Vector2.Distance(normalizedLandMark[i], gesture.relativeFingerPositions[i]);
            }
            float avgDifference = totalDifference / normalizedLandMark.Length;
            
            if(avgDifference < bestDifference){
                bestDifference = avgDifference;
                bestMatch = gesture;
            }
            recognitionInfoText.text = $"Current BestMatch {bestMatch.gestureName} with difference of {bestDifference}";
        }
        if(bestDifference < threshold){
            return bestMatch;
        }
        return null;
    }
}
