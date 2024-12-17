using UnityEngine;
using UnityEngine.Events;
using Sigtrap.VrTunnellingPro;


namespace TeleopReachy
{
    public class MotionSicknessManager : Singleton<MotionSicknessManager>
    {
        public bool IsReticleOn { get; private set; }

        public bool IsTunnellingAutoOn { get; private set; }
        public bool IsReducedScreenAutoOn { get; private set; }

        public bool IsTunnellingOnClickOn { get; private set; }
        public bool IsReducedScreenOnClickOn { get; private set; }

        public bool RequestNavigationEffect { get; private set; }

        private ControllersManager controllers;
        private RobotStatus robotStatus;
        private UserMobilityFakeMovement mobilityFakeMovement;

        private bool rightJoystickButtonPreviouslyPressed;
        private bool leftJoystickButtonPreviouslyPressed;

        public UnityEvent<bool> event_OnRequestNavigationEffect;
        private OptionsManager optionsManager;

        protected override void Init()
        {
            optionsManager = OptionsManager.Instance;
            controllers = ControllersManager.Instance;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            mobilityFakeMovement = UserInputManager.Instance.UserMobilityFakeMovement;

            IsReticleOn = optionsManager.isReticleOn;

            IsTunnellingAutoOn = robotStatus.IsMobileBaseOn() && (optionsManager.motionSicknessEffectAuto == OptionsManager.MotionSicknessEffect.Tunnelling);
            IsReducedScreenAutoOn = robotStatus.IsMobileBaseOn() && (optionsManager.motionSicknessEffectAuto == OptionsManager.MotionSicknessEffect.ReducedScreen);

            IsTunnellingOnClickOn = (optionsManager.motionSicknessEffectOnClick == OptionsManager.MotionSicknessEffect.Tunnelling);
            IsReducedScreenOnClickOn = (optionsManager.motionSicknessEffectOnClick == OptionsManager.MotionSicknessEffect.ReducedScreen);

            InitOnDemandRequest();
        }

        void ActivateDeactivateTunnelling(bool value)
        {
            GameObject camera = GameObject.Find("Main Camera");
            camera.transform.GetComponent<TunnellingMobile>().enabled = value;
            mobilityFakeMovement.SetClickMode(IsTunnellingOnClickOn);
            mobilityFakeMovement.SetNavigationMode(IsTunnellingAutoOn);
        }

        void InitOnDemandRequest()
        {
            RequestNavigationEffect = false;
            ActivateDeactivateTunnelling(IsTunnellingOnClickOn || IsTunnellingAutoOn);
        }

        void Update()
        {
            bool rightJoystickButtonPressed;
            bool leftJoystickButtonPressed;

            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out rightJoystickButtonPressed);
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out leftJoystickButtonPressed);

            if (!robotStatus.AreRobotMovementsSuspended() && !TeleoperationSceneManager.Instance.IsTeleoperationExitMenuActive)
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
