
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Xunit.Sdk;

namespace Validated.Core.Tests.SharedDataFixtures.Fixtures;

public class MSDependencyInjectionFixture 
{
    public IServiceProvider          ServiceProvider            { get; }
    public IValidatorFactoryProvider ValidationFactoryProvider  { get; }
    public InMemoryLoggerFactory     LoggerFactory              { get; }

    public MSDependencyInjectionFixture()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILoggerFactory, InMemoryLoggerFactory>();
        services.AddSingleton<IValidatorFactoryProvider, ValidatorFactoryProvider>();

        ServiceProvider = services.BuildServiceProvider();

        ValidationFactoryProvider = ServiceProvider.GetRequiredService<IValidatorFactoryProvider>();
        LoggerFactory             = (InMemoryLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    }

}
