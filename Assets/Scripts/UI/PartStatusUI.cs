using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TeleopReachy
{
    public class PartStatusUI : MonoBehaviour 
    {
        [SerializeField]
        private Texture okTexture;

        [SerializeField]
        private Texture errorTexture;

        [SerializeField]
        private Texture hotTexture;
        private RobotErrorManager errorManager;

        private string partName;
        private bool needUpdate;
        private bool needErrorDisplay;
        private bool needTemperatureErrorDisplay;
        private bool needTemperatureWarningDisplay;
        private string error;

        [SerializeField]
        private RobotStatePanelUIManager robotStatePanelManager;

        void Start()
        {
            needUpdate = false;
            needErrorDisplay = false;
            needTemperatureErrorDisplay = false;
            needTemperatureWarningDisplay = false;
            partName = gameObject.name.Split("_status")[0];
            errorManager = RobotDataManager.Instance.RobotErrorManager;
            errorManager.event_OnStatusError.AddListener(SetStatusError);
        }

        void Update()
        {
            if(needUpdate)
            {
                if(needErrorDisplay || needTemperatureErrorDisplay || needTemperatureWarningDisplay)
                {
                    transform.GetChild(1).gameObject.SetActive(true);
                    transform.GetChild(1).GetChild(0).GetComponent<Text>().text = error;
                    if(error == "HighTemperatureState") transform.GetComponent<RawImage>().texture = hotTexture;
                    else transform.GetComponent<RawImage>().texture = errorTexture;
                }
                else
                {
                    transform.GetChild(1).gameObject.SetActive(false);
                    transform.GetComponent<RawImage>().texture = okTexture;
                }
                needUpdate = false;
            }
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
    }
}
