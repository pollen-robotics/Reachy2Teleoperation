using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.XR.Interaction.Toolkit;


namespace TeleopReachy
{
    public class CanvasPositionAndOrientation : MonoBehaviour
    {
        public Transform observer; // The observer object in the scene
        public float sphereSize = 5.0f; // Distance to place the canvas to the left of the parent

        public Vector3 directionToCanvas; // Direction from observer to canvas
        public float leftDistance = 1.0f;

        private void Start()
        {
            transform.GetChild(0).gameObject.SetActive(false);
            EventManager.StartListening(EventNames.MirrorSceneLoaded, ShowCanva);
        }

        void ShowCanva()
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }

        void LateUpdate()
        {
            Transform parentTransform = transform.parent;
            Vector3 parentPosition = parentTransform.position;
            Vector3 referenceForward = observer.forward;
            Vector3 toCanvas = transform.position - parentPosition;
            Vector3 projected = Vector3.ProjectOnPlane(toCanvas, referenceForward);
            Vector3 desiredPosition = parentPosition + projected.normalized * toCanvas.magnitude;
            transform.position = desiredPosition;

            directionToCanvas = (transform.position - observer.position).normalized;
            transform.rotation = Quaternion.LookRotation(directionToCanvas, Vector3.up);
        }
    }
}
