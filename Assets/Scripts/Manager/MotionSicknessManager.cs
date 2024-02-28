using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TeleopReachy
{
    public class MotionSicknessManager : Singleton<MotionSicknessManager>
    {
        public bool IsReticleOn { get; set; }
        public bool IsReticleAlwaysShown { get; set; }

        public bool IsTunnellingOn { get; set; }
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
    }
}
