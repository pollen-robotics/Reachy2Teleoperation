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

        private OptionsManager optionsManager;

        void Start()
        {
            optionsManager = OptionsManager.Instance;

            ChooseButtonMode();

            alwaysButton.onClick.AddListener(SwitchToAlwaysMode);
            neverButton.onClick.AddListener(SwitchToNeverMode);
        }

        void ChooseButtonMode()
        {
            if (optionsManager.isReticleOn)
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
            optionsManager.SetReticleOn(true);

            needUpdateButtons = true;
        }

        void SwitchToNeverMode()
        {
            optionsManager.SetReticleOn(false);

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