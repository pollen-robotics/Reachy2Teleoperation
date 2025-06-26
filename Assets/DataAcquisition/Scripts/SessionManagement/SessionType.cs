using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace DataAcquisition
{
    public class SessionType : Singleton<SessionType>
    {
        public UnityEvent event_onDataAcquisitionSessionSelected;
        public UnityEvent event_onBasicControlSessionSelected;

        public void SelectDataAcquisitionSession()
        {
            event_onDataAcquisitionSessionSelected.Invoke();
            TeleopReachy.EventManager.StartListening(TeleopReachy.EventNames.TeleoperationSceneLoaded, SetupDataAcquisitionMenus);
        }

        public void SelectBasicControlSession()
        {
            event_onBasicControlSessionSelected.Invoke();
            TeleopReachy.EventManager.StartListening(TeleopReachy.EventNames.TeleoperationSceneLoaded, SetupBasicMenus);
        }

        void SetupDataAcquisitionMenus()
        {
            TeleopReachy.TeleoperationUIMenusManager.Instance.SpeedTimerUIManager.enabled = false;
        }

        void SetupBasicMenus()
        {
            TeleopReachy.TeleoperationUIMenusManager.Instance.SpeedTimerUIManager.enabled = true;
        }
    }
}
