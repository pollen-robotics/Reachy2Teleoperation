using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class TeleoperationExitMenuUIManager : LazyFollow
    {
        [SerializeField]
        private Transform loaderA;

        [SerializeField]
        private Transform menu;

        public GameObject lock_image = null;

        private bool isLoaderActive = true;

        private TeleoperationSceneManager sceneManager;
        private TeleoperationSceneManager.TeleoperationExitMenuItem previousItem;
    
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

            EventManager.StartListening(EventNames.OnStopTeleoperation, HideMenu);

            sceneManager = TeleoperationSceneManager.Instance;
            sceneManager.event_OnAskForTeleoperationMenu.AddListener(ShowMenu);
            sceneManager.event_OnLeaveTeleoperationMenu.AddListener(HideMenu);

            previousItem = sceneManager.teleoperationExitSelectedOption;

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
                loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = sceneManager.indicatorTimer;
                menu.gameObject.SetActive(false);
                isLoaderActive = false;
            }
        }

        void Update()
        {
            if (isLoaderActive)
            {
                loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = sceneManager.indicatorTimer;
                if (previousItem != sceneManager.teleoperationExitSelectedOption)
                {
                    previousItem = sceneManager.teleoperationExitSelectedOption;
                    HighlightSelectedItem();
                }
            }
        }

        void HighlightSelectedItem()
        {
            switch (sceneManager.teleoperationExitSelectedOption)
            {
                case TeleoperationSceneManager.TeleoperationExitMenuItem.LockAndHome:
                    {
                        lock_image.SetActive(true);
                        break;
                    }
                case TeleoperationSceneManager.TeleoperationExitMenuItem.Home:
                    {
                        lock_image.SetActive(false);
                        break;
                    }
            }
        }
    }
}

