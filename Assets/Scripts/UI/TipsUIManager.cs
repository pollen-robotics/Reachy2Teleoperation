using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class TipsUIManager : MonoBehaviour
    {
        [SerializeField]
        private Text tipText;

        // Start is called before the first frame update
        void Start()
        {
            MirrorSceneManager.Instance.event_OnReadyForTeleop.AddListener(BlueRobotIndication);
        }

        // Update is called once per frame
        void BlueRobotIndication()
        {
            tipText.text = "Your shoulders should be aligned with Reachy's ones! \nIf not, align them or reset your position.";
        }
    }
}
