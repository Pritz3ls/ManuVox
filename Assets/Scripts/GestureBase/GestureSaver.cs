using System.Collections;
using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine.UI;
using TMPro;

public enum CaptureType{Manual, Automatic}
public class GestureSaver : GestureBase{
    [SerializeField] private HandLandmarkerRunner runner;
    [SerializeField] private int numHands;
    [SerializeField] private CaptureType captureType = CaptureType.Manual;
    [SerializeField] private Gesture[] gestureSaveList;
    [SerializeField] private Gesture currentGesture;
    [SerializeField] private Image preview;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float captureDelay = 1f;
    [SerializeField] private float speed;
    
    int curIndex = 0;
    private bool started = false;

    private void Start() {
        curIndex = 0;
        currentGesture = gestureSaveList[curIndex];
        preview.sprite = currentGesture.referenceImage;
        Application.targetFrameRate = 60;
        runner.config.NumHands = numHands;

        // DynamicSetRunnerHands(currentGesture);
    }
    // Update is called once per frame
    void Update(){
        if(Input.GetKeyDown(KeyCode.Space)){
            if(captureType == CaptureType.Automatic && started){
                return;
            }
            StartCoroutine(SaveGesture());
        }
    }
    IEnumerator SaveGesture(){
        timerText.SetText("Get Ready!");
        float time = captureDelay;
        while (time > 0){
            time -= speed * Time.deltaTime;
            if(time < Mathf.Round(captureDelay / 1.5f)) {
                timerText.SetText($"{time.ToString("0")}");
            }
            yield return null;
        }

        if (currentGesture.handRequirement == HandRequirement.OneHand) {
            if (IsRightHanded()) {
                currentGesture.rightHandPositions = GetLandMarks(0);
                currentGesture.leftHandPositions = GetLandMarks_Reversed(0);
                Debug.LogWarning("Capture is Right Handed");
            } else {
                currentGesture.leftHandPositions = GetLandMarks(0);
                currentGesture.rightHandPositions = GetLandMarks_Reversed(0);
                Debug.LogWarning("Capture is Left Handed");
            }
            // if(IsTwoHandsActive()){
            //     currentGesture.rightHandPositions = GetLandMarks(1);
            // }else{
            //     Debug.LogWarning("To ensure left handedness compatibility, capture two hands.");
            //     currentGesture.rightHandPositions = GetLandMarks_Reversed(0);
            // }
        } else {
            if (IsTwoHandsActive()) {
                currentGesture.leftHandPositions = GetLandMarks(0);
                currentGesture.rightHandPositions = GetLandMarks(1);
            } else {
                Debug.LogError("Detected only one hand active! Make sure to capture two hands,");
                yield return null;
            }
        }
        // Debug.LogWarning($"Saved Gesture Name {currentGesture.name}");
        timerText.SetText($"Saved Gesture Name {currentGesture.name}");

        #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(currentGesture);
        #endif

        if(curIndex < gestureSaveList.Length-1){
            curIndex++;
            currentGesture = gestureSaveList[curIndex];
            preview.sprite = currentGesture.referenceImage;
        }else{
            #if UNITY_EDITOR
                UnityEditor.AssetDatabase.SaveAssets();
            #endif
            Debug.LogWarning("Saving Assets...");
            StopAllCoroutines();
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        if(captureType == CaptureType.Automatic) {
            started = true;
            StartCoroutine(SaveGesture());
        }
    }

    // void DynamicSetRunnerHands(Gesture gesture){
    //     if(gesture.handRequirement == HandRequirement.TwoHands){
    //         runner.config.NumHands = 2;
    //     }else{
    //         runner.config.NumHands = 1;
    //     }
    // }
}
