using UnityEngine;
using UnityEngine.Events;
using Reachy.Part.Arm;

namespace TeleopReachy
{
    public class RobotStatus : MonoBehaviour
    {
        private bool isRobotTeleoperationActive;

        private bool isRobotArmTeleoperationActive;

        private bool areRobotMovementsSuspended;

        private bool isRobotCompliant;

        private bool isMobileBaseOn = true; // true if operator want to have control of the mobile base, false otherwise

        private bool isLeftArmOn = true; // true if operator want to have control of the left arm, false otherwise
        private bool isLeftGripperOn = true; // true if operator want to have control of the left arm, false otherwise

        private bool isRightArmOn = true; // true if operator want to have control of the right arm, false otherwise
        private bool isRightGripperOn = true; // true if operator want to have control of the left arm, false otherwise

        private bool isHeadOn = true; // true if operator want to have control of the head, false otherwise

        private bool isMobilityActive; // true if panel must be shown, false otherwise

        private bool areEmotionsActive;

        private bool isEmotionPlaying;

        private bool statusChanged;

        private bool hasMotorsSpeedLimited;

        private bool isGraspingLockActivated;

        private bool isLeftGripperClosed = false;
        private bool isRightGripperClosed = false;

        private IKConstrainedMode armIkMode = IKConstrainedMode.LowElbow;

        public bool IsRobotPositionLocked { get; private set; }

        public UnityEvent<bool> event_OnGraspingLock;
        public UnityEvent event_OnRobotFullyCompliant;

        public UnityEvent<bool> event_OnSwitchMobilityOn;

        private void Start()
        {
            EventManager.StartListening(EventNames.OnStartArmTeleoperation, StartArmTeleoperation);
            EventManager.StartListening(EventNames.OnStartTeleoperation, StartRobotTeleoperation);
            EventManager.StartListening(EventNames.OnStopTeleoperation, StopRobotTeleoperation);

            EventManager.StartListening(EventNames.OnSuspendTeleoperation, SuspendRobotTeleoperation);
            EventManager.StartListening(EventNames.OnResumeTeleoperation, ResumeRobotTeleoperation);
        }

        public void LeftGripperClosed(bool isclosed)
        {
            isLeftGripperClosed = isclosed;
        }

        public void RightGripperClosed(bool isclosed)
        {
            isRightGripperClosed = isclosed;
        }

        public bool IsRightGripperClosed()
        {
            return isRightGripperClosed;
        }

        public bool IsLeftGripperClosed()
        {
            return isLeftGripperClosed;
        }

        public bool IsRobotTeleoperationActive()
        {
            return isRobotTeleoperationActive;
        }

        public bool IsRobotArmTeleoperationActive()
        {
            return isRobotArmTeleoperationActive;
        }

        public bool AreRobotMovementsSuspended()
        {
            return areRobotMovementsSuspended;
        }

        public bool IsRobotCompliant()
        {
            return isRobotCompliant;
        }

        public bool AreEmotionsActive()
        {
            return areEmotionsActive;
        }

        public bool IsMobileBaseOn()
        {
            return isMobileBaseOn;
        }

        public bool IsLeftArmOn()
        {
            return isLeftArmOn;
        }

        public bool IsLeftGripperOn()
        {
            return isLeftGripperOn;
        }

        public bool IsRightArmOn()
        {
            return isRightArmOn;
        }

        public bool IsRightGripperOn()
        {
            return isRightGripperOn;
        }

        public bool IsHeadOn()
        {
            return isHeadOn;
        }

        public bool IsEmotionPlaying()
        {
            return isEmotionPlaying;
        }

        public bool IsMobilityActive()
        {
            return isMobilityActive;
        }

        public bool HasMotorsSpeedLimited()
        {
            return hasMotorsSpeedLimited;
        }

        public bool IsGraspingLockActivated()
        {
            return isGraspingLockActivated;
        }

        public void SetGraspingLockActivated(bool isActivated, bool displayPopup = true)
        {
            isGraspingLockActivated = isActivated;
            if (displayPopup)
                event_OnGraspingLock.Invoke(isActivated);
        }

        public void SetEmotionPlaying(bool isPlaying)
        {
            isEmotionPlaying = isPlaying;
        }

        public void SetMobilityActive(bool isActive)
        {
            isMobilityActive = isActive;
        }

        public void SetEmotionsActive(bool isActive)
        {
            areEmotionsActive = isActive;
        }

        public void SetMobilityOn(bool isOn)
        {
            isMobileBaseOn = isOn;
            event_OnSwitchMobilityOn.Invoke(isOn);
        }

        public void SetLeftArmOn(bool isOn)
        {
            isLeftArmOn = isOn;
        }

        public void SetLeftGripperOn(bool isOn)
        {
            isLeftGripperOn = isOn;
        }

        public void SetRightArmOn(bool isOn)
        {
            isRightArmOn = isOn;
        }

        public void SetRightGripperOn(bool isOn)
        {
            isRightGripperOn = isOn;
        }

        public void SetHeadOn(bool isOn)
        {
            isHeadOn = isOn;
        }

        public void SetIKMode(IKConstrainedMode mode)
        {
            armIkMode = mode;
        }

        public IKConstrainedMode GetIKMode()
        {
            return armIkMode;
        }

        public void LockRobotPosition()
        {
            IsRobotPositionLocked = true;
        }

        private void StartRobotTeleoperation()
        {
            Debug.Log("[RobotStatus]: Start teleoperation");
            isRobotTeleoperationActive = true;
            IsRobotPositionLocked = false;
            // event_OnStartTeleoperation.Invoke();
        }

        private void StartArmTeleoperation()
        {
            Debug.Log("[RobotStatus]: Start arm teleoperation");
            isRobotArmTeleoperationActive = true;
            // event_OnStartArmTeleoperation.Invoke();
        }

        private void StopRobotTeleoperation()
        {
            Debug.Log("[RobotStatus]: Stop teleoperation");
            isRobotTeleoperationActive = false;
            isRobotArmTeleoperationActive = false;
            SetMobilityActive(false);
        }

        public void SetMotorsSpeedLimited(bool isLimited)
        {
            hasMotorsSpeedLimited = isLimited;
        }

        public void SetRobotCompliant(bool isCompliant)
        {
            isRobotCompliant = isCompliant;
            if (isRobotCompliant)
            {
                event_OnRobotFullyCompliant.Invoke();
            }
        }

        private void SuspendRobotTeleoperation()
        {
            areRobotMovementsSuspended = true;
        }

        private void ResumeRobotTeleoperation()
        {
            areRobotMovementsSuspended = false;
        }

        public override string ToString()
        {
            return string.Format(@"isRobotTeleoperationActive = {0},
             areRobotMovementsSuspended= {1},
             isRobotCompliant= {2},
             isMobileBaseOn= {5},
             isLeftArmOn= {6},
             isRightArmOn= {7},
             isHeadOn= {8},
             isMobilityActive= {9},
             areEmotionsActive= {10},
             isEmotionPlaying= {11},
             statusChanged= {12},
             hasMotorsSpeedLimited= {13}",
             isRobotTeleoperationActive, areRobotMovementsSuspended, isRobotCompliant,
              isMobileBaseOn, isLeftArmOn, isRightArmOn, isHeadOn,
               isMobilityActive, areEmotionsActive, isEmotionPlaying, statusChanged, isGraspingLockActivated);
        }
    }
}