using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;


namespace DataAcquisition
{
    public class DataAcquisitionServerUI : MonoBehaviour
    {
        void Start()
        {
            DataAcquisitionScenesManager.Instance.event_DataAcquisitionSceneLoaded.AddListener(InitDataAcquisitionManagerListener);
        }

        void InitDataAcquisitionManagerListener()
        {
            DataAcquisitionManager.Instance.DataController.event_DataAcquisitionServerConnectionFailed.AddListener(ShowServerPanel);
        }

        private void ShowServerPanel()
        {
            transform.GetComponent<RecordingSessionSetup>().OpenPanelByName("SetupServerPanel");
            transform.GetComponent<RecordingSessionSetup>().OpenPanelByName("InvalidServerAddress");
        }

        public void ResetPlayerPrefs()
        {
            DataAcquisitionServer.Instance.serverIpAddress.text = PlayerPrefs.GetString("server_ip", "localhost");
            DataAcquisitionServer.Instance.serverDataPort.text = PlayerPrefs.GetString("server_data_port", "50062");
        }

        public void SaveAsPlayerPrefs()
        {
            PlayerPrefs.SetString("server_ip", DataAcquisitionServer.Instance.GetServerIpAddress());
            PlayerPrefs.SetString("server_data_port", DataAcquisitionServer.Instance.GetServerDataPort());
        }
    }
}
