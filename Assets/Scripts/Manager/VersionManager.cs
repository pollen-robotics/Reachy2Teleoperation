using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class VersionManager : MonoBehaviour
    {
        public Text versionText;
        public string introText;

        void Start()
        {
            versionText.text = introText + Application.version;
        }
    }
}