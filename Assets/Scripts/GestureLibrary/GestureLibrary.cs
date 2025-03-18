using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class GestureLibrary : MonoBehaviour
{
    public static GestureLibrary instance;
    // Preloaded gestures, gestures that comes with the application
    [SerializeField] private List<Gesture> preLoadedGestures = new List<Gesture>();

    // Loaded Gestures that any script can access
    private List<Gesture> loadedGestures = new List<Gesture>();

    // Download location path
    string downloadPath = string.Empty;

    // Start is called before the first frame update
    void Start(){
        // Set the instance to this gameobject, destroy if the scene already has it
        if(instance == null){
            instance = this;
        }else{
            Destroy(gameObject);
        }
        // Dont destory this library object, so we can use it anywhere on the scene
        DontDestroyOnLoad(gameObject);
        
        // Set the application download path
        downloadPath = Path.Combine(Application.persistentDataPath,"DownloadedGestures");

        // Debug.Log(Application.persistentDataPath);
        // Debug.Log(Directory.Exists(downloadPath));
        LoadAllGestures();
    }

    private void LoadAllGestures(){
        loadedGestures.Clear(); // Clear the loaded gestures first
        loadedGestures.AddRange(preLoadedGestures); // Add the preloaded gestures

        // Load the downloaded gestures
        LoadDownloadedGestures();
    }
    private void LoadDownloadedGestures(){
        if(!Directory.Exists(downloadPath)) return; // The directory for downloaded gestures doesn't exist
        
        List<Gesture> tempGestures = new List<Gesture>();

        string[] files = Directory.GetFiles(downloadPath, "*.json"); // Get the files with .json type

        // Load the downloaded gestures into temporary, so we can update their sequence references later
        foreach (string file in files){
            string json = File.ReadAllText(file);

            // Create a new ScriptableObject instance
            Gesture newGesture = ScriptableObject.CreateInstance<Gesture>();

            // Overwrite the new instance with JSON data
            JsonUtility.FromJsonOverwrite(json, newGesture);

            // Ensure Scriptable Object has a valid name
            newGesture.name = Path.GetFileNameWithoutExtension(file); // Avoids Unity issues with spaces

            tempGestures.Add(newGesture);
        }

        // Update the sequence references from the temporary gestures list
        foreach (var gesture in tempGestures){
            // Convert sequence step names into actual Gesture references
            Gesture[] updatedSequence = new Gesture[gesture.sequence.Length];
            for (int i = 0; i < gesture.sequence.Length; i++){
                updatedSequence[i] = FindGestureByName(gesture.sequenceString[i], tempGestures);
            }
            gesture.sequence = updatedSequence;
        }

        Debug.Log($"Added {tempGestures.Count} downloaded gesture json data.");
        loadedGestures.AddRange(tempGestures);
    }

    private Gesture FindGestureByName(string name, List<Gesture> tempGestures){
        return tempGestures.Find(g => g.name == name);
    }

    public List<Gesture> GetLoadedGestures(){
        return loadedGestures;
    }
}
