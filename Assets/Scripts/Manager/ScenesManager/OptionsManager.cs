using System.Collections;
using UnityEngine;

namespace TeleopReachy
{
    public class OptionsManager : Singleton<OptionsManager>
    {
        public enum MotionSicknessEffect
        {
            None, Tunnelling, ReducedScreen
        }

        public MotionSicknessEffect motionSicknessEffectAuto { get; private set; }
        public MotionSicknessEffect motionSicknessEffectOnClick { get; private set; }
        public bool isReticleOn { get; private set; }

        void Start()
        {
            motionSicknessEffectAuto = MotionSicknessEffect.None;
            motionSicknessEffectOnClick = MotionSicknessEffect.None;
            isReticleOn = false;
        }

        public void SetMotionSicknessEffectAuto(MotionSicknessEffect effect)
        {
            motionSicknessEffectAuto = effect;
        }

        public void SetMotionSicknessEffectOnClick(MotionSicknessEffect effect)
        {
            motionSicknessEffectOnClick = effect;
        }

        public void SetReticleOn(bool isOn)
        {
            isReticleOn = isOn;
        }
    }
}
