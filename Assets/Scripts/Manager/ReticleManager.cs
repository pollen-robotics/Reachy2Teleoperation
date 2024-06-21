using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class ReticleManager : MonoBehaviour
    {
        [SerializeField]
        public Button alwaysButton;

        [SerializeField]
        public Button neverButton;

        [SerializeField]
        public Button withExtraOnlyButton;

        private bool needUpdateButtons = false;
        private bool needReinit = false;
        private ColorBlock alwaysColor;
        private ColorBlock neverColor;
        private ColorBlock withExtraOnlyColor;

        private MotionSicknessManager motionSicknessManager;

        [SerializeField]
        private bool mustBeInitialized;

        void Start()
        {
            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnUpdateMotionSicknessPreferences.AddListener(ChooseButtonMode);
            if(mustBeInitialized) ChooseButtonMode();

            alwaysButton.onClick.AddListener(SwitchToAlwaysMode);
            neverButton.onClick.AddListener(SwitchToNeverMode);
            withExtraOnlyButton.onClick.AddListener(SwitchTowithExtraOnlyMode);
        }

        void ChooseButtonMode()
        {
            if (motionSicknessManager.IsReticleOn)
            {
                if(motionSicknessManager.IsReticleAlwaysShown)
                {
                    alwaysButton.colors = ColorsManager.colorsActivated;
                    neverButton.colors = ColorsManager.colorsDeactivated;
                    withExtraOnlyButton.colors = ColorsManager.colorsDeactivated;
                }
                else 
                {
                    alwaysButton.colors = ColorsManager.colorsDeactivated;
                    neverButton.colors = ColorsManager.colorsDeactivated;
                    withExtraOnlyButton.colors = ColorsManager.colorsActivated;
                }
            }
            else
            {
                alwaysButton.colors = ColorsManager.colorsDeactivated;
                neverButton.colors = ColorsManager.colorsActivated;
                withExtraOnlyButton.colors = ColorsManager.colorsDeactivated;
            }
        }

        void SwitchToAlwaysMode()
        {
            motionSicknessManager.IsReticleOn = true;
            motionSicknessManager.IsReticleAlwaysShown = true;
            
            needUpdateButtons = true;
        }

        void SwitchToNeverMode()
        {
            motionSicknessManager.IsReticleOn = false;
            motionSicknessManager.IsReticleAlwaysShown = false;

            needUpdateButtons = true;
        }

        void SwitchTowithExtraOnlyMode()
        {
            motionSicknessManager.IsReticleOn = true;
            motionSicknessManager.IsReticleAlwaysShown = false;

            needUpdateButtons = true;
        }

        public void Reinit()
        {
            needReinit = true;
        }

        void Update()
        {
            if(needUpdateButtons)
            {
                needUpdateButtons = false;
                ChooseButtonMode();
            }
            if(needReinit)
            {
                needReinit = false;
                alwaysButton.colors = ColorsManager.colorsDeactivated;
                neverButton.colors = ColorsManager.colorsDeactivated;
                withExtraOnlyButton.colors = ColorsManager.colorsDeactivated;
            }
        }
    }
}