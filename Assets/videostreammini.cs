using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace TeleopReachy 
{
    public class videostreammini : MonoBehaviour
    {
        private RobotVideoStream videoStream;
        // Start is called before the first frame update
        void Start()
        {
            videoStream = RobotDataManager.Instance.RobotVideoStream;
            Texture tex = videoStream.GetLeftEyeTexture();
            GetComponent<Renderer>().material.SetTexture("_LeftTex", tex);
            GetComponent<Renderer>().material.SetTexture("_RightTex", tex);
        }
    }
}
