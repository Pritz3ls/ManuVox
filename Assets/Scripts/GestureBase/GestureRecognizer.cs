using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using Mediapipe.Unity;
using TMPro;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using System.Linq;

public class GestureRecognizer : GestureBase{
    public static GestureRecognizer instance;

    [Header("Core Components")]
    [SerializeField] private HandLandmarkerRunner runner;
    
    [Header("Recognizer Components")]
    [Range(1,2)]
    [SerializeField] private int numHands;
    [SerializeField] private float threshold = 0.5f; 
    [SerializeField] private float recognizerTickRate;
    
    [Header("Debug Components")]
    [SerializeField] private TextMeshProUGUI gestureText;
    [SerializeField] private TextMeshProUGUI recognitionInfoText;
    [SerializeField] private TextMeshProUGUI posInfoText;
    private Coroutine recognitionCoroutine;
    List<Gesture> oneHandGestures;
    List<Gesture> twoHandGestures;

    public bool recognizerState = true;

    private Coroutine recognizerLoop;

    private void Awake() {
        instance = this;
    }

    void Start(){
        runner.config.NumHands = numHands;
        Application.targetFrameRate = 60;

        OrganizeGestures(GestureLibrary.instance.GetLoadedGestures());

        // Start the recognizer
        SetRecognizerState(true);
    }
    private void OrganizeGestures(List<Gesture> gestures){
        oneHandGestures = gestures.Where(g => g.handRequirement == HandRequirement.OneHand).ToList();
        twoHandGestures = gestures.Where(g => g.handRequirement == HandRequirement.TwoHands).ToList();
    }
    public void SetRecognizerState(bool value) {
        recognizerState = value;
        // Stop/Pause the system
        StopCoroutine(recognizerLoop);

        // If the state set to true, then restart the system
        if(value == true){
            recognizerLoop = StartCoroutine(CoroutineRecognizer());
        }
    }

    public virtual void StartRecognizer(){
        recognizerLoop = StartCoroutine(CoroutineRecognizer());
    }

    public virtual IEnumerator CoroutineRecognizer(){
        while (true){
            Recognize();
            yield return new WaitForSecondsRealtime(recognizerTickRate);
        }
    }
    void OnDisable(){
        if (recognizerLoop != null){
            StopCoroutine(CoroutineRecognizer());
        }
    }

    public virtual void Recognize(){
        if(!recognizerState) return;
        if(!IsOneHandctive() || !IsTwoHandsActive()){
            // Might add UI here to tell user there's no active hands
            Debug.LogWarning("No hands active, idling");
            /*
                User pauses too long, this has double implementations, one here and on Gesture Recognizer,
                #Fix: The system tracks the number of ticks the user hasn't still sign or any active hands detected
                if it does, the system timeouts, and notify the user with UI elements.
            */
            // Might add another counter here if the user still hasn't signed for about 10 ticks now
            // If the user didn't still, pause the system, and recalibrate the user
            return;
        }

        if(FindObjectOfType<HandLandmarkListAnnotation>() == null) return; // There' isn't a active instantiated MediaPipe point annotation yet, return 

        Vector2[] firstAvailableHand = GetLandMarks(0);
        Vector2[] secondAvailableHand = IsTwoHandsActive() ? GetLandMarks(1) : new Vector2[0];

        Gesture recognizedGesture = RecognizeGesture(firstAvailableHand, secondAvailableHand);

        if(recognizedGesture != null){
            gestureText.text = $"Recognized Gesture: {recognizedGesture.name}";
            ContextualBase.instance.UpdateGestureHistory(recognizedGesture);
            // Debug.LogWarning($"Recognized Gesture: {recognizedGesture.name}");
        }else if(recognizedGesture == null){
            gestureText.text = "Recognized Gesture: Unknown";
            // Debug.LogWarning($"Recognized Gesture: Unknown");
        }
    }
    public virtual Gesture RecognizeGesture(Vector2[] firstLandmarks, Vector2[] secondLandmarks){
        Gesture bestMatch = null;
        StringBuilder sb = new StringBuilder();
        float bestDifference = float.MaxValue; // Start with a large value
        
        if(IsTwoHandsActive()){
            foreach (var gesture in twoHandGestures) {
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
        }else{
            foreach (var gesture in oneHandGestures) {
                bool handMatch = false;
                float difference = float.MaxValue;

                Vector2[] handedness = IsRightHanded() ? gesture.rightHandPositions : gesture.leftHandPositions;
                difference = GetHandDifference(firstLandmarks, handedness);
                handMatch = difference < threshold;

                if (handMatch && difference < bestDifference){
                    bestDifference = difference;
                    bestMatch = gesture;
                }
            }
        }

        sb.AppendLine($"Current match {bestMatch} with Best Difference of {bestDifference}");
        recognitionInfoText.text = sb.ToString();

        Debug.LogWarning(sb.ToString());

        return bestMatch;
    }
    public virtual float GetHandDifference(Vector2[] detectedHand, Vector2[] storedHand){
        if (storedHand == null || detectedHand == null || detectedHand.Length != storedHand.Length)
            return float.MaxValue; // Treat mismatch as maximum difference
        float totalDifference = 0f;
        for (int i = 0; i < storedHand.Length; i++){
            totalDifference += Vector2.Distance(detectedHand[i], storedHand[i]);
        }
        return totalDifference / storedHand.Length;
    }
}