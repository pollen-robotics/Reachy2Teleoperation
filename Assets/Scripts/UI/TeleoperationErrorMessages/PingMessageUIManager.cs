using UnityEngine;

namespace TeleopReachy
{
    public class PingMessageUIManager : InformationalPanel
    {
        private RobotErrorManager errorManager;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.27f, 0.8f));

            errorManager = RobotDataManager.Instance.RobotErrorManager;
            errorManager.event_OnWarningHighLatency.AddListener(WarningHighLatency);
            errorManager.event_OnWarningUnstablePing.AddListener(WarningUnstablePing);

            HideInfoMessage();
        }

        void WarningUnstablePing()
        {
            SetPingWarningMessage("Unstable network");
        }

        void WarningHighLatency()
        {
            SetPingWarningMessage("Low speed network");
        }

        private void SetPingWarningMessage(string warningText)
        {
            textToDisplay = warningText;
            ShowInfoMessage();
        }
    }
}
