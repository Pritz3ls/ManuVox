using UnityEngine;

public class GestureSaver : GestureBase
{
    [SerializeField] private Gesture currentGesture;
    public int test;

    // Update is called once per frame
    void Update(){
        if(Input.GetKeyDown(KeyCode.Space)){
            SaveGesture();
        }
    }
    void SaveGesture(){
        Vector2[] landMark = GetLandMarks();
        landMark = NormalizedLandMarks(landMark);
        currentGesture.relativeFingerPositions = landMark;
        Debug.Log($"Saved Gesture Name {currentGesture.name} with {landMark.Length} index");
    }
}
