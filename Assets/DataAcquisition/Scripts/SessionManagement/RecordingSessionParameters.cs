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
        public int BreakTimeDuration { get; private set; }
        public int EpisodeDuration { get; private set; }
        public int StartDelay { get; private set; }

        public string DatasetName { get; private set; }
        public string TaskDescription { get; private set; }

        public bool IsNewDataset { get; private set; }
        public bool IsDatasetOnline { get; private set; }
        

        public void SaveParameters(RecordingSessionSetup sessionSetup)
        {
            NbEpisodesGoal = sessionSetup.GetNbEpisodesGoal();
            BreakTimeDuration = sessionSetup.GetBreakTimeDuration();
            EpisodeDuration = sessionSetup.GetEpisodeDuration();
            StartDelay = sessionSetup.GetStartDelay();

            DatasetName = sessionSetup.GetDatasetName();
            TaskDescription = sessionSetup.GetTaskDescription();

            IsNewDataset = sessionSetup.isNewDataset;
            IsDatasetOnline = sessionSetup.IsDatasetOnline();
        }

        public void UpdateNbEpisodeGoal(int newNbOfEpisodes)
        {
            NbEpisodesGoal += newNbOfEpisodes;
        }
    }
}
