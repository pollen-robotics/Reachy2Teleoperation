using UnityEngine;

namespace TeleopReachy
{
    public class TrackedHandManager : MonoBehaviour
    {
        public ArmSide side_id;

        void Start()
        {
            ControllersManager.Instance.event_OnDevicesUpdate.AddListener(DefineTrackedHandOrientation_Old);
            //CaptureWristPose.Instance.event_NeutralPoseCaptured.AddListener(DefineTrackedHandOrientation_New);
            CaptureWristPose.Instance.event_NeutralPoseCaptured.AddListener(InitSwitchCalib);


        }

        private void InitSwitchCalib()
        {
            SwitchCalibrationManager.Instance.event_OldCalibAsked.AddListener(DefineTrackedHandOrientation_Old);
            SwitchCalibrationManager.Instance.event_NewCalibAsked.AddListener(DefineTrackedHandOrientation_New);
            SwitchCalibrationManager.Instance.event_FakeCalibAsked.AddListener(DefineTrackedHandOrientation_Fake);

        }

        public void DefineTrackedHandOrientation_New()
        {
            // Vector3 targetEulerAngles = new Vector3(275, 190, 170);
            Vector3 targetEulerAngles = new Vector3(270, 180, 180);

            UnityEngine.Quaternion targetRobotRotation = new UnityEngine.Quaternion();
            targetRobotRotation = Quaternion.Euler(targetEulerAngles);
            Quaternion rightRotationDifference = Quaternion.Inverse(CaptureWristPose.Instance.rightNeutralOrientation) * targetRobotRotation;
            Quaternion leftRotationDifference = Quaternion.Inverse(CaptureWristPose.Instance.leftNeutralOrientation) * targetRobotRotation;
            Debug.Log("rightRotationDifference: " + rightRotationDifference.eulerAngles);
            Debug.Log("leftRotationDifference: " + leftRotationDifference.eulerAngles);
            if (side_id == ArmSide.LEFT) transform.localRotation = leftRotationDifference;
            else transform.localRotation = rightRotationDifference;

        }

        public void DefineTrackedHandOrientation_Old()
        {
           
            switch (ControllersManager.Instance.controllerDeviceType)
            {
                case ControllersManager.SupportedDevices.Oculus:
                    {
                        //transform.localPosition = new Vector3(0, -0.03f, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(-95, -35, 50);
                        else targetRotation.eulerAngles = new Vector3(-95, 35, -50);
                        transform.localRotation = targetRotation;
                        break;
                    }
                case ControllersManager.SupportedDevices.MetaQuest3:
                    {
                        transform.localPosition = new Vector3(0, -0.03f, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(-70, 5, 5);
                        else targetRotation.eulerAngles = new Vector3(-70, -5, -5);
                        transform.localRotation = targetRotation;
                        break;
                    }
                case ControllersManager.SupportedDevices.ValveIndex:
                    {
                        transform.localPosition = new Vector3(0, 0, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(0, 0, 20);
                        else targetRotation.eulerAngles = new Vector3(0, 0, -20);
                        transform.localRotation = targetRotation;
                        break;
                    }
                case ControllersManager.SupportedDevices.HTCVive:
                    {
                        transform.localPosition = new Vector3(0, 0, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(0, 0, 0);
                        else targetRotation.eulerAngles = new Vector3(0, 0, 0);
                        transform.localRotation = targetRotation;
                        break;
                    }
            }

        }

        public void DefineTrackedHandOrientation_Fake()
        {
            Vector3 targetEulerAngles = new Vector3(275, 190, 170);
            UnityEngine.Quaternion targetRobotRotation = new UnityEngine.Quaternion();
            targetRobotRotation = Quaternion.Euler(targetEulerAngles);
            Quaternion rightRotationDifference = targetRobotRotation * Quaternion.Inverse(CaptureWristPose.Instance.rightNeutralOrientation);
            Quaternion leftRotationDifference = targetRobotRotation *  Quaternion.Inverse(CaptureWristPose.Instance.leftNeutralOrientation);
            Debug.Log("rightRotationDifference: " + rightRotationDifference.eulerAngles);
            Debug.Log("leftRotationDifference: " + leftRotationDifference.eulerAngles);
            if (side_id == ArmSide.LEFT) transform.localRotation = leftRotationDifference;
            else transform.localRotation = rightRotationDifference;

        }
            
            // WristCalibINCIA wristCalib = WristCalibINCIA.Instance;
            // Vector3 position;
            // if (side_id == ArmSide.LEFT) position = wristCalib.leftWristCenter;
            // else position = wristCalib.rightWristCenter;
            // transform.localPosition = position;
    



    }
}
