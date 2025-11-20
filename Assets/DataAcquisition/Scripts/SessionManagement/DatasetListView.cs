using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

using Data.Acquisition;
using TeleopReachy;


namespace DataAcquisition
{
    public class DatasetListView : MonoBehaviour
    {
        [Header("Datasets content")]
        public GameObject datasetItemPrefab;
        public Transform contentParent;

        [Header("UI managenement")]
        public Button continueButton;
        public Transform invalidAddress;

        private DatasetList datasetList;

        private gRPCDataController gRPCDataController;

        private bool datasetReady;
        private string selectedDataset;
        private bool isSelectedDatasetOnline;

        void OnEnable()
        {
            datasetReady = false;
            DataAcquisitionScenesManager.Instance.event_DataAcquisitionSceneLoaded.AddListener(GetDatasetsTask);
        }

        void GetDatasetsTask()
        {
            gRPCDataController = DataAcquisitionManager.Instance.DataController;
            Task.Run(() => GetDatasets());
        }

        public async void GetDatasets()
        {
            datasetList = await gRPCDataController.GetDatasetList();
            datasetReady = true;
        }

        public void PopulateList()
        {
            // Clear old items first
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            if (datasetList != null)
            {
                foreach (var dataset in datasetList.Datasets)
                {
                    GameObject item = Instantiate(datasetItemPrefab, contentParent);

                    // Set the button text
                    var text = item.transform.GetChild(0).GetComponent<TMP_Text>();
                    if (text != null)
                    {
                        text.text = dataset.DatasetName;
                    }

                    // Set the nbEpisodes text
                    var nbEpisodes = item.transform.GetChild(1).GetComponent<TMP_Text>();
                    if (nbEpisodes != null)
                    {
                        nbEpisodes.text = dataset.NbEpisodes + " episodes";
                    }

                    // Add a listener to the button
                    var button = item.GetComponent<Button>();
                    if (button != null)
                    {
                        string selectedDatasetName = dataset.DatasetName; // Avoid closure problem
                        item.GetComponentInChildren<TagModifier>().SelectTag(dataset.Pushed);
                        bool pushed = false;
                        if (dataset.Pushed == DatasetPushState.Pushed) pushed = true;
                        button.onClick.AddListener(() => OnDatasetSelected(selectedDatasetName, pushed));
                    }
                }
                continueButton.interactable = true;
                invalidAddress.ActivateChildren(false);
            }
            else 
            {
                continueButton.interactable = false;
                invalidAddress.ActivateChildren(true);
            }
            
        }

        private void OnDatasetSelected(string datasetName, bool pushed)
        {
            selectedDataset = datasetName;
            isSelectedDatasetOnline = pushed;
        }

        void Update()
        {
            if (datasetReady)
            {
                datasetReady = false;
                PopulateList();
            }
        }

        public string GetSelectedDataset()
        {
            return selectedDataset;
        }

        public bool IsSelectedDatasetOnline()
        {
            return isSelectedDatasetOnline;
        }
    }
}