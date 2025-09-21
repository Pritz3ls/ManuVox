using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReferenceManager : MonoBehaviour
{
    [Header("Reference Components")]
    [Header("Search")]
    [SerializeField] private GameObject searchParent;
    [SerializeField] private GameObject searchResultPrefab;
    [SerializeField] private TMP_InputField searchInputField;
    
    [Header("Category")]
    [SerializeField] private GameObject categoryParent;
    [SerializeField] private GameObject returnSelectionButton;
    [SerializeField] private TextMeshProUGUI categoryText;

    [Header("Misc")]
    [SerializeField] private GameObject emptySearchResultTextObj;
    [SerializeField] private GestureViewerHandler viewerHandler;

    public void LoadCameraScene(){
        CustomSceneManager.instance.StartLoadScene("Android-Shipping-Recognition", 1, CustomFillOrigin.Right);
    }

    private void Update() {   
        if(!viewerHandler.IsViewerActive) return;
        // Hook with Android back button or back function
        if(Input.GetKeyDown(KeyCode.Escape)){
            viewerHandler.CloseViewer();
        }
    }
    public void SelectByCategory(TextMeshProUGUI TMP){
        searchInputField.text = TMP.text;
        searchInputField.gameObject.SetActive(false);
        returnSelectionButton.SetActive(true);
        categoryText.SetText($"{TMP.text} Category");
        
        SearchGesture();
    }
    public void ReturnFromCategorySelection(){
        searchInputField.text = string.Empty;
        returnSelectionButton.SetActive(false);
        emptySearchResultTextObj.SetActive(false);
        searchParent.SetActive(false);

        searchInputField.gameObject.SetActive(true);
        categoryParent.SetActive(true);
    }

    public void SearchGesture(){
        if(searchInputField.text == string.Empty){
            categoryParent.SetActive(true);

            searchParent.SetActive(false);
            emptySearchResultTextObj.SetActive(false);
            return;
        }
        List<Gesture> results = GestureLibrary.instance.SearchGestureByName(searchInputField.text);
        categoryParent.SetActive(false);

        emptySearchResultTextObj.SetActive(false);
        // Clear the current search results
        for (int i = 0; i < searchParent.transform.childCount; i++){
            Destroy(searchParent.transform.GetChild(i).gameObject);
        }
        
        // Return if the results were empty
        if(results.Count <= 0){
            // Change this to a text obj instead if there's no result
            emptySearchResultTextObj.SetActive(true);
            Debug.Log("Sorry but there's no results.");
            return;
        }

        // Iterate every results
        for (int i = 0; i < results.Count; i++){
            // Create an instance of the UI Object
            GameObject instance = Instantiate(searchResultPrefab, searchParent.transform.position, Quaternion.identity);

            // Set the instance parent to the UI Object
            instance.transform.SetParent(searchParent.transform);
            instance.transform.localScale = Vector3.one;
            
            // Set the name of the instance to the result name
            string resultName = $"{results[i].category} - {results[i].phraseOrWord}";
            instance.transform.name = resultName;
            // Set the name inside the instance child component of TextMeshProUGUI
            instance.GetComponentInChildren<TextMeshProUGUI>().SetText(resultName);

            Gesture funcGesture = results[i];

            // Assign a listener for onClick event to view the gesture with the results (gesture) data
            instance.GetComponent<Button>().onClick.AddListener(() => ViewGesture(funcGesture));
        }
        searchParent.SetActive(true);
    }

    // For now, let's debug the option
    public void ViewGesture(Gesture gestureData){
        Debug.Log($"Viewing {gestureData.name}");
        List<Sprite> sequence = new List<Sprite>();
        if (gestureData.sequence != null){
            sequence.AddRange(
                gestureData.sequence
                .Where(g => g != null && g.referenceImage != null)
                .Select(g => g.referenceImage)
            );
        }
        sequence.Add(gestureData.referenceImage);
        
        // StringBuilder debug = new StringBuilder();
        // debug.Append("Default:\n");
        // foreach (var item in sequence){
        //     debug.Append(item.name);
        // }
        // Debug.LogWarning(debug.ToString());
        // debug.Clear();

        // // sequence.Reverse();

        // debug.Append("Reversed:\n");
        // foreach (var item in sequence){
        //     debug.Append(item.name);
        // }
        // Debug.LogWarning(debug.ToString());
        // debug.Clear();
        
        viewerHandler.SetupViewer(gestureData.phraseOrWord, gestureData.category, sequence.ToArray());
    }
}
