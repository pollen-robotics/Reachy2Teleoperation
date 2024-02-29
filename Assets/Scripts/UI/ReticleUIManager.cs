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
        private UserMobilityFakeMovement mobilityFakeMovement;
        private RobotStatus robotStatus;
        private bool isReticleActive;
        private bool needUpdate;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
            isReticleActive = false;
            transform.ActivateChildren(isReticleActive);
        }

        void Init()
        {
            mobilityFakeMovement = UserInputManager.Instance.UserMobilityFakeMovement;
            mobilityFakeMovement.event_OnStartMoving.AddListener(StartMoving);
            mobilityFakeMovement.event_OnStopMoving.AddListener(StopMoving);
            motionSicknessManager = MotionSicknessManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(CheckReticleState);
            robotStatus.event_OnStopTeleoperation.AddListener(HideReticle);
        }

        void Update()
        {
            if(needUpdate)
            {
                needUpdate = false;
                if(motionSicknessManager.IsReticleOn && !motionSicknessManager.IsReticleAlwaysShown)
                {
                    transform.ActivateChildren(isReticleActive);
                }
            }
        }

        void StartMoving()
        {
            isReticleActive = true;
            needUpdate = true;
        }

        void StopMoving()
        {
            isReticleActive = false;
            needUpdate = true;
        }

        void CheckReticleState()
        {
            if (motionSicknessManager.IsReticleOn && motionSicknessManager.IsReticleAlwaysShown)
            {
                transform.ActivateChildren(true);
            }
        }

        void HideReticle()
        {
            transform.ActivateChildren(false);
        }
    }
}