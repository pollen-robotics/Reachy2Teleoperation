using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class ErrorManager : MonoBehaviour
    {
        private DataMessageManager dataController;

        private RobotPingWatcher robotPing;

        private List<string> warningHotMotors;
        private List<string> errorOverheatingMotors;

        //private Queue<float> pingsQueue;
        //private const int PINGS_QUEUE_SIZE = 20;
        private const float THRESHOLD_WARNING_BATTERY_LEVEL = 24.5f;
        private const float THRESHOLD_ERROR_BATTERY_LEVEL = 23.1f;
        //private const float FPS_MINIMUM = 15f;
        public const float THRESHOLD_ERROR_MOTOR_TEMPERATURE = 54.0f;
        public const float THRESHOLD_WARNING_MOTOR_TEMPERATURE = 50.0f;

        public UnityEvent event_OnWarningHighLatency;
        public UnityEvent event_OnWarningUnstablePing;
        public UnityEvent<float> event_OnWarningLowBattery;
        public UnityEvent<float> event_OnErrorLowBattery;
        public UnityEvent<List<string>> event_OnWarningMotorsTemperatures;
        public UnityEvent<List<string>> event_OnErrorMotorsTemperatures;
        public UnityEvent<Dictionary<string, string>> event_OnStatusError;

        public float previousBatteryLevel;

        void Start()
        {
            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdateTemperature.AddListener(CheckTemperatures);
            dataController.event_OnBatteryUpdate.AddListener(CheckBatteryLevel);
            dataController.event_OnAuditUpdate.AddListener(CheckRobotStatus);

            robotPing = RobotDataManager.Instance.RobotPingWatcher;
            //pingsQueue = new Queue<float>();
        }

        void Update()
        {
            CheckPingQuality();
        }

        void CheckPingQuality()
        {
            if (robotPing.GetPing() > RobotPingWatcher.THRESHOLD_LOW_QUALITY_PING)
                event_OnWarningHighLatency.Invoke();
            else if (robotPing.GetIsUnstablePing())
                event_OnWarningUnstablePing.Invoke();
        }

        protected void CheckTemperatures(Dictionary<string, float> Temperatures)
        {
            warningHotMotors = new List<string>();
            errorOverheatingMotors = new List<string>();

            foreach (KeyValuePair<string, float> motor in Temperatures)
            {
                if (motor.Value >= THRESHOLD_ERROR_MOTOR_TEMPERATURE) errorOverheatingMotors.Add(motor.Key);
                else if (motor.Value >= THRESHOLD_WARNING_MOTOR_TEMPERATURE) warningHotMotors.Add(motor.Key);
            }

            if (warningHotMotors.Count > 0)
                event_OnWarningMotorsTemperatures.Invoke(warningHotMotors);
            if (errorOverheatingMotors.Count > 0)
                event_OnErrorMotorsTemperatures.Invoke(errorOverheatingMotors);
        }

        protected void CheckBatteryLevel(float batteryLevel)
        {
            previousBatteryLevel = batteryLevel;
            if (batteryLevel < THRESHOLD_ERROR_BATTERY_LEVEL)
                event_OnErrorLowBattery.Invoke(batteryLevel);
            else if (batteryLevel < THRESHOLD_WARNING_BATTERY_LEVEL)
                event_OnWarningLowBattery.Invoke(batteryLevel);
        }

        public void CheckBatteryStatus()
        {
            if (previousBatteryLevel > 0)
            {
                if (previousBatteryLevel < THRESHOLD_ERROR_BATTERY_LEVEL)
                    event_OnErrorLowBattery.Invoke(previousBatteryLevel);
                else if (previousBatteryLevel < THRESHOLD_WARNING_BATTERY_LEVEL)
                    event_OnWarningLowBattery.Invoke(previousBatteryLevel);
            }
        }

        public void CheckRobotStatus(Dictionary<string, string> RobotStatus)
        {
            //bool errorDetected = false;
            Dictionary<string, string> errors = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> status in RobotStatus)
            {
                if (status.Value != "Ok")
                {
                    //errorDetected = true;
                    errors.Add(status.Key, status.Value);
                }
            }
            event_OnStatusError.Invoke(errors);
        }
    }
}
