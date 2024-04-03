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
        public float armSizeWithCalib;
        public Transform userTracker;
        public Transform headset;
        public char buttonPressed;
        // public UnityEvent event_onbuttonXPressed;
        // public UnityEvent event_onbuttonYPressed;
        // public UnityEvent event_onbuttonBPressed;
        public UnityEvent event_onbuttonPressed;

        
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("[switchcalib manager] start");
            userTracker = GameObject.Find("UserTracker").transform;
            headset = GameObject.Find("Main Camera").transform;
            robotCalib = new RobotCalibration();
            robotStatus = new RobotStatus();
            robotCalib.event_OnCalibChanged.AddListener(SetUserArmSize); 
            
            // event_onbuttonBPressed.AddListener(OnButtonBPressed);
            // event_onbuttonXPressed.AddListener(OnButtonXPressed);
            // event_onbuttonYPressed.AddListener(OnButtonYPressed);
            event_onbuttonPressed.AddListener(OnButtonPressed);
            
        }

        // Update is called once per frame
        void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
            if (Input.GetKeyDown(KeyCode.B))
                {
                    // event_onbuttonBPressed.Invoke();
                    buttonPressed = 'B';
                    event_onbuttonPressed.Invoke();
                    Debug.Log("[switchcalib manager] button B pressed");
                }
                if (Input.GetKeyDown(KeyCode.X))
                {
                    // event_onbuttonXPressed.Invoke();
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