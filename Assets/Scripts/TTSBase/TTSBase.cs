using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class TTSBase : MonoBehaviour
{
    private AndroidJavaObject tts;  // Android TTS object
    [SerializeField] private TextMeshProUGUI ttsStatusText;
    private bool isTTSReady = false; // Flag to track if TTS is ready

    void Start(){
        if (Application.platform == RuntimePlatform.Android){
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")){
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new TTSListener(this));
            }
        }
    }

    public void SetTTSReady(bool ready){
        isTTSReady = ready;

        UpdateTTSStatusText(ready);
    }

    private void UpdateTTSStatusText(bool value){
        if(ttsStatusText == null) return;
        ttsStatusText.text = value ? $"TTS Status: <Detected {tts.Call<string>("getDefaultEngine")}>" : "TTS Status: <Error>";
    }

    public void Speak(string textToSpeak){
        if (!isTTSReady){
            Debug.LogWarning("TTS is not initialized yet!");
            return;
        }

        // AndroidJavaObject audioManager = new AndroidJavaObject("android.media.AudioManager");
        // audioManager.Call("setStreamVolume", 3, audioManager.Call<int>("getStreamMaxVolume", 3), 0);

        if (string.IsNullOrEmpty(textToSpeak)){
            Debug.LogError("TTS: No text provided!");
            return;
        }

        if (Application.platform == RuntimePlatform.Android){
            using (AndroidJavaObject hashMap = new AndroidJavaObject("java.util.HashMap")){
                hashMap.Call<string>("put", "utteranceId", "TTS_1");
                tts.Call<int>("speak", textToSpeak, 0, hashMap);
            }
        }   
    }
}

public class TTSListener : AndroidJavaProxy{
    private TTSBase ttsBase;

    public TTSListener(TTSBase baseScript) : base("android.speech.tts.TextToSpeech$OnInitListener"){
        ttsBase = baseScript;
    }

    public void onInit(int status){
        // SUCCESS
        if (status == 0){
            Debug.Log("TTS initialized successfully!");
            ttsBase.SetTTSReady(true); // Notify TTSBase that TTS is ready
        }else{
            Debug.LogError("TTS initialization failed!");
            ttsBase.SetTTSReady(false);
        }
    }
}
