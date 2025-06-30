using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;


namespace DataAcquisition
{
    public class EpisodeEvaluationUIElement : MonoBehaviour
    {
        private bool mustBeDisplayed = false;
        private bool needActivationChange = false;
        private RecordingSessionManager sessionManager;

        void OnEnable()
        {
            if (DataAcquisitionManager.Instance.RecordingSessionManager.SaveEpisode) StartSaving();
            else NoSaving();
        }

        public void StartSaving()
        {
            mustBeDisplayed = true;
            needActivationChange = true;
        }

        public void NoSaving()
        {
            mustBeDisplayed = false;
            needActivationChange = true;
        }

        void Update()
        {
            if (needActivationChange) 
            {
                needActivationChange = false;
                ActivateChildren(mustBeDisplayed);
            }
        }

        void ActivateChildren(bool enabled)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(enabled);
            }
        }
    }
}
