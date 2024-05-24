using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class InstructionsTextUIManager : MonoBehaviour
    {
        private Text instructions;

        [SerializeField]
        private Text instructionsDetails;

        //private ConnectionStatus connectionStatus;
        private TransitionRoomManager transitionRoomManager;
        // private WristCalibINCIA wristCalibINCIA;
        private CaptureWristPose captureWristPose;

        private TextButtonControllerModifier textButtonControllerModifier;

        private bool needUpdateText;

        private string instructionsText;
        private string instructionsDetailsText;
        private int nbWristPosition =1;

        private static InstructionsTextUIManager instance;

        public static InstructionsTextUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<InstructionsTextUIManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(InstructionsTextUIManager).Name;
                        instance = obj.AddComponent<InstructionsTextUIManager>();
                    }
                }
                return instance;
            }
        }


        void Start()
        {
            instructions = transform.GetComponent<Text>();

            transitionRoomManager = TransitionRoomManager.Instance;
            // wristCalibINCIA = WristCalibINCIA.Instance;
            captureWristPose = CaptureWristPose.Instance;
            
            textButtonControllerModifier = GetComponent<TextButtonControllerModifier>();

            if (Robot.IsCurrentRobotVirtual())
            {
                instructionsText = "Have fun with virtual Reachy!";
            }
            else
            {
                instructionsText = "Place your joysticks in a neutral position then Press X" ;
                instructionsDetailsText = "";
    
            }
            transitionRoomManager.event_OnReadyForTeleop.AddListener(IndicateToPressA);
            transitionRoomManager.event_OnAbortTeleop.AddListener(IndicateRobotNotReady);
            captureWristPose.event_onWristCalib.AddListener(IndicateToPressX);
            captureWristPose.event_WristPoseCaptured.AddListener(IndicateRobotReady);
            needUpdateText = true;
        }

        void Update()
        {
            if (needUpdateText)
            {
                instructions.text = instructionsText;
                if (instructionsDetails != null) instructionsDetails.text = instructionsDetailsText;
                needUpdateText = false;
            }
        }

        void IndicateToPressA()
        {
            instructionsText = "Press and hold " + textButtonControllerModifier.GetPrimRightButtonName() + " to start";
            instructionsText = textButtonControllerModifier.ChangeTextAccordingToController(instructionsText);
            instructionsDetailsText = "";
            needUpdateText = true;
        }

        void IndicateRobotNotReady()
        {
            instructionsText = "Reachy is not ready for teleop.";
            instructionsDetailsText = "Some required services are missing. Check the status panel for more details!";
            needUpdateText = true;
        }

        public void IndicateToPressX()
        {
            string specificInstruction = new string("");
            switch (nbWristPosition)
            {
                case 1:
                    specificInstruction = "Turn your joysticks outwards completely\n";
                    break;
                case 2:
                    specificInstruction = "Turn your joysticks inwards completely\n";
                    break;
                case 3:
                    specificInstruction = "Tilt your joystick downwards\n";
                    break;
                case 4:
                    specificInstruction = "Tilt your joystick upwards\n";
                    break;
                case 5:
                    specificInstruction = "Tilt your joystick inwards\n";
                    break;
                case 6:
                    specificInstruction = "Tilt your joystick outwards\n";
                    break;
            }
            instructionsText = specificInstruction + "then Press " + textButtonControllerModifier.GetPrimLeftButtonName() ;
            instructionsText = textButtonControllerModifier.ChangeTextAccordingToController(instructionsText);
            instructionsDetailsText = "";
            nbWristPosition++;
            needUpdateText = true;
        }

        public void IndicateRobotReady()
        {
            instructionsText = "Reachy is ready for teleop, you can press 'Ready' to start";
            instructionsDetailsText = "";
            needUpdateText = true;
        }
    }
}

