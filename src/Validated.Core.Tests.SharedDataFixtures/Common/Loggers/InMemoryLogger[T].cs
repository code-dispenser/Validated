using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;

namespace Validated.Core.Tests.SharedDataFixtures.Common.Loggers;

public class InMemoryLogger<T>(string category) : ILogger<T>
{
    public List<LogEntry> LogEntries { get; } = [];
    public string         Category   { get; } = category; 

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        
        =>  LogEntries.Add(new(Category,logLevel,eventId, formatter(state, exception), exception));

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull 
        
        => null;

}