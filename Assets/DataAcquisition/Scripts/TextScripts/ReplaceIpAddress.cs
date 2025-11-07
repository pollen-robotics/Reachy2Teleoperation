using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DataAcquisition
{
    public class ReplaceIpAddress : MonoBehaviour
    {
        TextMeshProUGUI textToChange;
        private string currentIpValue;
        private string currentPortValue;

        private const string ipPlaceholder = "<ip_address>";
        private const string portPlaceholder = "<port>";

        void OnEnable()
        {
            textToChange = GetComponent<TextMeshProUGUI>();
            textToChange.text = ChangeTextAccordingToValue(textToChange.text);
        }

        void OnDisable()
        {
            textToChange = GetComponent<TextMeshProUGUI>();
            textToChange.text = textToChange.text.Replace(currentIpValue, ipPlaceholder);
            textToChange.text = textToChange.text.Replace(currentPortValue, portPlaceholder);
        }

        public string ChangeTextAccordingToValue(string stringToChange)
        {
            currentIpValue = PlayerPrefs.GetString("server_ip");
            currentPortValue = PlayerPrefs.GetString("server_data_port");
            stringToChange = stringToChange.Replace(ipPlaceholder, currentIpValue);
            stringToChange = stringToChange.Replace(portPlaceholder, currentPortValue);
            return stringToChange;
        }
    }
}