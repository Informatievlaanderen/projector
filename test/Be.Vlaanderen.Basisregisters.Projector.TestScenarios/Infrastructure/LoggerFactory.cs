namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios.Infrastructure
{
    using System;
    using Microsoft.Extensions.Logging;

    public class LoggerFactory : ILoggerFactory
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName) => new TestLogger();

        public void AddProvider(ILoggerProvider provider) { }
    }

    public class TestLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Console.WriteLine("{0}: {1}", logLevel, formatter(state, exception));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
