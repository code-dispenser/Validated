using Microsoft.Extensions.Logging;

namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;

public record class LogEntry(string Category, LogLevel LogLevel, EventId EventId, string Message, Exception? Exception);

