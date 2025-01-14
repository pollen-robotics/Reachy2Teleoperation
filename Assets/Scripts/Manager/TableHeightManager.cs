using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class TableHeightManager : MonoBehaviour
    {
        [SerializeField]
        private Slider tableHeightSlider;

        private void Start()
        {
            tableHeightSlider.value = PlayerPrefs.GetInt("tableHeight", 75);
        }

        public void UpdateTableHeight()
        {
            TableHeight.Instance.UpdateTableHeight(tableHeightSlider.value);
        }

        public void IncrementValue()
        {
            tableHeightSlider.value += 1.0f;
            if (tableHeightSlider.value > tableHeightSlider.maxValue)
                tableHeightSlider.value = tableHeightSlider.maxValue;
            TableHeight.Instance.UpdateTableHeight(tableHeightSlider.value);
        }

        public void DecrementValue()
        {
            tableHeightSlider.value -= 1.0f;
            if (tableHeightSlider.value < tableHeightSlider.minValue)
                tableHeightSlider.value = tableHeightSlider.minValue;
            TableHeight.Instance.UpdateTableHeight(tableHeightSlider.value);
        }
    }
}

