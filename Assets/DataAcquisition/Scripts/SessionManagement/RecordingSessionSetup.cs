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

        public TMP_InputField breakTimeDurationHHField;
        public TMP_InputField breakTimeDurationMMField;
        public TMP_InputField breakTimeDurationSSField;

        public TMP_InputField episodeDurationHHField;
        public TMP_InputField episodeDurationMMField;
        public TMP_InputField episodeDurationSSField;

        public TMP_InputField startDelayField;

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

        public int GetBreakTimeDuration()
        {
            int hours = Int32.Parse(breakTimeDurationHHField.GetComponent<TMP_InputField>().text);
            int minutes = Int32.Parse(breakTimeDurationMMField.GetComponent<TMP_InputField>().text);
            int seconds = Int32.Parse(breakTimeDurationSSField.GetComponent<TMP_InputField>().text);
            return hours * 3600 + minutes * 60 + seconds;
        }

        public int GetEpisodeDuration()
        {
            int hours = Int32.Parse(episodeDurationHHField.GetComponent<TMP_InputField>().text);
            int minutes = Int32.Parse(episodeDurationMMField.GetComponent<TMP_InputField>().text);
            int seconds = Int32.Parse(episodeDurationSSField.GetComponent<TMP_InputField>().text);
            return hours * 3600 + minutes * 60 + seconds;
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
