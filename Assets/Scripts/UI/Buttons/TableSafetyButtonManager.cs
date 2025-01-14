using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;


namespace TeleopReachy
{
    public class TableSafetyButtonManager : MonoBehaviour
    {
        [SerializeField]
        private Button tableSafetyButton;

        private bool needUpdateButton = false;
        private ColorBlock buttonColor;
        private string buttonText;

        void Awake()
        {
            tableSafetyButton.onClick.AddListener(SwitchButtonMode);

            SwitchButtonMode();
        }

        void SwitchButtonMode()
        {
            if (TableHeight.Instance.SafetyActivated)
            {
                tableSafetyButton.colors = ColorsManager.colorsActivated;
                tableSafetyButton.transform.GetChild(0).GetComponent<Text>().text = "Safety ON";
            }
            else
            {
                tableSafetyButton.colors = ColorsManager.colorsDeactivated;
                tableSafetyButton.transform.GetChild(0).GetComponent<Text>().text = "Safety OFF";
            }
        }

        void Update()
        {
            if (needUpdateButton)
            {
                tableSafetyButton.colors = buttonColor;
                tableSafetyButton.transform.GetChild(0).GetComponent<Text>().text = buttonText;
                needUpdateButton = false;
            }
        }

        public void ChangeTableSafetyActivation()
        {
            TableHeight.Instance.ActivateDeactivateSafety(!TableHeight.Instance.SafetyActivated);
            SwitchButtonMode();
        }
    }
}