using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Reachy.Part.Arm;


namespace TeleopReachy
{
    public class RobotReachabilityManager : MonoBehaviour
    {
        private DataMessageManager dataController;
        private RobotConfig robotConfig;

        Queue<int> lArmReachabilityCounter;
        Queue<int> rArmReachabilityCounter;
        private int QUEUE_SIZE = 10;

        private ReachabilityError lArmLastReachabilityError;
        private ReachabilityError rArmLastReachabilityError;

        private bool isStatePanelStatusActive;
        private bool needUpdatePanelInfo;

        public UnityEvent<ReachabilityError> event_OnLArmPositionUnreachable;
        public UnityEvent<ReachabilityError> event_OnRArmPositionUnreachable;

        void Awake()
        {
            if (Robot.IsCurrentRobotVirtual())
            {
                isStatePanelStatusActive = false;
                needUpdatePanelInfo = true;
                return;
            }

            lArmReachabilityCounter = new Queue<int>(QUEUE_SIZE);
            rArmReachabilityCounter = new Queue<int>(QUEUE_SIZE);
   
            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdateReachability.AddListener(UpdateReachability);

            robotConfig = RobotDataManager.Instance.RobotConfig;
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

        private void UpdateCounter(List<ReachabilityAnswer> answers, ref Queue<int> counter, ref ReachabilityError reachabilityError)
        {
            foreach(ReachabilityAnswer element in answers)
            {
                if((bool)element.IsReachable) counter.Enqueue(0);
                else 
                {
                    counter.Enqueue(1);
                    reachabilityError = element.Description;
                }

                if (counter.Count > QUEUE_SIZE) counter.Dequeue();
            }
        }

        private void CheckReachability(Queue<int> counter, ref UnityEvent<ReachabilityError> event_Unreachable, ref ReachabilityError reachabilityError)
        {
            float mean = 0;
            foreach (int obj in counter)
            {
                mean += obj;
            }
            mean = mean / counter.Count;

            if(mean > 7)
            {
                event_Unreachable.Invoke(reachabilityError);
                Debug.LogError(reachabilityError);
            }
        }

        void Update()
        {
            if(needUpdatePanelInfo)
            {
                needUpdatePanelInfo = false;
                transform.GetChild(2).gameObject.SetActive(isStatePanelStatusActive);
                transform.GetChild(1).ActivateChildren(!isStatePanelStatusActive);
            }
        }
    }
}
