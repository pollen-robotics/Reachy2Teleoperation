using UnityEngine;


namespace TeleopReachy
{
    public class UserTrackerManager : Singleton<UserTrackerManager>
    {
        public HandsTracker HandsTracker { get; private set; }
        public HeadTracker HeadTracker { get; private set; }

        [SerializeField]
        private Transform headset;

        protected override void Init()
        {
            HeadTracker = transform.GetChild(0).GetComponent<HeadTracker>();
            HandsTracker = transform.GetChild(1).GetComponent<HandsTracker>();
        }

        protected void Start()
        {
            EventManager.StartListening(EventNames.OnFixUserOrigin, FixUserOrigin);
        }

        protected void FixUserOrigin()
        {
            Quaternion rotation = headset.localRotation;
            Vector3 eulerAngles = rotation.eulerAngles;

            // Only the rotation around the y axis is kept, z and x axis are considered parallel to the floor
            Quaternion systemRotation = Quaternion.Euler(0, eulerAngles.y, 0);

            transform.rotation = systemRotation;
            // Origin of the coordinate system is placed 15cm under the headset y position
            Vector3 headPosition = headset.position - headset.forward * 0.1f;
            transform.position = new Vector3(headPosition.x, headPosition.y - UserSize.Instance.UserShoulderHeadDistance, headPosition.z);
        }
    }
}
