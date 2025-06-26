using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class TeleoperationUIMenusManager : Singleton<TeleoperationUIMenusManager>
    {
        public TeleoperationSuspensionUIManager TeleoperationSuspensionUIManager { get; private set; }
        public TeleoperationExitMenuUIManager TeleoperationExitMenuUIManager { get; private set; }
        public SpeedTimerUIManager SpeedTimerUIManager { get; private set; }
        public StartArmTeleoperationUIManager StartArmTeleoperationUIManager { get; private set; }

        protected override void Init()
        {
            TeleoperationSuspensionUIManager = GetComponentInChildren<TeleoperationSuspensionUIManager>();
            TeleoperationExitMenuUIManager = GetComponentInChildren<TeleoperationExitMenuUIManager>();
            SpeedTimerUIManager = GetComponentInChildren<SpeedTimerUIManager>();
            StartArmTeleoperationUIManager = GetComponentInChildren<StartArmTeleoperationUIManager>();
        }
    }
}

