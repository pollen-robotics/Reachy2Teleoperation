using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;


namespace DataAcquisition
{
    public class EpisodeEvaluationUIElement : MonoBehaviour
    {
        void OnEnable()
        {
            if (DataAcquisitionManager.Instance.RecordingSessionManager.SaveEpisode) ActivateChildren(true);
            else ActivateChildren(false);
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
