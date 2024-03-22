using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class RobotConfigurationUIManager : MonoBehaviour
    {
        private RobotConfig robotConfig;
        private ConnectionStatus connectionStatus;

        private Color32 rightArmConfigColor;
        private Color32 leftArmConfigColor;
        private Color32 headConfigColor;
        private Color32 mobilePlaformConfigColor;

        private string rightArmConfigText;
        private string leftArmConfigText;
        private string headConfigText;
        private string mobilePlaformConfigText;

        private bool needUpdateConfig;


        void Awake()
        {
            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(ConfigChanged);

            connectionStatus = WebRTCManager.Instance.ConnectionStatus;

            CheckConfig();
        }

        private void Update()
        {
            if (needUpdateConfig)
            {
                transform.GetChild(0).GetComponent<Text>().color = rightArmConfigColor;
                transform.GetChild(0).GetComponent<Text>().text = rightArmConfigText;
                transform.GetChild(0).GetChild(0).GetComponent<Image>().color = rightArmConfigColor;
                transform.GetChild(1).GetComponent<Text>().color = leftArmConfigColor;
                transform.GetChild(1).GetComponent<Text>().text = leftArmConfigText;
                transform.GetChild(1).GetChild(0).GetComponent<Image>().color = leftArmConfigColor;
                transform.GetChild(2).GetComponent<Text>().color = headConfigColor;
                transform.GetChild(2).GetComponent<Text>().text = headConfigText;
                transform.GetChild(2).GetChild(0).GetComponent<Image>().color = headConfigColor;
                transform.GetChild(3).GetComponent<Text>().color = mobilePlaformConfigColor;
                transform.GetChild(3).GetComponent<Text>().text = mobilePlaformConfigText;
                transform.GetChild(3).GetChild(0).GetComponent<Image>().color = mobilePlaformConfigColor;

                needUpdateConfig = false;
            }
        }

        private void CheckConfig()
        {
            if (robotConfig.GotReachyConfig())
            {
                if (robotConfig.HasHead())
                {
                    headConfigText = "Head detected";
                    headConfigColor = ColorsManager.green;
                }
                else
                {
                    headConfigText = "No head";
                    headConfigColor = ColorsManager.white;
                }
                if (robotConfig.HasRightArm())
                {
                    rightArmConfigText = "Right arm detected";
                    rightArmConfigColor = ColorsManager.green;
                }
                else
                {
                    rightArmConfigText = "No right arm";
                    rightArmConfigColor = ColorsManager.white;
                }

                if (robotConfig.HasLeftArm())
                {
                    leftArmConfigText = "Left arm detected";
                    leftArmConfigColor = ColorsManager.green;
                }
                else
                {
                    leftArmConfigText = "No left arm";
                    leftArmConfigColor = ColorsManager.white;
                }

                if (robotConfig.HasMobileBase())
                {
                    mobilePlaformConfigText = "Mobile base detected";
                    mobilePlaformConfigColor = ColorsManager.green;
                }
                else
                {
                    mobilePlaformConfigText = "No mobile base";
                    mobilePlaformConfigColor = ColorsManager.white;
                }
            }
            else
            {
                headConfigText = "No head status";
                headConfigColor = ColorsManager.red;
                rightArmConfigText = "No right arm status";
                rightArmConfigColor = ColorsManager.red;
                leftArmConfigText = "No left arm status";
                leftArmConfigColor = ColorsManager.red;
                mobilePlaformConfigText = "No mobile base status";
                mobilePlaformConfigColor = ColorsManager.red;
            }

            needUpdateConfig = true;
        }

        private void ConfigChanged()
        {
            if (robotConfig.GotReachyConfig())
            {
                CheckConfig();
            }
            else
            {
                if (connectionStatus.AreRobotServicesRestarting())
                {
                    headConfigText = "Getting head status...";
                    headConfigColor = ColorsManager.blue;
                    rightArmConfigText = "Getting right arm status...";
                    rightArmConfigColor = ColorsManager.blue;
                    leftArmConfigText = "Getting left arm status...";
                    leftArmConfigColor = ColorsManager.blue;
                    mobilePlaformConfigText = "Getting mobile base status...";
                    mobilePlaformConfigColor = ColorsManager.blue;
                }
                else
                {
                    headConfigText = "No head status";
                    headConfigColor = ColorsManager.red;
                    rightArmConfigText = "No right arm status";
                    rightArmConfigColor = ColorsManager.red;
                    leftArmConfigText = "No left arm status";
                    leftArmConfigColor = ColorsManager.red;
                    mobilePlaformConfigText = "No mobile platform status";
                    mobilePlaformConfigColor = ColorsManager.red;
                }
            }
            needUpdateConfig = true;
        }
    }
}