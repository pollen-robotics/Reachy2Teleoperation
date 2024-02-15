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
        private UserMobilityInput mobilityInput;

        void Start()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
            transform.ActivateChildren(false);
        }

        void Init()
        {
            mobilityInput = UserInputManager.Instance.UserMobilityInput;
        }

        void Update()
        {
            if(robotStatus.IsRobotTeleoperationActive())
            {
                Vector2 mobileBaseTranslation = mobilityInput.GetMobileBaseDirection();
                Vector2 mobileBaseRotation = mobilityInput.GetAngleDirection();
                if(mobileBaseRotation != new Vector2(0, 0) || mobileBaseTranslation != new Vector2(0, 0))
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