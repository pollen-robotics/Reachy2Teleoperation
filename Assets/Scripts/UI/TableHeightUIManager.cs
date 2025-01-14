using System;
using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class TableHeightUIManager : MonoBehaviour
    {
        [SerializeField]
        private Text valueTextMeters;

        //[SerializeField]
        //private Text valueTextFeet;

        private void Start()
        {
            valueTextMeters.text = "Table: -" + PlayerPrefs.GetInt("tableHeight", 75) + " cm";
        }

        public void UpdateValue(float value)
        {
            valueTextMeters.text = "Table: -" + (int)value + " cm";
            //valueTextFeet.text = (value * 3.28f).ToString("F2") + "ft";
        }
    }
}
