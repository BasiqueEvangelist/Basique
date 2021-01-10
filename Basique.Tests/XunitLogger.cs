using System;
using Basique.Services;
using Xunit.Abstractions;

namespace Basique.Tests
{
    public class XunitLogger : IBasiqueLogger
    {
        private readonly ITestOutputHelper output;

        public XunitLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void Log(LogLevel level, string message)
        {
            output.WriteLine($"{Enum.GetName(typeof(LogLevel), level)}: {message}");
        }
    }
}