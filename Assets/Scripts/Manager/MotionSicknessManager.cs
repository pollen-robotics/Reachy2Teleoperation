using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.VrTunnellingPro;


namespace TeleopReachy
{
    public class MotionSicknessManager : Singleton<MotionSicknessManager>
    {
        public bool IsReticleOn { get; set; }
        public bool IsReticleAlwaysShown { get; set; }

        public bool IsTunnellingOn { 
            get {
                return IsTunnellingOn;
            }
            set { 
                ActivateDeactivateTunnelling(value);
                IsTunnellingOn = value;
            }
        }
        public bool IsReducedScreenOn { get; set; }
        public bool IsNavigationEffectOnDemand { get; set; }


        // Start is called before the first frame update
        protected override void Init()
        {
            IsReticleOn = true;
            IsReticleAlwaysShown = false;

            IsTunnellingOn = false;
            IsReducedScreenOn = false;
            IsNavigationEffectOnDemand = false;
        }

        void ActivateDeactivateTunnelling(bool value)
        {
            GameObject camera = GameObject.Find("MainCamera");
            camera.transform.GetComponent<TunnellingMobile>().enabled = value;
        }
    }
}
