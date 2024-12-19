using UnityEngine;

namespace TeleopReachy
{
    public class SetRobotAtUserTrackerPosition : MonoBehaviour
    {
        // Update is called once per frame
        void LateUpdate()
        {
            Vector3 userTrackerPosition = UserTrackerManager.Instance.transform.position; // - transform.forward * 0.1f;
            Quaternion userTrackerRotation = UserTrackerManager.Instance.transform.localRotation;
            Vector3 userTrackerEulerAngles = userTrackerRotation.eulerAngles;

            transform.rotation = Quaternion.Euler(0, userTrackerEulerAngles.y, 0);
            transform.position = new Vector3(userTrackerPosition.x, userTrackerPosition.y - 1, userTrackerPosition.z);
        }
    }
}
