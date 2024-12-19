using UnityEngine;

namespace TeleopReachy
{
    public class ReticleUIManager : MonoBehaviour
    {
        private ControllersManager controllers;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            motionSicknessManager = MotionSicknessManager.Instance;
            InitPosition();
            CheckReticleState();
        }

        void InitPosition()
        {
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
            else
            {
                transform.ActivateChildren(false);
            }
        }
    }
}