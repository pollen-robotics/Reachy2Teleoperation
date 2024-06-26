using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class NavigationEffectsManager : MonoBehaviour
    {
        [SerializeField]
        public Button noEffectAutoButton;

        [SerializeField]
        public Button tunnellingAutoButton;

        [SerializeField]
        public Button reducedScreenAutoButton;

        [SerializeField]
        public Button noEffectOnClickButton;

        [SerializeField]
        public Button tunnellingOnClickButton;

        [SerializeField]
        public Button reducedScreenOnClickButton;

        private bool needUpdateButtons;

        private ColorBlock noEffectAutoButtonColor;
        private ColorBlock tunnellingAutoButtonColor;
        private ColorBlock reducedScreenAutoButtonColor;

        private ColorBlock noEffectOnClickButtonColor;
        private ColorBlock tunnellingOnClickButtonColor;
        private ColorBlock reducedScreenOnClickButtonColor;

        // private ColorBlock autoDisplayButtonColor;
        // private ColorBlock onDemandOnlyButtonColor;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            noEffectAutoButton?.onClick.AddListener(SwitchToNoEffectAutoMode);
            tunnellingAutoButton?.onClick.AddListener(SwitchToTunnellingAutoMode);
            reducedScreenAutoButton?.onClick.AddListener(SwitchToReducedScreenAutoMode);

            noEffectOnClickButton?.onClick.AddListener(SwitchToNoEffectOnClickMode);
            tunnellingOnClickButton?.onClick.AddListener(SwitchToTunnellingOnClickMode);
            reducedScreenOnClickButton?.onClick.AddListener(SwitchToReducedScreenOnClickMode);

            tunnellingAutoButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenAutoButtonColor = ColorsManager.colorsDeactivated;
            noEffectAutoButtonColor = ColorsManager.colorsDeactivated;

            tunnellingOnClickButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenOnClickButtonColor = ColorsManager.colorsDeactivated;
            noEffectOnClickButtonColor = ColorsManager.colorsDeactivated;

            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnUpdateMotionSicknessPreferences.AddListener(ChooseButtonsMode);
            ChooseButtonsMode();
        }

        void ChooseButtonsMode()
        {
            if (motionSicknessManager.IsTunnellingAutoOn) SwitchToTunnellingAutoMode();
            else if (motionSicknessManager.IsReducedScreenAutoOn) SwitchToReducedScreenAutoMode();
            else SwitchToNoEffectAutoMode();

            if (motionSicknessManager.IsTunnellingOnClickOn) SwitchToTunnellingOnClickMode();
            else if (motionSicknessManager.IsReducedScreenOnClickOn) SwitchToReducedScreenOnClickMode();
            else SwitchToNoEffectOnClickMode();
        }

        void SwitchToNoEffectAutoMode()
        {
            motionSicknessManager.IsTunnellingAutoOn = false;
            motionSicknessManager.IsReducedScreenAutoOn = false;

            noEffectAutoButtonColor = ColorsManager.colorsActivated;
            tunnellingAutoButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenAutoButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToTunnellingAutoMode()
        {
            motionSicknessManager.IsTunnellingAutoOn = true;
            motionSicknessManager.IsReducedScreenAutoOn = false;

            noEffectAutoButtonColor = ColorsManager.colorsDeactivated;
            tunnellingAutoButtonColor = ColorsManager.colorsActivated;
            reducedScreenAutoButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToReducedScreenAutoMode()
        {
            motionSicknessManager.IsTunnellingAutoOn = false;
            motionSicknessManager.IsReducedScreenAutoOn = true;

            noEffectAutoButtonColor = ColorsManager.colorsDeactivated;
            tunnellingAutoButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenAutoButtonColor = ColorsManager.colorsActivated;

            needUpdateButtons = true;
        }

        void SwitchToNoEffectOnClickMode()
        {
            motionSicknessManager.IsTunnellingOnClickOn = false;
            motionSicknessManager.IsReducedScreenOnClickOn = false;

            noEffectOnClickButtonColor = ColorsManager.colorsActivated;
            tunnellingOnClickButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenOnClickButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToTunnellingOnClickMode()
        {
            motionSicknessManager.IsTunnellingOnClickOn = true;
            motionSicknessManager.IsReducedScreenOnClickOn = false;

            noEffectOnClickButtonColor = ColorsManager.colorsDeactivated;
            tunnellingOnClickButtonColor = ColorsManager.colorsActivated;
            reducedScreenOnClickButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToReducedScreenOnClickMode()
        {
            motionSicknessManager.IsTunnellingOnClickOn = false;
            motionSicknessManager.IsReducedScreenOnClickOn = true;

            noEffectOnClickButtonColor = ColorsManager.colorsDeactivated;
            tunnellingOnClickButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenOnClickButtonColor = ColorsManager.colorsActivated;

            needUpdateButtons = true;
        }

        void Update()
        {
            if (needUpdateButtons)
            {
                if(tunnellingAutoButton != null) tunnellingAutoButton.colors = tunnellingAutoButtonColor;
                if(reducedScreenAutoButton != null) reducedScreenAutoButton.colors = reducedScreenAutoButtonColor;
                if(noEffectAutoButton != null) noEffectAutoButton.colors = noEffectAutoButtonColor;

                if(tunnellingOnClickButton != null) tunnellingOnClickButton.colors = tunnellingOnClickButtonColor;
                if(reducedScreenOnClickButton != null) reducedScreenOnClickButton.colors = reducedScreenOnClickButtonColor;
                if(noEffectOnClickButton != null) noEffectOnClickButton.colors = noEffectOnClickButtonColor;

                needUpdateButtons = false;
            }
        }
    }
}