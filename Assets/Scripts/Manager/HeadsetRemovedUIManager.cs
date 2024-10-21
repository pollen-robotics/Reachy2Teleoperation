using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class HeadsetRemovedUIManager : Singleton<HeadsetRemovedUIManager>
    {
        [SerializeField]
        private Transform headsetRemovedMenu;

        [SerializeField]
        private Button goToTransitionRoomButton;

        public const string mirrorLayer = "Mirror";
        public const string reachyLayer = "Reachy";

        private MirrorSceneManager sceneManager;

        void Start()
        {
            sceneManager = MirrorSceneManager.Instance;

            headsetRemovedMenu.gameObject.SetActive(false);
            EventManager.StartListening(EventNames.HeadsetReset, ShowResetPosition);
            goToTransitionRoomButton.onClick.AddListener(BackToTransitionRoom);
        }

        void ShowResetPosition()
        {
            headsetRemovedMenu.gameObject.SetActive(true);
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer(mirrorLayer));
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer(reachyLayer));
        }

        void HideResetPosition()
        {
            headsetRemovedMenu.gameObject.SetActive(false);
            Camera.main.cullingMask |= 1 << LayerMask.NameToLayer(mirrorLayer);
            Camera.main.cullingMask |= 1 << LayerMask.NameToLayer(reachyLayer);
        }

        void BackToTransitionRoom()
        {
            HideResetPosition();
            sceneManager.ResetPosition();
        }
    }
}
