using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace DataAcquisition
{
    public class SessionTypeSelector : PagesManager
    {
        void Start()
        {
            if (TeleopReachy.ScenesManager.Instance.FirstMirrorSceneAccess)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        public void SelectDataAcquisitionSession()
        {
            if(SessionType.Instance != null) SessionType.Instance.SelectDataAcquisitionSession();
        }

        public void SelectBasicControlSession()
        {
            if(SessionType.Instance != null) SessionType.Instance.SelectBasicControlSession();
        }

        public void LaunchDataAcquisitionSession()
        {
            if(SessionType.Instance != null) SessionType.Instance.LaunchDataAcquisitionSession();
        }
    }
}
