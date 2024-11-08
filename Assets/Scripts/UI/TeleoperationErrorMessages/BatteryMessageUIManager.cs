using UnityEngine;

namespace TeleopReachy
{
    public class BatteryMessageUIManager : InformationalPanel
    {
        private RobotErrorManager errorManager;

        private float previousBatteryLevel = 0;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.27f, 0.8f));

            errorManager = RobotDataManager.Instance.RobotErrorManager;
            errorManager.event_OnWarningLowBattery.AddListener(WarningLowBattery);
            errorManager.event_OnErrorLowBattery.AddListener(ErrorLowBattery);

            HideInfoMessage();
        }

        void WarningLowBattery(float batteryLevel)
        {
            if (previousBatteryLevel == 0 || (previousBatteryLevel - batteryLevel > 0.2f))
            {
                SetErrorBatteryMessage("Low battery", ColorsManager.error_black);
                previousBatteryLevel = batteryLevel;
            }
        }

        void ErrorLowBattery(float batteryLevel)
        {
            SetErrorBatteryMessage("No battery", ColorsManager.error_red);
            previousBatteryLevel = batteryLevel;
        }

        private void SetErrorBatteryMessage(string errorText, Color32 color)
        {
            textToDisplay = errorText;
            backgroundColor = color;
            ShowInfoMessage();
        }
    }
}
