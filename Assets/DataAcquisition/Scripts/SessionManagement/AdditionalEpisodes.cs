using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System;


namespace DataAcquisition
{
    public class AdditionalEpisodes : MonoBehaviour
    {
        [Header("Additional Episodes Field")]
        [SerializeField]
        private TMP_InputField nbAdditionalEpisodesInputField;
        
        public void AddEpisodesToSession()
        {
            int nbAdditionalEpisodes = Int32.Parse(nbAdditionalEpisodesInputField.GetComponent<TMP_InputField>().text);
            DataAcquisitionManager.Instance.RecordingSessionManager.ContinueRecordingSession(nbAdditionalEpisodes);
        }
    }
}
