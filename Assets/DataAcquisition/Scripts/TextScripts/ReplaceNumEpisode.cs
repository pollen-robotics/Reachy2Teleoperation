using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DataAcquisition
{
    public class ReplaceNumEpisode : MonoBehaviour
    {
        TextMeshProUGUI textToChange;
        private RecordingSessionManager sessionManager;
        private string currentValue;

        private const string textPlaceholder = "<numEpisode>";

        void OnEnable()
        {
            sessionManager = DataAcquisitionManager.Instance.RecordingSessionManager;
            textToChange = GetComponent<TextMeshProUGUI>();
            textToChange.text = ChangeTextAccordingToValue(textToChange.text);
        }

        void OnDisable()
        {
            textToChange = GetComponent<TextMeshProUGUI>();
            textToChange.text = textToChange.text.Replace(currentValue, textPlaceholder);
        }

        public string ChangeTextAccordingToValue(string stringToChange)
        {
            currentValue = sessionManager.GetCurrentEpisode().ToString();
            stringToChange = stringToChange.Replace(textPlaceholder, currentValue);
            return stringToChange;
        }
    }
}