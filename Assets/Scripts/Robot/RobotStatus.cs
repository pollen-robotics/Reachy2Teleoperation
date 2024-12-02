using UnityEngine;
using UnityEngine.Events;
using Reachy.Part.Arm;

namespace TeleopReachy
{
    public class RobotStatus : MonoBehaviour
    {
        private bool areRobotMovementsSuspended;

        private bool isRobotCompliant= true;

        // Parts are "On" if operators want to have control on it, false otherwise
        private bool isMobileBaseOn = true;
        private bool isLeftArmOn = true;
        private bool isLeftGripperOn = true;
        private bool isRightArmOn = true;
        private bool isRightGripperOn = true;
        private bool isHeadOn = true;

        private bool isEmotionPlaying;

        private bool hasMotorsSpeedLimited;

        private bool isGraspingLockActivated;

        private bool isLeftGripperClosed = false;
        private bool isRightGripperClosed = false;

        private IKConstrainedMode armIkMode = IKConstrainedMode.LowElbow;

        public bool IsRobotPositionLocked { get; private set; }

        public UnityEvent<bool> event_OnGraspingLock;
        public UnityEvent event_OnRobotFullyCompliant;

        private void Start()
        {
            EventManager.StartListening(EventNames.OnStartTeleoperation, StartRobotTeleoperation);
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

        public bool AreRobotMovementsSuspended()
        {
            return areRobotMovementsSuspended;
        }

        public bool IsRobotCompliant()
        {
            return isRobotCompliant;
        }

        public bool IsPartOn(Part part)
        {
            switch (part)
            {
                case Part.LeftArm:
                    return IsLeftArmOn();
                case Part.RightArm:
                    return IsRightArmOn();
                case Part.LeftGripper:
                    return IsLeftGripperOn();
                case Part.RightGripper:
                    return IsRightGripperOn();
                case Part.Head:
                    return IsHeadOn();
                case Part.MobileBase:
                    return IsMobileBaseOn();
                default:
                    return false;
            }
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

        public bool IsMobileBaseOn()
        {
            return isMobileBaseOn;
        }

        public bool IsEmotionPlaying()
        {
            return isEmotionPlaying;
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

        public void SetPartOn(Part part, bool isOn, bool subPartManaged=false)
        {
            switch (part)
            {
                case Part.LeftArm:
                    SetLeftArmOn(isOn);
                    if (subPartManaged) SetLeftGripperOn(isOn);
                    break;
                case Part.RightArm:
                    SetRightArmOn(isOn);
                    if (subPartManaged) SetRightGripperOn(isOn);
                    break;
                case Part.LeftGripper:
                    SetLeftGripperOn(isOn);
                    break;
                case Part.RightGripper:
                    SetRightGripperOn(isOn);
                    break;
                case Part.Head:
                    SetHeadOn(isOn);
                    break;
                case Part.MobileBase:
                    SetMobileBaseOn(isOn);
                    break;
            }
        }

        public void SetMobileBaseOn(bool isOn)
        {
            isMobileBaseOn = isOn;
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
            areRobotMovementsSuspended = false;
            IsRobotPositionLocked = false;
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
            return string.Format(@"areRobotMovementsSuspended= {0},
             isRobotCompliant= {1},
             isMobileBaseOn= {2},
             isLeftArmOn= {3},
             isRightArmOn= {4},
             isHeadOn= {5},
             isEmotionPlaying= {6},
             hasMotorsSpeedLimited= {7}",
             areRobotMovementsSuspended, isRobotCompliant,
              isMobileBaseOn, isLeftArmOn, isRightArmOn, isHeadOn,
              isEmotionPlaying, isGraspingLockActivated);
        }
    }
}