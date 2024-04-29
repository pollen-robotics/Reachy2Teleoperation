using UnityEngine;

namespace TeleopReachy
{
    public class TrackedHandManager : MonoBehaviour
    {
        public ArmSide side_id;

        void Start()
        {
            //ControllersManager.Instance.event_OnDevicesUpdate.AddListener(DefineTrackedHandOrientation);
            WristCalibINCIA.Instance.event_OnWristCalibChanged.AddListener(DefineTrackedHandOrientation);
        }

        private void DefineTrackedHandOrientation()
        {
            WristCalibINCIA wristCalib = WristCalibINCIA.Instance;
            Vector3 position;
            if (side_id == ArmSide.LEFT) position = wristCalib.leftWristCenter;
            else position = wristCalib.rightWristCenter;
            transform.localPosition = position;

            switch (ControllersManager.Instance.controllerDeviceType)
            {
                case ControllersManager.SupportedDevices.Oculus:
                    {
                        //transform.localPosition = new Vector3(0, -0.03f, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(-70, 5, 5);
                        else targetRotation.eulerAngles = new Vector3(-70, -5, -5);
                        transform.localRotation = targetRotation;
                        break;
                    }
                case ControllersManager.SupportedDevices.MetaQuest3:
                    {
                        //transform.localPosition = new Vector3(0, -0.03f, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(-70, 5, 5);
                        else targetRotation.eulerAngles = new Vector3(-70, -5, -5);
                        transform.localRotation = targetRotation;
                        break;
                    }
                case ControllersManager.SupportedDevices.ValveIndex:
                    {
                        //transform.localPosition = new Vector3(0, 0, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(0, 0, 20);
                        else targetRotation.eulerAngles = new Vector3(0, 0, -20);
                        transform.localRotation = targetRotation;
                        break;
                    }
                case ControllersManager.SupportedDevices.HTCVive:
                    {
                        //transform.localPosition = new Vector3(0, 0, 0);
                        UnityEngine.Quaternion targetRotation = new UnityEngine.Quaternion();
                        if (side_id == ArmSide.LEFT) targetRotation.eulerAngles = new Vector3(0, 0, 0);
                        else targetRotation.eulerAngles = new Vector3(0, 0, 0);
                        transform.localRotation = targetRotation;
                        break;
                    }
            }
        }
    }
}
