using System;
using System.Diagnostics;

namespace Basique.Services
{
    public struct Watcher : IDisposable
    {
        private readonly string prefix;
        private readonly IBasiqueLogger logger;
        private readonly long start;

        public Watcher(string prefix, IBasiqueLogger logger)
        {
            this.prefix = prefix;
            this.logger = logger;
            start = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            double duration = (double)(Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;
            logger.Log(LogLevel.Trace, $"{prefix}{duration} ms");
        }
    }
}