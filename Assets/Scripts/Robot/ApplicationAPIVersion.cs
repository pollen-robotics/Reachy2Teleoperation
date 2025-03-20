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
                // CopyFileIfNeeded("__init__.py");
                string path = Application.streamingAssetsPath + "/__init__.py";
            #endif

            using (StreamReader reader = new StreamReader(path))
            {
                bool versionFound = false;
                while (!versionFound)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("__version__"))
                    {
                        versionFound = true;
                        string[] subs = line.Split('=');
                        appVersion = subs[1].Trim(' ', '"');
                    }
                }
            }
        }

        public string GetApplicationAPIVersion()
        {
            return appVersion;
        }

        // private void CopyFileIfNeeded(string file_name)
        // {
        //     string sourcePath = Path.Combine(Application.streamingAssetsPath, file_name);
        //     string destinationPath = Path.Combine(Application.persistentDataPath, file_name);

        //     if (!File.Exists(destinationPath))
        //     {
        //         if (File.Exists(sourcePath))
        //         {
        //             File.Copy(sourcePath, destinationPath, true);
        //             Debug.Log($"Copied file to PersistentDataPath: {destinationPath}");
        //         }
        //         else
        //         {
        //             Debug.LogError($"Unable to find source file: {sourcePath}");
        //         }
        //     }
        //     else
        //     {
        //         Debug.Log($"File already found in PersistentDataPath: {destinationPath}");
        //     }
        // }
    }
}
