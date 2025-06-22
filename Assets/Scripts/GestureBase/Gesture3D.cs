using UnityEngine;

public class Gesture3D : GestureBase
{
    [Header("Hand Model")]
    public Transform[] jointTargets = new Transform[21];  // Assign your 3D hand joints in Inspector

    [Header("Fake Depth Settings")]
    public float depthStep = 0.02f;    // Z offset between joints
    public float handScale = 1.0f;     // Overall scale

    [Header("Debug")]
    public bool logLandmarks = false;

    // Example structure: simulate 2D landmark input from MediaPipe
    public Vector2[] inputLandmarks = new Vector2[21]; // Update this array per frame

    // Fake Z depth values (flat layer for each finger)
    private float[] fakeZ = new float[21]
    {
        0.0f,     // 0 Wrist
        0.1f, 0.2f, 0.3f, 0.4f,   // Thumb
        0.1f, 0.2f, 0.3f, 0.4f,   // Index
        0.1f, 0.2f, 0.3f, 0.4f,   // Middle
        0.1f, 0.2f, 0.3f, 0.4f,   // Ring
        0.1f, 0.2f, 0.3f, 0.4f    // Pinky
    };

    void Update()
    {
        return;
        inputLandmarks = GetLandMarks(0);
        if (inputLandmarks.Length != 21 || jointTargets.Length != 21)
            return;

        Vector2 wrist2D = inputLandmarks[0];
        float scale = Vector2.Distance(inputLandmarks[0], inputLandmarks[12]); // Wrist to middle tip

        for (int i = 0; i < 21; i++)
        {
            // Normalize relative to wrist and scale
            Vector2 relative2D = (inputLandmarks[i] - wrist2D) / scale;
            
            // Add fake Z based on joint hierarchy
            float z = fakeZ[i] * depthStep;

            // Final 3D position with fake Z
            Vector3 targetPos = new Vector3(relative2D.x, relative2D.y, z) * handScale;

            // Apply to the model
            jointTargets[i].localPosition = targetPos;

            if (logLandmarks)
                Debug.Log($"[{i}] Fake3D Pos: {targetPos}");
        }
    }
}
