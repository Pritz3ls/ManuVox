using UnityEngine;

[CreateAssetMenu(fileName = "New Gesture", menuName = "Create Gesture", order = 0)]
public class Gesture : ScriptableObject {
    public string gestureName = string.Empty;
    public Vector2[] relativeFingerPositions = new Vector2[5];
}