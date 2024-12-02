public class Debug
{
    public static void Log(object obj)
    {
        UnityEngine.Debug.Log(System.DateTime.Now.ToString("HH:mm:ss.fff") + " : " + obj);
    }

    public static void LogWarning(object obj)
    {
        UnityEngine.Debug.LogWarning(System.DateTime.Now.ToString("HH:mm:ss.fff") + " : " + obj);
    }

    public static void LogError(object obj)
    {
        UnityEngine.Debug.LogError(System.DateTime.Now.ToString("HH:mm:ss.fff") + " : " + obj);
    }

    public static void LogError(string format, UnityEngine.Renderer renderer)
    {
        string message = System.DateTime.Now.ToString("HH:mm:ss.fff") + " : " + format;
        UnityEngine.Debug.LogError(message, renderer);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        string message = string.Format(format, args);
        UnityEngine.Debug.LogError(System.DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message);
    }

    public static void LogErrorFormat(UnityEngine.Renderer renderer, string format, params object[] args)
    {
        string message = string.Format(format, args);
        UnityEngine.Debug.LogError(System.DateTime.Now.ToString("HH:mm:ss.fff") + " : " + message, renderer);
    }
}
