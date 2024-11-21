using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class GpuManager : MonoBehaviour
    {
        public Text versionText;
        public string introText;

        void Start()
        {
            versionText.text = SystemInfo.graphicsDeviceName;
        }
    }
}