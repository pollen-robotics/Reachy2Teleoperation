using System;
using UnityEngine;


namespace TeleopReachy
{
    public class TableHeight : Singleton<TableHeight>
    {
        public float Height { get; private set; }
        public bool SafetyActivated { get; private set; }

        protected override void Init()
        {
            Height = PlayerPrefs.GetInt("tableHeight", 75);
            SafetyActivated = Convert.ToBoolean(PlayerPrefs.GetInt("tableSafety", 0));
        }

        public void UpdateTableHeight(float height)
        {
            Height = height;
            PlayerPrefs.SetInt("tableHeight", (int)Height);
        }

        public void ActivateDeactivateSafety(bool activation)
        {
            SafetyActivated = activation;
            PlayerPrefs.SetInt("tableSafety", Convert.ToInt32(activation));
        }
    }
}
