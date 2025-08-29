using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity;

public class GestureReference : GestureBase
{
    public Gesture gesture;
    public Image image;
    
    private void Start() {
        // image.sprite = Sprite;
    }
    // [SerializeField] private Transform wristTarget; // Assign in Inspector
    // [SerializeField] private float smoothFactor = 0.2f; // Adjust for smoother rotation

    // void Update(){
    //     if(FindObjectOfType<HandLandmarkListAnnotation>() == null) return;
    //     if (wristTarget == null) return;
    //     // Get wrist landmark (index 0 in MediaPipe)
    //     Vector2 wristPosition = GetLandmarks(0)[0];
    //     Quaternion wristRotation = GetRotation(); // Assuming a GetRotation() function

    //     // Apply position (if needed)
    //     wristTarget.position = Vector2.Lerp(wristTarget.position, wristPosition, smoothFactor);

    //     // Apply rotation smoothly
    //     wristTarget.rotation = Quaternion.Slerp(wristTarget.rotation, wristRotation, smoothFactor);
    // }

    // private Quaternion GetRotation(){
    //     // Example: Get rotation using wrist + index knuckle as reference
    //     Vector2 wrist = GetLandmarks(0)[0];
    //     Vector2 indexKnuckle = GetLandmarks(0)[5];

    //     Vector2 handDirection = (indexKnuckle - wrist).normalized;
    //     return Quaternion.LookRotation(handDirection);
    // }
}