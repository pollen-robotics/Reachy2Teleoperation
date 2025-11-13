using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DataAcquisition
{
    public class NonNullInputField : MonoBehaviour
    {
        private TMP_InputField field;

        [SerializeField]
        private string defaultValue;

        void Start()
        {
            field = GetComponent<TMP_InputField>();
            field.onValueChanged.AddListener(delegate {ValueChangeCheck(); });
        }   

        void ValueChangeCheck()
        {
            if (Int32.Parse(field.text) == 0) field.text = defaultValue;
        }
    }
}