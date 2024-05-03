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


    public class SwitchCalibrationManager : Singleton<SwitchCalibrationManager>
    {
        private RobotStatus robotStatus;
        private ControllersManager controllers;
        public CalibrationType selectedCalibration = CalibrationType.NewCalib;
        private CalibrationType currentCalibration = CalibrationType.NewCalib;
        public UnityEvent event_NewCalibAsked;
        public UnityEvent event_OldCalibAsked;
        public UnityEvent event_FakeCalibAsked;

        
        // Start is called before the first frame update
        void Start()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            UnityEvent event_NewCalibAsked = new UnityEvent();
            UnityEvent event_OldCalibAsked = new UnityEvent();
            UnityEvent event_FakeCalibAsked = new UnityEvent();
            RandomSequence(); 
        }

        // Update is called once per frame
        void Update()
        {
            if (robotStatus.IsRobotTeleoperationActive())
            {
                CheckForCalibrationChange();
            }
        }

        void CheckForCalibrationChange()
        {
            if (selectedCalibration != currentCalibration)
            {
                currentCalibration = selectedCalibration;
                Debug.Log("[SwitchCalibrationManager] Calibration type changed to: " + currentCalibration.ToString());

                switch (selectedCalibration)
                {
                    case CalibrationType.NewCalib:
                        event_NewCalibAsked.Invoke();
                        break;
                    case CalibrationType.OldCalib:
                        event_OldCalibAsked.Invoke();
                        break;
                    case CalibrationType.FakeCalib:
                        event_FakeCalibAsked.Invoke();
                        break;
                }
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
    }
}
