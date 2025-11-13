using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

namespace DataAcquisition
{
    public class ControlInputFieldInteractability : MonoBehaviour
    {
        public List<TMP_InputField> inputFields;
        private Toggle toggle;

        private int defaultValue;

        void Start()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate {
                ToggleValueChanged(toggle);
            });
        }   

        void ToggleValueChanged(Toggle value)
        {
            foreach (TMP_InputField field in inputFields)
            {
                field.interactable = toggle.isOn;
                if (!toggle.isOn)
                {
                    field.text = "00";
                }
            }
        }
    }
}
