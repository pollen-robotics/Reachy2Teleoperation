using UnityEngine;
using UnityEngine.Events;
using Sigtrap.VrTunnellingPro;


namespace TeleopReachy
{
    public class MotionSicknessManager : Singleton<MotionSicknessManager>
    {
        public bool IsReticleOn { get; set; }
        // public bool IsReticleAlwaysShown { get; set; }

        public bool IsTunnellingAutoOn { get; set; }
        public bool IsReducedScreenAutoOn { get; set; }
        // public bool IsNavigationEffectOnDemandOnly { get; set; }

        public bool IsTunnellingOnClickOn { get; set; }
        public bool IsReducedScreenOnClickOn { get; set; }

        public bool RequestNavigationEffect { get; private set; }

        private ControllersManager controllers;
        private RobotStatus robotStatus;
        private UserMobilityFakeMovement mobilityFakeMovement;

        private bool rightJoystickButtonPreviouslyPressed;
        private bool leftJoystickButtonPreviouslyPressed;

        public UnityEvent<bool> event_OnRequestNavigationEffect;
        public UnityEvent event_OnNewTeleopSession;
        public UnityEvent event_OnUpdateMotionSicknessPreferences;

        private bool firstStart;

        protected override void Init()
        {
            IsReticleOn = false;
            // IsReticleAlwaysShown = false;

            IsTunnellingAutoOn = true;
            IsReducedScreenAutoOn = false;
            // IsNavigationEffectOnDemandOnly = false;

            IsTunnellingOnClickOn = false;
            IsReducedScreenOnClickOn = true;

            controllers = ControllersManager.Instance;
            EventManager.StartListening(EventNames.MirrorSceneLoaded, FinishInit);
        }

        private void Start()
        {
            firstStart = true;
        }

        private void BeginNewSession()
        {
            event_OnNewTeleopSession.Invoke();
        }

        void FinishInit()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(InitOnDemandRequest);
            HeadsetRemovedInMirrorManager.Instance.event_OnHeadsetReset.AddListener(BeginNewSession);
            if(firstStart)
            {
                firstStart = false;
                BeginNewSession();
            }

            mobilityFakeMovement = UserInputManager.Instance.UserMobilityFakeMovement;
        }

        void ActivateDeactivateTunnelling(bool value)
        {
            mobilityFakeMovement.AskForFakeStaticMovement(!RequestNavigationEffect && IsTunnellingOnClickOn);
            GameObject camera = GameObject.Find("Main Camera");
            camera.transform.GetComponent<TunnellingMobile>().enabled = value;
        }

        void InitOnDemandRequest()
        {
            RequestNavigationEffect = false;
            ActivateDeactivateTunnelling(IsTunnellingOnClickOn || IsTunnellingAutoOn);
        }

        public void UpdateMotionSicknessPreferences()
        {
            event_OnUpdateMotionSicknessPreferences.Invoke();
        }

        void Update()
        {
            bool rightJoystickButtonPressed;
            bool leftJoystickButtonPressed;

            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out rightJoystickButtonPressed);
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out leftJoystickButtonPressed);

            if (robotStatus!= null && robotStatus.IsRobotTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                if(IsTunnellingOnClickOn || IsReducedScreenOnClickOn)
                {
                    if (rightJoystickButtonPressed && !rightJoystickButtonPreviouslyPressed)
                    {
                        RequestNavigationEffect = !RequestNavigationEffect;
                        mobilityFakeMovement.AskForFakeConstantMovement(RequestNavigationEffect && IsTunnellingOnClickOn);
                        mobilityFakeMovement.AskForFakeStaticMovement(!RequestNavigationEffect && IsTunnellingOnClickOn);
                        event_OnRequestNavigationEffect.Invoke(RequestNavigationEffect);
                    }
                    if (leftJoystickButtonPressed && !leftJoystickButtonPreviouslyPressed)
                    {
                        RequestNavigationEffect = !RequestNavigationEffect;
                        mobilityFakeMovement.AskForFakeConstantMovement(RequestNavigationEffect && IsTunnellingOnClickOn);
                        mobilityFakeMovement.AskForFakeStaticMovement(!RequestNavigationEffect && IsTunnellingOnClickOn);
                        event_OnRequestNavigationEffect.Invoke(RequestNavigationEffect);
                    }
                }
            }

            rightJoystickButtonPreviouslyPressed = rightJoystickButtonPressed;
            leftJoystickButtonPreviouslyPressed = leftJoystickButtonPressed;
        }
    }
}
