using UnityEngine;

namespace TeleopReachy
{
    public class ReticleUIManager : MonoBehaviour
    {
        private RobotStatus robotStatus;

        private ControllersManager controllers;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
            HideReticle();
        }

        void Init()
        {
            motionSicknessManager = MotionSicknessManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(CheckReticleState);
            robotStatus.event_OnStopTeleoperation.AddListener(HideReticle);
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus)
            {
                transform.localPosition = new Vector3(0, 0, -550);
            }
            else if (controllers.headsetType == ControllersManager.SupportedDevices.MetaQuest3)
            {
                transform.localPosition = new Vector3(0, -50, -750);
            }
        }

        void CheckReticleState()
        {
            if (motionSicknessManager.IsReticleOn)
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