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

        private bool needUpdateButtons = false;
        private ColorBlock alwaysColor;
        private ColorBlock neverColor;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnUpdateMotionSicknessPreferences.AddListener(ChooseButtonMode);
            ChooseButtonMode();

            alwaysButton.onClick.AddListener(SwitchToAlwaysMode);
            neverButton.onClick.AddListener(SwitchToNeverMode);
        }

        void ChooseButtonMode()
        {
            if (motionSicknessManager.IsReticleOn)
            {
                alwaysButton.colors = ColorsManager.colorsActivated;
                neverButton.colors = ColorsManager.colorsDeactivated;
            }
            else
            {
                alwaysButton.colors = ColorsManager.colorsDeactivated;
                neverButton.colors = ColorsManager.colorsActivated;
            }
        }

        void SwitchToAlwaysMode()
        {
            motionSicknessManager.IsReticleOn = true;

            needUpdateButtons = true;
        }

        void SwitchToNeverMode()
        {
            motionSicknessManager.IsReticleOn = false;

            needUpdateButtons = true;
        }

        void Update()
        {
            if(needUpdateButtons)
            {
                needUpdateButtons = false;
                ChooseButtonMode();
            }
        }
    }
}