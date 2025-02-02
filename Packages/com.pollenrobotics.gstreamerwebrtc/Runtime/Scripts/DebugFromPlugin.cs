/* Copyright(c) Pollen Robotics, all rights reserved.
 This source code is licensed under the license found in the
 LICENSE file in the root directory of this source tree. */

using UnityEngine;
using AOT;
using System;
using System.Runtime.InteropServices;

namespace GstreamerWebRTC
{
    public class DebugFromPlugin
    {
        public DebugFromPlugin()
        {
            RegisterDebugCallback(OnDebugCallback);
        }

        //------------------------------------------------------------------------------------------------
        [DllImport("UnityGStreamerPlugin", CallingConvention = CallingConvention.Cdecl)]
        static extern void RegisterDebugCallback(debugCallback cb);

        delegate void debugCallback(IntPtr request, int level, int size);
        enum Level { info, warning, error };
        [MonoPInvokeCallback(typeof(debugCallback))]
        static void OnDebugCallback(IntPtr request, int level, int size)
        {
            string debug_string = Marshal.PtrToStringAnsi(request, size);
            switch (level)
            {
                case (int)Level.info:
                    {
                        Debug.Log("UnityGStreamerPlugin: " + debug_string);
                        break;
                    }
                case (int)Level.warning:
                    {
                        Debug.LogWarning("UnityGStreamerPlugin: " + debug_string);
                        break;
                    }
                case (int)Level.error:
                    {
                        Debug.LogError("UnityGStreamerPlugin: " + debug_string);
                        break;
                    }
            }
        }
    }
}