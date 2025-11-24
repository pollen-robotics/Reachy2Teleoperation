using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace DataAcquisition
{
    public class AbortDataAcquisitionSessionOnClick : PagesManager
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(AbortDataAcquisitionSession);
        }

        void AbortDataAcquisitionSession()
        {
            SessionType.Instance.AbortDataAcquisitionSession();
        }
    }
}
