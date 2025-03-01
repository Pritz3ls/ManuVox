using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class DeviceCameraTester : MonoBehaviour
{
    public TextMeshProUGUI cameraFindResText;
    // Start is called before the first frame update
    void Start(){
        #if PLATFORM_ANDROID
            if(!Permission.HasUserAuthorizedPermission(Permission.Camera)){
                Permission.RequestUserPermission(Permission.Camera);
            }
        #endif
        
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++){
            cameraFindResText.text += devices[i].name;
            Debug.Log(devices[i].name);
            if(devices[i].isFrontFacing){
                cameraFindResText.text += " Front Facing";
            }
            cameraFindResText.text += "\n";
        }   
    }
}
