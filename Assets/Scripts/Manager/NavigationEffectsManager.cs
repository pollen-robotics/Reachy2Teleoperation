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
        public Button autoDisplayButton;

        [SerializeField]
        public Button onDemandOnlyButton;

        [SerializeField]
        private bool mustBeInitialized;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        private bool needUpdateButtons = false;
        private bool needReinit = false;

        private bool areButtonsInteractable = false;
        private ColorBlock noEffectButtonColor;
        private ColorBlock tunnellingButtonColor;
        private ColorBlock reducedScreenButtonColor;

        private ColorBlock autoDisplayButtonColor;
        private ColorBlock onDemandOnlyButtonColor;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            noEffectButton?.onClick.AddListener(SwitchToNoEffectMode);
            tunnellingButton?.onClick.AddListener(SwitchToTunnellingMode);
            reducedScreenButton?.onClick.AddListener(SwitchToReducedScreenMode);

            autoDisplayButton?.onClick.AddListener(SwitchToAutoMode);
            onDemandOnlyButton?.onClick.AddListener(SwitchToOnDemandeOnlyMode);

            tunnellingButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenButtonColor = ColorsManager.colorsDeactivated;
            noEffectButtonColor = ColorsManager.colorsDeactivated;
            autoDisplayButtonColor = ColorsManager.colorsDeactivated;
            onDemandOnlyButtonColor = ColorsManager.colorsDeactivated;

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;

            robotConfig.event_OnConfigChanged.AddListener(CheckNavigationOptionsAvailability);

            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnUpdateMotionSicknessPreferences.AddListener(ChooseButtonsMode);
            if(mustBeInitialized) ChooseButtonsMode();
            else needReinit = true;
        }

        void ChooseButtonsMode()
        {
            if (motionSicknessManager.IsTunnellingOn) SwitchToTunnellingMode();
            else if (motionSicknessManager.IsReducedScreenOn) SwitchToReducedScreenMode();
            else SwitchToNoEffectMode();

            if(motionSicknessManager.IsNavigationEffectOnDemandOnly) SwitchToOnDemandeOnlyMode();
            else SwitchToAutoMode();

            CheckNavigationOptionsAvailability();
        }

        void SwitchToAutoMode()
        {
            motionSicknessManager.IsNavigationEffectOnDemandOnly = false;

            autoDisplayButtonColor = ColorsManager.colorsActivated;
            onDemandOnlyButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToOnDemandeOnlyMode()
        {
            motionSicknessManager.IsNavigationEffectOnDemandOnly = true;

            autoDisplayButtonColor = ColorsManager.colorsDeactivated;
            onDemandOnlyButtonColor = ColorsManager.colorsActivated;

            needUpdateButtons = true;
        }

        void SwitchToNoEffectMode()
        {
            motionSicknessManager.IsTunnellingOn = false;
            motionSicknessManager.IsReducedScreenOn = false;

            noEffectButtonColor = ColorsManager.colorsActivated;
            tunnellingButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenButtonColor = ColorsManager.colorsDeactivated;

            CheckNavigationOptionsAvailability();
            needUpdateButtons = true;
        }

        void SwitchToTunnellingMode()
        {
            motionSicknessManager.IsTunnellingOn = true;
            motionSicknessManager.IsReducedScreenOn = false;

            noEffectButtonColor = ColorsManager.colorsDeactivated;
            tunnellingButtonColor = ColorsManager.colorsActivated;
            reducedScreenButtonColor = ColorsManager.colorsDeactivated;

            CheckNavigationOptionsAvailability();
            needUpdateButtons = true;
        }

        void SwitchToReducedScreenMode()
        {
            motionSicknessManager.IsTunnellingOn = false;
            motionSicknessManager.IsReducedScreenOn = true;

            noEffectButtonColor = ColorsManager.colorsDeactivated;
            tunnellingButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenButtonColor = ColorsManager.colorsActivated;

            CheckNavigationOptionsAvailability();
            needUpdateButtons = true;
        }

        void Update()
        {
            if (needUpdateButtons)
            {
                if(tunnellingButton != null) tunnellingButton.colors = tunnellingButtonColor;
                if(reducedScreenButton != null) reducedScreenButton.colors = reducedScreenButtonColor;
                if(noEffectButton != null) noEffectButton.colors = noEffectButtonColor;

                if(autoDisplayButton != null) 
                {
                    autoDisplayButton.interactable = areButtonsInteractable;
                    autoDisplayButton.colors = autoDisplayButtonColor;
                }
                
                if(onDemandOnlyButton != null) 
                {
                    onDemandOnlyButton.interactable = areButtonsInteractable;
                    onDemandOnlyButton.colors = onDemandOnlyButtonColor;
                }

                needUpdateButtons = false;
            }

            if(needReinit)
            {
                needReinit = false;
                if (robotConfig.HasMobileBase() && robotStatus.IsMobilityOn())
                {
                    areButtonsInteractable = true;
                }
                else
                {
                    areButtonsInteractable = false;
                }
                if (!motionSicknessManager.IsTunnellingOn && !motionSicknessManager.IsReducedScreenOn)
                {
                    areButtonsInteractable = false;
                }

                if(tunnellingButton != null) tunnellingButton.colors = ColorsManager.colorsDeactivated;
                if(reducedScreenButton != null) reducedScreenButton.colors = ColorsManager.colorsDeactivated;
                if(noEffectButton != null) noEffectButton.colors = ColorsManager.colorsDeactivated;

                if(autoDisplayButton != null) 
                {
                    autoDisplayButton.interactable = areButtonsInteractable;
                    autoDisplayButton.colors = ColorsManager.colorsDeactivated;
                }
                
                if(onDemandOnlyButton != null) 
                {
                    onDemandOnlyButton.interactable = areButtonsInteractable;
                    onDemandOnlyButton.colors = ColorsManager.colorsDeactivated;
                }
            }
        }

        void CheckNavigationOptionsAvailability()
        {
            if (robotConfig.HasMobileBase() && robotStatus.IsMobilityOn())
            {
                areButtonsInteractable = true;
            }
            else
            {
                areButtonsInteractable = false;
            }
            if (!motionSicknessManager.IsTunnellingOn && !motionSicknessManager.IsReducedScreenOn)
            {
                areButtonsInteractable = false;
            }
            needUpdateButtons = true;
        }

        public void Reinit()
        {
            needReinit = true;
        }
    }
}