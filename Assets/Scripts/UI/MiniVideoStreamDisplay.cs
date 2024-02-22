using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace TeleopReachy 
{
    public class MiniVideoStreamDisplay : MonoBehaviour
    {
        private RobotVideoStream videoStream;
        private bool need_update_mini_viewer = false;
        
        // Used to update display when going back in mirror scene
        void Start()
        {
            videoStream = RobotDataManager.Instance.RobotVideoStream;
            videoStream.event_OnVideoTextureReady.AddListener(ReadyToUpdate);
            UpdateTexture();
        }

        void ReadyToUpdate()
        {
            need_update_mini_viewer = true;
        }

        void UpdateTexture()
        {
            Texture tex = videoStream.GetLeftEyeTexture();
            if(tex != null)
            {
                GetComponent<Renderer>().material.SetTexture("_LeftTex", tex);
                GetComponent<Renderer>().material.SetTexture("_RightTex", tex);
            }
        }

        void Update()
        {
            // Done only on connection
            if (need_update_mini_viewer)
            {
                need_update_mini_viewer = false;
                UpdateTexture();
            }
        }
    }
}
