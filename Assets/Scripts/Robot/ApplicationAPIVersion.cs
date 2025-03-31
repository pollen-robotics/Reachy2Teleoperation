using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine;

using UnityEngine.XR.Management;

namespace TeleopReachy
{
    public class ApplicationAPIVersion : Singleton<ApplicationAPIVersion>
    {
        private string appVersion;

        void Start()
        {
            #if UNITY_EDITOR_WIN
                string path = "Assets/Scripts/reachy2-sdk-api/python/reachy2_sdk_api/__init__.py";
            #elif UNITY_STANDALONE_WIN
                string path = Application.streamingAssetsPath + "/api_version.txt";
            #endif

            if (!File.Exists(path))
            {
                Debug.LogError($"Version file not found at path: {path}");
                return;
            }

            try
            {
                string line = File.ReadAllText(path);
                if (line.StartsWith("__version__"))
                {
                    string[] parts = line.Split('=');
                    if (parts.Length >= 2)
                    {
                        appVersion = parts[1].Trim(' ', '"');
                        Debug.Log($"App version found: {appVersion}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to read version file: {ex.Message}");
            }
        }

        public string GetApplicationAPIVersion()
        {
            return appVersion;
        }
    }
}
