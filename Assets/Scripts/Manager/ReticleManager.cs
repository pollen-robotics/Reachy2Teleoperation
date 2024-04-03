using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class ReticleManager : MonoBehaviour
    {
        [SerializeField]
        public Button reticleButton;

        [SerializeField]
        public Toggle reticleToggle;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        //private bool needUpdateButton = false;
        private bool needUpdateToggle = false;
        //private bool isButtonInteractable = false;
        private bool isToggleInteractable = false;
        //private ColorBlock buttonColor;
        //private string buttonText;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            motionSicknessManager = MotionSicknessManager.Instance;
            ChooseButtonMode();

            reticleButton.onClick.AddListener(SwitchButtonMode);
            reticleToggle.onValueChanged.AddListener(SwitchToggleMode);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotStatus = RobotDataManager.Instance.RobotStatus;

            robotConfig.event_OnConfigChanged.AddListener(CheckToggleInteractibility);

            CheckToggleInteractibility();
        }

        void ChooseButtonMode()
        {
            if (motionSicknessManager.IsReticleOn)
            {
                reticleButton.colors = ColorsManager.colorsActivated;
                reticleButton.transform.GetChild(0).GetComponent<Text>().text = "Reticle ON";
            }
            else
            {
                reticleButton.colors = ColorsManager.colorsDeactivated;
                reticleButton.transform.GetChild(0).GetComponent<Text>().text = "Reticle OFF";
            }
        }

        void SwitchToggleMode(bool value)
        {
            motionSicknessManager.IsReticleAlwaysShown = value;
        }

        void SwitchButtonMode()
        {
            motionSicknessManager.IsReticleOn = !motionSicknessManager.IsReticleOn;
            ChooseButtonMode();
            CheckToggleInteractibility();
        }

        void Update()
        {
            if (needUpdateToggle)
            {
                reticleToggle.interactable = isToggleInteractable;
                if (!isToggleInteractable) reticleToggle.transform.GetChild(1).GetComponent<Text>().color = ColorsManager.grey;
                else reticleToggle.transform.GetChild(1).GetComponent<Text>().color = ColorsManager.white;
                needUpdateToggle = false;
            }
        }

        void CheckToggleInteractibility()
        {
            if (motionSicknessManager.IsReticleOn)
            {
                if (robotConfig.HasMobileBase() && robotStatus.IsMobilityOn())
                {
                    reticleToggle.isOn = motionSicknessManager.IsReticleAlwaysShown;
                    isToggleInteractable = true;
                }
                else
                {
                    reticleToggle.isOn = true;
                    isToggleInteractable = false;
                }
            }
            else
            {
                reticleToggle.isOn = false;
                isToggleInteractable = false;
            }
            needUpdateToggle = true;
        }
    }
}