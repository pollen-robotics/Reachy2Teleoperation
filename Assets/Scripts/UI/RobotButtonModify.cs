using UnityEngine;
using UnityEngine.EventSystems;


namespace TeleopReachy
{
    public class RobotButtonModify : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            ActiveControllerManager.Instance.ControllersVibrations.OnUIEnterVibration();
        }
    }
}
