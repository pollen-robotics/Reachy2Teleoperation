using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Reachy.Part.Arm;


namespace TeleopReachy
{
    public class RobotReachabilityUIManager : MonoBehaviour
    {
        private DataMessageManager dataController;
        private ConnectionStatus connectionStatus;

        private Dictionary<string, float> panelTemperature;
        private Dictionary<string, string> panelStatus;

        private bool isStatePanelStatusActive;
        private bool needUpdatePanelInfo;

        void Awake()
        {
            if (Robot.IsCurrentRobotVirtual())
            {
                isStatePanelStatusActive = false;
                needUpdatePanelInfo = true;
                return;
            }

            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdateReachability.AddListener(UpdateReachability);
        }

        private void UpdateReachability(Dictionary<int, List<ReachabilityAnswer>> Temperatures)
        {
            
        }


        private void CheckTemperatureInfo()
        {
            
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
