using System;
using Microsoft.Extensions.Logging;

namespace TweetFi.Logging
{
    public class ConsoleColorLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleColorLogger(categoryName); // ✅ precisa enxergar ConsoleColorLogger
        }

        public void Dispose()
        {
            // Nada para liberar
        }
    }
}
