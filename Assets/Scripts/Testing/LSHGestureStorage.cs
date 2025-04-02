using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSHGestureStorage
{
    private Dictionary<int, List<Gesture>> hashBuckets = new Dictionary<int, List<Gesture>>();
    private int numHashes = 4; // More hashes = better accuracy, but slower

    // Generate multiple random vectors for hashing
    private Vector2[][] randomVectors;

    public LSHGestureStorage(int dimensions) {
        randomVectors = new Vector2[numHashes][];
        for (int i = 0; i < numHashes; i++) {
            randomVectors[i] = GenerateRandomVector(dimensions);
        }
    }

    // Hash function: Project gesture data onto random vectors
    private int HashGesture(Gesture gesture) {
        int hash = 0;
        for (int i = 0; i < numHashes; i++) {
            hash <<= 1;

            // Make sure you're using full random vector to calculate the dot product
            float dotProduct = Vector2.Dot(gesture.rightHandPositions[i], randomVectors[i][0]); 

            // Debug log for each dot product
            Debug.Log($"Gesture {gesture.name}: DotProduct {dotProduct} with randomVector {randomVectors[i]}");

            hash |= (dotProduct > 0) ? 1 : 0; // Convert to binary hash
        }
        return hash;
    }


    // Insert gesture into hash table
    public void AddGesture(Gesture gesture) {
        int hash = HashGesture(gesture);
        Debug.LogWarning($"{gesture.name} hashed to {hash}");
        if (!hashBuckets.ContainsKey(hash)) {
            hashBuckets[hash] = new List<Gesture>();
        }
        hashBuckets[hash].Add(gesture);
    }

    // Retrieve candidates from the same bucket
    public List<Gesture> GetCandidates(Gesture inputGesture) {
        int hash = HashGesture(inputGesture);
        return hashBuckets.ContainsKey(hash) ? hashBuckets[hash] : new List<Gesture>();
    }

    private Vector2[] GenerateRandomVector(int dimensions) {
        Vector2[] vector = new Vector2[dimensions];
        for (int i = 0; i < dimensions; i++) {
            // Use random x, y values to form a 2D vector for each landmark
            vector[i] = new Vector2(Random.value, Random.value);  // Ensure this is appropriate for your use case
        }
        return vector;
    }
}
