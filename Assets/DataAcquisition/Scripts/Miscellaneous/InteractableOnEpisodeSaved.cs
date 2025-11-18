using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


namespace DataAcquisition
{
    public class InteractableOnEpisodeSaved : MonoBehaviour
    {
        void Start()
        {
            DataAcquisitionManager.Instance.RecordingSessionManager.event_OnEpisodeSaved.AddListener(SkipBreakPossible);
        }

        void OnEnable()
        {
            if (DataAcquisitionManager.Instance.RecordingSessionManager.SaveEpisode) GetComponent<Button>().interactable = false;
            else GetComponent<Button>().interactable = true;
        }

        void SkipBreakPossible()
        {
            GetComponent<Button>().interactable = true;
        }
    }
}