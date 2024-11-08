using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class RobotDisplayManager : MonoBehaviour
    {
        [SerializeField]
        private Reachy2Controller.Reachy2Controller simulatedReachy;

        [SerializeField]
        private Reachy2Controller.Reachy2Controller realReachy;

        [SerializeField]
        private Transform realRobotLabel;

        private bool realRobotDisplayed;
        private bool needUpdateRobotDisplay;

        private RobotConfig robotConfig;

        private MirrorSceneManager sceneManager;

        void Start()
        {
            needUpdateRobotDisplay = false;
            realRobotDisplayed = false;

            sceneManager = MirrorSceneManager.Instance;
            sceneManager.event_OnTeleopInitializationStepChanged.AddListener(CheckStep);

            robotConfig = RobotDataManager.Instance.RobotConfig;
            robotConfig.event_OnConfigChanged.AddListener(ModifyRobotsDisplayed);

            HideRealRobot();
        }

        private void CheckStep()
        {
            switch (sceneManager.initializationState)
            {
                case InitializationState.ReadyForTeleop:
                    DisplayRealRobot();
                    break;
                case InitializationState.WaitingForRobotReady:
                    HideRealRobot();
                    break;
            }
        }

        private void ModifyRobotsDisplayed()
        {
            needUpdateRobotDisplay = true;
        }

        private void DisplayRealRobot()
        {
            realRobotDisplayed = true;
            needUpdateRobotDisplay = true;
        }

        private void HideRealRobot()
        {
            realRobotDisplayed = false;
            needUpdateRobotDisplay = true;
        }

        void Update()
        {
            if(needUpdateRobotDisplay)
            {
                needUpdateRobotDisplay = false;
                realReachy.transform.switchRenderer(realRobotDisplayed);
                realRobotLabel.gameObject.SetActive(realRobotDisplayed);
                if (robotConfig.GotReachyConfig())
                {
                    realReachy.Head.transform.switchRenderer(robotConfig.HasHead() && realRobotDisplayed);
                    realReachy.LeftArm.transform.switchRenderer(robotConfig.HasLeftArm() && realRobotDisplayed);
                    realReachy.RightArm.transform.switchRenderer(robotConfig.HasRightArm() && realRobotDisplayed);
                    realReachy.MobileBase.transform.switchRenderer(robotConfig.HasMobileBase() && realRobotDisplayed);

                    simulatedReachy.Head.transform.switchRenderer(robotConfig.HasHead());
                    simulatedReachy.LeftArm.transform.switchRenderer(robotConfig.HasLeftArm());
                    simulatedReachy.RightArm.transform.switchRenderer(robotConfig.HasRightArm());
                    simulatedReachy.MobileBase.transform.switchRenderer(robotConfig.HasMobileBase());
                }
            }
        }
    }
}

