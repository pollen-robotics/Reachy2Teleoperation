using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DataAcquisition
{
    public class ReplaceNbEpisodes : MonoBehaviour
    {
        TextMeshProUGUI textToChange;

        private const string textPlaceholder = "<nbEpisodes>";

        void OnEnable()
        {
            textToChange = GetComponent<TextMeshProUGUI>();
            textToChange.text = ChangeTextAccordingToValue(textToChange.text);
        }

        public string ChangeTextAccordingToValue(string stringToChange)
        {
            stringToChange = stringToChange.Replace(textPlaceholder, RecordingSessionParameters.Instance.NbEpisodesGoal.ToString());
            return stringToChange;
        }
    }
}