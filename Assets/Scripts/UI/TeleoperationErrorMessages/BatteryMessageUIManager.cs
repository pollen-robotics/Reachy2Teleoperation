using UnityEngine;
using System.Collections;

namespace TeleopReachy
{
    public class BatteryMessageUIManager : InformationalPanel
    {
        private RobotErrorManager errorManager;

        private float previousBatteryLevel = 0;
        private string textToDisplay;
        private Color32 backgroundColor;

        private Coroutine batteryWarningCoroutine;

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
            Debug.Log("Warning low battery");
            if (previousBatteryLevel == 0 || (previousBatteryLevel - batteryLevel > 0.2f))
            {
                textToDisplay = "Low battery";
                backgroundColor = ColorsManager.error_black;
                StartBatteryCoroutine(batteryLevel);
            }
        }

        void ErrorLowBattery(float batteryLevel)
        {
            Debug.Log("Error low battery");
            textToDisplay = "No battery";
            backgroundColor = ColorsManager.error_red;
            StartBatteryCoroutine(batteryLevel);
        }

        private void StartBatteryCoroutine(float batteryLevel)
        {
            previousBatteryLevel = batteryLevel;

            if (batteryWarningCoroutine != null)
            {
                StopCoroutine(batteryWarningCoroutine);
            }
            batteryWarningCoroutine = StartCoroutine(SetErrorBatteryMessage());
        }

        private IEnumerator SetErrorBatteryMessage()
        {
            ShowInfoMessage();
            yield return new WaitForSeconds(30);
        }
    }
}
