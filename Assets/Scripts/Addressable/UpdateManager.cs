using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System; // Added for StringComparison
using System.Collections.Generic; // Added for List<Gesture> type hint in var gestures

public class UpdateManager : MonoBehaviour{
    [SerializeField] private Button updateButton;
    [SerializeField] private Button updateListButton;
    [SerializeField] private TextMeshProUGUI gesturesListText;

    // Start is called before the first frame update
    void Start(){
        updateButton.onClick.AddListener(UpdateGesture);
        updateListButton.onClick.AddListener(UpdateList);
    }

    private void UpdateGesture(){
        GestureLibrary.instance.CheckForUpdatesAsync();
    }

    private void UpdateList(){
        // Ensure GestureLibrary instance and loadedGestures are available before proceeding
        if (GestureLibrary.instance == null) {
            Debug.LogError("GestureLibrary.instance is NULL! Cannot update list.");
            gesturesListText.SetText("Error: GestureLibrary not initialized.");
            return;
        }

        List<Gesture> gestures = GestureLibrary.instance.GetLoadedGestures(); // Get the list once
        if (gestures == null) {
            Debug.LogError("GestureLibrary.instance.GetLoadedGestures() returned NULL! Cannot update list.");
            gesturesListText.SetText("Error: Gesture list is null.");
            return;
        }

        int total = gestures.Count;
        gesturesListText.SetText($"Total Gestures from the library: {total}"); 

        foreach (var item in gestures){
            // THIS IS THE CRUCIAL NULL CHECK FOR THE 'item' ITSELF
            if (item == null){
                Debug.LogError("Found a NULL Gesture object in the loadedGestures list from GestureLibrary!");
                gesturesListText.text += "\n[NULL GESTURE ENTRY DETECTED]";
                continue; // Skip this null entry and go to the next one
            }

            // These Debug.LogWarnings will now only run if 'item' is NOT null
            Debug.LogWarning($"UpdateManager: Accessing GestureData Name: {item.name}");

            if(item.type == GestureType.Dynamic){
                string sequenceString = string.Empty;
                foreach (var sequence in item.sequence){
                    sequenceString += $"{sequence.name},\n";
                }
                Debug.LogWarning($"UpdateManager: Accessing GestureData Sequence:\n {sequenceString}");
            }

            // Use ?? "N/A" for category to handle null string values
            Debug.LogWarning($"UpdateManager: Accessing GestureData Category: {item.category ?? "N/A"}"); 
            
            // This line updates the UI Text, also with the safe null-coalescing operator
            gesturesListText.text += $"\n{item.name} + {item.category ?? "N/A"}";
        }
    }
}