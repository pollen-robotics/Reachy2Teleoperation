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

        private bool _IsTunnellingOn { get; set; }

        public bool IsTunnellingOn { 
            get {
                return _IsTunnellingOn;
            }
            set { 
                ActivateDeactivateTunnelling(value);
                _IsTunnellingOn = value;
            }
        }
        public bool IsReducedScreenOn { get; set; }
        public bool IsNavigationEffectOnDemand { get; set; }

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
            GameObject camera = GameObject.Find("Main Camera");
            camera.transform.GetComponent<TunnellingMobile>().enabled = value;
        }
    }
}
