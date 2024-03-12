using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;


namespace TeleopReachy
{
    public class EyeScript : MonoBehaviour
    {
        Renderer rend;

        float alpha = 1.0f;

        private ControllersManager controllers;
        private UserMobilityFakeMovement mobilityFakeMovement;

        private bool needUpdateScale;

        private float _timeElapsed;

        private Vector3 lerpStartingScale;
        private Vector3 lerpGoalScale;

        private Vector3 fullScreenScale = new Vector3(55111, 31000, 1);
        private Vector3 smallerScreenScale = new Vector3(27550, 15500, 1);

        Coroutine blackScreenAppears;
        public GameObject blackScreen;

        private MotionSicknessManager motionSicknessManager;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                Debug.Log("Oculus 2 detected");
                transform.position = new Vector3(-159.0f, -595.0f, 18473.0f);
            }
            else
            {
                Debug.Log("Oculus 3 or other detected");
            }
            motionSicknessManager = MotionSicknessManager.Instance;
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
        }

        void Init()
        {
            mobilityFakeMovement = UserInputManager.Instance.UserMobilityFakeMovement;

            mobilityFakeMovement.event_OnStartMoving.AddListener(SetImageSmaller);
            mobilityFakeMovement.event_OnStopMoving.AddListener(SetImageFullScreen);
        }

        void Update()
        {
            if(needUpdateScale)
            {
                needUpdateScale = false;
                StartCoroutine(BlackScreenAppears());
                transform.localScale = lerpGoalScale;
            }
        }

        IEnumerator BlackScreenAppears()
        {
            blackScreen.GetComponent<MeshRenderer>().enabled = true;
            yield return new WaitForSeconds(0.1f);
            blackScreen.GetComponent<MeshRenderer>().enabled = false;

        }

        void SetImageSmaller()
        {
            if (motionSicknessManager.IsReducedScreenOn)
            {
                if(!motionSicknessManager.IsNavigationEffectOnDemand || (motionSicknessManager.IsNavigationEffectOnDemand && motionSicknessManager.RequestNavigationEffect))
                {
                    lerpStartingScale = transform.localScale;
                    _timeElapsed = 0;
                    lerpGoalScale = smallerScreenScale;
                    needUpdateScale = true;
                }
            }
            
        }

        void SetImageFullScreen()
        {
            if (motionSicknessManager.IsReducedScreenOn)
            {
                if(!motionSicknessManager.IsNavigationEffectOnDemand || (motionSicknessManager.IsNavigationEffectOnDemand && motionSicknessManager.RequestNavigationEffect))
                {
                    lerpStartingScale = transform.localScale;
                    _timeElapsed = 0;
                    lerpGoalScale = fullScreenScale;
                    needUpdateScale = true;
                }
            }
        }

    }
}