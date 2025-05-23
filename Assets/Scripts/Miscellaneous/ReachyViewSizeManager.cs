using System.Collections;
using UnityEngine;


namespace TeleopReachy
{
    public class ReachyViewSizeManager : MonoBehaviour
    {
        private ControllersManager controllers;
        private UserMobilityFakeMovement mobilityFakeMovement;

        private bool needUpdateScale;
        private bool userRequestedSmallSize;

        private Vector3 lerpGoalScale;

        private Vector3 fullScreenScale = new Vector3(41333, -31000, 1);
        private Vector3 smallerScreenScale = new Vector3(20666, -15500, 1);

        public GameObject blackScreen;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            userRequestedSmallSize = false;
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                Debug.Log("Oculus 2 detected");
                transform.localPosition = new Vector3(0f, -595.0f, 18473.0f);
            }
            else
            {
                Debug.Log("Oculus 3 or other detected");
                transform.localPosition = new Vector3(0f, -3266f, 15093f);
            }
            motionSicknessManager = MotionSicknessManager.Instance;
            motionSicknessManager.event_OnRequestNavigationEffect.AddListener(ResizeView);
            
            mobilityFakeMovement = UserInputManager.Instance.UserMobilityFakeMovement;
            mobilityFakeMovement.event_OnStartMoving.AddListener(SetImageSmaller);
            mobilityFakeMovement.event_OnStopMoving.AddListener(SetImageFullScreen);
        }

        void Update()
        {
            if (needUpdateScale)
            {
                needUpdateScale = false;
                StartCoroutine(BlackScreenAppears());
                transform.localScale = lerpGoalScale;
            }
        }

        IEnumerator BlackScreenAppears()
        {
            if (blackScreen != null)
            {
                blackScreen.GetComponent<MeshRenderer>().enabled = true;
                yield return new WaitForSeconds(0.1f);
                blackScreen.GetComponent<MeshRenderer>().enabled = false;
            }
            else yield return null;
        }

        void SetImageSmaller()
        {
            if (motionSicknessManager.IsReducedScreenAutoOn && !userRequestedSmallSize )
            {
                lerpGoalScale = smallerScreenScale;
                needUpdateScale = true;
            }

        }

        void ResizeView(bool activate)
        {
            if (motionSicknessManager.IsReducedScreenOnClickOn && !activate)
            {
                lerpGoalScale = fullScreenScale;
                userRequestedSmallSize = false;
                needUpdateScale = true;
            }
            else if(motionSicknessManager.IsReducedScreenOnClickOn && activate)
            {
                lerpGoalScale = smallerScreenScale;
                userRequestedSmallSize = true;
                needUpdateScale = true;
            }
        }

        void SetImageFullScreen()
        {
            if (motionSicknessManager.IsReducedScreenAutoOn && !userRequestedSmallSize)
            {
                lerpGoalScale = fullScreenScale;
                needUpdateScale = true;
            }
        }
    }
}