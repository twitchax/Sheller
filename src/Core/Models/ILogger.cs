

namespace Sheller.Models
{
    public interface ILogger
    {
        void Log(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}