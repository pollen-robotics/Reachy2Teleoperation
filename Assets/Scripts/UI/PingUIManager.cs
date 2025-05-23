using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace TeleopReachy
{
    public class PingUIManager : MonoBehaviour
    {
        [SerializeField]
        private Text pingValue;
        [SerializeField]
        private Text pingQualityText;
        [SerializeField]
        private RawImage pingIcon;

        private RobotPingWatcher pingWatcher;
        private RobotErrorManager errorManager;

        private Coroutine warningEnd;

        private bool hasWarningActivated;

        void Start()
        {
            pingWatcher = RobotDataManager.Instance.RobotPingWatcher;

            errorManager = RobotDataManager.Instance.RobotErrorManager;
            errorManager.event_OnWarningHighLatency.AddListener(WarningHighLatency);
            errorManager.event_OnWarningUnstablePing.AddListener(WarningUnstablePing);

            hasWarningActivated = false;
        }

        void Update()
        {
            float currentPing = pingWatcher.GetPing();
            if (pingValue != null) 
            {
                switch (currentPing)
                {
                    case -1000:
                        pingValue.text = "Ping failed";
                        break;
                    case -1:
                        pingValue.text = "Ping: waiting...";
                        break;
                    case 0:
                        pingValue.text = "Ping: <0.1 ms";
                        break;
                    default:
                        pingValue.text = "Ping: " + Math.Round(currentPing, 2) + " ms";
                        break;
                }
            }

            if (!hasWarningActivated)
            {
                if (currentPing >= 0)
                {
                    if (pingQualityText != null)
                    {
                        pingQualityText.text = "Good network connection";
                        pingQualityText.color = ColorsManager.blue;
                    }
                    if (pingIcon != null)
                    {
                        pingIcon.color = ColorsManager.blue;
                    }
                }
                else
                {
                    if (pingQualityText != null)
                    {
                        if (currentPing == -1000)
                        {
                            pingQualityText.text = "Unable to reach robot";
                            pingQualityText.color = ColorsManager.red;
                        }
                        if (currentPing == -1)
                        {
                            pingQualityText.text = "Trying to reach robot...";
                            pingQualityText.color = ColorsManager.blue;
                        }
                    }
                    if (pingIcon != null)
                    {
                        if (currentPing == -1000)
                        {
                            pingIcon.color = ColorsManager.red;
                        }
                        if (currentPing == -1)
                        {
                            pingIcon.color = ColorsManager.blue;
                        }
                    }
                }
            }
        }

        void WarningUnstablePing()
        {
            SetPingMessage("Unstable network connection", ColorsManager.orange);
        }

        void WarningHighLatency()
        {
            SetPingMessage("Low speed network connection", ColorsManager.purple);
        }

        private void SetPingMessage(string message, Color32 color)
        {
            if (warningEnd != null) StopCoroutine(warningEnd);
            hasWarningActivated = true;
            if (pingQualityText != null)
            {
                pingQualityText.text = message;
                pingQualityText.color = color;
            }
            if (pingIcon != null)
            {
                pingIcon.color = color;
            }
            warningEnd = StartCoroutine(KeepOneSecond());
        }

        IEnumerator KeepOneSecond()
        {
            yield return new WaitForSeconds(1);
            hasWarningActivated = false;
        }
    }
}
