using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class TeleoperationSuspensionUIManager : CustomLazyFollowUI
    {
        [SerializeField]
        private Transform loaderA;

        [SerializeField]
        private Text suspensionReasonText;

        private bool isLoaderActive = false;

        private TeleoperationSceneManager sceneManager;

        private bool needUpdateText = false;
        private string reasonString;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.15f, 0.8f));

            EventManager.StartListening(EventNames.OnSuspendTeleoperation, DisplaySuspensionWarning);

            sceneManager = TeleoperationSceneManager.Instance;
            HideSuspensionWarning();
        }

        void HeadsetRemoved()
        {
            reasonString = "Headset has been removed";
            needUpdateText = true;
        }

        void EmergencyStopCalled()
        {
            reasonString = "Emergency stop activated";
            needUpdateText = true;
        }

        void DisplaySuspensionWarning()
        {
            switch (TeleoperationManager.Instance.reasonForSuspension)
            {
                case TeleoperationManager.TeleoperationSuspensionCase.HeadsetRemoved:
                    HeadsetRemoved();
                    break;
                case TeleoperationManager.TeleoperationSuspensionCase.EmergencyStopActivated:
                    EmergencyStopCalled();
                    break;
                default:
                    break;
            }
            isLoaderActive = true;
            transform.ActivateChildren(true);
        }

        void HideSuspensionWarning()
        {
            loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = 0;
            isLoaderActive = false;
            transform.ActivateChildren(false);
        }


        void Update()
        {
            if (isLoaderActive)
            {
                loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = sceneManager.indicatorTimer;
            }
            if (needUpdateText)
            {
                needUpdateText = false;
                suspensionReasonText.text = reasonString;
            }
        }
    }
}

