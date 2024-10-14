using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TeleopReachy
{
    public class PartStatusUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
    {
        [SerializeField]
        private Texture okTexture;

        [SerializeField]
        private Texture errorTexture;

        [SerializeField]
        private Texture hotTexture;
        private RobotErrorManager errorManager;

        private string partName;
        private Dictionary<int, string> motors;
        private bool needUpdate;
        private bool needErrorDisplay;
        private bool needTemperatureErrorDisplay;
        private bool needTemperatureWarningDisplay;
        private bool isTemperatureDisplayed;
        private string error;

        [SerializeField]
        private RobotStatePanelUIManager robotStatePanelManager;

        // Start is called before the first frame update
        void Start()
        {
            needUpdate = false;
            needErrorDisplay = false;
            needTemperatureErrorDisplay = false;
            needTemperatureWarningDisplay = false;
            isTemperatureDisplayed = false;
            partName = gameObject.name.Split("_status")[0];
            errorManager = RobotDataManager.Instance.RobotErrorManager;
            errorManager.event_OnStatusError.AddListener(SetStatusError);
            errorManager.event_OnWarningMotorsTemperatures.AddListener(SetWarningTemperatures);
            errorManager.event_OnErrorMotorsTemperatures.AddListener(SetErrorTemperatures);

            motors = new Dictionary<int, string>();

            foreach (Transform child in transform.GetChild(2))
            {
                string motorName = child.name.Split("_temperature")[0];
                motorName = partName + "_" + motorName;
                motors.Add(child.GetSiblingIndex(), motorName);
            }

            transform.GetChild(2).ActivateChildren(false);
        }

        // Update is called once per frame
        void Update()
        {
            if(needUpdate)
            {
                if(needErrorDisplay || needTemperatureErrorDisplay || needTemperatureWarningDisplay)
                {
                    transform.GetChild(1).gameObject.SetActive(true);
                    transform.GetChild(1).GetChild(0).GetComponent<Text>().text = error;
                    if(needTemperatureWarningDisplay) transform.GetComponent<RawImage>().texture = hotTexture;
                    else transform.GetComponent<RawImage>().texture = errorTexture;
                    Vector3 pos = transform.GetChild(2).localPosition;
                    transform.GetChild(2).localPosition = new Vector3(pos.x, -40, pos.z);
                }
                else
                {
                    transform.GetChild(1).gameObject.SetActive(false);
                    transform.GetComponent<RawImage>().texture = okTexture;
                    Vector3 pos = transform.GetChild(2).localPosition;
                    transform.GetChild(2).localPosition = new Vector3(pos.x, 0, pos.z);
                }
                needUpdate = false;
            }
            

            if (isTemperatureDisplayed)
            {
                foreach (var motor in motors)
                {
                    float value = robotStatePanelManager.GetTemperature(motor.Value);
                    string message;
                    if (motor.Value.Contains("hand"))
                    {
                        string[] typeParsed = motor.Value.Split("hand_");
                        message = typeParsed[1] + ": " + Mathf.Round(value).ToString();
                    }
                    else
                    {
                        string[] typeParsed = motor.Value.Split("motor_");
                        message = "Motor: " + typeParsed[1] + ": " + Mathf.Round(value).ToString();
                    }
                    transform.GetChild(2).GetChild(motor.Key).GetComponent<Text>().text = message;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.GetChild(2).ActivateChildren(true);
            isTemperatureDisplayed = true;
            Vector3 pos = transform.GetChild(2).localPosition;
            if(needErrorDisplay || needTemperatureErrorDisplay || needTemperatureWarningDisplay) transform.GetChild(2).localPosition = new Vector3(pos.x, -40, pos.z);
            else transform.GetChild(2).localPosition = new Vector3(pos.x, 0, pos.z);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.GetChild(2).ActivateChildren(false);
            isTemperatureDisplayed = false;
        }

        private void SetStatusError(Dictionary<string, string> errorList)
        {
            if(errorList.ContainsKey(partName))
            {
                error = errorList[partName];
                needErrorDisplay = true;
            }
            else
            {
                needErrorDisplay = false;
            }
            needUpdate = true;
        }

        private void SetWarningTemperatures(List<string> TemperatureList)
        {
            if (!needErrorDisplay && TemperatureList.Contains(partName))
            {
                error = "Motors heating up";
                needTemperatureWarningDisplay = true;
            }
            else
            {
                needTemperatureWarningDisplay = false;
            }
            needUpdate = true;
        }

        private void SetErrorTemperatures(List<string> TemperatureList)
        {
            if (TemperatureList.Contains(partName))
            {
                error = "Temperature error";
                needTemperatureErrorDisplay = true;
            }
            else
            {
                needTemperatureErrorDisplay = false;
            }
            needUpdate = true;
        }
    }
}
