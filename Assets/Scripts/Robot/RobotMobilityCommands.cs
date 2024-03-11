using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Reachy;
using Mobile.Base.Mobility;


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
            robotStatus.event_OnStartTeleoperation.AddListener(StartMobility);
            robotStatus.event_OnStopTeleoperation.AddListener(StopMobility);
            robotStatus.event_OnSuspendTeleoperation.AddListener(StopMobileBaseMovements);
        }

        private void StartMobility()
        {
            if (robotConfig.HasMobileBase() && robotStatus.IsMobilityOn())
            {
                dataController.TurnMobileBaseOn();
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
            TargetDirectionCommand command = new TargetDirectionCommand
            {
                Direction = new DirectionVector
                {
                    X = direction[0],
                    Y = direction[1],
                    Theta = direction[2],
                }
            };
            dataController.SendMobileBaseCommand(command);
        }

        void StopMobileBaseMovements()
        {
            try
            {
                if(robotConfig.HasMobileBase())
                {
                    Vector2 direction = new Vector2(0, 0);
                    SendMobileBaseDirection(direction);
                    dataController.TurnMobileBaseOff();
                }
            }
            catch (Exception exc)
            {
                Debug.Log($"[RobotMobilityCommands]: StopMobileBaseMovements error: {exc}");
            }
        }
    }
}