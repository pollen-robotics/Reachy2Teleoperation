using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class ReticleUIManager : MonoBehaviour
    {
        private RobotStatus robotStatus;
        private UserMobilityFakeMovement mobilityFakeMovement;

        void Start()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
            transform.ActivateChildren(false);
        }

        void Init()
        {
            mobilityFakeMovement = UserInputManager.Instance.UserMobilityFakeMovement;
        }

        void Update()
        {
            if(robotStatus.IsRobotTeleoperationActive())
            {
                if(mobilityFakeMovement.IsMoving())
                {
                        transform.ActivateChildren(true);
                }
                else {
                        transform.ActivateChildren(false);
                }
            }
        }
    }
}