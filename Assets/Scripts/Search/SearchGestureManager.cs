using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchGestureManager : MonoBehaviour
{
    [SerializeField] private List<Gesture> gestures = new List<Gesture>();
    [SerializeField] private List<Gesture> result = new List<Gesture>();
    [SerializeField] private string searchQuery = string.Empty;

    void Start(){
        StartCoroutine(SearchGesture(searchQuery));
    }

    IEnumerator SearchGesture(string query){
        foreach (Gesture item in gestures){
            yield return new WaitForSeconds(0.05f);
            if(MatchCheckGestureName(item.name, query)){
                result.Add(item);
            }
        }
        yield return new WaitForSeconds(1f);
        Debug.Log($"Search finished with {result.Count} results.");
    }

    private bool MatchCheckGestureName(string gestureName, string query){
        return gestureName.Contains(query, System.StringComparison.Ordinal);
    }
}
