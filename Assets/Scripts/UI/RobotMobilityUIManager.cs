using UnityEngine;


namespace TeleopReachy
{
    public class RobotMobilityUIManager : MonoBehaviour
    {
        [SerializeField]
        private Reachy2Controller.Reachy2Controller reachyController;

        private UserMobilityInput userMobilityInput = null;
        private RobotStatus robotStatus;
        private RobotConfig robotConfig;

        private ConnectionStatus connectionStatus;

        private Vector2 directionLeft;
        private Vector2 directionRight;

        private bool ShowMobilityUIListenerSet = false;
        private bool HideMobilityUIListenerSet = false;

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

        private ControllersManager controllers;

        private void OnEnable()
        {
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, Init);
        }

        private void OnDisable()
        {
            EventManager.StopListening(EventNames.TeleoperationSceneLoaded, Init);
        }

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                transform.localPosition = new Vector3(0, -189, -479);
            }
            connectionStatus = ConnectionStatus.Instance;
            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(Init);
        }

        private void Init()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            UpdateMobilityUI(robotConfig.HasMobileBase());

            if (robotConfig.HasMobileBase())
                robotStatus.event_OnSwitchMobilityOn.AddListener(UpdateMobilityUI);
        }

        void UpdateMobilityUI(bool on)
        {
            if (!on)
            {
                userMobilityInput = null;
                ShowMobilityUIListenerSet = false;
                HideMobilityUIListenerSet = false;
                EventManager.StopListening(EventNames.OnStartTeleoperation, ShowMobilityUI);
                EventManager.StopListening(EventNames.OnStopTeleoperation, HideMobilityUI);
                HideMobilityUI();
            }
            else
            {
                userMobilityInput = UserInputManager.Instance.UserMobilityInput;

                if (ShowMobilityUIListenerSet == false)
                {
                    EventManager.StartListening(EventNames.OnStartTeleoperation, ShowMobilityUI);
                    ShowMobilityUIListenerSet = true;
                }
                if (HideMobilityUIListenerSet == false)
                {
                    EventManager.StartListening(EventNames.OnStopTeleoperation, HideMobilityUI);
                    HideMobilityUIListenerSet = true;
                }
            }
        }

        void Update()
        {
            //not initialized yet.
            if (userMobilityInput == null)
                return;

            float orbita_yaw = -reachyController.headOrientation[2];
            if (orbita_yaw > 180)
            {
                orbita_yaw -= 360;
            }
            float x_pos = Mathf.Abs(orbita_yaw * 4) < 360 ? orbita_yaw * 4 : (orbita_yaw > 0 ? 360 - Mathf.Abs(360 - Mathf.Abs(orbita_yaw * 4)) : -360 + Mathf.Abs(-360 + Mathf.Abs(orbita_yaw * 4)));
            arrow.parent.localPosition = new Vector3(x_pos, 0, 0);

            arrow.localEulerAngles = new Vector3(0, 0, -orbita_yaw);

            if (userMobilityInput.CanGetUserMobilityInputs())
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

        public void HideMobilityUI()
        {
            transform.ActivateChildren(false);
        }

        private void ShowMobilityUI()
        {
            transform.ActivateChildren(true);
            arrowRightRotationCommand.gameObject.SetActive(false);
            arrowLeftRotationCommand.gameObject.SetActive(false);
        }
    }
}