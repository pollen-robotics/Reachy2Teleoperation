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

        void Awake()
        {
            if (Robot.IsCurrentRobotVirtual())
            {
                isStatePanelStatusActive = false;
                needUpdatePanelInfo = true;
                return;
            }

            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdateTemperature.AddListener(UpdateTemperatures);

            connectionStatus = WebRTCManager.Instance.ConnectionStatus;
            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckTemperatureInfo);

            CheckTemperatureInfo();

            isStatePanelStatusActive = true;
            needUpdatePanelInfo = false;
        }

        private void UpdateTemperatures(Dictionary<string, float> Temperatures)
        {
            panelTemperature = new Dictionary<string, float>();
            foreach (KeyValuePair<string, float> motor in Temperatures)
            {
                if (motor.Key.Contains("hand"))
                {
                    string[] nameParsed = motor.Key.Split("_hand_");
                    string actuatorName = nameParsed[0] + "_hand_temperature";

                    string panelName = actuatorName + nameParsed[1];
                    Debug.LogError(panelName);
                    panelTemperature.Add(panelName, motor.Value);
                }
                else
                {
                    panelTemperature.Add(motor.Key, motor.Value);
                }
            }
        }

        public float GetTemperature(string motor)
        {
            float temperature;
            if (panelTemperature != null && panelTemperature.TryGetValue(motor, out temperature))
            {
                return temperature;
            }
            else return 0;
        }

        private void CheckTemperatureInfo()
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
                transform.GetChild(2).gameObject.SetActive(isStatePanelStatusActive);
                transform.GetChild(1).ActivateChildren(!isStatePanelStatusActive);
            }
        }
    }
}
