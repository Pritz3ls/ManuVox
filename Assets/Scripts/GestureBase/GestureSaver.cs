using System.Collections;
using UnityEngine;

public class GestureSaver : GestureBase
{
    [SerializeField] private Gesture[] gestureSaveList;
    [SerializeField] private Gesture currentGesture;
    [SerializeField] private float captureDelay = 1f;
    // Update is called once per frame
    int curIndex = 0;

    private void Start() {
        curIndex = 0;
        currentGesture = gestureSaveList[curIndex];
    }
    void Update(){
        if(Input.GetKeyDown(KeyCode.Space)){
            StartCoroutine(SaveGesture());
        }
    }
    IEnumerator SaveGesture(){
        yield return new WaitForSeconds(captureDelay);
        if(currentGesture.handRequirement == HandRequirement.OneHand){
            currentGesture.leftHandPositions = GetLandMarks(0);
            if(IsTwoHandsActive()){
                currentGesture.rightHandPositions = GetLandMarks(1);
            }else{
                Debug.LogWarning("To ensure left handedness compatibility, capture two hands.");
                currentGesture.rightHandPositions = GetLandMarks_Reversed(0);
            }
        }else{
            if(IsTwoHandsActive()){
                currentGesture.leftHandPositions = GetLandMarks(0);
                currentGesture.rightHandPositions = GetLandMarks(1);
            }else{
                Debug.LogError("Detected only one hand active! Make sure to capture two hands,");
                yield return null;
            }
        }
        Debug.LogWarning($"Saved Gesture Name {currentGesture.name}");

        if(gestureSaveList.Length != 0){
            curIndex++;
            currentGesture = gestureSaveList[curIndex];
        }
    }
}
