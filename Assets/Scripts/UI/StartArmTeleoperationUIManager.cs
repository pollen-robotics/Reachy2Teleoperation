using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class StartArmTeleoperationUIManager : CustomLazyFollowUI
    {
        private bool needUpdateInfoMessage;
        private bool wantInfoMessageDisplayed;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.1f, 0.5f));

            EventManager.StartListening(EventNames.OnStartArmTeleoperation, HideMenu);
            EventManager.StartListening(EventNames.OnSuspendTeleoperation, HideMenu);

            needUpdateInfoMessage = false;
            wantInfoMessageDisplayed = false;
        }

        void Update()
        {
            if (needUpdateInfoMessage)
            {
                transform.ActivateChildren(wantInfoMessageDisplayed);
                needUpdateInfoMessage = false;
            }
        }

        void HideMenu()
        {
            wantInfoMessageDisplayed = false;
            needUpdateInfoMessage = true;
        }
    }
}