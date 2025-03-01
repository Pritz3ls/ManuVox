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

    public virtual Vector2[] GetLandMarks(){
        List<Vector2> relativeLandMarks = new List<Vector2>();
        Vector3 wristPos = HandLandmarkListAnnotation.instance.GetPointListAnnotation()[0].transform.position;
        for (int i = 1; i < 6; i++){
            relativeLandMarks.Add(HandLandmarkListAnnotation.instance.GetPointListAnnotation()[i*4].transform.position - wristPos);
        }
        return relativeLandMarks.ToArray();
    }
}
