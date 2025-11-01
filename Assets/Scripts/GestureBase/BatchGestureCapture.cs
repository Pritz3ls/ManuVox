using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using Mediapipe.Unity.Sample;
using System.Linq;

public class BatchGestureCapture : GestureBase{
    [Header("Components")]
    [SerializeField] private HandLandmarkerRunner runner;
    [SerializeField] private GameObject captureNotify;

    [Header("Capture Batch")]
    [SerializeField] private int numHands = 1; 
    [Tooltip("Using this script requires intensive knowledge. Beware!")]
    [SerializeField] private List<Gesture> gestureSaveList = new List<Gesture>();
    [Range(2, 5)]
    [SerializeField] private float captureDelay = 2f;
    [SerializeField] private Sprite[] imageReferences;
    [SerializeField] private BatchType type;

    private int curIndex = 0;
    private int length = 0;
    bool isCapturing = false;

    private void Awake() {
        runner.config.NumHands = numHands;
    }

    void Start(){
        Application.targetFrameRate = 60;
        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart(){
        yield return new WaitForSecondsRealtime(1f);
        length = ImageSourceProvider.ImageSource.sourceCandidateNames.ToArray().Length;

        curIndex = 0;

        // if(gestureSaveList.Length > 15){
        //     Debug.LogError("Batch exceeded max point '15' revert back.");
        //     yield return null;
        // }

        if(gestureSaveList.Count != length){
            Debug.LogError("Batch not matched with the source list!");
            enabled = false;
        }else if(imageReferences.Length != length){
            Debug.LogError("Images length not matched with source list");
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update(){
        if(Input.GetKeyDown(KeyCode.Space) && !isCapturing){
            StartCoroutine(BatchCapture());
        }
    }
    IEnumerator BatchCapture(){
        Debug.LogWarning("Starting Batch Capture...");
        isCapturing = true;
        yield return StartCoroutine(VerifyNamesAndMatch());

        foreach (var gesture in gestureSaveList){
            yield return StartCoroutine(CaptureGesture(gesture));
            curIndex++;

            // if (curIndex % 5 == 0) {
            //     #if UNITY_EDITOR
            //     UnityEditor.AssetDatabase.SaveAssets();
            //     #endif
            // }
        }
        
        yield return new WaitForSecondsRealtime(1f);

        #if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
        #endif
        
        Debug.LogWarning("Batch Capture Finished!");
        gameObject.SetActive(true);
        runner.Stop();
    }

    public void UpdateSource(){
        var imageSource = ImageSourceProvider.ImageSource;
        imageSource.SelectSource(curIndex);
        RestartBaseRunnger(true);
    }
    IEnumerator VerifyNamesAndMatch(){
        Debug.LogWarning("Verifying...");
        for (int i = 0; i < gestureSaveList.Count; i++){
            string[] aString = gestureSaveList[i].name.Split('_','-');
            string[] bString = imageReferences[i].name.Split('_','-');

            if(!aString[1].Contains(bString[1])){
                Debug.LogError($"Detected unmatched names {gestureSaveList[i].name} : {imageReferences[i].name} from gesture savelist to image references.");
                break;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
        Debug.LogWarning("Verified, continuing to batch capture");
    }

    IEnumerator CaptureGesture(Gesture currentGesture){
        UpdateSource(); // Update the image source
        yield return new WaitForSecondsRealtime(captureDelay);

        switch (type){
            case BatchType.All:
                yield return StartCoroutine(CapturePosition(currentGesture));
                currentGesture.SetReferenceImage(imageReferences[curIndex]);
            break;
            case BatchType.Check:
                if(IsGesturePositionIsEmpty(currentGesture)){
                    Debug.LogWarning($"Detected {currentGesture.name} has an empty gesture positions, capturing.");
                    yield return StartCoroutine(CapturePosition(currentGesture));

                    currentGesture.SetReferenceImage(imageReferences[curIndex]);
                }else{
                    Debug.LogWarning($"{currentGesture.name} has gesture positions, setting image instead.");
                    currentGesture.SetReferenceImage(imageReferences[curIndex]);
                }
            break;
            case BatchType.CaptureOnly:
                yield return StartCoroutine(CapturePosition(currentGesture));
            break;
            case BatchType.ImageOnly:
                currentGesture.SetReferenceImage(imageReferences[curIndex]);
            break;
        }

        IEnumerator CapturePosition(Gesture currentGesture){
            if(currentGesture.handRequirement == HandRequirement.OneHand){
                if(IsRightHanded()){
                    currentGesture.rightHandPositions = GetLandMarks(0);
                    currentGesture.leftHandPositions = GetLandMarks_Reversed(0);
                    Debug.LogWarning("Capture is Right Handed");
                }else{
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
            }else{
                if(IsTwoHandsActive()){
                    currentGesture.leftHandPositions = GetLandMarks(0);
                    currentGesture.rightHandPositions = GetLandMarks(1);
                }else{
                    Debug.LogError("Detected only one hand active! Make sure to capture two hands,");
                    yield return null;
                }
            }
        }

        // Debug.LogWarning($"Successfully Captured Gesture Name {currentGesture.name}");
        // yield return StartCoroutine(DisplayLog($"Captured and Saved Gesture: {currentGesture.name}\nAssigned Reference image: {imageReferences[curIndex].name}"));
        Debug.LogWarning($"Saved gesture data: {currentGesture.name}");

        #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(currentGesture);
        #endif
    }

    private bool IsGesturePositionIsEmpty(Gesture currentGesture){
        return currentGesture.leftHandPositions.Length == 0 || currentGesture.leftHandPositions[0] == Vector2.zero;
    }

    public void RestartBaseRunnger(bool forceRestart = false){
        if (runner == null){
          return;
        }

        if (forceRestart){
            if (runner != null){
                runner.Play();
            }
        }else{
            if (runner != null){
                runner.Resume();
            }
        }
    }

    public enum BatchType{
        All, Check, CaptureOnly, ImageOnly
    }
}