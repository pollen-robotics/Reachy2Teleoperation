using UnityEngine;
using GstreamerWebRTC;

namespace TeleopReachy
{
    public class MiniVideoStreamDisplay : MonoBehaviour
    {
        private RobotVideoStream videoController;
        private Renderer screen;

        // Used to update display when going back in mirror scene
        void Start()
        {
            screen = GetComponent<Renderer>();
            videoController = RobotDataManager.Instance.RobotVideoStream;
            screen.material.SetTexture("_LeftTex", videoController.GetLeftTexture());
            screen.material.SetTexture("_RightTex", videoController.GetLeftTexture());
        }

    }
}
