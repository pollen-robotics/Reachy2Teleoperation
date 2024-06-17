using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace TeleopReachy
{
    public enum CalibrationType
    {
        NewCalib,
        OldCalib,
        FakeCalib
    }


    public class SwitchCalibrationManager : MonoBehaviour
    {

        private RobotStatus robotStatus;
        private ControllersManager controllers;
        private float leftArmSizeWithCalib;
        private float rightArmSizeWithCalib;
        private Transform userTracker;
        private Transform headset;
        private Transform newCalibTransform;
        private Transform oldCalibTransform;
        private Transform floorTransform;
        public CalibrationType selectedCalibration = CalibrationType.NewCalib;
        public CalibrationType currentCalibration = CalibrationType.NewCalib;

        
        // Start is called before the first frame update
        void Start()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(SetUserArmSize);
            Debug.Log("[SwitchCalibrationManager] Start : " + selectedCalibration.ToString());
        }

        // Update is called once per frame
        void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                CheckForCalibrationChange();
            }
        }

        public void RandomSequence() 
        {
            List<string> seq1 = new List<string>();
            List<string> seq2 = new List<string>();
            List<string> seq3 = new List<string>();
            List<string> seq = new List<string>();

            for (int i=1; i<4; i++)
            {
                seq1.Add($"1_{i}");
                seq2.Add($"2_{i}");
                seq3.Add($"3_{i}");
            }

            //select the calib to be done 2 times (different from task 2 and 3)
            int k = UnityEngine.Random.Range(1, 4);
            seq2.Add($"2_{k}");
            int l = UnityEngine.Random.Range(1, 4);
            seq3.Add($"3_{l}");

            System.Random rnd = new System.Random();
            seq1 = seq1.OrderBy(x => rnd.Next()).ToList();
            seq2 = seq2.OrderBy(x => rnd.Next()).ToList();
            seq3 = seq3.OrderBy(x => rnd.Next()).ToList();

            //concatenate the 3 lists in a list called seq
            seq.AddRange(seq1);
            seq.AddRange(seq2);
            seq.AddRange(seq3);

            Debug.Log("Random sequence for calib test : " + string.Join(", ", seq));
        }


        void CheckForCalibrationChange()
        {
            if (selectedCalibration != currentCalibration)
            {
                currentCalibration = selectedCalibration;
                Debug.Log("[SwitchCalibrationManager] Calibration type changed to: " + currentCalibration.ToString());
                ChangeCalibration();
                ChangeOriginofRobot();
            }
        }


        void SetUserArmSize()
        {
            Debug.Log("[switchcalib manager] setuserarmsize");
            userTracker = GameObject.Find("UserTracker").transform;
            headset = GameObject.Find("Main Camera").transform;
            newCalibTransform = GameObject.Find("NewUserCenter").transform;
            oldCalibTransform = GameObject.Find("OldUserCenter").transform;
            floorTransform = GameObject.Find("Floor").transform;
            floorTransform.position = new Vector3(headset.position.x, 0.0f, headset.position.z);
            floorTransform.rotation = Quaternion.Euler(0, headset.rotation.eulerAngles.y, 0);
            leftArmSizeWithCalib = UserSize.Instance.leftUserArmSize;
            rightArmSizeWithCalib = UserSize.Instance.rightUserArmSize;
            controllers = ActiveControllerManager.Instance.ControllersManager;
            RandomSequence();
            
        }


        void ChangeCalibration()
        {
            Quaternion rotation = headset.rotation;
            Vector3 eulerAngles = rotation.eulerAngles;
            userTracker.rotation = Quaternion.Euler(0, eulerAngles.y, 0);

            switch (selectedCalibration)
            {
                case CalibrationType.NewCalib:
                    userTracker.position = newCalibTransform.position;
                    UserSize.Instance.leftUserArmSize = leftArmSizeWithCalib;
                    UserSize.Instance.rightUserArmSize = rightArmSizeWithCalib;
                    break;
                case CalibrationType.OldCalib:
                    userTracker.position = oldCalibTransform.position;
                    UserSize.Instance.leftUserArmSize = 0.0f;
                    UserSize.Instance.rightUserArmSize = 0.0f;
                    break;
                case CalibrationType.FakeCalib: //offset upside and forward
                    userTracker.position = new Vector3(newCalibTransform.position.x+0.05f, newCalibTransform.position.y + 0.1f, newCalibTransform.position.z + 0.1f);
                    UserSize.Instance.leftUserArmSize = leftArmSizeWithCalib + 0.1f; //10cm more on the armsize
                    UserSize.Instance.rightUserArmSize = rightArmSizeWithCalib + 0.1f; //10cm more on the armsize
                    break;
            }
        }  

        void ChangeOriginofRobot() 
        {
            floorTransform.position = new Vector3(headset.position.x, 0.0f, headset.position.z);
            floorTransform.rotation = Quaternion.Euler(0, headset.rotation.eulerAngles.y, 0);
        }
    }
}