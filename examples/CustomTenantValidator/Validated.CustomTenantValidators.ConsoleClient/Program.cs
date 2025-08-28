using Autofac;
using Microsoft.Extensions.Logging;
using Validated.Core.Factories;
using Validated.CustomTenantValidators.ConsoleClient.CustomValidators;
using Validated.CustomTenantValidators.ConsoleClient.Examples;

namespace Validated.CustomTenantValidators.ConsoleClient;

internal class Program
{
    static async Task Main()
    {
        var container = ConfigureAutofac();

        using (var scope = container.BeginLifetimeScope())
        {
            var validatorFactoryProvider      = scope.Resolve<IValidatorFactoryProvider>();
            var businessHoursValidatorFactory = scope.Resolve<BusinessHoursValidatorFactory>();

            validatorFactoryProvider.AddOrUpdateFactory(BusinessHoursValidatorFactory.RuleType_BusinessHours, businessHoursValidatorFactory);

            await Business_Hours_Validator.Run(validatorFactoryProvider);
        }

        await container.DisposeAsync();

        Console.ReadLine();
    }

    public static IContainer ConfigureAutofac()
    {
        var builder = new ContainerBuilder();

        var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Error);
        });

        builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();
        /*
            * Validators for the config data are created from factories that are stored inside this class so just register it as a singleton.
        */
        builder.RegisterType<ValidatorFactoryProvider>().As<IValidatorFactoryProvider>().SingleInstance();
        /*
            * Register your custom validator or any dependencies such as the ILoggerFactory 
        */ 
        builder.RegisterType<BusinessHoursValidatorFactory>().AsSelf().SingleInstance();

        return builder.Build();
    }
}
