using UnityEngine;
using UnityEngine.Events;
using Sigtrap.VrTunnellingPro;


namespace TeleopReachy
{
    public class MotionSicknessManager : Singleton<MotionSicknessManager>
    {
        public bool IsReticleOn { get; set; }
        public bool IsReticleAlwaysShown { get; set; }

        public bool IsTunnellingOn { get; set; }
        public bool IsReducedScreenOn { get; set; }
        public bool IsNavigationEffectOnDemandOnly { get; set; }

        public bool RequestNavigationEffect { get; private set; }

        public bool AreOptionsSaved { get; private set; }

        private ControllersManager controllers;
        private RobotStatus robotStatus;

        private bool rightJoystickButtonPreviouslyPressed;
        private bool leftJoystickButtonPreviouslyPressed;

        public UnityEvent<bool> event_OnRequestNavigationEffect;
        public UnityEvent event_OnNewTeleopSession;
        public UnityEvent event_OnUpdateMotionSicknessPreferences;

        private bool firstStart;

        protected override void Init()
        {
            IsReticleOn = false;
            IsReticleAlwaysShown = false;

            IsTunnellingOn = false;
            IsReducedScreenOn = false;
            IsNavigationEffectOnDemandOnly = false;

            AreOptionsSaved = false;

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

        public void SaveOptionsForAllSessions(bool value)
        {
            AreOptionsSaved = value;
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
        }

        void ActivateDeactivateTunnelling(bool value)
        {
            GameObject camera = GameObject.Find("Main Camera");
            camera.transform.GetComponent<TunnellingMobile>().enabled = value;
        }

        void InitOnDemandRequest()
        {
            RequestNavigationEffect = false;
            ActivateDeactivateTunnelling(IsTunnellingOn && !IsNavigationEffectOnDemandOnly);
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

            if (IsNavigationEffectOnDemandOnly && robotStatus.IsRobotTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
            {
                if (rightJoystickButtonPressed && !rightJoystickButtonPreviouslyPressed)
                {
                    RequestNavigationEffect = !RequestNavigationEffect;
                    ActivateDeactivateTunnelling(IsTunnellingOn && RequestNavigationEffect);
                    event_OnRequestNavigationEffect.Invoke(RequestNavigationEffect);
                }
                if (leftJoystickButtonPressed && !leftJoystickButtonPreviouslyPressed)
                {
                    RequestNavigationEffect = !RequestNavigationEffect;
                    ActivateDeactivateTunnelling(IsTunnellingOn && RequestNavigationEffect);
                    event_OnRequestNavigationEffect.Invoke(RequestNavigationEffect);
                }
            }

            rightJoystickButtonPreviouslyPressed = rightJoystickButtonPressed;
            leftJoystickButtonPreviouslyPressed = leftJoystickButtonPressed;
        }
    }
}
