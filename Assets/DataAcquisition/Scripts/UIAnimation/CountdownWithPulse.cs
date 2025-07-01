using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DataAcquisition
{
    public class CountdownWithPulse : MonoBehaviour
    {
        [Header("References")]
        public TextMeshProUGUI countdownText;
        public RectTransform pulseImage; // The image that will scale (heartbeat)

        [Header("Countdown Settings")]
        public SessionPanel panel;
        public int startingCount = 10;
        
        [Header("Pulse Settings")]
        public float pulseDuration = 0.5f;
        public float pulseOffset = 0.1f; // Time (in seconds) before next tick to end the pulse
        public Vector3 pulseScale = new Vector3(1.2f, 1.2f, 1f); // How big it grows during heartbeat

        private int currentCount;
        private Vector3 originalScale;
        private float timer;
        private bool isCounting = false;
        private bool firstTimeEnabled = true;

        public enum SessionPanel {
            StartDelay, BreakTime, RecordingTimer, EpisodeSaving, Other
        }
        
        void OnEnable()
        {
            if (panel == SessionPanel.StartDelay)
            {
                startingCount = RecordingSessionParameters.Instance.StartDelay;
                if (DataAcquisitionManager.Instance.RecordingSessionManager.FirstCycle) startingCount += 3;
            }
            else if (panel == SessionPanel.BreakTime) startingCount = RecordingSessionParameters.Instance.BreakTimeDuration;
            else if (panel == SessionPanel.RecordingTimer) startingCount = RecordingSessionParameters.Instance.EpisodeDuration;
            else if (panel == SessionPanel.EpisodeSaving) startingCount = 5;
            if (!firstTimeEnabled) pulseImage.localScale = originalScale;
            StartCountdown();
        }

        void Start()
        {
            if (pulseImage == null)
            {
                Debug.LogError("CountdownWithPulse not set up properly!");
                enabled = false;
                return;
            }
            originalScale = pulseImage.localScale;
            firstTimeEnabled = false;
        }

        void Update()
        {
            if (!isCounting)
                return;

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                timer = pulseDuration; // Reset timer **first**

                currentCount--;

                if (currentCount > 0)
                {
                    UpdateCountdownText();
                }
                else
                {
                    isCounting = false;
                    if (countdownText != null)  countdownText.text = "Go";
                }
            }

            PulseImage();
        }

        void StartCountdown()
        {
            currentCount = startingCount;
            timer = pulseDuration;
            isCounting = true;
            UpdateCountdownText();
        }

        void UpdateCountdownText()
        {
            if (countdownText != null) countdownText.text = currentCount.ToString();
        }

        void PulseImage()
        {
            if (panel != SessionPanel.RecordingTimer || currentCount <= 5)
            {
                float pulseAvailableTime = pulseDuration - pulseOffset; // Time available for pulse animation
                float elapsedSinceLastTick = pulseDuration - timer;

                if (elapsedSinceLastTick < pulseAvailableTime)
                {
                    float normalizedTime = elapsedSinceLastTick / pulseAvailableTime; // 0 → 1 smoothly
                    float pulseCurve = Mathf.PingPong(normalizedTime * pulseDuration * 2f, pulseDuration); // up and down within available time
                    pulseImage.localScale = Vector3.Lerp(originalScale, pulseScale, pulseCurve);
                }
                else
                {
                    // After pulse finishes, hold the original scale
                    pulseImage.localScale = originalScale;
                }
            }
        }

        public void SuspendCountdown()
        {
            isCounting = false;
        }

        public void ResumeCountdown()
        {
            isCounting = true;
        }
    }
}