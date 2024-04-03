using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AOT;
using System;
using System.Runtime.InteropServices;

namespace GstreamerWebRTC
{
    public class DebugFromPlugin : MonoBehaviour
    {
        // Use this for initialization
        void OnEnable()
        {
            RegisterDebugCallback(OnDebugCallback);
        }

        //------------------------------------------------------------------------------------------------
        [DllImport("UnityGStreamerPlugin", CallingConvention = CallingConvention.Cdecl)]
        static extern void RegisterDebugCallback(debugCallback cb);
        //Create string param callback delegate
        delegate void debugCallback(IntPtr request, int level, int size);
        enum Level { info, warning, error };
        [MonoPInvokeCallback(typeof(debugCallback))]
        static void OnDebugCallback(IntPtr request, int level, int size)
        {
            //Ptr to string
            string debug_string = Marshal.PtrToStringAnsi(request, size);
            switch (level)
            {
                case (int)Level.info:
                    {
                        Debug.Log(debug_string);
                        break;
                    }
                case (int)Level.warning:
                    {
                        Debug.LogWarning(debug_string);
                        break;
                    }
                case (int)Level.error:
                    {
                        Debug.LogError(debug_string);
                        break;
                    }
            }
        }
    }
}