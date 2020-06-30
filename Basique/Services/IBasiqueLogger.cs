namespace Basique.Services
{
    public interface IBasiqueLogger
    {
        void Log(LogLevel level, string message);
    }
    public enum LogLevel
    {
        Trace, Debug, Info, Warning, Error
    }
}