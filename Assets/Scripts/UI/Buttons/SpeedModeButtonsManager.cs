using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class SpeedModeButtonsManager : MonoBehaviour
    {
        public Button speed37Button;
        public Button speed50Button;
        public Button speed100Button;

        private bool needUpdateButton;

        void Awake()
        {
            speed37Button.onClick.AddListener(delegate { ChangeMaxSpeed(37); });
            speed50Button.onClick.AddListener(delegate { ChangeMaxSpeed(50); });
            speed100Button.onClick.AddListener(delegate { ChangeMaxSpeed(100); });

            if (!PlayerPrefs.HasKey("SpeedLimit")) PlayerPrefs.SetInt("SpeedLimit", 100);

            needUpdateButton = true;
        }

        void ChangeMaxSpeed(int speedLimit)
        {
            PlayerPrefs.SetInt("SpeedLimit", speedLimit);
            needUpdateButton = true;
        }

        void Update()
        {
            if (needUpdateButton)
            {
                if (PlayerPrefs.GetInt("SpeedLimit") == 37)
                {
                    speed37Button.colors = ColorsManager.colorsActivated;
                    speed50Button.colors = ColorsManager.colorsDeactivated;
                    speed100Button.colors = ColorsManager.colorsDeactivated;
                }
                if (PlayerPrefs.GetInt("SpeedLimit") == 50)
                {
                    speed37Button.colors = ColorsManager.colorsDeactivated;
                    speed50Button.colors = ColorsManager.colorsActivated;
                    speed100Button.colors = ColorsManager.colorsDeactivated;
                }
                if (PlayerPrefs.GetInt("SpeedLimit") == 100)
                {
                    speed37Button.colors = ColorsManager.colorsDeactivated;
                    speed50Button.colors = ColorsManager.colorsDeactivated;
                    speed100Button.colors = ColorsManager.colorsActivated;
                }
                needUpdateButton = false;
            }
        }
    }
}