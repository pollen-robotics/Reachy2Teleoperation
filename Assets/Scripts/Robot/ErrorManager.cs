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

        private int queueSize = 40;
        private Queue<int> lArmReachabilityQueue = new Queue<int>();
        private Queue<int> rArmReachabilityQueue = new Queue<int>();

        public UnityEvent event_OnWarningHighLatency;
        public UnityEvent event_OnWarningUnstablePing;
        public UnityEvent event_OnUnreachablePositionLArm;
        public UnityEvent event_OnUnreachablePositionRArm;
        public UnityEvent<float> event_OnWarningLowBattery;
        public UnityEvent<float> event_OnErrorLowBattery;
        public UnityEvent<List<string>> event_OnWarningMotorsTemperatures;
        public UnityEvent<List<string>> event_OnErrorMotorsTemperatures;

        public float previousBatteryLevel;

        void Start()
        {
            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdateTemperature.AddListener(CheckTemperatures);
            dataController.event_OnBatteryUpdate.AddListener(CheckBatteryLevel);
            dataController.event_OnReachabilityUpdate.AddListener(CheckReachability);

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

        public void CheckReachability(Dictionary<string, bool> Reachability)
        {
            bool lArmReachable;
            bool rArmReachable;
            if (Reachability.TryGetValue("l_arm", out lArmReachable))
            {
                if (lArmReachable) AddToQueue(lArmReachabilityQueue, 1);
                else AddToQueue(lArmReachabilityQueue, 0);
                if (Sum(lArmReachabilityQueue) > 35) event_OnUnreachablePositionLArm.Invoke();
            }
            if (Reachability.TryGetValue("r_arm", out rArmReachable))
            {
                if (rArmReachable) AddToQueue(rArmReachabilityQueue, 1);
                else AddToQueue(rArmReachabilityQueue, 0);
                if (Sum(rArmReachabilityQueue) > 35) event_OnUnreachablePositionRArm.Invoke();
            }
        }

        private void AddToQueue(Queue<int> queue, int element)
        {
            if (queue.Count == queueSize)
            {
                queue.Dequeue();
            }
            queue.Enqueue(element);
        }

        private int Sum(Queue<int> queue)
        {
            int sum = 0;
            foreach (int value in queue)
            {
                sum += value;
            }
            return sum;
        }
    }
}
