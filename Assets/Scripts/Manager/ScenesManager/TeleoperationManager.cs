using System.Collections;
using UnityEngine;

namespace TeleopReachy
{
    public class TeleoperationManager : Singleton<TeleoperationManager>
    {
        private ConnectionStatus connectionStatus;

        public enum TeleoperationSuspensionCase 
        {
            None, HeadsetRemoved, EmergencyStopActivated,
        }

        public TeleoperationSuspensionCase reasonForSuspension { get; private set; }

        void Start()
        {
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, StartTeleoperation);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, StopTeleoperation);
            EventManager.StartListening(EventNames.HeadsetRemoved, HeadsetRemoved);
            EventManager.StartListening(EventNames.OnEmergencyStop, EmergencyStopActivated);

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

        void EmergencyStopActivated()
        {
            reasonForSuspension = TeleoperationSuspensionCase.EmergencyStopActivated;
            SuspendTeleoperation();
        }

        void HeadsetRemoved()
        {
            reasonForSuspension = TeleoperationSuspensionCase.HeadsetRemoved;
            SuspendTeleoperation();
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

        public void AskForRobotSmoothlyCompliant()
        {
            EventManager.TriggerEvent(EventNames.OnRobotSmoothlyCompliantRequested);
        }
    }
}
