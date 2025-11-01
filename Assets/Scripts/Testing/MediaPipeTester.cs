using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MediaPipeTester : GestureBase {
    public float warmupTime;
    public float tickRate = 0.1f;
    public List<float> confidenceLevels = new List<float>();
    public int amount = 50;
    private List<Vector2> previousLandmarks = new List<Vector2>();
    public float movementSensitivity = 0.0015f;
    public float distanceThreshold = 0.75f;
    private Vector2[] currentLandmarks;

    private void Start() {
        StartCoroutine(TrackConfidenceLevels());
    }

    private IEnumerator TrackConfidenceLevels(){
        yield return new WaitForSeconds(warmupTime); // Delay the startup of the confidence evaluation
        Debug.LogWarning("Confidence Tracker Started!");
        while (confidenceLevels.Count < amount){ // Record enough confidence level based on the amount presented
            yield return new WaitForSeconds(tickRate);
            RecordConfidence();
        }
        // End the evaluation after recording enough confidence levels
        EndEvaluation();
    }

    private void EndEvaluation() {
        if(confidenceLevels.Count == 0) {
            Debug.LogWarning("No confidence levels recorded.");
            return;
        }

        float averageConfidence = confidenceLevels.Average(); // Average the confidence level
        Debug.LogWarning($"Average Hand Detection Confidence Levels: {averageConfidence:0.00}");
        Debug.LogWarning("End of MediaPipe evaluation.");
    }

    private void RecordConfidence() {
        // No hands are active, return
        if(!IsOneHandctive()) {
            Debug.LogWarning("No hands are active, returning...");
            return;
        }

        // Retrieve all 21 landmarks extracted by MediaPipe
        currentLandmarks = GetAllLandmarks(0);
        if (currentLandmarks == null || currentLandmarks.Length == 0) return;

        // Add the computed confidence level using the extracted 21 landmarks
        confidenceLevels.Add(ComputeConfidence(currentLandmarks.ToList()));
    }

    public float ComputeConfidence(List<Vector2> currentLandmarks) {
        // Theres no previous landmarks yet, assume mid confidence level
        if (previousLandmarks.Count == 0) {
            previousLandmarks = new List<Vector2>(currentLandmarks);
            return 0.5f;
        }

        // Get the temporal stability and spatial stability using the current landmarks
        float temporalStability = ComputeTemporalStability(currentLandmarks);
        float spatialConsistency = ComputeSpatialConsistency(currentLandmarks);

        previousLandmarks = new List<Vector2>(currentLandmarks);

        float confidence = (temporalStability * 0.7f) + (spatialConsistency * 0.3f); // Compute the confidence level
        Debug.Log($"Logged {confidence:0.00} confidence level.");
        return Mathf.Clamp01(confidence); // Return the confidence level but clamped it using Mathf.Clamp01 so it doesn't exceed float value of 1
    }

    private float ComputeTemporalStability(List<Vector2> current) {
        float total = 0f;
        for (int i = 0; i < current.Count; i++) {
            // Compute the distance between the current landmark that was recorded and the previous landmark that was recorded
            float dist = Vector2.Distance(current[i], previousLandmarks[i]);
            // Lower dist = higher confidence
            total += Mathf.Clamp01(1f - (dist * movementSensitivity));
        }
        return total / current.Count; // Return the temporal stability by dividing the total with the total of the landmark counts
    }

    private float ComputeSpatialConsistency(List<Vector2> current) {
        // Refer to the MediaPipe documentation on what is the indexing of the hand landmarks ;>
        Vector2 wrist = current[0]; // Get the wrist position from the landmark with the 0 index
        Vector2 indexBase = current[5]; // Get the base index position from the landmark with the 5 index
        Vector2 pinkyBase = current[17]; // Get the base of the pinky position from the landmarks with the 17 index

        float handWidth = Vector2.Distance(indexBase, pinkyBase); // Compute the distance between the base of the index finger and the base of the pinky
        float handLength = Vector2.Distance(wrist, indexBase); // Compute the distance between the base of the wrist and the base of the index

        float ratio = handWidth / (handLength + 1e-5f); // Get the ratio of the handwitdth by dividing it with the handLength
        float deviation = Mathf.Abs(ratio - 1f); // Get the deviation from the ratio that was computed

        return Mathf.Clamp01(1f - (deviation / distanceThreshold)); // Return the spatial consistency
    }
}
