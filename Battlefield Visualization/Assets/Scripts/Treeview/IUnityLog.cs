namespace Assets
{
    public interface IUnityLog
    {
        void LogInfo(string message, params object[] parameters);

        void LogWarning(string message, params object[] parameters);

        void LogError(string message, params object[] parameters);
    }
}
