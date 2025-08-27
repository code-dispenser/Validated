using Autofac;
using Microsoft.Extensions.Logging;
using Validated.CollectionsRecursion.ConsoleClient.Examples;
using Validated.Core.Factories;

namespace Validated.CollectionsRecursion.ConsoleClient;

internal class Program
{
    static async Task Main()
    {
        var container = ConfigureAutofac();

        using (var scope = container.BeginLifetimeScope())
        {

            var validatorFactoryProvider = scope.Resolve<IValidatorFactoryProvider>();

            await Recursion_No_ValidationBuilder.Scenario_One();
            await Recursion_No_ValidationBuilder.Scenario_Two();

            await Recursion_With_ValidationBuilder.Scenario_One();
            await Recursion_With_ValidationBuilder.Scenario_Two();

            await Recursion_With_TenantValidationBuilder.Scenario_One(validatorFactoryProvider);
            await Recursion_With_TenantValidationBuilder.Scenario_Two(validatorFactoryProvider);

            await Recursion_No_TenantValidationBuilder.Scenario_One(validatorFactoryProvider);
            await Recursion_No_TenantValidationBuilder.Scenario_Two(validatorFactoryProvider);

            await Dynamic_Collection_Validations.Scenario_One(validatorFactoryProvider);
            await Dynamic_Collection_Validations.Scenario_Two(validatorFactoryProvider);
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

        /*
            * The library does its best not to raise exceptions (especially with dynamic/config data) but if one does occur it will fail the validation with a cause of RuleConfigError or SystemError.
            * Instead of raising exceptions it will log them for you so register the logger factory if you want to see them.
        */

        builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();
        /*
            * Validators for the config data are created from factories that are stored inside this class so just register it as a singleton.
            * 
            * Equally the ValidatorFactoryProvider is where you can add you own custom config driven validators via its AddOrUpdateFactory method.
        */
        builder.RegisterType<ValidatorFactoryProvider>().As<IValidatorFactoryProvider>().SingleInstance();


        return builder.Build();
    }

}
