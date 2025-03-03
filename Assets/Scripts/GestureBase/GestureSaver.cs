using System.Collections;
using UnityEngine;

public class GestureSaver : GestureBase
{
    [SerializeField] private Gesture currentGesture;
    [SerializeField] private float captureDelay = 1f;
    // Update is called once per frame
    void Update(){
        if(Input.GetKeyDown(KeyCode.Space)){
            StartCoroutine(SaveGesture());
        }
    }
    IEnumerator SaveGesture(){
        yield return new WaitForSeconds(captureDelay);
        currentGesture.leftHandPositions = GetLandMarks(0);
        
        if(currentGesture.handRequirement == HandRequirement.OneHand){
            if(IsTwoHandsActive()){
                currentGesture.rightHandPositions = GetLandMarks(1);
            }else{
                Debug.LogWarning("To ensure left handedness compatibility, capture two hands.");
            }
        }else{
            if(IsTwoHandsActive()){
                currentGesture.rightHandPositions = GetLandMarks(1);
            }else{
                Debug.LogError("Detected only one hand active! Make sure to capture two hands,");
            }
        }
        Debug.LogWarning($"Saved Gesture Name {currentGesture.name}");
    }
}
