using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Validated.Core.Factories;

namespace Validated.Core.Tests.SharedDataFixtures.Common.Loggers;

public class InMemoryLoggerFactory : ILoggerFactory
{
    private readonly ConcurrentDictionary<string, ILogger> _loggers = [];

    public ILogger<T> CreateLogger<T>()
        => (ILogger<T>)_loggers.GetOrAdd(GetContextName<T>(), name => new InMemoryLogger<T>(name));

    public ILogger CreateLogger(string categoryName)

        => _loggers.GetOrAdd(categoryName, name => new InMemoryLogger<object>(name));

    public ILogger<T> GetLogger<T>()

        => (_loggers.TryGetValue(GetContextName<T>(), out var logger) && logger is ILogger<T> typedLogger) ? typedLogger : new InMemoryLogger<T>(GetContextName<T>());

    private string GetContextName<T>()

        => typeof(T).FullName ?? typeof(T).Name;

    public InMemoryLogger<object>? GetTestLogger(string categoryName)
    {
        _loggers.TryGetValue(categoryName, out var logger);
        return logger as InMemoryLogger<object>;
    }


    public void AddProvider(ILoggerProvider provider) { }
    public void Dispose() { }
}
