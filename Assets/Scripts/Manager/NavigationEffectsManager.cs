using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class NavigationEffectsManager : MonoBehaviour
    {
        [SerializeField]
        public Button noEffectButton;

        [SerializeField]
        public Button tunnellingButton;

        [SerializeField]
        public Button reducedScreenButton;

        [SerializeField]
        public Toggle onDemandToggle;
        
        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        private bool needUpdateButtons = false;
        private bool needUpdateToggle = false;
        private bool areButtonsInteractable = false;
        private bool isToggleInteractable = false;
        private ColorBlock noEffectButtonColor;
        private ColorBlock tunnellingButtonColor;
        private ColorBlock reducedScreenButtonColor;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            motionSicknessManager = MotionSicknessManager.Instance;
            ChooseButtonsMode();
            onDemandToggle.isOn = motionSicknessManager.IsNavigationEffectOnDemand;

            noEffectButton.onClick.AddListener(SwitchToNoEffectMode);
            tunnellingButton.onClick.AddListener(SwitchToTunnellingMode);
            reducedScreenButton.onClick.AddListener(SwitchToReducedScreenMode);
            onDemandToggle.onValueChanged.AddListener(SwitchToggleMode);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;

            robotConfig.event_OnConfigChanged.AddListener(CheckMobileBasePresence);

            onDemandToggle.interactable = false;

            CheckMobileBasePresence();
        }

        void ChooseButtonsMode()
        {
            if(motionSicknessManager.IsTunnellingOn) SwitchToTunnellingMode();
            else if (motionSicknessManager.IsReducedScreenOn) SwitchToReducedScreenMode();
            else SwitchToNoEffectMode();
        }

        void SwitchToggleMode(bool value)
        {
            motionSicknessManager.IsNavigationEffectOnDemand = value;
        }

        void SwitchToNoEffectMode()
        {
            motionSicknessManager.IsTunnellingOn = false;
            motionSicknessManager.IsReducedScreenOn = false;
            
            noEffectButtonColor = ColorsManager.colorsActivated;
            tunnellingButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenButtonColor = ColorsManager.colorsDeactivated;

            isToggleInteractable = false;
            needUpdateToggle = true;
            needUpdateButtons = true;
        }

        void SwitchToTunnellingMode()
        {
            motionSicknessManager.IsTunnellingOn = true;
            motionSicknessManager.IsReducedScreenOn = false;
            
            noEffectButtonColor = ColorsManager.colorsDeactivated;
            tunnellingButtonColor = ColorsManager.colorsActivated;
            reducedScreenButtonColor = ColorsManager.colorsDeactivated;
            
            isToggleInteractable = true;
            needUpdateToggle = true;
            needUpdateButtons = true;
        }

        void SwitchToReducedScreenMode()
        {
            motionSicknessManager.IsTunnellingOn = false;
            motionSicknessManager.IsReducedScreenOn = true;
            
            noEffectButtonColor = ColorsManager.colorsDeactivated;
            tunnellingButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenButtonColor = ColorsManager.colorsActivated;
            
            isToggleInteractable = true;
            needUpdateToggle = true;
            needUpdateButtons = true;
        }

        void Update()
        {
            if(needUpdateToggle)
            {
                onDemandToggle.interactable = isToggleInteractable;
                if(!isToggleInteractable) onDemandToggle.transform.GetChild(1).GetComponent<Text>().color = ColorsManager.grey;
                else onDemandToggle.transform.GetChild(1).GetComponent<Text>().color = ColorsManager.white;
                needUpdateToggle = false;
            }
            if(needUpdateButtons)
            {
                tunnellingButton.interactable = areButtonsInteractable;
                tunnellingButton.colors = tunnellingButtonColor;
                reducedScreenButton.interactable = areButtonsInteractable;
                reducedScreenButton.colors = reducedScreenButtonColor;
                noEffectButton.colors = noEffectButtonColor;
                needUpdateButtons = false;
            }
        }

        void CheckMobileBasePresence()
        {
            if (robotConfig.HasMobileBase() && robotStatus.IsMobilityOn())
            {
                areButtonsInteractable = true;
            }
            else 
            {
                areButtonsInteractable = false;
                motionSicknessManager.IsTunnellingOn = false;
                motionSicknessManager.IsReducedScreenOn = false;
                tunnellingButtonColor = ColorsManager.colorsDeactivated;
                reducedScreenButtonColor = ColorsManager.colorsDeactivated;
                noEffectButtonColor = ColorsManager.colorsActivated;
            }
            if(motionSicknessManager.IsTunnellingOn || motionSicknessManager.IsReducedScreenOn)
            {
                isToggleInteractable = true;
            }
            else
            {
                isToggleInteractable = false;
            }
            needUpdateToggle = true;
            needUpdateButtons = true;
        }
    }
}