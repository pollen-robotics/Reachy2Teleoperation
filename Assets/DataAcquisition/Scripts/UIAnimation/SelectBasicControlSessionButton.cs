using UnityEngine;
using UnityEngine.UI;

namespace DataAcquisition
{
    public class SelectBasicControlSessionButton : MonoBehaviour
    {
        private CustomButtonSelector buttonSelector;

        void Start()
        {
            buttonSelector = GetComponent<CustomButtonSelector>();
            SessionType.Instance.event_onBasicControlSessionSelected.AddListener(SelectBasicControlButton);
        }

        void SelectBasicControlButton()
        {
            buttonSelector.SelectSuccessButton();
        }
    }
}