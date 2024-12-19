using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Reachy.Part.Mobile.Base.Lidar;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class WarningLidarObstacleDetectedUIManager : InformationalPanel
    {
        // TODO: remove DataMessageManager
        private DataMessageManager dataController;
        private RobotStatus robotStatus;

        private UserMobilityInput userMobilityInput;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.22f, 0.8f));
            SetMinimumTimeDisplayed(1);

            dataController = DataMessageManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            userMobilityInput = UserInputManager.Instance.UserMobilityInput;

            dataController.event_OnLidarDetectionUpdate.AddListener(ChooseMessageAndDisplay);

            HideInfoMessage();
        }

        void ChooseMessageAndDisplay(LidarObstacleDetectionEnum lidarStatus)
        {
            if (lidarStatus == LidarObstacleDetectionEnum.ObjectDetectedSlowdown)
            {
                textToDisplay = "Close to obstacle: mobile base slowed down";
            }
            else
            {
                textToDisplay = "Obstacle detected: can't move in direction";
            }
            if (userMobilityInput.GetMobileBaseDirection() != new Vector2(0, 0) || userMobilityInput.GetAngleDirection() != new Vector2(0, 0))
            {
                ShowInfoMessage();
            }
        }
    }
}