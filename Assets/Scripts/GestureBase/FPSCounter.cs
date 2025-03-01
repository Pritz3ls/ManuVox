using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float refreshTime;
    private void Start() {
        InvokeRepeating("UpdateFrameCounter", refreshTime, refreshTime);
    }
    private void UpdateFrameCounter() {
        int fps = (int)(1f / Time.unscaledDeltaTime);
        fpsText.text = $"FPS: {fps}";
    }
}
