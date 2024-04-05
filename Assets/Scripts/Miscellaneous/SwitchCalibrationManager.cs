using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TeleopReachy
{
    public class SwitchCalibrationManager : MonoBehaviour
    {
        private RobotCalibration robotCalib;
        private RobotStatus robotStatus;
        private float armSizeWithCalib;
        private Transform userTracker;
        private Transform headset;
        private char buttonPressed;
        public UnityEvent event_onbuttonPressed;

        
        // Start is called before the first frame update
        void Start()
        {
            if (RobotDataManager.Instance != null)
            {
                robotStatus = RobotDataManager.Instance.RobotStatus;
                if (robotStatus != null)
                {
                    robotStatus.event_OnStartTeleoperation.AddListener(SetUserArmSize);
                }
                else
                {
                    Debug.LogError("RobotStatus is null in RobotDataManager.");
                }
            }
            else
            {
                Debug.LogError("RobotDataManager.Instance is null.");
            }
            event_onbuttonPressed.AddListener(OnButtonPressed);
            
        }

        // Update is called once per frame
        void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
            if (Input.GetKeyDown(KeyCode.B))
                {
                    buttonPressed = 'B';
                    event_onbuttonPressed.Invoke();
                    Debug.Log("[switchcalib manager] button B pressed");
                }
                if (Input.GetKeyDown(KeyCode.X))
                {
                    buttonPressed = 'X';
                    event_onbuttonPressed.Invoke();
                    Debug.Log("[switchcalib manager] button X pressed");
                }
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    buttonPressed = 'Y';
                    event_onbuttonPressed.Invoke();
                    Debug.Log("[switchcalib manager] button Y pressed");
                }
            }
        }

        void SetUserArmSize()
        {
            Debug.Log("[switchcalib manager] setuserarmsize");
            userTracker = GameObject.Find("UserTracker").transform;
            headset = GameObject.Find("Main Camera").transform;
            armSizeWithCalib = UserSize.Instance.UserArmSize;
        }

        //buttonB : new calib, buttonX : old calib, buttonY : fake calib 
        void OnButtonPressed()
        {

            Debug.Log("[switchcalib manager] function Button " + buttonPressed );
            Transform newCalibTransform = GameObject.Find("NewUserCenter").transform;
            Transform oldCalibTransform = GameObject.Find("OldUserCenter").transform;
            Quaternion rotation = headset.rotation;
            Vector3 eulerAngles = rotation.eulerAngles;
            userTracker.rotation = Quaternion.Euler(0, eulerAngles.y, 0);



            switch (buttonPressed)
            {
                case 'B':
                    userTracker.position = newCalibTransform.position;
                    UserSize.Instance.UserArmSize = armSizeWithCalib;
                    break;
                case 'X':
                    userTracker.position = oldCalibTransform.position;
                    UserSize.Instance.UserArmSize = 0.0f;
                    break;
                case 'Y': //offset upside and forward
                    userTracker.position = new Vector3(newCalibTransform.position.x, newCalibTransform.position.y + 0.05f, newCalibTransform.position.z + 0.05f);
                    UserSize.Instance.UserArmSize = armSizeWithCalib + 0.1f; //10cm more on the armsize
                    break;
                default:
                    Debug.Log("[switchcalib manager] button not recognized");
                    break;
            }

            Debug.Log("[switchcalib manager] calib " + buttonPressed + " on");;
        }
        
    }
}