using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class BatteryUIManager : MonoBehaviour
    {
        [SerializeField]
        private Text batteryValue;
        [SerializeField]
        private Text batteryStatusText;
        [SerializeField]
        private RawImage batteryIcon;

        private RobotErrorManager errorManager;
        private DataMessageManager dataController;

        private Coroutine warningEnd;

        private bool hasWarningActivated;
        private static float batteryLevelValue;

        private bool needUpdateUI;

        private RobotConfig robotConfig;

        private string statusText;
        private Color32 statusColor;

        private bool isBatteryInfoAvailable;

        void Start()
        {
            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(ConfigChanged);

            errorManager = RobotDataManager.Instance.RobotErrorManager;
            errorManager.event_OnWarningLowBattery.AddListener(WarningLowBattery);
            errorManager.event_OnErrorLowBattery.AddListener(ErrorLowBattery);

            dataController = DataMessageManager.Instance;
            dataController.event_OnBatteryUpdate.AddListener(UpdateBatteryLevel);

            needUpdateUI = true;
            hasWarningActivated = false;
            isBatteryInfoAvailable = false;

            ConfigChanged();
        }

        void Update()
        {
            if (needUpdateUI)
            {
                if (robotConfig.HasMobileBase())
                {
                    if (isBatteryInfoAvailable)
                    {
                        if (batteryValue != null) batteryValue.text = "Voltage : " + batteryLevelValue + " V";

                        if (!hasWarningActivated)
                        {
                            statusText = "Battery OK";
                            statusColor = ColorsManager.blue;
                        }
                        else
                        {
                            if (warningEnd != null) StopCoroutine(warningEnd);
                            warningEnd = StartCoroutine(KeepOneSecond());
                        }
                    }
                    else
                    {
                        if (batteryValue != null) batteryValue.text = "No voltage information";
                        statusText = "Missing mobility service";
                        statusColor = ColorsManager.purple;
                    }
                }
                else
                {
                    if (batteryValue != null) batteryValue.text = "No voltage information";
                    statusText = "No battery information";
                    statusColor = ColorsManager.black;
                }
                UpdateUI();
                needUpdateUI = false;
            }
        }

        void UpdateUI()
        {
            if (batteryStatusText != null)
            {
                batteryStatusText.text = statusText;
                batteryStatusText.color = statusColor;
            }
            if (batteryIcon != null)
            {
                batteryIcon.color = statusColor;
            }
        }

        void UpdateBatteryLevel(float batteryLevel)
        {
            batteryLevelValue = batteryLevel;
            needUpdateUI = true;
        }

        void WarningLowBattery(float batteryLevel)
        {
            SetErrorBatteryMessage("Low battery", ColorsManager.orange);
        }

        void ErrorLowBattery(float batteryLevel)
        {
            SetErrorBatteryMessage("No battery", ColorsManager.purple);
        }

        private void SetErrorBatteryMessage(string message, Color32 color)
        {
            hasWarningActivated = true;
            statusText = message;
            statusColor = color;
            needUpdateUI = true;
        }

        IEnumerator KeepOneSecond()
        {
            yield return new WaitForSeconds(1);
            hasWarningActivated = false;
        }

        void ConfigChanged()
        {
            if (robotConfig.HasMobileBase())
            {
                isBatteryInfoAvailable = true;
            }
            batteryLevelValue = errorManager.previousBatteryLevel;
            errorManager.CheckBatteryStatus();
            needUpdateUI = true;
        }
    }
}
