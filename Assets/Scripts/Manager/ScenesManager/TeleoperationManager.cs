using System.Collections;
using UnityEngine;

namespace TeleopReachy
{
    public class TeleoperationManager : Singleton<TeleoperationManager>
    {
        private ConnectionStatus connectionStatus;

        void Start()
        {
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, StartTeleoperation);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, StopTeleoperation);
            EventManager.StartListening(EventNames.HeadsetRemoved, SuspendTeleoperation);
            EventManager.StartListening(EventNames.OnEmergencyStop, SuspendTeleoperation);

            EventManager.StartListening(EventNames.RobotDataSceneLoaded, InitConnectionStatus);
        }

        void InitConnectionStatus()
        {
            connectionStatus = ConnectionStatus.Instance;
            connectionStatus.event_OnRobotReady.AddListener(ReadyForTeleop);
        }

        void ReadyForTeleop()
        {
            EventManager.TriggerEvent(EventNames.OnInitializeRobotStateRequested);
        }

        void StartTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnRobotStiffRequested);
            EventManager.TriggerEvent(EventNames.OnStartTeleoperation);
        }
        
        void StopTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnStopTeleoperation);
        }

        void SuspendTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnSuspendTeleoperation);
        }

        public void AskForResumingTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnResumeTeleoperation);
        }

        public void AskForStartingArmTeleoperation()
        {
            EventManager.TriggerEvent(EventNames.OnStartArmTeleoperation);
        }
    }
}
