using System;
using UnityEngine;


namespace TeleopReachy
{
    public class TableHeight : Singleton<TableHeight>
    {
        public float Height { get; private set; }

        protected override void Init()
        {
            Height = PlayerPrefs.GetInt("tableHeight", 75);
        }

        public void UpdateTableHeight(float height)
        {
            Height = height;
            PlayerPrefs.SetInt("tableHeight", (int)Height);
        }
    }
}
