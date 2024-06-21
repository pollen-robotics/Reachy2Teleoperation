using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace TeleopReachy
{
    public class CanvaMotionSicknessUIManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject renderingCanva;

        [SerializeField]
        private GameObject navigationCanva;

        [SerializeField]
        private GameObject reticleCanva;

        [SerializeField]
        private Button nextRenderingCanva;

        [SerializeField]
        private Button nextNavigationCanva;

        [SerializeField]
        private Button nextReticleCanva;

        [SerializeField]
        private Toggle keepOptions;

        private MotionSicknessManager motionSicknessManager;

        private bool keepOptionsForAllSession;

        void Awake()
        {
            DisplayNoCanva();
        }

        void Start()
        {
            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnNewTeleopSession.AddListener(DisplayMotionSicknessCanva);

            keepOptions.onValueChanged.AddListener(delegate { ToggleValueChanged(keepOptions); });
        }

        public void OnDemandOnly()
        {
            motionSicknessManager.IsNavigationEffectOnDemand = true;
        }

        public void AutoOnNavigation()
        {
            motionSicknessManager.IsNavigationEffectOnDemand = false;
        }

        public void NoNavigationEffect()
        {
            motionSicknessManager.IsReducedScreenOn = false;
            motionSicknessManager.IsTunnellingOn = false;
        }

        public void TunnellingNavigationEffect()
        {
            motionSicknessManager.IsReducedScreenOn = false;
            motionSicknessManager.IsTunnellingOn = true;
        }

        public void ReducedScreenNavigationEffect()
        {
            motionSicknessManager.IsReducedScreenOn = true;
            motionSicknessManager.IsTunnellingOn = false;
        }

        public void NoReticle()
        {
            motionSicknessManager.IsReticleOn = false;
            motionSicknessManager.IsReticleAlwaysShown = false;
        }

        public void AlwaysReticle()
        {
            motionSicknessManager.IsReticleOn = true;
            motionSicknessManager.IsReticleAlwaysShown = true;
        }

        public void NavigationOnlyReticle()
        {
            motionSicknessManager.IsReticleOn = true;
            motionSicknessManager.IsReticleAlwaysShown = false;
        }

        public void DisplayRenderingCanva()
        {
            renderingCanva.SetActive(true);
            navigationCanva.SetActive(false);
            reticleCanva.SetActive(false);
        }

        public void DisplayNavigationOptionCanva()
        {
            renderingCanva.SetActive(false);
            navigationCanva.SetActive(true);
            reticleCanva.SetActive(false);
        }

        public void DisplayReticleOptionCanva()
        {
            renderingCanva.SetActive(false);
            navigationCanva.SetActive(false);
            reticleCanva.SetActive(true);
        }

        public void DisplayNoCanva()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        public void DisplayMotionSicknessCanva()
        {
            Debug.LogError("DisplayMotionSicknessCanva");
            if(!motionSicknessManager.AreOptionsSaved)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                DisplayRenderingCanva();
            }
        }

        void ToggleValueChanged(Toggle toggle)
        {
            motionSicknessManager.SaveOptionsForAllSessions(toggle.isOn);
        }

        public void SetNextRenderingButtonInteractable()
        {
            nextRenderingCanva.interactable = true;
        }

        public void SetNextNavigationButtonInteractable()
        {
            nextNavigationCanva.interactable = true;
        }

        public void SetNextReticleButtonInteractable()
        {
            nextReticleCanva.interactable = true;
        }

        public void SetNextRenderingButtonNotInteractable()
        {
            nextRenderingCanva.interactable = false;
        }

        public void SetNextNavigationButtonNotInteractable()
        {
            nextNavigationCanva.interactable = false;
        }

        public void SetNextReticleButtonNotInteractable()
        {
            nextReticleCanva.interactable = false;
        }
    }
}