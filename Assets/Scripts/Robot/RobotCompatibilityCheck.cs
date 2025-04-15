using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class RobotCompatibilityCheck : MonoBehaviour
    {
        [SerializeField]
        private Text resolution;
        private string resolution_str;

        [SerializeField]
        private Text explanation;
        private string explanation_str;

        private bool needUpdate;

        private RobotConfig robotConfig;

        private ConnectionStatus connectionStatus;

        void Start()
        {
            needUpdate = false;
            robotConfig = RobotDataManager.Instance.RobotConfig;
            connectionStatus = ConnectionStatus.Instance;

            robotConfig.event_OnConfigChanged.AddListener(CheckCompatibility);
            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckCompatibility);
        }

        void Update()
        {
            if (needUpdate)
            {
                explanation.text = explanation_str;
                transform.ActivateChildren(true);
                resolution.text = resolution_str;
                needUpdate = false;
            }
        }

        public void CheckCompatibility()
        {
            if (connectionStatus.IsRobotReady() && !connectionStatus.IsCommandChannelReady())
            {
                explanation_str = "The teleoperation app is sending data on new channels that the robot doesn't recognize.";
                resolution_str = "Please update your robot webrtc service (or download an older version of the teleoperation app).";
                StartCoroutine(WaitToCheckChannelStatus(2));
            }

            if (robotConfig.GotReachyConfig())
            {
                string appApiVersion = ApplicationAPIVersion.Instance.GetApplicationAPIVersion();
                string robotApiVersion = robotConfig.GetRobotAPIVersion();

                switch (String.Compare(appApiVersion, robotApiVersion))
                {
                    case < 0:
                        explanation_str = "The teleoperation API version (<app_version>) differs from the robot API version (<robot_api>).";
                        resolution_str = "Please download a newer teleoperation app.";
                        needUpdate = true;
                        break;
                    case 0:
                        break;
                    case > 0:
                        explanation_str = "The teleoperation API version (<app_version>) differs from the robot API version (<robot_api>).";
                        resolution_str = "Please update your robot core service (or download an older version of the teleoperation app).";
                        needUpdate = true;
                        break;
                }
            }
        }

        IEnumerator WaitToCheckChannelStatus(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (connectionStatus.IsRobotReady() && !connectionStatus.IsCommandChannelReady()) needUpdate = true;
        }
    }
}