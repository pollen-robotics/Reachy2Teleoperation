using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GstreamerWebRTC;


namespace TeleopReachy
{
    public class RobotVideoStream : MonoBehaviour
    {
        private GStreamerPluginCustom videoController;


        void Start()
        {
            videoController = WebRTCManager.Instance.gstreamerPlugin;

            // EventManager.StartListening(EventNames.QuitMirrorScene, UpdateRobot);
            // EventManager.StartListening(EventNames.MirrorSceneLoaded, UpdateModelRobot);
        }

        // void UpdateRobot()
        // {
        //     reachy = GameObject.Find("Reachy2").GetComponent<Reachy2Controller.Reachy2Controller>();
        // }

        // void UpdateModelRobot()
        // {
        //     reachy = GameObject.Find("Reachy2Ghost").GetComponent<Reachy2Controller.Reachy2Controller>();
        // }

        public Texture GetLeftTexture()
        {
            return videoController.GetLeftTexture();
        }

        public Texture GetRightTexture()
        {
            return videoController.GetLeftTexture();
        }
    }
}