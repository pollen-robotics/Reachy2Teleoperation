using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

using Data.Acquisition;

namespace DataAcquisition
{
    public class RecordingSessionParameters : Singleton<RecordingSessionParameters>
    {
        public int NbEpisodesGoal { get; private set; }
        public int RecordFrequency { get; private set; }
        public int BreakTimeDuration { get; private set; }
        public int EpisodeDuration { get; private set; }
        public int StartDelay { get; private set; }

        public string DatasetName { get; private set; }
        public string TaskDescription { get; private set; }

        public bool IsNewDataset { get; private set; }
        public bool IsDatasetOnline { get; private set; }

        public bool IsRArmRecorded { get; private set; }
        public bool IsLArmRecorded { get; private set; }
        public bool IsNeckRecorded { get; private set; }
        public bool AreAntennasRecorded { get; private set; }
        public bool IsMobileBaseRecorded { get; private set; }

        public bool IsLTeleopCamRecorded { get; private set; }
        public bool IsRTeleopCamRecorded { get; private set; }
        public bool IsTorsoCamRecorded { get; private set; }
        public bool UseVideos { get; private set; }
        

        public void SaveParameters(RecordingSessionSetup sessionSetup)
        {
            NbEpisodesGoal = sessionSetup.GetNbEpisodesGoal();
            RecordFrequency = sessionSetup.GetRecordFrequency();
            BreakTimeDuration = sessionSetup.GetBreakTimeDuration();
            EpisodeDuration = sessionSetup.GetEpisodeDuration();
            StartDelay = sessionSetup.GetStartDelay();

            DatasetName = sessionSetup.GetDatasetName();
            TaskDescription = sessionSetup.GetTaskDescription();

            IsNewDataset = sessionSetup.isNewDataset;
            IsDatasetOnline = sessionSetup.IsDatasetOnline();

            IsRArmRecorded = sessionSetup.IsRArmRecorded();
            IsLArmRecorded = sessionSetup.IsLArmRecorded();
            IsNeckRecorded = sessionSetup.IsNeckRecorded();
            AreAntennasRecorded = sessionSetup.AreAntennasRecorded();
            IsMobileBaseRecorded = sessionSetup.IsMobileBaseRecorded();

            IsLTeleopCamRecorded = sessionSetup.IsLTeleopCamRecorded();
            IsRTeleopCamRecorded = sessionSetup.IsRTeleopCamRecorded();
            IsTorsoCamRecorded = sessionSetup.IsTorsoCamRecorded();
            UseVideos = sessionSetup.UseVideos();
        }

        public void SetDatasetType(bool isNewDataset)
        {
            IsNewDataset = isNewDataset;
        }

        public void UpdateNbEpisodeGoal(int newNbOfEpisodes)
        {
            NbEpisodesGoal += newNbOfEpisodes;
        }
    }
}
