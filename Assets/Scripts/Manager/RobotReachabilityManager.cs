using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Reachy.Part.Arm;
using System;


namespace TeleopReachy
{
    public class RobotReachabilityManager : MonoBehaviour
    {
        private DataMessageManager dataController;
        private RobotConfig robotConfig;

        Queue<bool> lArmReachabilityCounter;
        Queue<bool> rArmReachabilityCounter;
        private const int QUEUE_SIZE = 10;
        private const int ERROR_THRESHOLD = 7;

        private ReachabilityError lArmLastReachabilityError;
        private ReachabilityError rArmLastReachabilityError;

        public UnityEvent<ReachabilityError> event_OnLArmPositionUnreachable;
        public UnityEvent<ReachabilityError> event_OnRArmPositionUnreachable;
        public UnityEvent<ReachabilityError> event_OnArmIKFreeze;

        void Start()
        {
            lArmReachabilityCounter = new Queue<bool>(QUEUE_SIZE);
            rArmReachabilityCounter = new Queue<bool>(QUEUE_SIZE);

            robotConfig = RobotDataManager.Instance.RobotConfig;

            EventManager.StartListening(EventNames.OnStopTeleoperation, ReinitializeCounter);

            robotConfig.event_OnConfigChanged.AddListener(ReadyToCheckReachability);
            if (robotConfig.GotReachyConfig()) ReadyToCheckReachability();
        }

        private void ReadyToCheckReachability()
        {
            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdateReachability.AddListener(UpdateReachability);
        }

        private void ReinitializeCounter()
        {
            Debug.LogError("ReinitializeCounter");
            lArmReachabilityCounter.Clear();
            rArmReachabilityCounter.Clear();
        }

        private void UpdateReachability(Dictionary<int, List<ReachabilityAnswer>> reachabilityAnswer)
        {
            List<ReachabilityAnswer> lArmAnswers = reachabilityAnswer[(int)robotConfig.partsId["l_arm"].Id];
            List<ReachabilityAnswer> rArmAnswers = reachabilityAnswer[(int)robotConfig.partsId["r_arm"].Id];

            UpdateCounter(lArmAnswers, ref lArmReachabilityCounter, ref lArmLastReachabilityError);
            UpdateCounter(rArmAnswers, ref rArmReachabilityCounter, ref rArmLastReachabilityError);

            CheckReachability(lArmReachabilityCounter, ref event_OnLArmPositionUnreachable, ref lArmLastReachabilityError);
            CheckReachability(rArmReachabilityCounter, ref event_OnRArmPositionUnreachable, ref rArmLastReachabilityError);
        }

        private void UpdateCounter(List<ReachabilityAnswer> answers, ref Queue<bool> counter, ref ReachabilityError reachabilityError)
        {
            foreach(ReachabilityAnswer element in answers)
            {
                counter.Enqueue(!(bool)element.IsReachable);
                if(!(bool)element.IsReachable)
                {
                    reachabilityError = element.Description;
                }

                if (counter.Count > QUEUE_SIZE) counter.Dequeue();
            }
        }

        private void CheckReachability(Queue<bool> counter, ref UnityEvent<ReachabilityError> event_Unreachable, ref ReachabilityError reachabilityError)
        {
            int sum = 0;
            Debug.LogError(counter.Count);
            foreach (bool obj in counter)
            {
                sum += Convert.ToInt32(obj);
            }

            if (sum > ERROR_THRESHOLD)
            {
                if (reachabilityError == ReachabilityError.DistanceLimit)
                {
                    event_Unreachable.Invoke(reachabilityError);
                }

                if (reachabilityError == ReachabilityError.DiscontinuityFreeze || 
                reachabilityError == ReachabilityError.MultiturnFreeze)
                {
                    event_OnArmIKFreeze.Invoke(reachabilityError);
                }
            }
        }
    }
}
