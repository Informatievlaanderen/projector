namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Internal;
    using Moq;

    public class FakeLoggerFactory : ILoggerFactory
    {
        private readonly Dictionary<string, FakeLogger> _loggerMocks = new Dictionary<string, FakeLogger>();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return ResolveLoggerMock(categoryName).AsLogger();
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        //override the extension method
        public ILogger<T> CreateLogger<T>()
        {
            return ResolveLoggerMock<T>().AsLoggerOf<T>();
        }

        public FakeLogger ResolveLoggerMock<T>()
        {
            var name = typeof(T).FullName;
            return ResolveLoggerMock(name);
        }

        private FakeLogger ResolveLoggerMock(string name)
        {
            if(false == _loggerMocks.ContainsKey(name))
                _loggerMocks[name] = new FakeLogger();

            return _loggerMocks[name];
        }
    }

    public class FakeLogger
    {
        private readonly Mock<ILogger> _mock;

        public FakeLogger()
        {
            _mock = new Mock<ILogger>();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _mock.Object.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _mock.Object.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _mock.Object.BeginScope(state);
        }

        public void Verify(LogLevel logLevel, string message, Func<Times> times)
        {
            _mock.Verify(
                logger => logger.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(o => o.ToString() == message),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                times);
        }

        public void Verify(string message, Exception exception, Func<Times> times)
        {
            _mock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(o => o.ToString() == message),
                    It.Is<Exception>(ex => ex == exception || ex.Message == exception.Message),
                    It.IsAny<Func<object, Exception, string>>()),
                times);
        }

        public static implicit operator Mock<ILogger>(FakeLogger fake) => fake.AsLoggerMock();

        public Mock<ILogger> AsLoggerMock() => _mock;
        public ILogger AsLogger() => _mock.Object;
        public ILogger<T> AsLoggerOf<T>() => (ILogger<T>)_mock.Object;
    }
}
