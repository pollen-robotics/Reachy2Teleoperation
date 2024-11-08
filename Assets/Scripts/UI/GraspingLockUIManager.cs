using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class GraspingLockUIManager : InformationalPanel
    {
        private RobotStatus robotStatus;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.22f, 0.8f));

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnGraspingLock.AddListener(ChooseMessageAndDisplay);
            HideInfoMessage();
        }

        void ChooseMessageAndDisplay(bool graspLockActivated)
        {
            if (graspLockActivated)
                textToDisplay = "Grasping Lock Activated";
            else
                textToDisplay = "Grasping Lock Deactivated";
            ShowInfoMessage();
        }
    }
}