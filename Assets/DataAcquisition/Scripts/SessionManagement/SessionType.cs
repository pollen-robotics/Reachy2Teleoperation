using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace DataAcquisition
{
    public class SessionType : Singleton<SessionType>
    {
        public UnityEvent event_onDataAcquisitionSessionSelected;
        public UnityEvent event_onDataAcquisitionSessionLaunched;
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

        public void LaunchDataAcquisitionSession()
        {
            event_onDataAcquisitionSessionLaunched.Invoke();
        }

        void SetupDataAcquisitionMenus()
        {
            TeleopReachy.TeleoperationUIMenusManager.Instance.SpeedTimerUIManager.enabled = false;
            TeleopReachy.TeleoperationUIMenusManager.Instance.TeleoperationExitMenuUIManager.enabled = false;
            TeleopReachy.TeleoperationSceneManager.Instance.DisableTeleoperationExitMenu();
        }

        void SetupBasicMenus()
        {
            TeleopReachy.TeleoperationUIMenusManager.Instance.SpeedTimerUIManager.enabled = true;
            TeleopReachy.TeleoperationUIMenusManager.Instance.TeleoperationExitMenuUIManager.enabled = true;
            TeleopReachy.TeleoperationSceneManager.Instance.EnableTeleoperationExitMenu();
        }
    }
}
