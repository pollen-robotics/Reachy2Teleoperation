using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using Data.Acquisition;

namespace DataAcquisition
{
    public class DataAcquisitionSessionErrorManager : MonoBehaviour
    {
        private gRPCDataController gRPCDataController;
        private bool triggerAudit;

        void Start()
        {
            gRPCDataController = DataAcquisitionManager.Instance.DataController;
            triggerAudit = false;
            StartCoroutine(WaitBetweenAudit());
        }

        void Update()
        {
            if (triggerAudit) 
            {
                triggerAudit = false;
                Audit();
            }
        }

        public async void Audit()
        {
            Data.Acquisition.Error error = await DataAcquisitionManager.Instance.DataController.AuditSession();
            Debug.LogError(error);
            StartCoroutine(WaitBetweenAudit());
        }

        IEnumerator WaitBetweenAudit()
        {
            yield return new WaitForSeconds(2.0f);
            triggerAudit = true;
        }
    }
}
