using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

using Data.Acquisition;

namespace DataAcquisition
{
    public class RecordingSessionSetup : PagesManager
    {
        [Header("Dataset Information")]
        public TMP_InputField datasetNameField;
        public DatasetListView existingDatasetList;
        public TMP_InputField taskDescriptionField;

        [Header("Session Parameters")]
        public TMP_InputField nbEpisodesGoalField;
        public TMP_InputField recordFrequencyField;

        public TMP_InputField breakTimeDurationMMField;
        public TMP_InputField breakTimeDurationSSField;

        public TMP_InputField episodeDurationMMField;
        public TMP_InputField episodeDurationSSField;

        public TMP_InputField startDelayField;

        [Header("Recorded Parts")]
        public Toggle lArmRecorded;
        public Toggle rArmRecorded;
        public Toggle neckRecorded;
        public Toggle antennasRecorded;
        public Toggle mobileBaseRecorded;
    
        [Header("Recorded Cameras")]
        public Toggle lTeleopCamRecorded;
        public Toggle rTeleopCamRecorded;
        public Toggle torsoCamRecorded;
        public Toggle useVideos;

        public bool isNewDataset;

        void Start()
        {
            if (SessionType.Instance != null)
            {
                SessionType.Instance.event_onDataAcquisitionSessionSelected.AddListener(delegate{OpenPageByName("IntroPage");});
            }
        }

        public void DeclareNewDataset(bool isNew)
        {
            isNewDataset = isNew;
        }

        public bool IsDatasetOnline()
        {
            return existingDatasetList.IsSelectedDatasetOnline();
        }

        public string GetDatasetName()
        {
            if (isNewDataset) return datasetNameField.GetComponent<TMP_InputField>().text;
            else return existingDatasetList.GetSelectedDataset();
        }

        public string GetTaskDescription()
        {
            return taskDescriptionField.GetComponent<TMP_InputField>().text;
        }

        public int GetNbEpisodesGoal()
        {
            return Int32.Parse(nbEpisodesGoalField.GetComponent<TMP_InputField>().text);
        }

        public int GetRecordFrequency()
        {
            return Int32.Parse(recordFrequencyField.GetComponent<TMP_InputField>().text);
        }

        public int GetBreakTimeDuration()
        {
            int minutes = Int32.Parse(breakTimeDurationMMField.GetComponent<TMP_InputField>().text);
            int seconds = Int32.Parse(breakTimeDurationSSField.GetComponent<TMP_InputField>().text);
            return minutes * 60 + seconds;
        }

        public int GetEpisodeDuration()
        {
            int minutes = Int32.Parse(episodeDurationMMField.GetComponent<TMP_InputField>().text);
            int seconds = Int32.Parse(episodeDurationSSField.GetComponent<TMP_InputField>().text);
            return minutes * 60 + seconds;
        }

        public bool IsRArmRecorded()
        {
            return rArmRecorded.isOn;
        }

        public bool IsLArmRecorded()
        {
            return lArmRecorded.isOn;
        }

        public bool IsNeckRecorded()
        {
            return neckRecorded.isOn;
        }

        public bool AreAntennasRecorded()
        {
            return antennasRecorded.isOn;
        }

        public bool IsMobileBaseRecorded()
        {
            return mobileBaseRecorded.isOn;
        }

        public bool IsLTeleopCamRecorded()
        {
            return lTeleopCamRecorded.isOn;
        }

        public bool IsRTeleopCamRecorded()
        {
            return rTeleopCamRecorded.isOn;
        }

        public bool IsTorsoCamRecorded()
        {
            return torsoCamRecorded.isOn;
        }

        public bool UseVideos()
        {
            return useVideos.isOn;
        }

        public int GetStartDelay()
        {
            return Int32.Parse(startDelayField.GetComponent<TMP_InputField>().text);
        }

        private void SaveParameters()
        {
            DataAcquisitionManager.Instance.RecordingSessionParameters.SaveParameters(this);
        }

        public void FinishSetup()
        {
            SaveParameters();
            DataAcquisitionManager.Instance.RecordingSessionManager.StartRecordingSession();
            DataAcquisitionManager.Instance.RecordingSessionManager.event_OnStartSessionOver.AddListener(ConfigurationOver);
        }

        private void ConfigurationOver(bool success)
        {
            if (success) CloseAllPagesDelayed(2.0f);
        }

        public void AbortSetup()
        {
            CloseAllPanels();
            CloseAllPages();
            if (SessionType.Instance != null) SessionType.Instance.SelectBasicControlSession();
        }
    }
}
