using UnityEngine;
using UnityEngine.UI;
using TMPro; // Only if you use TextMeshPro

namespace DataAcquisition
{
    public class CustomButtonSelector : MonoBehaviour
    {
        [Header("Selection shape")]
        public Transform shape;

        [Header("Buttons")]
        public Transform successButton;
        public Transform failButton;
        
        [Header("Duration")]
        public float translationDuration = 0.5f;

        public bool isSuccess { get; protected set; }

        private float timer;
        private bool isTranslating = false;
        private Vector3 targetLocalPosition;
        private Vector3 originalLocalPosition;

        void OnEnable()
        {
            isSuccess = true;
            if (successButton != null) shape.localPosition = successButton.localPosition;
        }

        void Start()
        {
            isSuccess = true;
            if (successButton == null || failButton == null || shape == null)
            {
                Debug.LogError("CustomButtonSelector not set up properly!");
                return;
            }
            shape.localPosition = successButton.localPosition;
        }

        public void SelectFailButton()
        {
            isSuccess = false;
            timer = translationDuration;
            isTranslating = true;
            originalLocalPosition = shape.localPosition;
            targetLocalPosition = failButton.localPosition;
        }

        public void SelectSuccessButton()
        {
            isSuccess = true;
            timer = translationDuration;
            isTranslating = true;
            originalLocalPosition = shape.localPosition;
            targetLocalPosition = successButton.localPosition;
        }

        void Update()
        {
            if (!isTranslating)
                return;

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                timer = translationDuration;
                isTranslating = false;
            }

            if (successButton == null || failButton == null || shape == null) TranslateShape();
        }
        

        void TranslateShape()
        {
            float elapsedSinceLastTick = translationDuration - timer;

            if (elapsedSinceLastTick > 0)
            {
                float normalizedTime = elapsedSinceLastTick / translationDuration;
                shape.localPosition = Vector3.Lerp(originalLocalPosition, targetLocalPosition, normalizedTime);
            }
            else
            {
                shape.localPosition = targetLocalPosition;
            }
        }
    }
}