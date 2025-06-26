using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;


namespace DataAcquisition
{
    public class SavingUIMessage : MonoBehaviour
    {
        private bool isSaving = false;
        private bool isEpisodeSaved = false;
        private bool needActivationChange = false;
        private RecordingSessionManager sessionManager;

        [SerializeField]
        private Sprite validateIcon;
        [SerializeField]
        private Sprite savingIcon;

        void Start()
        {
            sessionManager = DataAcquisitionManager.Instance.RecordingSessionManager;
            sessionManager.event_OnEpisodeSaved.AddListener(EpisodeSaved);
        }

        void OnEnable()
        {
            transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Saving...";
            transform.GetChild(1).GetComponent<Image>().sprite = savingIcon;
            transform.GetChild(1).localRotation = Quaternion.identity;

            StartSaving();
        }

        public void StartSaving()
        {
            isSaving = true;
            needActivationChange = true;
        }

        public void EpisodeSaved()
        {
            isSaving = false;
            isEpisodeSaved = true;
        }

        void Update()
        {
            if (isSaving) 
            {
                if (needActivationChange) 
                {
                    needActivationChange = false;
                    ActivateChildren(true);
                }
                transform.GetChild(1).Rotate(0, 0, -2.0f);
            }
            else 
            {
                if (needActivationChange) 
                {
                    needActivationChange = false;
                    ActivateChildren(false);
                }
            }

            if (isEpisodeSaved)
            {
                isEpisodeSaved = false;
                transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Saved";
                transform.GetChild(1).localRotation = Quaternion.identity;
                transform.GetChild(1).GetComponent<Image>().sprite = validateIcon;
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
