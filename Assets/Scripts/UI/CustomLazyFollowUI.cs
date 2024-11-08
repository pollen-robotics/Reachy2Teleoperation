using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class CustomLazyFollowUI : LazyFollow
    {
        private ControllersManager controllers;

        protected void SetOculusTargetOffset(Vector3 offset)
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                targetOffset = offset;
            }
            else
            {
                targetOffset = offset + new Vector3(0, 0, 0.2f);
            }
            maxDistanceAllowed = 0;
        }
    }
}