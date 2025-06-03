using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DEBUG_LSHGestureConversion : MonoBehaviour
{
    public List<float> lshBucket = new List<float>();
    // Start is called before the first frame update
    void Start(){
        Gesture[] gestures = GestureLibrary.instance.GetLoadedGestures().ToArray();
        float temp;
        foreach (var item in gestures){
            temp = 0;
            for (int i = 0; i < item.rightHandPositions.Length; i++){
                temp += item.rightHandPositions[i].x;
            }
            lshBucket.Add(temp);
            EvaluateDuplicates();
        }
    }
    void EvaluateDuplicates(){
        float[] temp;
        foreach (var item in lshBucket){
            temp = lshBucket.FindAll(i => lshBucket.Contains(item)).ToArray();
            if(temp.Length == 1){
                return;
            }
            Debug.LogWarning($"{item} duplicates found in bucket with {temp.Length} times");
        }
    }
}
