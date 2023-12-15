using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Mobile.Base.Lidar;


namespace TeleopReachy
{
    public class WarningLidarObstacleDetectedUIManager : MonoBehaviour
    {
        private DataMessageManager dataController;
        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        [SerializeField]
        private Text warningMessage;

        private string messageToDisplay;

        private bool needUpdateWarningMessage;
        private bool wantWarningMessageDisplayed;

        private Coroutine limitDisplayInTime;

        private UserMobilityInput userMobilityInput;

        void Start()
        {
            dataController = DataMessageManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            dataController.event_OnLidarDetectionUpdate.AddListener(ShowWarningMessage);
            robotStatus.event_OnStopTeleoperation.AddListener(HideWarningMessage);

            needUpdateWarningMessage = false;
            wantWarningMessageDisplayed = false;

            transform.ActivateChildren(false);
        }

        void ShowWarningMessage(LidarObstacleDetectionEnum lidarStatus)
        {
            if(lidarStatus == LidarObstacleDetectionEnum.ObjectDetectedSlowdown)
            {
                messageToDisplay = "Close to obstacle: mobile base slowed down";
            }
            else
            {
                messageToDisplay = "Obstacle detected: can't move in direction";
            }
            if (robotStatus.IsRobotTeleoperationActive())
            {
                wantWarningMessageDisplayed = true;
                needUpdateWarningMessage = true;
            }
        }

        void Update()
        {
            if(needUpdateWarningMessage)
            {
                warningMessage.text = messageToDisplay;
                if(wantWarningMessageDisplayed) 
                {
                    if(limitDisplayInTime != null) StopCoroutine(limitDisplayInTime);
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