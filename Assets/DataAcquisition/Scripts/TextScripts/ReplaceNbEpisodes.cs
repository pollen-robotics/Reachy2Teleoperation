using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DataAcquisition
{
    public class ReplaceNbEpisodes : MonoBehaviour
    {
        TextMeshProUGUI textToChange;
        private string currentValue;

        private const string textPlaceholder = "<nbEpisodes>";

        void OnEnable()
        {
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
            currentValue = RecordingSessionParameters.Instance.NbEpisodesGoal.ToString();
            stringToChange = stringToChange.Replace(textPlaceholder, currentValue);
            return stringToChange;
        }
    }
}