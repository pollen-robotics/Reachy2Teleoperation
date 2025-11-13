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
            GetComponent<Button>().interactable = false;
        }

        void SkipBreakPossible()
        {
            GetComponent<Button>().interactable = true;
        }
    }
}