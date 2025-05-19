using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class PreBuildFileCopier : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        CopyFileBeforeBuild();
    }

    static void CopyFileBeforeBuild()
    {
        string sourcePath = Path.Combine(Application.dataPath, "Scripts/reachy2-sdk-api/python/reachy2_sdk_api/__init__.py");
        string destinationPath = Path.Combine(Application.streamingAssetsPath, "api_version.txt");

        if (!File.Exists(sourcePath))
        {
            Debug.LogError("Could not find __init__.py to extract version.");
            return;
        }

        string versionLine = null;
        foreach (string line in File.ReadLines(sourcePath))
        {
            if (line.TrimStart().StartsWith("__version__"))
            {
                versionLine = line;
                break;
            }
        }

        if (versionLine == null)
        {
            Debug.LogError("__version__ line not found in __init__.py.");
            return;
        }

        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        File.WriteAllText(destinationPath, versionLine + "\n");
        Debug.Log($"Version written to version.txt: {versionLine}. Copied to {destinationPath}");
    }
}
