using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class HeadsetRemovedInMirrorManager : Singleton<HeadsetRemovedInMirrorManager>
    {
        [SerializeField]
        private Transform headsetRemovedMessage;
        private bool wasUserPresent;
        public const string mirrorLayer = "Mirror";
        public const string reachyLayer = "Reachy";

        public UnityEvent event_OnHeadsetReset;

        // Start is called before the first frame update
        void Start()
        {
            headsetRemovedMessage.gameObject.SetActive(false);
            EventManager.StartListening(EventNames.HeadsetReset, ShowResetPosition);
        }

        void ShowResetPosition()
        {
            headsetRemovedMessage.gameObject.SetActive(true);
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer(mirrorLayer));
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer(reachyLayer));
        }

        public void HideResetPosition()
        {
            headsetRemovedMessage.gameObject.SetActive(false);
            Camera.main.cullingMask |= 1 << LayerMask.NameToLayer(mirrorLayer);
            Camera.main.cullingMask |= 1 << LayerMask.NameToLayer(reachyLayer);
            event_OnHeadsetReset.Invoke();
        }
    }
}
