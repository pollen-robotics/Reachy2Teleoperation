using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class ReticleUIManager : MonoBehaviour
    {
        private UserMobilityFakeMovement mobilityFakeMovement;
        private bool isReticleActive;
        private bool needUpdate;

        void Start()
        {
            EventManager.StartListening(EventNames.MirrorSceneLoaded, Init);
            isReticleActive = false;
            transform.ActivateChildren(isReticleActive);
        }

        void Init()
        {
            mobilityFakeMovement = UserInputManager.Instance.UserMobilityFakeMovement;
            mobilityFakeMovement.event_OnStartMoving.AddListener(StartMoving);
            mobilityFakeMovement.event_OnStopMoving.AddListener(StopMoving);
        }

        void Update()
        {
            if(needUpdate)
            {
                needUpdate = false;
                transform.ActivateChildren(isReticleActive);
            }
        }

        void StartMoving()
        {
            isReticleActive = true;
            needUpdate = true;
        }

        void StopMoving()
        {
            isReticleActive = false;
            needUpdate = true;
        }
    }
}