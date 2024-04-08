using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TeleopReachy
{
    public class SwitchCalibrationManager : MonoBehaviour
    {
        private RobotStatus robotStatus;
        private ControllersManager controllers;
        private float armSizeWithCalib;
        private Transform userTracker;
        private Transform headset;
        private char buttonPressed;
        private bool buttonB = false;
        private bool buttonX = false;
        private bool buttonY = false;
        public UnityEvent event_onbuttonPressed;
        public AudioClip audioCalib_B;
        public AudioClip audioCalib_X;
        public AudioClip audioCalib_Y;
        private AudioSource audioSource;

        
        // Start is called before the first frame update
        void Start()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(SetUserArmSize);
            event_onbuttonPressed.AddListener(OnButtonPressed);
            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonY);
                controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out buttonB);

                
                if (buttonB)
                    {
                        buttonPressed = 'B';
                        event_onbuttonPressed.Invoke();
                        Debug.Log("[switchcalib manager] button B pressed");
                        buttonB = false;
                    }
                if (buttonX)
                    {
                        buttonPressed = 'X';
                        event_onbuttonPressed.Invoke();
                        Debug.Log("[switchcalib manager] button X pressed");
                        buttonX = false;
                    }
                if (buttonY)
                    {
                        buttonPressed = 'Y';
                        event_onbuttonPressed.Invoke();
                        Debug.Log("[switchcalib manager] button Y pressed");
                        buttonY = false;
                    }
            }
        }

        void PlayAudioClip(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        void SetUserArmSize()
        {
            Debug.Log("[switchcalib manager] setuserarmsize");
            userTracker = GameObject.Find("UserTracker").transform;
            headset = GameObject.Find("Main Camera").transform;
            armSizeWithCalib = UserSize.Instance.UserArmSize;
            controllers = ActiveControllerManager.Instance.ControllersManager;
            
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
                    PlayAudioClip(audioCalib_B);
                    break;
                case 'X':
                    userTracker.position = oldCalibTransform.position;
                    UserSize.Instance.UserArmSize = 0.0f;
                    PlayAudioClip(audioCalib_X);
                    break;
                case 'Y': //offset upside and forward
                    userTracker.position = new Vector3(newCalibTransform.position.x, newCalibTransform.position.y + 0.07f, newCalibTransform.position.z + 0.07f);
                    UserSize.Instance.UserArmSize = armSizeWithCalib + 0.1f; //10cm more on the armsize
                    PlayAudioClip(audioCalib_Y);
                    break;
                default:
                    Debug.Log("[switchcalib manager] button not recognized");
                    break;
            }

            Debug.Log("[switchcalib manager] COOKIE calib " + buttonPressed + " on");
        }   
    }
}