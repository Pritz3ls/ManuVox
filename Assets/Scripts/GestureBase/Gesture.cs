using UnityEngine;


public enum HandRequirement{OneHand,TwoHands}
public enum GestureType{Static,Dynamic}
[CreateAssetMenu(fileName = "New Gesture", menuName = "Create Gesture", order = 0)]
public class Gesture : ScriptableObject {
    public HandRequirement handRequirement = HandRequirement.OneHand;
    public GestureType type = GestureType.Static;

    public Vector2[] rightHandPositions = new Vector2[5];
    public Vector2[] leftHandPositions = new Vector2[5];

    public Gesture[] sequence;
    [HideInInspector] public string[] sequenceString;
    public bool canBeStandalone = false;

    public string phraseOrWord;

}