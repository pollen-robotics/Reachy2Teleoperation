using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Reachy.Part.Mobile.Base.Lidar;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class WarningLidarObstacleDetectedUIManager : LazyFollow
    {
        private DataMessageManager dataController;
        private RobotStatus robotStatus;
        //private RobotConfig robotConfig;

        [SerializeField]
        private Text warningMessage;

        private string messageToDisplay;

        private bool needUpdateWarningMessage;
        private bool wantWarningMessageDisplayed;

        private Coroutine limitDisplayInTime;

        private UserMobilityInput userMobilityInput;

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
            dataController = DataMessageManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            //robotConfig = RobotDataManager.Instance.RobotConfig;

            dataController.event_OnLidarDetectionUpdate.AddListener(ShowWarningMessage);
            EventManager.StartListening(EventNames.OnStopTeleoperation, HideWarningMessage);

            needUpdateWarningMessage = false;
            wantWarningMessageDisplayed = false;

            transform.ActivateChildren(false);
        }

        void ShowWarningMessage(LidarObstacleDetectionEnum lidarStatus)
        {
            if (lidarStatus == LidarObstacleDetectionEnum.ObjectDetectedSlowdown)
            {
                messageToDisplay = "Close to obstacle: mobile base slowed down";
            }
            else
            {
                messageToDisplay = "Obstacle detected: can't move in direction";
            }
            if (robotStatus.IsRobotTeleoperationActive())
            {
                if (userMobilityInput == null) userMobilityInput = UserInputManager.Instance.UserMobilityInput;
                if (userMobilityInput.GetMobileBaseDirection() != new Vector2(0, 0) || userMobilityInput.GetAngleDirection() != new Vector2(0, 0))
                {
                    wantWarningMessageDisplayed = true;
                    needUpdateWarningMessage = true;
                }
            }
        }

        void Update()
        {
            if (needUpdateWarningMessage)
            {
                warningMessage.text = messageToDisplay;
                if (wantWarningMessageDisplayed)
                {
                    if (limitDisplayInTime != null) StopCoroutine(limitDisplayInTime);
                    limitDisplayInTime = StartCoroutine(DisplayLimitedInTime());
                    transform.ActivateChildren(true);
                }
                else
                {
                    transform.ActivateChildren(false);
                }
                needUpdateWarningMessage = false;
            }
        }

        IEnumerator DisplayLimitedInTime()
        {
            yield return new WaitForSeconds(1);
            wantWarningMessageDisplayed = false;
            needUpdateWarningMessage = true;
        }

        void HideWarningMessage()
        {
            wantWarningMessageDisplayed = false;
            needUpdateWarningMessage = true;
        }
    }
}