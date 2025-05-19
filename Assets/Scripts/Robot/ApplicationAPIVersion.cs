using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;


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
                ParsePythonFile(path);
#elif (UNITY_STANDALONE_WIN)
            string path = Application.streamingAssetsPath + "/api_version.txt";
            ParseTxtFile(path);
#elif UNITY_ANDROID
            StartCoroutine(ParseTxtFileAndroid("api_version.txt"));
#endif
        }

        public string GetApplicationAPIVersion()
        {
            return appVersion;
        }

        private void ParsePythonFile(string path)
        {
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
        }

        private void ParseTxtFile(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Version file not found at path: {path}");
                return;
            }

            try
            {
                line = File.ReadAllText(path);
                ParseString(line);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to read version file: {ex.Message}");
            }
        }

        private void ParseString(string str)
        {
            if (str.StartsWith("__version__"))
            {
                string[] parts = str.Split('=');
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

        IEnumerator ParseTxtFileAndroid(string filename)
        {
            string jsonData = "";
            string filePath = Path.Combine(Application.streamingAssetsPath, filename);

            if (filePath.StartsWith("jar") || filePath.StartsWith("http"))
            {
                // Special case to access StreamingAsset content on Android and Web
                UnityWebRequest request = UnityWebRequest.Get(filePath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    jsonData = request.downloadHandler.text;
                }
            }
            else
            {
                // This is a regular file path on most platforms and in playmode of the editor
                jsonData = System.IO.File.ReadAllText(filePath);
            }

            Debug.Log("Loaded JSON Data: " + jsonData);
            ParseString(jsonData);
        }

    }
}
