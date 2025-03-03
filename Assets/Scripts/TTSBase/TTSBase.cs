using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TTSBase : MonoBehaviour
{
    private AndroidJavaObject tts;  // Android TTS object
    [SerializeField] private TextMeshProUGUI ttsStatusText;
    [SerializeField] private TextMeshProUGUI inputText;
    private bool isTTSReady = false; // Flag to track if TTS is ready

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new TTSListener(this));
            }
        }
    }

    public void SetTTSReady(bool ready)
    {
        isTTSReady = ready;
        ttsStatusText.text = ready ? $"TTS Status: <Detected {tts.Call<string>("getDefaultEngine")}>" : "TTS Status: <Error>";
    }

    public void Speak()
    {
        if (!isTTSReady)
        {
            ttsStatusText.text = "TTS Status: Not Ready!";
            Debug.LogError("TTS is not initialized yet!");
            return;
        }

        string textToSpeak = inputText.text;

        // AndroidJavaObject audioManager = new AndroidJavaObject("android.media.AudioManager");
        // audioManager.Call("setStreamVolume", 3, audioManager.Call<int>("getStreamMaxVolume", 3), 0);

        if (string.IsNullOrEmpty(textToSpeak))
        {
            Debug.LogError("TTS: No text provided!");
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            ttsStatusText.text = $"TTS Status: Speaking \"{inputText.text}\"";

            using (AndroidJavaObject hashMap = new AndroidJavaObject("java.util.HashMap"))
            {
                hashMap.Call<string>("put", "utteranceId", "TTS_1");
                tts.Call<int>("speak", inputText.text, 0, hashMap);
            }
        }   
    }
}

public class TTSListener : AndroidJavaProxy
{
    private TTSBase ttsBase;

    public TTSListener(TTSBase baseScript) : base("android.speech.tts.TextToSpeech$OnInitListener")
    {
        ttsBase = baseScript;
    }

    public void onInit(int status)
    {
        if (status == 0) // SUCCESS
        {
            Debug.Log("TTS initialized successfully!");
            ttsBase.SetTTSReady(true); // Notify TTSBase that TTS is ready
        }
        else
        {
            Debug.LogError("TTS initialization failed!");
            ttsBase.SetTTSReady(false);
        }
    }
}
