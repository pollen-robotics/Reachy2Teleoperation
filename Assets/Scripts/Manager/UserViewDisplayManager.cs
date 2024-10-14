using UnityEngine;

namespace TeleopReachy
{
    public class UserViewDisplayManager : MonoBehaviour
    {
        [SerializeField]
        private Transform reachyEyeView;
        public Renderer screen;

        //private EyeScript eyeScript;
        private RobotStatus robotStatus;
        private RobotVideoStream robotVideoStream;
        // private gRPCVideoController videoController;

        void Start()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotVideoStream = RobotDataManager.Instance.RobotVideoStream;
            robotStatus.event_OnStartTeleoperation.AddListener(ShowReachyView);
            robotStatus.event_OnStopTeleoperation.AddListener(HideReachyView);

            screen.material.SetTexture("_LeftTex", robotVideoStream.GetLeftTexture());
            screen.material.SetTexture("_RightTex", robotVideoStream.GetRightTexture());

            // videoController = gRPCManager.Instance.gRPCVideoController;
            // videoController.event_OnVideoRoomStatusHasChanged.AddListener(ModifyTextureTransparency);

            //eyeScript = reachyEyeView.GetComponent<EyeScript>();

            reachyEyeView.gameObject.SetActive(false);
        }

        void ShowReachyView()
        {
            reachyEyeView.gameObject.SetActive(true);
        }

        void HideReachyView()
        {
            Camera.main.stereoTargetEye = StereoTargetEyeMask.Both;
            reachyEyeView.gameObject.SetActive(false);
        }

        // void ModifyTextureTransparency(bool isRobotInVideoRoom)
        // {
        //     if(isRobotInVideoRoom)
        //     {
        //         eyeScript.SetImageOpaque();
        //     }
        //     else
        //     {
        //         if(robotStatus.IsRobotTeleoperationActive())
        //         {
        //             eyeScript.SetImageTransparent();
        //         }
        //     }
        // }
    }
}