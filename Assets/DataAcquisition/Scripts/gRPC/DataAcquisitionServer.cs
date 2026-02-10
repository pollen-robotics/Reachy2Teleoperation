using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

namespace DataAcquisition
{
    public class DataAcquisitionServer : Singleton<DataAcquisitionServer>
    {
        public TMP_InputField serverIpAddress;
        public TMP_InputField serverDataPort;

        void Start()
        {
            serverIpAddress.text = PlayerPrefs.GetString("server_ip", "localhost");
            serverDataPort.text = PlayerPrefs.GetString("server_data_port", "50062");
        }

        public string GetServerIpAddress()
        {
            return serverIpAddress.text;
        }

        public string GetServerDataPort()
        {
            return serverDataPort.text;
        }
    }
}
