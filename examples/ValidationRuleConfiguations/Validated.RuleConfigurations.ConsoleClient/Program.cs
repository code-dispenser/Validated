using Autofac;
using Microsoft.Extensions.Logging;
using Validated.Core.Factories;
using Validated.RuleConfigurations.ConsoleClient.Examples;

namespace Validated.RuleConfigurations.ConsoleClient;

internal class Program
{
    static async Task Main(string[] args)
    {
        var container = ConfigureAutofac();

        using (var scope = container.BeginLifetimeScope())
        {
            var validatorFactoryProvider = scope.Resolve<IValidatorFactoryProvider>();

            await Just_Different_Culture.Run(validatorFactoryProvider);

            await Just_Different_Tenant.Run(validatorFactoryProvider);

            await Different_Tenant_And_Culture.Run(validatorFactoryProvider);

            await Just_Different_Version.Run(validatorFactoryProvider);
        }

        await container.DisposeAsync();

        Console.ReadLine();
    }

    public static IContainer ConfigureAutofac()
    {
        var builder = new ContainerBuilder();
        /*
            * The ValidatorFactoryProvider has an optional ILoggerFactory param in its constructor which is passes to the built-in validator factories.
            * You can omit the logger but it is strongly recommended that you provide one as the library strives not to raise exceptions and will log them
            * if a logger is available.
        */
        var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddConsole(); //use your desired sink
            logging.SetMinimumLevel(LogLevel.Error);
        });

        builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();
        /*
            * Validators for the config data are created from factories that are stored inside this class so just register it as a singleton.
        */
        builder.RegisterType<ValidatorFactoryProvider>().As<IValidatorFactoryProvider>().SingleInstance();

        return builder.Build();
    }
    /*
        * For Microsoft DI you would use the following. LoggerFactory is added automatically
        * 
        *   services.AddLogging(logging =>
        *   {
        *       logging.AddConsole();
        *       logging.SetMinimumLevel(LogLevel.Error);
        *   });
        * 
        * services.AddSingleton<IValidatorFactoryProvider, ValidatorFactoryProvider>();
        * 
    */
}
