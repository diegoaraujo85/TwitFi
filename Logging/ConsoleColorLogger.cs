using System;
using Microsoft.Extensions.Logging;

namespace TweetFi.Logging
{
    public class ConsoleColorLogger : ILogger
    {
        private readonly string _categoryName;

        public ConsoleColorLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        // Implementação explícita da interface
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return new NullDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId,
                                TState state, Exception? exception,
                                Func<TState, Exception?, string> formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            string message = formatter(state, exception);

            ConsoleColor color = logLevel switch
            {
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.Cyan,
                LogLevel.Information => ConsoleColor.Green,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.Magenta,
                _ => ConsoleColor.White
            };

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{logLevel}] {_categoryName}: {message}");
            Console.ForegroundColor = originalColor;
        }
    }

    public class NullDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
