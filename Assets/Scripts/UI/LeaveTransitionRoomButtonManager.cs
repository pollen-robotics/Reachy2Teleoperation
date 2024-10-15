using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class LeaveTransitionRoomButtonManager : MonoBehaviour
    {
        private Button leaveRoomButton;
        private RobotStatus robotStatus;

        [SerializeField]
        private Transform canvaWarningLockPosition;

        void Awake()
        {
            robotStatus = RobotDataManager.Instance.RobotStatus;

            leaveRoomButton = transform.GetComponent<Button>();
            leaveRoomButton.onClick.AddListener(QuitTransitionRoom);
        }

        void QuitTransitionRoom()
        {
            if (!robotStatus.IsRobotPositionLocked)
                MirrorSceneManager.Instance.BackToConnectionScene();
            else
            {
                canvaWarningLockPosition.ActivateChildren(true);
            }
        }
    }
}