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

        public bool IsTunnellingOn { get; private set; }
        public bool IsReducedScreenOn { get; private set; }
        public bool IsNavigationEffectOnDemand { get; private set; }

        void Awake()
        {
            noEffectButton.onClick.AddListener(SwitchToNoEffectMode);
            tunnellingButton.onClick.AddListener(SwitchToTunnellingMode);
            reducedScreenButton.onClick.AddListener(SwitchToReducedScreenMode);
            onDemandToggle.onValueChanged.AddListener(SwitchToggleMode);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;

            robotConfig.event_OnConfigChanged.AddListener(CheckMobileBasePresence);

            IsTunnellingOn = false;
            IsReducedScreenOn = false;
            onDemandToggle.interactable = false;

            CheckMobileBasePresence();
        }

        void SwitchToggleMode(bool value)
        {
            IsNavigationEffectOnDemand = value;
        }

        void SwitchToNoEffectMode()
        {
            IsTunnellingOn = false;
            IsReducedScreenOn = false;
            
            noEffectButton.colors = ColorsManager.colorsActivated;
            tunnellingButton.colors = ColorsManager.colorsDeactivated;
            reducedScreenButton.colors = ColorsManager.colorsDeactivated;

            isToggleInteractable = false;
            needUpdateToggle = true;
        }

        void SwitchToTunnellingMode()
        {
            IsTunnellingOn = true;
            IsReducedScreenOn = false;
            
            noEffectButton.colors = ColorsManager.colorsDeactivated;
            tunnellingButton.colors = ColorsManager.colorsActivated;
            reducedScreenButton.colors = ColorsManager.colorsDeactivated;
            
            isToggleInteractable = true;
            needUpdateToggle = true;
        }

        void SwitchToReducedScreenMode()
        {
            IsTunnellingOn = false;
            IsReducedScreenOn = true;
            
            noEffectButton.colors = ColorsManager.colorsDeactivated;
            tunnellingButton.colors = ColorsManager.colorsDeactivated;
            reducedScreenButton.colors = ColorsManager.colorsActivated;
            
            isToggleInteractable = true;
            needUpdateToggle = true;
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
            Debug.LogError("CheckMobileBasePresence");
            if (robotConfig.HasMobileBase())
            {
                areButtonsInteractable = true;
            }
            else 
            {
                areButtonsInteractable = false;
                IsTunnellingOn = false;
                IsReducedScreenOn = false;
                tunnellingButtonColor = ColorsManager.colorsDeactivated;
                reducedScreenButtonColor = ColorsManager.colorsDeactivated;
                noEffectButtonColor = ColorsManager.colorsActivated;
            }
            if(IsTunnellingOn || IsReducedScreenOn)
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