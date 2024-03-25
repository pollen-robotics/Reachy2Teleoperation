using UnityEngine;
using UnityEngine.EventSystems;

namespace TeleopReachy
{
    public class RobotButtonDelete : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            ActiveControllerManager.Instance.ControllersVibrations.OnUIEnterVibration();
        }
    }
}
