using UnityEngine;

namespace TeleopReachy
{
    public class ReachyViewDisplayManager : MonoBehaviour
    {
        private Renderer screen;

        private RobotVideoStream robotVideoStream;

        void Start()
        {
            screen = this.GetComponent<Renderer>();
            robotVideoStream = RobotDataManager.Instance.RobotVideoStream;

            screen.material.SetTexture("_LeftTex", robotVideoStream.GetLeftTexture());
            screen.material.SetTexture("_RightTex", robotVideoStream.GetRightTexture());
        }
    }
}