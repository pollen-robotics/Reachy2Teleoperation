using UnityEngine;


namespace TeleopReachy
{
    public enum Emotion
    {
        Happy, Sad, Confused, NoEmotion
    }

    public class UserEmotionInput : MonoBehaviour
    {
        public EmotionMenuManager emotionMenuManager;

        public ControllersManager controllers;
        
        private Vector2 selectedDirection;

        private RobotConfig robotConfig;
        private RobotJointCommands robotCommands;

        private ReachySimulatedCommands robotSimulatedCommands;
        private RobotStatus robotStatus;

        private TeleoperationManager teleoperationManager;

        private Emotion selectedEmotion;


        private void OnEnable()
        {
            EventManager.StartListening(EventNames.RobotDataSceneLoaded, Init);
        }

        private void OnDisable()
        {
            EventManager.StopListening(EventNames.RobotDataSceneLoaded, Init);
        }

        void Awake()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
        }

        private void Init()
        {
            robotCommands = RobotDataManager.Instance.RobotJointCommands;
            robotCommands.event_OnEmotionOver.AddListener(EmotionIsOver);
            robotSimulatedCommands = ReachySimulatedManager.Instance.ReachySimulatedCommands;
            robotSimulatedCommands.event_OnEmotionOver.AddListener(EmotionIsOver);
            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;
            emotionMenuManager.event_OnAskEmotion.AddListener(AskToPlayEmotion);

            teleoperationManager = TeleoperationManager.Instance;
        }

        void Update()
        {
            // For joystick commands
            controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out selectedDirection);

            float phi = Mathf.Atan2(selectedDirection[1], selectedDirection[0]);

            if (Mathf.Abs(phi) < (Mathf.PI / 8)) selectedEmotion = Emotion.Happy;
            if ((phi > (Mathf.PI / 2 - Mathf.PI / 8)) && (phi < (Mathf.PI / 2 + Mathf.PI / 8))) selectedEmotion = Emotion.Sad;
            if (Mathf.Abs(phi) > (Mathf.PI - Mathf.PI / 8)) selectedEmotion = Emotion.Confused;
            if ((phi > (-Mathf.PI / 2 - Mathf.PI / 8)) && (phi < (-Mathf.PI / 2 + Mathf.PI / 8))) selectedEmotion = Emotion.NoEmotion;
        }


        private void AskToPlayEmotion(Emotion emotion)
        {
            RobotCommands robot;
            if (robotConfig.HasHead() && teleoperationManager.IsRobotTeleoperationActive && !robotStatus.IsEmotionPlaying() && !robotStatus.AreRobotMovementsSuspended())
            {
                robot = robotCommands;
            }
            else
            {
                robot = robotSimulatedCommands;
            }
            robotStatus.SetEmotionPlaying(true);
            switch (emotion)
            {
                case Emotion.Sad:
                    robot.ReachySad();
                    break;

                case Emotion.Happy:
                    robot.ReachyHappy();
                    break;
                case Emotion.Confused:
                    robot.ReachyConfused();
                    break;
            }
        }

        public void EmotionIsOver(Emotion emotion)
        {
            robotStatus.SetEmotionPlaying(false);
        }
    }
}