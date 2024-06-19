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

        private MotionSicknessManager motionSicknessManager;

        private bool keepOptionsForAllSession;

        void Awake()
        {
            Debug.LogError("awake");
            DisplayNoCanva();
            EventManager.StartListening(EventNames.LoadConnectionScene, ReinitSession);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, DisplayMotionSicknessCanva);
        }

        void Start()
        {
            motionSicknessManager = MotionSicknessManager.Instance;
            keepOptionsForAllSession = PlayerPrefs.GetString("motionSicknessOptions") != "" ? Convert.ToBoolean(PlayerPrefs.GetString("motionSicknessOptions")) : false;

            HeadsetRemovedInMirrorManager.Instance.event_OnHeadsetReset.AddListener(DisplayMotionSicknessCanva);
        }

        public void FullScreenRendering()
        {
            motionSicknessManager.IsReducedScreenOn = false;
            motionSicknessManager.IsNavigationEffectOnDemand = false;
        }

        public void ReducedScreenRendering()
        {
            motionSicknessManager.IsReducedScreenOn = true;
            motionSicknessManager.IsNavigationEffectOnDemand = true;
        }

        public void NoNavigationEffect()
        {
            motionSicknessManager.IsReducedScreenOn = false;
            motionSicknessManager.IsTunnellingOn = false;

            motionSicknessManager.IsNavigationEffectOnDemand = false;
        }

        public void TunnellingNavigationEffect()
        {
            motionSicknessManager.IsReducedScreenOn = false;
            motionSicknessManager.IsTunnellingOn = true;

            motionSicknessManager.IsNavigationEffectOnDemand = false;
        }

        public void ReducedScreenNavigationEffect()
        {
            motionSicknessManager.IsReducedScreenOn = true;
            motionSicknessManager.IsTunnellingOn = false;

            motionSicknessManager.IsNavigationEffectOnDemand = false;
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
            Debug.LogError("DisplayNoCanva");

            transform.GetChild(0).gameObject.SetActive(false);
        }

        public void DisplayMotionSicknessCanva()
        {
            Debug.LogError("DisplayMotionSicknessCanva");

            if(!keepOptionsForAllSession)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                DisplayRenderingCanva();
            }
        }

        public void KeepValueForAllSession(bool value)
        {
            PlayerPrefs.SetString("motionSicknessOptions", value.ToString());
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

        private void ReinitSession()
        {
            bool value = false;
            PlayerPrefs.SetString("motionSicknessOptions", value.ToString());
        }
    }
}