using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

using Data.Acquisition;


namespace DataAcquisition
{
    public class DatasetListView : MonoBehaviour
    {
        public GameObject datasetItemPrefab;
        public Transform contentParent;
        private DatasetList datasetList;

        private gRPCDataController gRPCDataController;

        private bool datasetReady;
        private string selectedDataset;
        private bool isSelectedDatasetOnline;

        void OnEnable()
        {
            datasetReady = false;
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

            foreach (var dataset in datasetList.Datasets)
            {
                GameObject item = Instantiate(datasetItemPrefab, contentParent);

                // Set the button text
                var text = item.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = dataset.DatasetName;
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