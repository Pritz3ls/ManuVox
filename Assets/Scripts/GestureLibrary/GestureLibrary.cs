using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets; // Required for Addressables
using UnityEngine.ResourceManagement.AsyncOperations; // Required for AsyncOperationStatus

public class GestureLibrary : MonoBehaviour
{
    public static GestureLibrary instance;
    // Preloaded gestures, gestures that comes with the application
    // Loaded Gestures that any script can access
    [Header("Preloaded Gestures Groups")]
    [SerializeField] private List<PreloadGesturesGroup> preloadGroups = new List<PreloadGesturesGroup>();

    List<Gesture> loadedGestures = new List<Gesture>();
    List<Gesture> referenceGestures = new List<Gesture>();

    // Download location path
    // string downloadPath = string.Empty;

    // Key for your Addressable Gesture Group
    [SerializeField] private string addressableGestureGroupKey = "RemoteGesture";

    // Start is called before the first frame update
    void Awake(){
        // Set the instance to this gameobject, destroy if the scene already has it
        if(instance == null){
            instance = this;
        }else{
            Destroy(gameObject);
        }
        // Dont destory this library object, so we can use it anywhere on the scene
        DontDestroyOnLoad(gameObject);

        PreloadGestures(); // Preload the gestures first
        SynchronizeGesturesAsync();
    }

    private void LoadReferenceOnlyGestures(){
        referenceGestures = loadedGestures.Where(g => g.type == GestureType.Dynamic || g.canBeStandalone).ToList();
    }

    private Gesture FindGestureByName(string name, List<Gesture> tempGestures){
        return tempGestures.Find(g => g.name == name);
    }
    public Gesture FindGestureByWord(string word){
        return loadedGestures.Find(g => string.Equals(g.phraseOrWord.Trim(), word.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public bool DoesGestureExistByWord(string word){
        return loadedGestures.Exists(g => string.Equals(g.phraseOrWord.Trim(), word.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public List<Gesture> GetLoadedGestures(){
        return loadedGestures;
    }

    // UI Manager Related Function
    public List<Gesture> SearchGestureByName(string text){
        return referenceGestures.FindAll(
            g => g.name.Contains(text, StringComparison.OrdinalIgnoreCase) || 
            g.phraseOrWord.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            g.category.Contains(text, StringComparison.OrdinalIgnoreCase)
        );
    }

    // Check for update (just detects new gestures)
    public async void CheckForUpdatesAsync(){
        // Use your existing NetworkUtils for IP check
        if (Application.internetReachability == NetworkReachability.NotReachable){
            PopupsManager.instance.Popup(PopupType.Error, "No internet detected. Please ensure you're connected to Wi-Fi."); 
            return;
        }
        // Check if there's catalog updates (optional but recommended)
        var catalogHandle = Addressables.CheckForCatalogUpdates(false);
        await catalogHandle.Task;
        
        if (catalogHandle.Status == AsyncOperationStatus.Succeeded && catalogHandle.Result.Count > 0){
            // Pull down the new catalog so GetDownloadSizeAsync can see new blobs
            var updateHandle = Addressables.UpdateCatalogs(catalogHandle.Result);
            await updateHandle.Task;
        }

        // Determine actual download size
        var sizeHandle = Addressables.GetDownloadSizeAsync(addressableGestureGroupKey);
        await sizeHandle.Task;

        // Only prompt if there are bytes to download
        if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result > 0){
            System.Action onConfirm = SynchronizeGesturesAsync;
            PopupsManager.instance.Popup(
                PopupType.Info,
                "New updates available! Download now?",
                new PopupEvent("Update", onConfirm, "Maybe later", null)
            );
        }else{
            PopupsManager.instance.Popup(
                PopupType.Info, 
                "No updates available!\nAll gestures are up to date!"
            );
        }
    }

    // Download gestures from CCD via | Load cached gestures via Addressables
    public async void SynchronizeGesturesAsync(){
        var handle = Addressables.LoadAssetsAsync<Gesture>(addressableGestureGroupKey, null);
        try{
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded){
                var downloadedGestures = handle.Result.ToList();

                // Update sequence references (if any)
                foreach (var gesture in downloadedGestures){
                    Gesture[] updatedSequence = new Gesture[gesture.sequence.Length];
                    for (int i = 0; i < gesture.sequence.Length; i++){
                        // Find the referenced gesture within the newly downloaded gestures or existing loaded gestures
                        updatedSequence[i] = FindGestureByName(gesture.sequence[i].name, downloadedGestures) ?? FindGestureByName(gesture.sequence[i].name, loadedGestures);
                    }
                    gesture.sequence = updatedSequence;
                }

                /* Deprecated */
                // loadedGestures.Clear();
                // PreloadGestures(); // Preload the gestures again
                // loadedGestures.AddRange(downloadedGestures);
                // LoadReferenceOnlyGestures(); // Refresh filtered list (e.g., for UI search)
                // Debug.Log($"[CCD] Successfully loaded {downloadedGestures.Count} gestures from CCD.");

                // --- IMPORTANT CHANGE HERE TO PREVENT DUPLICATION ---
                loadedGestures.Clear();
                PreloadGestures(); // Load built-in gestures first
                HashSet<string> existingNames = new HashSet<string>(loadedGestures.Select(g => g.name));

                // Merge CCD gestures without duplication
                foreach (var gesture in downloadedGestures) {
                    if (gesture == null) continue;
                    // If a gesture with the same name exists, replace it with the CCD one
                    Gesture existing = loadedGestures.FirstOrDefault(g => g.name == gesture.name);
                    if (existing != null) {
                        int index = loadedGestures.IndexOf(existing);
                        loadedGestures[index] = gesture;
                        Debug.Log($"[CCD] Updated gesture: {gesture.name}");
                    } else if (!existingNames.Contains(gesture.name)) { // Else just let the loaded gesture one on
                        loadedGestures.Add(gesture);
                        existingNames.Add(gesture.name);
                        Debug.Log($"[CCD] Added new gesture: {gesture.name}");
                    }
                }

                // Refresh reference list for search/UI
                LoadReferenceOnlyGestures();
                await Resources.UnloadUnusedAssets(); // Free old memory
                Debug.Log($"[CCD] Synced successfully — {loadedGestures.Count} total gestures after merge.");

            }
        }catch (System.Exception){
            Debug.LogError("[CCD] Failed to load addressable handle, preloading assets only");
            PopupsManager.instance.Popup(PopupType.Info, "Unable to connect to the server.\nRunning in offline mode.");
            loadedGestures.Clear();
            PreloadGestures();
        }
    }

    public async void DownloadUpdate(List<Gesture> lastGestures) {
        var handle = Addressables.LoadAssetsAsync<Gesture>(addressableGestureGroupKey, null);
        try{
            await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded){
                var downloadedGestures = handle.Result.ToList();

                // Update sequence references (if any)
                foreach (var gesture in downloadedGestures){
                    Gesture[] updatedSequence = new Gesture[gesture.sequence.Length];
                    for (int i = 0; i < gesture.sequence.Length; i++){
                        // Find the referenced gesture within the newly downloaded gestures or existing loaded gestures
                        updatedSequence[i] = FindGestureByName(gesture.sequence[i].name, downloadedGestures) ?? FindGestureByName(gesture.sequence[i].name, loadedGestures);
                    }
                    gesture.sequence = updatedSequence;
                }

                // --- IMPORTANT CHANGE HERE TO PREVENT DUPLICATION ---
                loadedGestures.Clear();
                PreloadGestures(); // Load built-in gestures first
                HashSet<string> existingNames = new HashSet<string>(loadedGestures.Select(g => g.name));

                // Merge CCD gestures without duplication
                foreach (var gesture in downloadedGestures) {
                    if (gesture == null) continue;
                    // If a gesture with the same name exists, replace it with the CCD one
                    Gesture existing = loadedGestures.FirstOrDefault(g => g.name == gesture.name);
                    if (existing != null) {
                        int index = loadedGestures.IndexOf(existing);
                        loadedGestures[index] = gesture;
                        Debug.Log($"[CCD] Updated gesture: {gesture.name}");
                    } else if (!existingNames.Contains(gesture.name)) { // Else just let the loaded gesture one on
                        loadedGestures.Add(gesture);
                        existingNames.Add(gesture.name);
                        Debug.Log($"[CCD] Added new gesture: {gesture.name}");
                    }
                }

                // Refresh reference list for search/UI
                LoadReferenceOnlyGestures();
                System.Action sub = () => {
                    Application.Quit();
                };
                PopupsManager.instance.Popup(PopupType.Info, "Updates download successfully!\nRestart now?", new PopupEvent(
                    "Yes", sub,
                    "Later", null
                ));
                await Resources.UnloadUnusedAssets(); // Free old memory
            }
        }catch (System.Exception){
            Debug.LogError("[CCD] Failed to load addressable handle, preloading assets only");
            PopupsManager.instance.Popup(PopupType.Info, "Unable to connect to the server.\nPlease try again later.");
            loadedGestures.Clear();
            PreloadGestures();
        }
    }

    private void PreloadGestures(){
        foreach (var group in preloadGroups){
            loadedGestures.AddRange(group.gestures);
        }
    }

    public string[] GetAllCategory() {
        HashSet<string> uniqueCategories = new HashSet<string>();
        
        foreach (var gesture in loadedGestures) {
            if (!string.IsNullOrWhiteSpace(gesture.category))
                uniqueCategories.Add(gesture.category.Trim());
        }

        return uniqueCategories.ToArray();
    }
    
    [System.Serializable]
    public class PreloadGesturesGroup{
        [SerializeField] private string groupName;
        public List<Gesture> gestures;
    }
}