using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DataAcquisition
{
    public class ReplaceDatasetName : MonoBehaviour
    {
        TextMeshProUGUI textToChange;
        private string currentValue;

        private const string textPlaceholder = "<datasetName>";

        void OnEnable()
        {
            textToChange = GetComponent<TextMeshProUGUI>();
            textToChange.text = ChangeTextAccordingToValue(textToChange.text);
        }

        public string ChangeTextAccordingToValue(string stringToChange)
        {
            currentValue = RecordingSessionParameters.Instance.DatasetName;
            stringToChange = stringToChange.Replace(textPlaceholder, currentValue);
            return stringToChange;
        }
    }
}