using UnityEngine;


public enum HandRequirement{OneHand,TwoHands}
public enum GestureType{Static,Dynamic}
public enum GestureContext{None,Letter,Number}
[CreateAssetMenu(fileName = "New Gesture", menuName = "Create Gesture", order = 0)]
public class Gesture : ScriptableObject {
    public HandRequirement handRequirement = HandRequirement.OneHand;
    public GestureType type = GestureType.Static;
    public GestureContext context;
    public Sprite referenceImage;

    public Vector2[] rightHandPositions;
    public Vector2[] leftHandPositions;

    public Gesture[] sequence;
    // [HideInInspector] public string[] sequenceString;
    public bool canBeStandalone = false;
    public string phraseOrWord;
    public string category;

    public void SetReferenceImage(Sprite sprite) => this.referenceImage = sprite;

    #if UNITY_EDITOR
    void OnValidate() {
        if (referenceImage == null) {
            Debug.LogWarning($"Reference Image on '{this.name}' was just set to null.");
        }
    }
    #endif
}