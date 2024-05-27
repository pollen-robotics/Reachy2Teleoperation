using UnityEngine;
using System.Collections;

namespace TeleopReachy
{
    public class TrackedHandManager : MonoBehaviour
    {
        public ArmSide side_id;
        private Quaternion rightRotationDifference = new Quaternion();
        private Quaternion leftRotationDifference= new Quaternion();
        public Quaternion neutralOrientation = Quaternion.Euler(270, 180, 180);
        private CaptureWristPose captureWristPose;

        void Start()
        {
            StartCoroutine(WaitForWristCalibration());
            EventManager.StartListening(EventNames.BackToMirrorScene, ReinitValue);
            //ControllersManager.Instance.event_OnDevicesUpdate.AddListener(DefineTrackedHandOrientation_Old);

        }

        private IEnumerator WaitForWristCalibration()
        {
            while (true)
            {
                var newCaptureWristPose = FindObjectOfType<CaptureWristPose>();
                if (newCaptureWristPose != null && newCaptureWristPose != captureWristPose)
                {
                    if (captureWristPose != null) captureWristPose.event_NeutralPoseCaptured.RemoveListener(DefineTrackedHandOrientation_New);

                    captureWristPose = newCaptureWristPose;
                    captureWristPose.event_NeutralPoseCaptured.AddListener(DefineTrackedHandOrientation_New);
                    SwitchCalibrationManager.Instance.event_OldCalibAsked.AddListener(DefineTrackedHandOrientation_Old);
                    SwitchCalibrationManager.Instance.event_NewCalibAsked.AddListener(DefineTrackedHandOrientation_New);
                    SwitchCalibrationManager.Instance.event_FakeCalibAsked.AddListener(DefineTrackedHandOrientation_Fake);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void ReinitValue()
        {
            transform.localRotation = Quaternion.identity;
            StartCoroutine(WaitForWristCalibration());
            Debug.Log("[TrackedHandManager] : Reinit value");
        }

        public void DefineTrackedHandOrientation_New()
        {
            Vector3 targetEulerAngles = neutralOrientation.eulerAngles;
            UnityEngine.Quaternion targetRobotRotation = new UnityEngine.Quaternion();
            targetRobotRotation = Quaternion.Euler(targetEulerAngles);
            
            rightRotationDifference = Quaternion.Inverse(CaptureWristPose.Instance.rightNeutralOrientation) * targetRobotRotation;
            leftRotationDifference = Quaternion.Inverse(CaptureWristPose.Instance.leftNeutralOrientation) * targetRobotRotation;

            if (side_id == ArmSide.LEFT) transform.localRotation = leftRotationDifference;
            else transform.localRotation = rightRotationDifference;
        }

        public void DefineTrackedHandOrientation_Old()
        {
           
            switch (ControllersManager.Instance.controllerDeviceType)
            {
                case ControllersManager.SupportedDevices.Oculus:
                    {
                        transform.localPosition = new Vector3(0, -0.03f, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(-70, 5, 5);
                        else targetRotation.eulerAngles = new Vector3(-70, -5, -5);
                        // if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(-95, -35, 50);
                        // else targetRotation.eulerAngles = new Vector3(-95, 35, -50);
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
            if (side_id == ArmSide.LEFT) transform.localRotation = leftRotationDifference;
            else transform.localRotation = rightRotationDifference;
        }

    }
}
