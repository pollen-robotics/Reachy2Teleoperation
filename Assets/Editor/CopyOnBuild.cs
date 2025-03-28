using UnityEngine;
using UnityEditor;
using System.IO;

public class CopyOnBuild
{
    [InitializeOnLoadMethod]
    static void CopyFileBeforeBuild()
    {
        string sourcePath = Path.Combine(Application.dataPath, "Scripts/reachy2-sdk-api/python/reachy2_sdk_api/__init__.py");
        string destinationPath = Path.Combine(Application.streamingAssetsPath, "__init__.py");

        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destinationPath, true);
            Debug.Log($"File copied before build: {destinationPath}");
        }
        else
        {
            Debug.LogError("Unable to find file: " + sourcePath);
        }
    }
}
