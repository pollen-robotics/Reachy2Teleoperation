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

        private string line;

        void Start()
        {
            #if UNITY_EDITOR_WIN
                string path = "Assets/Scripts/reachy2-sdk-api/python/reachy2_sdk_api/__init__.py";
                if (!File.Exists(path))
                {
                    Debug.LogError($"Version file not found at path: {path}");
                    return;
                }

                try
                {
                    bool versionFound = false;
                    using (StreamReader reader = new StreamReader(path))
                    {
                        while (reader.Peek() >= 0 && !versionFound)
                        {
                            string line = reader.ReadLine();
                            if (line.StartsWith("__version__"))
                            {
                                versionFound = true;
                                string[] subs = line.Split('=');
                                appVersion = subs[1].Trim(' ', '"');
                                Debug.Log($"App version found: {appVersion}");
                            }
                        }
                    }

                    if (!versionFound)
                    {
                        Debug.LogError($"App version not found: {appVersion}");
                    }

                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to read version file: {ex.Message}");
                }
            #elif UNITY_STANDALONE_WIN
                string path = Application.streamingAssetsPath + "/api_version.txt";
                if (!File.Exists(path))
                {
                    Debug.LogError($"Version file not found at path: {path}");
                    return;
                }

                try
                {
                    line = File.ReadAllText(path);
                    if (line.StartsWith("__version__"))
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length >= 2)
                        {
                            appVersion = parts[1].Trim(' ', '"', '\n');
                            Debug.Log($"App version found: {appVersion}");
                        }
                    }
                    else
                    {
                        Debug.LogError("App version not found");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to read version file: {ex.Message}");
                }
            #endif

            
        }

        public string GetApplicationAPIVersion()
        {
            return appVersion;
        }
    }
}
