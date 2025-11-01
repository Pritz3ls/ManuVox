using UnityEngine;

public class UI_CalibrationEvent : MonoBehaviour
{
    [SerializeField] private GameObject calibrationObj;
    #region Calibration Process
        // Possible speed options, the lower the faster
        // Might test the performance on varying speed
        /*
            1.5s - Slow signer
            1s   - Average Signer
            0.75s - Fast Signer

            gestureRecognition + contextual = total seconds;

            3s - Slow signer
            2s   - Average Signer
            1.5s - Fast Signer
        */
        public void SetupSpeedCalibration(){
            calibrationObj.SetActive(true);
            GestureRecognizer.instance.SetRecognizerState(false);
        }
        // Select sign speed with presets
        public void SelectSignSpeed(int option){
            switch (option){
                case 1:
                    GestureRecognizer.instance.SetTickSpeed(1.5f);
                break;
                case 2:
                    GestureRecognizer.instance.SetTickSpeed(1f);
                break;
                case 3:
                    GestureRecognizer.instance.SetTickSpeed(.75f);
                break;
            }
            calibrationObj.SetActive(false);
            GestureRecognizer.instance.SetRecognizerState(true);
        }
    #endregion
}