using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class GraspingLockUIManager : LazyFollow
    {
        private RobotStatus robotStatus;

        private Coroutine limitDisplayInTime;

        [SerializeField]
        private Text infoMessage;

        private ControllersManager controllers;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                targetOffset = new Vector3(0, -0.22f, 0.8f);
            }
            else
            {
                targetOffset = new Vector3(0, -0.22f, 0.7f);
            }
            maxDistanceAllowed = 0;
            transform.ActivateChildren(false);
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnGraspingLock.AddListener(ShowMessage);
        }


        void ShowMessage(bool graspLockActivated)
        {
            if (graspLockActivated)
                infoMessage.text = "Grasping Lock Activated";
            else
                infoMessage.text = "Grasping Lock Deactivated";
            if (limitDisplayInTime != null) StopCoroutine(limitDisplayInTime);
            limitDisplayInTime = StartCoroutine(DisplayLimitedInTime());
        }

        IEnumerator DisplayLimitedInTime()
        {
            transform.ActivateChildren(true);
            yield return new WaitForSeconds(3);
            transform.ActivateChildren(false);
        }
    }
}