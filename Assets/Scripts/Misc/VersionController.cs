using UnityEngine;

public static class VersionController{
    #region Miscellaneous
        public static string GetBasicAppVersion(){
            return $"ManuVox-{Application.version}-{Application.unityVersion}";
        }
        public static string GetFullAppVersionID(){
            return $"ManuVox-{Application.version}-{Application.companyName}-Unity-{Application.unityVersion}";
        }
    #endregion
}
