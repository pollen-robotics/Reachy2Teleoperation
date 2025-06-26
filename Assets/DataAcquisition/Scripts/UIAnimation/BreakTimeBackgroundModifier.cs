using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Data.Acquisition;


namespace DataAcquisition
{
    public class BreakTimeBackgroundModifier : MonoBehaviour
    {
        private bool needUpdate = false;
        private bool currentState;
        private bool previousState;

        private TeleopReachy.RobotStatus robotStatus;

        void Start()
        {
            robotStatus = TeleopReachy.RobotDataManager.Instance.RobotStatus;
            previousState = robotStatus.AreRobotMovementsSuspended();
        }

        void Update()
        {
            currentState = robotStatus.AreRobotMovementsSuspended();
            if (currentState != previousState)
            {
                if (currentState)
                {
                    GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
                }
                else
                {
                    GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
                }
            }
            previousState = currentState;
        }
    }
}
