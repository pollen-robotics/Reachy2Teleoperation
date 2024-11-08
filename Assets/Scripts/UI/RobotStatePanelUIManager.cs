using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class RobotStatePanelUIManager : MonoBehaviour
    {
        private DataMessageManager dataController;
        private ConnectionStatus connectionStatus;

        private Dictionary<string, float> panelTemperature;
        private Dictionary<string, string> panelStatus;

        private bool isStatePanelStatusActive;
        private bool needUpdatePanelInfo;

        void OnEnable()
        {
            needUpdatePanelInfo = false;

            if (Robot.IsCurrentRobotVirtual())
            {
                isStatePanelStatusActive = false;
                needUpdatePanelInfo = true;
                return;
            }
            connectionStatus = ConnectionStatus.Instance;
            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckMotorsInfo);

            CheckMotorsInfo();
        }

        private void CheckMotorsInfo()
        {
            if (connectionStatus.AreRobotServicesRestarting())
            {
                transform.GetChild(2).GetChild(1).GetComponent<Text>().text = "Waiting for motors info...";
                transform.GetChild(2).GetChild(1).GetComponent<Text>().color = ColorsManager.blue;
                isStatePanelStatusActive = true;
            }
            else
            {
                if (!connectionStatus.IsRobotInDataRoom())
                {
                    transform.GetChild(2).GetChild(1).GetComponent<Text>().text = "No motors information";
                    transform.GetChild(2).GetChild(1).GetComponent<Text>().color = ColorsManager.red;
                    isStatePanelStatusActive = true;
                }
                else
                {
                    isStatePanelStatusActive = false;
                }
            }
            needUpdatePanelInfo = true;
        }

        void Update()
        {
            if(needUpdatePanelInfo)
            {
                needUpdatePanelInfo = false;
                transform.GetChild(1).ActivateChildren(!isStatePanelStatusActive);
                transform.GetChild(2).gameObject.SetActive(isStatePanelStatusActive);
            }
        }
    }
}
