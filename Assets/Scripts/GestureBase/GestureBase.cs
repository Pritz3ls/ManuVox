using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;

public class GestureBase : MonoBehaviour
{
    public virtual Vector2[] NormalizedLandMarks(Vector2[] landMarks){
        // Get the scale of the hand using the middle finger
        float scale = landMarks[2].magnitude;
        // prevent division by zero
        if(scale == 0) scale = 1f;

        Vector2[] normalizedPositions = new Vector2[landMarks.Length];
        // 
        for (int i = 0; i < landMarks.Length; i++){
            // Divide the positions using the scale
            normalizedPositions[i] = landMarks[i] / scale;
        }
        return normalizedPositions;
    }

    public virtual Vector2[] GetLandMarks(int index){
        List<Vector2> relativeLandMarks = new List<Vector2>();
        Vector3 wristPos = MultiHandLandmarkListAnnotation.instance[index].GetPointListAnnotation()[0].transform.position;
        for (int i = 1; i < 6; i++){
            relativeLandMarks.Add(MultiHandLandmarkListAnnotation.instance[index].GetPointListAnnotation()[i*4].transform.position - wristPos);
        }
        return NormalizedLandMarks(relativeLandMarks.ToArray());
    }
    public virtual Vector2[] GetAllLandmarks(int index){
        List<Vector2> landMarks = new List<Vector2>();
        for (int i = 0; i < 21; i++){
            landMarks.Add(MultiHandLandmarkListAnnotation.instance[index].GetPointListAnnotation()[i].transform.position);
        }
        return landMarks.ToArray();
    }
    public virtual Vector2[] GetLandMarks_Reversed(int index){
        List<Vector2> relativeLandMarks = new List<Vector2>();
        Vector3 wristPos = MultiHandLandmarkListAnnotation.instance[index].GetPointListAnnotation()[0].transform.position;
        for (int i = 1; i < 6; i++){
            relativeLandMarks.Add(-(MultiHandLandmarkListAnnotation.instance[index].GetPointListAnnotation()[i*4].transform.position - wristPos));
        }
        return NormalizedLandMarks(relativeLandMarks.ToArray());
    }
    public virtual bool IsTwoHandsTracked(){
        return MultiHandLandmarkListAnnotation.instance.gameObject.transform.childCount >= 2;
    }
    public bool IsOneHandctive(){
        bool handActive = MultiHandLandmarkListAnnotation.instance[0].gameObject.activeSelf; 
        return handActive;
    }
    public virtual bool IsTwoHandsActive(){
        if(!IsTwoHandsTracked()) return false;
        bool firstAvailableHand = MultiHandLandmarkListAnnotation.instance[0].gameObject.activeSelf;
        bool secondAvailableHand = MultiHandLandmarkListAnnotation.instance[1].gameObject.activeSelf;
        return firstAvailableHand && secondAvailableHand;
    }
    public virtual bool IsRightHanded(){
        return MultiHandLandmarkListAnnotation.instance[0].GetHandedness() == HandLandmarkListAnnotation.Hand.Right;
    }
}