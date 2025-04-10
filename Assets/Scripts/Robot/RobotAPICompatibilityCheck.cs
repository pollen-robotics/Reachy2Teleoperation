using System;
using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class RobotAPICompatibilityCheck : MonoBehaviour
    {
        [SerializeField]
        private Text resolution;
        private string resolution_str;

        private bool needUpdate;

        private RobotConfig robotConfig;

        void Start()
        {
            needUpdate = false;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            robotConfig.event_OnConfigChanged.AddListener(CheckCompatibility);
        }

        void Update()
        {
            if (needUpdate)
            {
                transform.ActivateChildren(true);
                resolution.text = resolution_str;
                needUpdate = false;
            }
        }

        public void CheckCompatibility()
        {
            if (robotConfig.GotReachyConfig())
            {
                string appApiVersion = ApplicationAPIVersion.Instance.GetApplicationAPIVersion();
                string robotApiVersion = robotConfig.GetRobotAPIVersion();
                switch (String.Compare(appApiVersion, robotApiVersion))
                {
                    case < 0:
                        resolution_str = "Please download a newer teleoperation app.";
                        needUpdate = true;
                        break;
                    case 0:
                        break;
                    case > 0:
                        resolution_str = "Please update your robot core service (or download an older version of the teleoperation app).";
                        needUpdate = true;
                        break;
                }
            }
        }
    }
}