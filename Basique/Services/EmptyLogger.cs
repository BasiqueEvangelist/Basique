namespace Basique.Services
{
    public class EmptyLogger : IBasiqueLogger
    {
        public void Log(LogLevel level, string message) { }
    }
}