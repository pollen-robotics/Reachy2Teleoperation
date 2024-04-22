using UnityEngine;

namespace TeleopReachy
{
    public class MirrorPanelUIManager : Singleton<MirrorPanelUIManager>
    {
        [SerializeField]
        private Transform statusPanel;

        [SerializeField]
        private Transform advancedOptions;

        [SerializeField]
        private Transform helpPanel;

        private bool isStatusPanelOpen;
        private bool isAdvancedOptionsOpen;
        private bool isHelpPanelOpen;

        private float timeElapsedStatusPanel;
        private float timeElapsedAdvancedOptions;

        private float timeElapsedHelpPanel;

        private bool needUpdateStatusPanel;
        private bool needUpdateAdvancedOptions;
        private bool needUpdateHelpPanel;

        private readonly Vector3 closedStatusPanelPosition = new Vector3(745, -14, 0);
        private readonly Vector3 openStatusPanelPosition = new Vector3(50, -14, 0);

        private readonly Vector3 closedHelpPanelPosition = new Vector3(745, -367, 0);
        private readonly Vector3 openHelpPanelPosition = new Vector3(50, -367, 0);

        private readonly Vector3 closedAdvancedOptionsPosition = new Vector3(-605, 544, 0);
        private readonly Vector3 openAdvancedOptionsPosition = new Vector3(-52, 544, 0);

        private Vector3 lerpStatusPanelStartingPosition;
        private Vector3 lerpStatusPanelGoalPosition;

        private Vector3 lerpHelpPanelStartingPosition;
        private Vector3 lerpHelpPanelGoalPosition;

        private Vector3 lerpAdvancedOptionsStartingPosition;
        private Vector3 lerpAdvancedOptionsGoalPosition;

        void Start()
        {
            isStatusPanelOpen = false;
            isAdvancedOptionsOpen = false;
            isHelpPanelOpen = false;

            needUpdateStatusPanel = false;
            needUpdateAdvancedOptions = false;
            needUpdateHelpPanel = false;

            statusPanel.ActivateChildren(false);
            statusPanel.GetChild(0).gameObject.SetActive(true);
            advancedOptions.ActivateChildren(false);
            advancedOptions.GetChild(0).gameObject.SetActive(true);
            helpPanel.ActivateChildren(false);
            helpPanel.GetChild(0).gameObject.SetActive(true);
        }

        private void OpenClosePanel(ref Vector3 lerpPanelStartingPosition, ref Transform panel, ref float timeElapsedPanel, ref Vector3 lerpPanelGoalPosition,
                                    Vector3 closedPanelPosition, Vector3 openPanelPosition, ref bool isPanelOpen, ref bool needUpdatePanel)
        {
            panel.ActivateChildren(true);
            lerpPanelStartingPosition = panel.localPosition;
            timeElapsedPanel = 0;
            if (isPanelOpen)
            {
                lerpPanelGoalPosition = closedPanelPosition;
            }
            else
            {
                lerpPanelGoalPosition = openPanelPosition;
            }
            isPanelOpen = !isPanelOpen;
            needUpdatePanel = true;
        }

        public void OpenCloseStatusPanel()
        {
            OpenClosePanel(ref lerpStatusPanelStartingPosition, ref statusPanel, ref timeElapsedStatusPanel, ref lerpStatusPanelGoalPosition,
                           closedStatusPanelPosition, openStatusPanelPosition, ref isStatusPanelOpen, ref needUpdateStatusPanel);
        }

        public void OpenCloseHelpPanel()
        {
            OpenClosePanel(ref lerpHelpPanelStartingPosition, ref helpPanel, ref timeElapsedHelpPanel, ref lerpHelpPanelGoalPosition,
                           closedHelpPanelPosition, openHelpPanelPosition, ref isHelpPanelOpen, ref needUpdateHelpPanel);
        }

        public void OpenCloseAdvancedOptions()
        {
            OpenClosePanel(ref lerpAdvancedOptionsStartingPosition, ref advancedOptions, ref timeElapsedAdvancedOptions, ref lerpAdvancedOptionsGoalPosition,
                           closedAdvancedOptionsPosition, openAdvancedOptionsPosition, ref isAdvancedOptionsOpen, ref needUpdateAdvancedOptions);
        }

        private void NeedUpdatePanel(ref float timeElapsedPanel, ref Transform panel, ref bool needUpdatePanel, bool isPanelOpen, Vector3 lerpPanelGoalPosition, Vector3 lerpPanelStartingPosition)
        {
            timeElapsedPanel += Time.deltaTime;
            if (timeElapsedPanel >= 1)
            {
                timeElapsedPanel = 0;
                panel.localPosition = lerpPanelGoalPosition;
                needUpdatePanel = false;
                if(!isPanelOpen)
                {
                    panel.ActivateChildren(false);
                    panel.GetChild(0).gameObject.SetActive(true);
                }
            }
            else
            {
                float fTime = timeElapsedPanel / 1;
                panel.localPosition = Vector3.Lerp(lerpPanelStartingPosition, lerpPanelGoalPosition, fTime);
            }
        }

        void Update()
        {
            if (needUpdateStatusPanel)
            {
                NeedUpdatePanel(ref timeElapsedStatusPanel, ref statusPanel, ref needUpdateStatusPanel, isStatusPanelOpen, lerpStatusPanelGoalPosition, lerpStatusPanelStartingPosition);
            }

            if (needUpdateHelpPanel)
            {
                NeedUpdatePanel(ref timeElapsedHelpPanel, ref helpPanel, ref needUpdateHelpPanel, isHelpPanelOpen, lerpHelpPanelGoalPosition, lerpHelpPanelStartingPosition);
            }

            if (needUpdateAdvancedOptions)
            {
                NeedUpdatePanel(ref timeElapsedAdvancedOptions, ref advancedOptions, ref needUpdateAdvancedOptions, isAdvancedOptionsOpen, lerpAdvancedOptionsGoalPosition, lerpAdvancedOptionsStartingPosition);
            }
        }
    }
}

