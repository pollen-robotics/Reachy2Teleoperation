using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;


namespace DataAcquisition
{
    public class LoadingButton : MonoBehaviour
    {
        private bool isLoading = false;

        public void SetLoading()
        {
            transform.GetComponent<Button>().interactable = false;
            transform.GetChild(0).localPosition = new Vector3(12.1f, 0, 0);
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Starting session...";
            transform.GetChild(1).gameObject.SetActive(true);
            isLoading = true;
        }

        void Update()
        {
            if (isLoading) transform.GetChild(1).Rotate(0, 0, -2.0f);
        }

        void OnDisable()
        {
            isLoading = false;
            transform.GetComponent<Button>().interactable = true;
            transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Access session recording";
            transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
