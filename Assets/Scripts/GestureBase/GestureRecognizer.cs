using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;
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
    [SerializeField] private int maxtimeOutSeconds = 15;
    private int curtimeOutSeconds = 0;
    
    // [Header("Debug Components")]
    // [SerializeField] private TextMeshProUGUI gestureText;
    // [SerializeField] private TextMeshProUGUI recognitionInfoText;
    // [SerializeField] private TextMeshProUGUI posInfoText;

    private Coroutine recognitionCoroutine;
    List<Gesture> oneHandGestures;
    List<Gesture> twoHandGestures;

    public bool recognizerState = true;

    private Coroutine recognizerLoop;
    private readonly List<Gesture> _contextualBuffer = new List<Gesture>(); 

    private void Awake() {
        instance = this;
    }

    void Start(){
        runner.config.NumHands = numHands;
        Application.targetFrameRate = 60;

        OrganizeGestures(GestureLibrary.instance.GetLoadedGestures());

        #if PLATFORM_ANDROID
            System.Action action = () => {
                Debug.LogWarning("Quiting application...");
                Application.Quit();
            };
            if(!IsAccessGranted_Camera()){
                PopupsManager.instance.Popup(PopupType.Error, "Application has no Camera Access! Please enable camera permission.\nERR_CODE_CAMERA_01",
                new PopupEvent("Quit", action, "Close", action));
                return;
            }
        #endif
        // // Start the recognizer
        // SetRecognizerState(true);
    }
    private void OrganizeGestures(List<Gesture> gestures){
        oneHandGestures = gestures.Where(g => g.handRequirement == HandRequirement.OneHand).ToList();
        twoHandGestures = gestures.Where(g => g.handRequirement == HandRequirement.TwoHands).ToList();
    }
    public void SetRecognizerState(bool value) {
        recognizerState = value;

        // If the state set to true, then restart the system
        if(value){
            recognizerLoop = StartCoroutine(CoroutineRecognizer());
        }else{
            // Stop/Pause the system
            if(recognizerLoop != null){
                StopCoroutine(recognizerLoop);
            }
        }
        ResetTimeOuerTimer();
    }

    public void SetTickSpeed(float speed){
        recognizerTickRate = speed;
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
        // Debug.LogWarning("Recognizing...");
        if(!IsOneHandctive() && !IsTwoHandsActive()){
            // Might add UI here to tell user there's no active hands
            /*
                User pauses too long, this has double implementations, one here and on Gesture Recognizer,
                #Fix: The system tracks the number of ticks the user hasn't still sign or any active hands detected
                if it does, the system timeouts, and notify the user with UI elements.
            */
            // Might add another counter here if the user still hasn't signed for about 10 ticks now
            // If the user didn't still, pause the system, and recalibrate the user
            ContextualBase.instance.SetContext(GestureContext.None);
            TimeOutSystem();
            if(IsTimeOut){
                SetRecognizerState(false);
                System.Action sub1 = () => {
                    GestureRecognizer.instance.SetRecognizerState(true);
                };
                PopupsManager.instance.Popup(PopupType.Info, "I've detected no activity, let me know when to continue.", new PopupEvent(
                    "Continue", sub1
                ));
                Debug.LogWarning("The system has detected no activity, pausing system");
                return;
            }
            return;
        }

        if(FindAnyObjectByType<HandLandmarkListAnnotation>() == null) return; // There' isn't a active instantiated MediaPipe point annotation yet, return 

        Vector2[] firstAvailableHand = GetLandMarks(0);
        Vector2[] secondAvailableHand = IsTwoHandsActive() ? GetLandMarks(1) : new Vector2[0];

        Gesture recognizedGesture = RecognizeGesture(firstAvailableHand, secondAvailableHand);

        // string pos = string.Empty;
        // foreach (Vector2 landmark in firstAvailableHand){
        //     pos += $"{landmark}\n";
        // }
        // posInfoText.SetText($"First Hand Landmark:\n{pos}");

        if(recognizedGesture != null){
            // gestureText.text = $"Recognized Gesture: {recognizedGesture.name}";
            ContextualBase.instance.UpdateGestureHistory(recognizedGesture);
        }else if(recognizedGesture == null){
            // gestureText.text = "Recognized Gesture: Unknown";
        }

        ResetTimeOuerTimer();
    }
    
    public virtual Gesture RecognizeGesture(Vector2[] firstLandmarks, Vector2[] secondLandmarks){
        Gesture bestMatch = null;
        float bestDifference = 9999f;

        if (!IsTwoHandsActive()){
            List<Gesture> candidates = GetContextualGestureList(oneHandGestures);
            bestMatch = RecognizeOneHandGesture(firstLandmarks, candidates, ref bestDifference);
        }else{
            bestMatch = RecognizeTwoHandGesture(firstLandmarks, secondLandmarks, twoHandGestures, ref bestDifference);
        }

        // Prevent overflow from too high best difference
        if (float.IsNaN(bestDifference) || float.IsInfinity(bestDifference)) {
            Debug.LogWarning("Invalid score detected, resetting recognizer state.");
            bestDifference = 9999f;
        }
        
        #if UNITY_EDITOR
            LogGestureRecognitionResult(bestMatch, bestDifference);
        #endif
        
        return bestMatch;
    }

    private Gesture RecognizeOneHandGesture(Vector2[] handLandmarks, List<Gesture> candidates, ref float bestDifference){
        Gesture bestMatch = null;

        foreach (var gesture in candidates){
            Vector2[] handedLandmarks = IsRightHanded() ? gesture.rightHandPositions : gesture.leftHandPositions;
            float difference = GetHandDifference(handLandmarks, handedLandmarks);
            if (difference < threshold && difference < bestDifference){
                bestDifference = difference;
                bestMatch = gesture;
            }
        }

        // If still null, try global gestures
        if (bestMatch == null) {
            HashSet<Gesture> contextualSet = new HashSet<Gesture>(_contextualBuffer);
            foreach (var gesture in oneHandGestures){
                // Skip gestures already in the context list (avoid double check)
                if (contextualSet.Contains(gesture)) continue;

                Vector2[] handedLandmarks = IsRightHanded() ? gesture.rightHandPositions : gesture.leftHandPositions;
                float difference = GetHandDifference(handLandmarks, handedLandmarks);
                if (difference < threshold && difference < bestDifference) {
                    bestDifference = difference;
                    bestMatch = gesture;
                }
            }
        }

        return bestMatch;
    }
    private Gesture RecognizeTwoHandGesture(Vector2[] hand1, Vector2[] hand2, List<Gesture> candidates, ref float bestDifference){
        Gesture bestMatch = null;
        float safeThreshold = threshold + .05f;

        foreach (var gesture in candidates){
            float bestLocal = Mathf.Min(
                GetHandDifference(hand2, gesture.leftHandPositions) + GetHandDifference(hand1, gesture.rightHandPositions),
                GetHandDifference(hand1, gesture.leftHandPositions) + GetHandDifference(hand2, gesture.rightHandPositions)
            );

            if (bestLocal < safeThreshold && bestLocal < bestDifference) {
                bestDifference = bestLocal;
                bestMatch = gesture;
            }
            // float normalDiff = GetHandDifference(hand2, gesture.leftHandPositions) + GetHandDifference(hand1, gesture.rightHandPositions);
            // float swappedDiff = GetHandDifference(hand1, gesture.leftHandPositions) + GetHandDifference(hand2, gesture.rightHandPositions);

            // if (normalDiff < safeThreshold && normalDiff < bestDifference){
            //     bestDifference = normalDiff;
            //     bestMatch = gesture;
            // }

            // if (swappedDiff < safeThreshold && swappedDiff < bestDifference){
            //     bestDifference = swappedDiff;
            //     bestMatch = gesture;
            // }
        }
        return bestMatch;
    }

    private List<Gesture> GetContextualGestureList(List<Gesture> source){
        GestureContext ctx = ContextualBase.instance.GetCurrentContext();
        _contextualBuffer.Clear();
        
        if (ctx == GestureContext.None) {
            _contextualBuffer.AddRange(source);
            return _contextualBuffer;
        }

        foreach (var g in source){
            if (g.context == ctx)
                _contextualBuffer.Add(g);
        }

        return _contextualBuffer;
    }

    private void LogGestureRecognitionResult(Gesture gesture, float score){
        // if(gesture == null) return;
        string result = $"Current match: {(gesture != null ? gesture.name : "None")} | Score: {score:F4}";
        // recognitionInfoText.text = result;
        Debug.LogWarning(result);
    }

    public virtual float GetHandDifference(Vector2[] detectedHand, Vector2[] storedHand){
        if (storedHand == null || detectedHand == null || detectedHand.Length != storedHand.Length)
            return 9999f;

        float totalDifference = 0f;
        for (int i = 0; i < storedHand.Length; i++){
            /* Series of Conditions to check if the hand difference is overflow with high difference */
            if (float.IsNaN(detectedHand[i].x) || float.IsNaN(detectedHand[i].y)) return 9999f;
            // Difference is at infinity, return a max value of 9999f not float.Max;
            if (float.IsInfinity(detectedHand[i].x) || float.IsInfinity(detectedHand[i].y)) return 9999f;

            // totalDifference += Vector2.Distance(detectedHand[i], storedHand[i]);
            float dx = detectedHand[i].x - storedHand[i].x;
            float dy = detectedHand[i].y - storedHand[i].y;
            totalDifference += (dx * dx + dy * dy);
        }
        return Mathf.Sqrt(totalDifference / storedHand.Length);;
    }
    public void TimeOutSystem(){
        curtimeOutSeconds += 1;
    }
    private bool IsTimeOut => curtimeOutSeconds >= maxtimeOutSeconds;
    public void ResetTimeOuerTimer(){
        curtimeOutSeconds = 0;
    }
    private bool IsAccessGranted_Camera(){
        return Permission.HasUserAuthorizedPermission(Permission.Camera);
    }

    public bool IsHandsHiddenFromFrame => !IsOneHandctive() && !IsTwoHandsActive();
}