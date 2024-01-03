using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class LoaderToStopManager : LazyFollow
    {
        [SerializeField]
        private Transform loaderA;

        [SerializeField]
        private Transform menu;

        public GameObject lock_image = null;

        private bool isLoaderActive = true;

        private RobotStatus robotStatus;
        private OfflineMenuManager offlineMenuManager;

        private OfflineMenuManager.OfflineMenuItem previousItem;
        // private ControllersManager controllers;

        // Start is called before the first frame update
        void Start()
        {
            // controllers = ActiveControllerManager.Instance.ControllersManager;
            // if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            // {
            //     targetOffset = new Vector3(0, -0.13f, 0.6f);
            // }
            // else{
            targetOffset = new Vector3(0, -0.13f, 0.8f);
            // }
            maxDistanceAllowed = 0;
            
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStopTeleoperation.AddListener(HideMenu);

            offlineMenuManager = OfflineMenuManager.Instance;
            offlineMenuManager.event_OnAskForOfflineMenu.AddListener(ShowMenu);
            offlineMenuManager.event_OnLeaveOfflineMenu.AddListener(HideMenu);

            previousItem = offlineMenuManager.selectedItem;

            lock_image.SetActive(false);

            HideMenu();
        }

        void ShowMenu()
        {
            menu.gameObject.SetActive(true);
            HighlightSelectedItem();
            isLoaderActive = true;
        }

        void HideMenu()
        {
            if (isLoaderActive)
            {
                loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = offlineMenuManager.indicatorTimer;
                menu.gameObject.SetActive(false);
                isLoaderActive = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isLoaderActive)
            {
                loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = offlineMenuManager.indicatorTimer;
                if (previousItem != offlineMenuManager.selectedItem)
                {
                    previousItem = offlineMenuManager.selectedItem;
                    HighlightSelectedItem();
                }
            }
        }

        void HighlightSelectedItem()
        {
            switch (offlineMenuManager.selectedItem)
            {
                case OfflineMenuManager.OfflineMenuItem.LockAndHome:
                    {
                        lock_image.SetActive(true);
                        break;
                    }
                case OfflineMenuManager.OfflineMenuItem.Home:
                    {
                        lock_image.SetActive(false);
                        break;
                    }
            }
        }
    }
}

