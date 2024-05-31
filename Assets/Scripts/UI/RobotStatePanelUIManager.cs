using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class RobotStatePanelUIManager : MonoBehaviour
    {
        private DataMessageManager dataController;
        private ConnectionStatus connectionStatus;

        //private RobotStatus robotStatus;

        private List<GameObject> actuators;

        private Dictionary<string, float> panelTemperature;
        private Dictionary<string, string> panelStatus;

        private bool isStatePanelStatusActive;
        private bool needUpdatePanel;

        void Awake()
        {
            if (Robot.IsCurrentRobotVirtual())
            {
                isStatePanelStatusActive = false;
                UpdateStatePanelStatus();
                return;
            }

            dataController = DataMessageManager.Instance;
            dataController.event_OnStateUpdateTemperature.AddListener(UpdateTemperatures);

            connectionStatus = WebRTCManager.Instance.ConnectionStatus;
            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckTemperatureInfo);

            actuators = new List<GameObject>();
            foreach (Transform child in transform.GetChild(1))
            {
                actuators.Add(child.gameObject);
            }

            CheckTemperatureInfo();

            isStatePanelStatusActive = true;
            needUpdatePanel = false;
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

                    panelTemperature.Add(panelName, motor.Value);
                }
                else
                {
                    string[] nameParsed = motor.Key.Split("_motor_");
                    string actuatorName = nameParsed[0] + "_temperature";

                    string panelName = actuatorName + "motor_" + nameParsed[1];

                    panelTemperature.Add(panelName, motor.Value);
                }
            }
            needUpdatePanel = true;
        }

        public float GetTemperature(string motor)
        {
            float temperature;
            if (panelTemperature.TryGetValue(motor, out temperature))
            {
                return temperature;
            }
            else return 0;
        }

        private void CheckTemperatureInfo()
        {
            // if (connectionStatus.AreRobotServicesRestarting())
            // {
            //     transform.GetChild(2).GetChild(1).GetComponent<Text>().text = "Waiting for temperatures...";
            //     transform.GetChild(2).GetChild(1).GetComponent<Text>().color = ColorsManager.blue;
            //     isStatePanelStatusActive = true;
            // }
            // else
            // {
            //     if (!connectionStatus.IsRobotInDataRoom())
            //     {
            //         transform.GetChild(2).GetChild(1).GetComponent<Text>().text = "No temperature information";
            //         transform.GetChild(2).GetChild(1).GetComponent<Text>().color = ColorsManager.red;
            //         isStatePanelStatusActive = true;
            //     }
            //     else
            //     {
            //         isStatePanelStatusActive = false;
            //     }
            // }
            // UpdateStatePanelStatus();
        }

        private void UpdateStatePanelStatus()
        {
            transform.GetChild(2).gameObject.SetActive(isStatePanelStatusActive);
        }

        // void Update()
        // {
        //     if (needUpdatePanel)
        //     {
        //         needUpdatePanel = false;

        //         foreach (KeyValuePair<string, float> motor in panelTemperature)
        //         {
        //             string[] nameParsed = motor.Key.Split("_child_");

        //             GameObject currentActuator = actuators.Find(act => act.name == nameParsed[0]);
        //             Transform currentMotor = currentActuator.transform.Find(nameParsed[1]);

        //             string[] typeParsed = nameParsed[1].Split("_");
        //             if (nameParsed[0].Contains("hand"))
        //             {
        //                 currentMotor.GetComponent<Text>().text = typeParsed[0] + ": " + Mathf.Round(motor.Value).ToString();
        //             }
        //             else
        //             {
        //                 currentMotor.GetComponent<Text>().text = typeParsed[0] + " " + typeParsed[1] + ": " + Mathf.Round(motor.Value).ToString();
        //             }

        //             if (motor.Value >= ErrorManager.THRESHOLD_ERROR_MOTOR_TEMPERATURE)
        //             {
        //                 currentActuator.transform.GetChild(1).gameObject.SetActive(true);
        //             }
        //             else
        //             {
        //                 if (motor.Value >= ErrorManager.THRESHOLD_WARNING_MOTOR_TEMPERATURE)
        //                 {
        //                     currentActuator.transform.GetChild(0).gameObject.SetActive(true);
        //                 }
        //                 else
        //                 {
        //                     currentActuator.transform.GetChild(0).gameObject.SetActive(false);
        //                 }
        //             }
        //         }
        //     }
        // }
    }
}
