using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ContextualBase : GestureBase {
    public static ContextualBase instance;
    [SerializeField] private TTSBase ttsEngine;
    [SerializeField] private List<Gesture> gestureHistory = new List<Gesture>();
    [SerializeField] private int maxHistory = 5;

    [Header("Context Aware")]
    [SerializeField] private int maxContextTimeOut = 2; 
    private int currentContextTimeOut = 0; 
    [SerializeField] private GestureContext currentContext = GestureContext.None;
    public Gesture temporaryGesture = null;
    public StringBuilder stringBuilder = new StringBuilder();
    private List<Gesture> dynamicGestures = new List<Gesture>();

    // --- New: Prediction System ---
    [Header("Prediction System")]
    [SerializeField] private string currentPrediction = string.Empty;
    [SerializeField] private Gesture predictedGesture = null;
    [SerializeField] private bool isPredicting = false;

    private void Start() {
        instance = this;
        dynamicGestures = GestureLibrary.instance
            .GetLoadedGestures()
            .Where(g => g.type == GestureType.Dynamic)
            .ToList();
    }

    // Update Gesture History
    public void UpdateGestureHistory(Gesture gesture) {
        if (gesture == null) {
            // User is idle or detection lost
            PostBuildSentence();
            return;
        }

        // Reset timeout
        currentContextTimeOut = 0;

        // Add a new gesture
        gestureHistory.Add(gesture);

        // Trim gesture history
        if (gestureHistory.Count > maxHistory) {
            gestureHistory.RemoveAt(0);
        }

        // Detect repeated gesture (static feedback)
        if (gesture == temporaryGesture) {
            if (gesture.type == GestureType.Static && gesture.canBeStandalone) {
                BuildSentence(gesture);
                temporaryGesture = null;
                return;
            }
        }
        else {
            if (gesture.type == GestureType.Dynamic) {
                DetectDynamicGestureSequence(gesture);
                PredictDynamicGestureFlow(); // <<< New predictive behavior
            }
        }

        // Set new context and track temporary
        SetContext(gesture.context);
        temporaryGesture = gesture;
    }

    public void BuildSentence(Gesture gesture) {
        switch (gesture.context) {
            case GestureContext.Number:
            case GestureContext.Letter:
                stringBuilder.Append($"{gesture.phraseOrWord}");
                break;

            case GestureContext.None:
            default:
                stringBuilder.Append($"{gesture.phraseOrWord} ");
                break;
        }

        Debug.LogWarning($"Phrase or word here for {gesture} is {gesture.phraseOrWord}");

        // Update text on semi-transparent context layer (active text)
        CameraManager.instance.Text_ContextScreenText(stringBuilder.ToString());
    }

    // --- PREDICTION FEATURE ---
    private void PredictDynamicGestureFlow() {
        Gesture prediction = PredictDynamicGestureSequence(gestureHistory.ToArray());
        if (prediction != null) {
            // Only update if the prediction changed
            if (predictedGesture != prediction) {
                predictedGesture = prediction;
                currentPrediction = prediction.phraseOrWord;
                isPredicting = true;

                // Display prediction on semi-transparent text
                // Example: "Hello (Good Morning)"
                CameraManager.instance.Text_ContextScreenText(
                    $"{stringBuilder.ToString()} ({currentPrediction})"
                );

                Debug.LogWarning($"Predicting dynamic gesture: {currentPrediction}");
            }
        } else {
            // No prediction found, clear any existing one
            ClearPrediction();
        }
    }

    private Gesture PredictDynamicGestureSequence(Gesture[] history) {
        foreach (var dynamic in dynamicGestures) {
            if (dynamic.sequence == null || dynamic.sequence.Length == 0)
                continue;

            int matchCount = 0;
            for (int i = 0; i < Mathf.Min(history.Length, dynamic.sequence.Length); i++) {
                if (history[i] == dynamic.sequence[i])
                    matchCount++;
                else
                    break;
            }

            // If user has matched at least one frame but not all frames
            if (matchCount > 0 && matchCount < dynamic.sequence.Length) {
                return dynamic;
            }
        }
        return null;
    }

    private void ClearPrediction() {
        if (!isPredicting) return;

        isPredicting = false;
        currentPrediction = string.Empty;
        predictedGesture = null;

        // Reset the semi-transparent display to current sentence only
        CameraManager.instance.Text_ContextScreenText(stringBuilder.ToString());
        Debug.LogWarning("Prediction cleared.");
    }

    // --- CONTEXT-BASED DETECTION ---
    private void DetectDynamicGestureSequence(Gesture dynamicGesture) {
        if (dynamicGestures == null || dynamicGestures.Count == 0)
            return;

        Debug.LogWarning("Detecting Dynamic Gesture Sequence");

        if (DynamicMatch(gestureHistory.ToArray(), dynamicGesture.sequence)) {
            Debug.LogWarning($"Detected Dynamic Gesture is {dynamicGesture.name}");
            BuildSentence(dynamicGesture);
            ClearPrediction(); // Clear prediction once confirmed
            FlushHistory();
            return;
        }
    }

    public void PostBuildSentence() {
        currentContextTimeOut++;
        if (currentContextTimeOut >= maxContextTimeOut && stringBuilder.Length > 0) {
            Call_TextToSpeech();
        }
    }

    private void Call_TextToSpeech() {
        currentContextTimeOut = 0;
        CameraManager.instance.Text_OnScreenText(stringBuilder.ToString());

        if (PlayerPrefsHandler.instance.GetTTSState) {
            ttsEngine.Speak(stringBuilder.ToString());
        } else {
            Debug.LogWarning("Text to Speech is off!");
        }

        CameraManager.instance.Text_ClearContextText();
        stringBuilder.Clear();
    }

    private bool DynamicMatch(Gesture[] history, Gesture[] sequence) {
        if (sequence == null || sequence.Length == 0) {
            Debug.LogWarning("No sequence");
            return false;
        }
        if (history.Length < sequence.Length) {
            Debug.LogWarning("History not sufficient for sequence");
            return false;
        }

        int requiredMatch = sequence.Length;
        int match = 0;

        for (int s = 0; s < sequence.Length; s++) {
            for (int i = 0; i < history.Length; i++) {
                if (history[i] == sequence[s]) {
                    match++;
                }
            }
        }

        return match >= requiredMatch;
    }

    private void FlushHistory() {
        if (gestureHistory.Count != 0)
            gestureHistory.Clear();
    }

    public GestureContext GetCurrentContext() => currentContext;

    public void SetContext(GestureContext newContext) {
        // If the context changes (e.g., from Word to Letter), clear any prediction
        if (currentContext != newContext) {
            ClearPrediction();
        }
        currentContext = newContext;
    }
}
