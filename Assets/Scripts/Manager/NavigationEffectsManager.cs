using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class NavigationEffectsManager : MonoBehaviour
    {
        [SerializeField]
        private Button noEffectAutoButton;

        [SerializeField]
        private Button tunnellingAutoButton;

        [SerializeField]
        private Button reducedScreenAutoButton;

        [SerializeField]
        private Button noEffectOnClickButton;

        [SerializeField]
        private Button tunnellingOnClickButton;

        [SerializeField]
        private Button reducedScreenOnClickButton;

        private bool needUpdateButtons;

        private ColorBlock noEffectAutoButtonColor;
        private ColorBlock tunnellingAutoButtonColor;
        private ColorBlock reducedScreenAutoButtonColor;

        private ColorBlock noEffectOnClickButtonColor;
        private ColorBlock tunnellingOnClickButtonColor;
        private ColorBlock reducedScreenOnClickButtonColor;

        private OptionsManager optionsManager;

        void Start()
        {
            optionsManager = OptionsManager.Instance;

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

            ChooseButtonsMode();
        }

        void ChooseButtonsMode()
        {
            if (optionsManager.motionSicknessEffectAuto == OptionsManager.MotionSicknessEffect.Tunnelling) SwitchToTunnellingAutoMode();
            else if (optionsManager.motionSicknessEffectAuto == OptionsManager.MotionSicknessEffect.ReducedScreen) SwitchToReducedScreenAutoMode();
            else SwitchToNoEffectAutoMode();

            if (optionsManager.motionSicknessEffectOnClick == OptionsManager.MotionSicknessEffect.Tunnelling) SwitchToTunnellingOnClickMode();
            else if (optionsManager.motionSicknessEffectOnClick == OptionsManager.MotionSicknessEffect.ReducedScreen) SwitchToReducedScreenOnClickMode();
            else SwitchToNoEffectOnClickMode();
        }

        void SwitchToNoEffectAutoMode()
        {
            optionsManager.SetMotionSicknessEffectAuto(OptionsManager.MotionSicknessEffect.None);

            noEffectAutoButtonColor = ColorsManager.colorsActivated;
            tunnellingAutoButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenAutoButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToTunnellingAutoMode()
        {
            optionsManager.SetMotionSicknessEffectAuto(OptionsManager.MotionSicknessEffect.Tunnelling);

            noEffectAutoButtonColor = ColorsManager.colorsDeactivated;
            tunnellingAutoButtonColor = ColorsManager.colorsActivated;
            reducedScreenAutoButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToReducedScreenAutoMode()
        {
            optionsManager.SetMotionSicknessEffectAuto(OptionsManager.MotionSicknessEffect.ReducedScreen);

            noEffectAutoButtonColor = ColorsManager.colorsDeactivated;
            tunnellingAutoButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenAutoButtonColor = ColorsManager.colorsActivated;

            needUpdateButtons = true;
        }

        void SwitchToNoEffectOnClickMode()
        {
            optionsManager.SetMotionSicknessEffectOnClick(OptionsManager.MotionSicknessEffect.None);

            noEffectOnClickButtonColor = ColorsManager.colorsActivated;
            tunnellingOnClickButtonColor = ColorsManager.colorsDeactivated;
            reducedScreenOnClickButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToTunnellingOnClickMode()
        {
            optionsManager.SetMotionSicknessEffectOnClick(OptionsManager.MotionSicknessEffect.Tunnelling);

            noEffectOnClickButtonColor = ColorsManager.colorsDeactivated;
            tunnellingOnClickButtonColor = ColorsManager.colorsActivated;
            reducedScreenOnClickButtonColor = ColorsManager.colorsDeactivated;

            needUpdateButtons = true;
        }

        void SwitchToReducedScreenOnClickMode()
        {
            optionsManager.SetMotionSicknessEffectOnClick(OptionsManager.MotionSicknessEffect.ReducedScreen);

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