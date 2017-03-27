namespace Assets
{
    using UnityEngine;

    public class UnityLog : IUnityLog
    {
        public void LogInfo(string message, params object[] parameters)
        {
            Debug.Log(string.Format(message, parameters));
        }

        public void LogWarning(string message, params object[] parameters)
        {
            Debug.LogWarning(string.Format(message, parameters));
        }

        public void LogError(string message, params object[] parameters)
        {
            Debug.LogError(string.Format(message, parameters));
        }
    }
}