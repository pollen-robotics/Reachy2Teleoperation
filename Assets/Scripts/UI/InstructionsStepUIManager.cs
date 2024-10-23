using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class InstructionsStepUIManager : MonoBehaviour
    {
        private MirrorSceneManager sceneManager;

        [SerializeField]
        private InitializationState instructionsStep;
        
        [SerializeField]
        private bool virtualRobotInstructions;

        private bool needUpdateInstructions;

        void Start()
        {
            sceneManager = MirrorSceneManager.Instance;
            needUpdateInstructions = false;

            if(Robot.IsCurrentRobotVirtual())
            {
                transform.ActivateChildren(virtualRobotInstructions);
            }
            else
            {
                sceneManager.event_OnTeleopInitializationStepChanged.AddListener(CheckInstructions);
                CheckInstructions();
            }
        }

        void CheckInstructions()
        {
            needUpdateInstructions = true;
        }

        void Update()
        {
            if(needUpdateInstructions)
            {
                needUpdateInstructions = false;
                transform.ActivateChildren(sceneManager.initializationState == instructionsStep);
            }
        }
    }
}
