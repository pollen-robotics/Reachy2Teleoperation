using UnityEngine;
using System.Collections.Generic;


namespace TeleopReachy
{
    public class RobotDirectionUIManager : MonoBehaviour
    {
        private UserMobilityInput userMobilityInput = null;
        private RobotJointState robotJointState;
        private RobotStatus robotStatus;

        private ConnectionStatus connectionStatus;
        private TeleoperationManager teleoperationManager;

        private Vector2 directionLeft;
        private Vector2 directionRight;

        private Vector3 headOrientation;

        [SerializeField]
        private Transform arrow;

        [SerializeField]
        private Transform arrowMobilityCommand;

        [SerializeField]
        private Transform circleMobilityCommand;

        [SerializeField]
        private Transform arrowRightRotationCommand;

        [SerializeField]
        private Transform arrowLeftRotationCommand;

        void Start()
        {
            InitializeUIPosition();
            InitializeDisplayedElements();
            headOrientation = new Vector3(0, 0, 0);

            teleoperationManager = TeleoperationManager.Instance;
            userMobilityInput = UserInputManager.Instance.UserMobilityInput;
            robotJointState = RobotDataManager.Instance.RobotJointState;
            robotJointState.event_OnPresentPositionsChanged.AddListener(GetHeadOrientation);
            robotStatus = RobotDataManager.Instance.RobotStatus;
        }

        void InitializeUIPosition()
        {
            ControllersManager controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                transform.localPosition = new Vector3(0, -189, -479);
            }
        }

        private void InitializeDisplayedElements()
        {
            arrowRightRotationCommand.gameObject.SetActive(false);
            arrowLeftRotationCommand.gameObject.SetActive(false);
        }

        protected void GetHeadOrientation(Dictionary<string, float> presentPositions)
        {
            foreach (KeyValuePair<string, float> kvp in presentPositions)
            {
                string motorName = kvp.Key;
                if (motorName == "head_neck_roll")
                {
                    headOrientation[0] = kvp.Value;
                }
                if (motorName == "head_neck_pitch")
                {
                    headOrientation[1] = kvp.Value;
                }
                if (motorName == "head_neck_yaw")
                {
                    headOrientation[2] = -kvp.Value;
                }
            }
        }

        void Update()
        {
            float orbita_yaw = headOrientation[2];
            if (orbita_yaw > 180)
            {
                orbita_yaw -= 360;
            }
            float x_pos = Mathf.Abs(orbita_yaw * 4) < 360 ? orbita_yaw * 4 : (orbita_yaw > 0 ? 360 - Mathf.Abs(360 - Mathf.Abs(orbita_yaw * 4)) : -360 + Mathf.Abs(-360 + Mathf.Abs(orbita_yaw * 4)));
            arrow.parent.localPosition = new Vector3(x_pos, 0, 0);

            arrow.localEulerAngles = new Vector3(0, 0, -orbita_yaw);

            if (teleoperationManager.IsMobileBaseTeleoperationActive && robotStatus.IsMobileBaseOn() && !robotStatus.AreRobotMovementsSuspended())
            {
                directionLeft = userMobilityInput.GetMobileBaseDirection();
                directionRight = userMobilityInput.GetAngleDirection();

                float rotation = -directionRight[0];

                arrowRightRotationCommand.gameObject.SetActive(rotation < 0);
                arrowLeftRotationCommand.gameObject.SetActive(rotation > 0);


                if (directionLeft[0] == 0 && directionLeft[1] == 0)
                {
                    circleMobilityCommand.localEulerAngles = new Vector3(0, 0, orbita_yaw + 90);
                    IsRobotStatic(true);
                }
                else
                {
                    IsRobotStatic(false);
                    float phi = Mathf.Atan2(directionLeft[1], directionLeft[0]);
                    circleMobilityCommand.localEulerAngles = -new Vector3(0, 0, orbita_yaw) + new Vector3(0, 0, Mathf.Rad2Deg * phi);
                }
            }
            else
            {
                IsRobotStatic(true);
            }
        }

        private void IsRobotStatic(bool isStatic)
        {
            arrowMobilityCommand.gameObject.SetActive(!isStatic);
        }
    }
}