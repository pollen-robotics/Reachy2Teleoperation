using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class TextAPIModifier : MonoBehaviour
    {
        private Text textToChange;
        private string newText;

        private const string appVersion = "<app_version>";
        private const string robotVersion = "<robot_api>";

        private bool needUpdate;

        private RobotConfig robotConfig;

        void OnEnable()
        {
            needUpdate = false;
            robotConfig = RobotDataManager.Instance.RobotConfig;
            textToChange = GetComponent<Text>();
            ChangeText(textToChange.text);
        }

        void Update()
        {
            if (needUpdate)
            {
                textToChange.text = newText;
                needUpdate = false;
            }
        }

        public void ChangeText(string stringToChange)
        {
            stringToChange = stringToChange.Replace(appVersion, ApplicationAPIVersion.Instance.GetApplicationAPIVersion());
            newText = stringToChange.Replace(robotVersion, robotConfig.GetRobotAPIVersion());
            needUpdate = true;
        }
    }
}