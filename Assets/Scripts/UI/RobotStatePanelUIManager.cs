using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class RobotStatePanelUIManager : MonoBehaviour
    {
        private gRPCDataController dataController;
        private ConnectionStatus connectionStatus;

        private RobotStatus robotStatus;

        private List<GameObject> actuators;

        private bool isStatePanelStatusActive;

        void Awake()
        {
            if (Robot.IsCurrentRobotVirtual())
            {
                isStatePanelStatusActive = false;
                UpdateStatePanelStatus();
                return;
            }

            dataController = gRPCManager.Instance.gRPCDataController;
            dataController.event_OnStateUpdateTemperature.AddListener(UpdateTemperatures);

            connectionStatus = gRPCManager.Instance.ConnectionStatus;
            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckTemperatureInfo);

            actuators = new List<GameObject>();
            foreach (Transform child in transform.GetChild(1))
            {
                actuators.Add(child.gameObject);
            }

            CheckTemperatureInfo();

            isStatePanelStatusActive = true;
        }

        private void UpdateTemperatures(Dictionary<string, float> Temperatures)
        {
            foreach(KeyValuePair<string, float> motor in Temperatures)
            {
                if(motor.Key.Contains("hand"))
                {
                    string[] nameParsed = motor.Key.Split("_hand_");
                    string actuatorName = nameParsed[0] + "_hand_temperature";

                    GameObject currentActuator = actuators.Find(act => act.name == actuatorName);
                    Transform currentMotor = currentActuator.transform.Find(nameParsed[1]+"_temperature");
                    currentMotor.GetComponent<Text>().text = char.ToUpper(nameParsed[1][0]) + nameParsed[1].Substring(1) + ": " + Mathf.Round(motor.Value).ToString();
                    if(motor.Value >= ErrorManager.THRESHOLD_ERROR_MOTOR_TEMPERATURE)
                    {
                        currentActuator.transform.GetChild(1).gameObject.SetActive(true);
                    }
                    else
                    {
                        if(motor.Value >= ErrorManager.THRESHOLD_WARNING_MOTOR_TEMPERATURE)
                        {
                            currentActuator.transform.GetChild(0).gameObject.SetActive(true);
                        }
                        else 
                        {
                            currentActuator.transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    string[] nameParsed = motor.Key.Split("_motor_");
                    string actuatorName = nameParsed[0] + "_temperature";

                    GameObject currentActuator = actuators.Find(act => act.name == actuatorName);
                    Transform currentMotor = currentActuator.transform.Find("motor_"+nameParsed[1]+"_temperature");
                    currentMotor.GetComponent<Text>().text = "Motor " + nameParsed[1] + ": " + Mathf.Round(motor.Value).ToString();
                    if(motor.Value >= ErrorManager.THRESHOLD_ERROR_MOTOR_TEMPERATURE)
                    {
                        currentActuator.transform.GetChild(1).gameObject.SetActive(true);
                    }
                    else
                    {
                        if(motor.Value >= ErrorManager.THRESHOLD_WARNING_MOTOR_TEMPERATURE)
                        {
                            currentActuator.transform.GetChild(0).gameObject.SetActive(true);
                        }
                        else 
                        {
                            currentActuator.transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }
                
            }
        }

        private void CheckTemperatureInfo()
        {
            if(connectionStatus.AreRobotServicesRestarting())
            {
                transform.GetChild(2).GetChild(1).GetComponent<Text>().text = "Waiting for temperatures...";
                transform.GetChild(2).GetChild(1).GetComponent<Text>().color = ColorsManager.blue;
                isStatePanelStatusActive = true;
            }
            else
            {
                if(!connectionStatus.IsServerConnected() || !connectionStatus.IsRobotInDataRoom())
                {
                    transform.GetChild(2).GetChild(1).GetComponent<Text>().text = "No temperature information";
                    transform.GetChild(2).GetChild(1).GetComponent<Text>().color = ColorsManager.red;
                    isStatePanelStatusActive = true;
                }
                else
                {
                    isStatePanelStatusActive = false;
                }
            }
            UpdateStatePanelStatus();
        }

        private void UpdateStatePanelStatus()
        {
            transform.GetChild(2).gameObject.SetActive(isStatePanelStatusActive);
        }
    }
}
