using System;
using UnityEngine;
using Reachy.Part.Mobile.Base.Mobility;


namespace TeleopReachy
{
    public class RobotMobilityCommands : MonoBehaviour
    {
        private DataMessageManager dataController;

        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        void Start()
        {
            dataController = DataMessageManager.Instance;

            robotConfig = transform.GetComponent<RobotConfig>();
            robotStatus = transform.GetComponent<RobotStatus>();

            EventManager.StartListening(EventNames.OnStartTeleoperation, StartMobility);
            EventManager.StartListening(EventNames.OnStopTeleoperation, StopMobility);
            EventManager.StartListening(EventNames.OnSuspendTeleoperation, StopMobileBaseMovements);
        }

        private void StartMobility()
        {
            if (robotConfig.HasMobileBase() && robotStatus.IsMobileBaseOn())
            {
                dataController.TurnMobileBaseOn(robotConfig.partsId["mobile_base"]);
            }
        }

        private void StopMobility()
        {
            if (robotConfig.HasMobileBase())
            {
                StopMobileBaseMovements();
            }
        }

        public void SendMobileBaseDirection(Vector3 direction)
        {
            if (robotConfig.HasMobileBase() && robotStatus.IsMobileBaseOn())
            {
                TargetDirectionCommand command = new TargetDirectionCommand
                {
                    Id = robotConfig.partsId["mobile_base"],
                    Direction = new DirectionVector
                    {
                        X = direction[0],
                        Y = direction[1],
                        Theta = direction[2],
                    }
                };
                dataController.SendMobileBaseCommand(command);
            }
        }

        void StopMobileBaseMovements()
        {
            try
            {
                if (robotConfig.HasMobileBase())
                {
                    Vector2 direction = new Vector2(0, 0);
                    SendMobileBaseDirection(direction);
                    dataController.TurnMobileBaseOff(robotConfig.partsId["mobile_base"]);
                }
            }
            catch (Exception exc)
            {
                Debug.Log($"[RobotMobilityCommands]: StopMobileBaseMovements error: {exc}");
            }
        }
    }
}