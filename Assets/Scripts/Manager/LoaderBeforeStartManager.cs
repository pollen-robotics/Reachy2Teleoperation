using UnityEngine;

namespace TeleopReachy
{
    public class LoaderBeforeStartManager : MonoBehaviour
    {
        [SerializeField]
        private Transform loaderA;

        private ConnectionStatus connectionStatus;

        private ControllersManager controllers;

        private float indicatorTimer = 0.0f;
        private float minIndicatorTimer = 0.0f;

        private bool isLoaderActive = true;

        // Start is called before the first frame update
        void Start()
        {
            HideLoader();

            controllers = ActiveControllerManager.Instance.ControllersManager;

            connectionStatus = ConnectionStatus.Instance;

            connectionStatus.event_OnRobotUnready.AddListener(HideLoader);
            MirrorSceneManager.Instance.event_OnReadyForTeleop.AddListener(ShowLoader);
            MirrorSceneManager.Instance.event_OnAbortTeleop.AddListener(HideLoader);
        }

        void ShowLoader()
        {
            if (connectionStatus.IsRobotReady())
            {
                loaderA.gameObject.SetActive(true);
                isLoaderActive = true;
            }
        }

        void HideLoader()
        {
            if (isLoaderActive)
            {
                loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = minIndicatorTimer;
                indicatorTimer = minIndicatorTimer;
                loaderA.gameObject.SetActive(false);
                isLoaderActive = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (MirrorSceneManager.Instance.State == TransitionState.ReadyForTeleop && isLoaderActive)
            {
                bool rightPrimaryButtonPressed;

                if ((controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed) && rightPrimaryButtonPressed))
                {
                    indicatorTimer += Time.deltaTime;
                    loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = indicatorTimer;

                    if (indicatorTimer >= 1.0f)
                    {
                        MirrorSceneManager.Instance.ExitTransitionRoomRequested();
                    }
                }
                else
                {
                    indicatorTimer = minIndicatorTimer;
                    loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = minIndicatorTimer;
                }
            }
        }
    }
}

