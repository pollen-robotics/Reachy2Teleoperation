using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class MiniVideoStreamDisplay : MonoBehaviour
    {
        private RobotVideoStream videoStream;
        private bool need_update_mini_viewer = false;
        private RawImage miniscreen;

        // Used to update display when going back in mirror scene
        void Start()
        {
            miniscreen = GetComponent<RawImage>();
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
            if (tex != null)
            {
                //GetComponent<Renderer>().material.SetTexture("_LeftTex", tex);
                //GetComponent<Renderer>().material.SetTexture("_RightTex", tex);
                miniscreen.texture = tex;
                // same texture in right eye
                //miniscreen.material.SetTexture("_MainTex_right", tex);
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
