using System.Collections;
using System.Collections.Generic;
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
        public bool IsNavigationEffectOnDemand { get; set; }

        public bool RequestNavigationEffect { get; private set; }

        private ControllersManager controllers;
        private RobotStatus robotStatus;

        private bool rightJoystickButtonPreviouslyPressed;
        private bool leftJoystickButtonPreviouslyPressed;

        public UnityEvent<bool> event_OnRequestNavigationEffect;

        protected override void Init()
        {
            IsReticleOn = true;
            IsReticleAlwaysShown = false;

            IsTunnellingOn = false;
            IsReducedScreenOn = false;
            IsNavigationEffectOnDemand = false;

            controllers = ControllersManager.Instance;
            EventManager.StartListening(EventNames.MirrorSceneLoaded, FinishInit);
        }

        void FinishInit()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(InitOnDemandRequest);
        }

        void ActivateDeactivateTunnelling(bool value)
        {
            GameObject camera = GameObject.Find("Main Camera");
            camera.transform.GetComponent<TunnellingMobile>().enabled = value;
        }

        void InitOnDemandRequest()
        {
            RequestNavigationEffect = false;
            ActivateDeactivateTunnelling(IsTunnellingOn && !IsNavigationEffectOnDemand);
        }

        void Update()
        {
            bool rightJoystickButtonPressed;
            bool leftJoystickButtonPressed;

            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out rightJoystickButtonPressed);
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out leftJoystickButtonPressed);

            if (IsNavigationEffectOnDemand && robotStatus.IsRobotTeleoperationActive() && !robotStatus.AreRobotMovementsSuspended())
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
