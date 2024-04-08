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

        //ajout calibration
        private RobotCalibration robotCalib;

        private TextButtonControllerModifier textButtonControllerModifier;

        private bool needUpdateText;

        private string instructionsText;
        private string instructionsDetailsText;

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
            Debug.Log("transitionroommanager instance = " + transitionRoomManager);
            robotCalib = RobotCalibration.Instance; // calibration
            Debug.Log("robotcalibration instance = " + robotCalib);
            textButtonControllerModifier = GetComponent<TextButtonControllerModifier>();

            if (Robot.IsCurrentRobotVirtual())
            {
                instructionsText = "Have fun with virtual Reachy!";
            }
            else
            {
                instructionsText = "Please face the mirror then press Ready";
                instructionsDetailsText = "";
            }

            transitionRoomManager.event_OnReadyForTeleop.AddListener(IndicateToPressA);
            transitionRoomManager.event_OnAbortTeleop.AddListener(IndicateRobotNotReady);
            robotCalib.event_WaitForCalib.AddListener(IndicateToPressX);
            robotCalib.event_StartRightCalib.AddListener(() => IndicateInitialCalibration("right"));
            robotCalib.event_StartLeftCalib.AddListener(() => IndicateInitialCalibration("left"));
            robotCalib.event_OnCalibChanged.AddListener(IndicateRobotReady);
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

        public void IndicateRobotReady()
        {
            instructionsText = "Reachy is ready for teleop, you can press 'Ready' to start";
            instructionsDetailsText = "";
            needUpdateText = true;
        }
        public void IndicateToPressA()
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


        //ajout calibration
        public void IndicateToPressX()
        {
            instructionsText = "Place arms on either side of body then Press " + textButtonControllerModifier.GetPrimLeftButtonName() + " to start the calibration";
            instructionsText = textButtonControllerModifier.ChangeTextAccordingToController(instructionsText);
            instructionsDetailsText = "";
            needUpdateText = true;
        }
        public void IndicateInitialCalibration(string side)
        {
            instructionsText = "Calibration of your " + side + " arm : " ;
            instructionsDetailsText = "Keep your " + side + " arm straight and move it in all directions.";
            needUpdateText = true;
        }

        public void IndicateEndofCalibration()
        {
            instructionsText = "Calibration done. " ;
            instructionsDetailsText = "";
            needUpdateText = true;
        }
    }
}

