using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TeleopReachy
{
    public class MenuWithHeadScript : LazyFollow
    {
        // private ControllersManager controllers;
        // Start is called before the first frame update
        void Start()
        {
            // controllers = ActiveControllerManager.Instance.ControllersManager;
            // if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            // {
            //     targetOffset = new Vector3(0.0f, 0.0f, 0.6f);   
            // }
            // else{
            targetOffset = new Vector3(0.0f, 0.0f, 0.8f);   
            // }
            maxDistanceAllowed = 0;
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }

}