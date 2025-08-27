using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validated.Core.Factories;
using Validated.ValueObject.Application;
using Validated.ValueObject.Application.Configuration;
using Validated.ValueObject.Infrastructure.Configuration;

namespace Validated.ValueObject.ConsoleClient;
internal class Program
{
    static async Task Main()
    {
        var container = ConfigureAutofac();

        using (var scope = container.BeginLifetimeScope())
        {
            var applicationFacade = scope.Resolve<ApplicationFacade>();

            await StaticallyCreateFullName(applicationFacade);

            await DynamicallyCreateFullName(applicationFacade);

            await StaticallyCreateDateRangeWithCompareTo(applicationFacade);

            await DynamicallyCreateDateRangeWithCompareTo(applicationFacade);
        }

        Console.ReadLine();;
    }
    /*
        * Send data that would eventually get used for the FullName value object fields
        * You should not leal domain data back but for the demo we will use a string to get the full name object if valid or the failures.
    */
    public static async Task StaticallyCreateFullName(ApplicationFacade applicationFacade)
    {
        var staticGoodResult = await applicationFacade.StaticallyCreateFullName("John", "Doe"); //good data passes character and length check;
        var staticBadResult  = await applicationFacade.StaticallyCreateFullName("John", "D");   //bad data fails half of the family name rule, the length check;

        Console.WriteLine($"Static good result: {staticGoodResult}\r\n");
        Console.WriteLine($"Static bad result:  {staticBadResult}\r\n");
    }

    public static async Task DynamicallyCreateFullName(ApplicationFacade applicationFacade)
    {
        var dynamicGoodResult = await applicationFacade.DynamicallyCreateFullName("John", "Doe"); //good data passes character and length check;
        var dynamicBadResult  = await applicationFacade.DynamicallyCreateFullName("J", "Doe");    //bad data, fails the length check part of the rule, as there is only one rule the full single message is displayed;

        Console.WriteLine($"Dynamic good result: {dynamicGoodResult}\r\n");
        Console.WriteLine($"Dynamic bad result:  {dynamicBadResult}\r\n");
    }

    public static async Task StaticallyCreateDateRangeWithCompareTo(ApplicationFacade applicationFacade)
    {
        var staticGoodResult = await applicationFacade.StaticallyCreateDateRangeWithCompareTo(startDate: DateOnly.FromDateTime(new DateTime(2025, 1, 1)), endDate: DateOnly.FromDateTime(new DateTime(2025, 12, 1)));
        var staticBadResult  = await applicationFacade.StaticallyCreateDateRangeWithCompareTo(startDate: DateOnly.FromDateTime(new DateTime(2025, 7, 1)), endDate: DateOnly.FromDateTime(new DateTime(2025, 6, 1)));

        Console.WriteLine($"Static good result: {staticGoodResult}\r\n");
        Console.WriteLine($"Static bad result:  {staticBadResult}\r\n");
    }
    public static async Task DynamicallyCreateDateRangeWithCompareTo(ApplicationFacade applicationFacade)
    {
        var dynamicGoodResult = await applicationFacade.DynamicallyCreateDateRangeWithCompareTo(startDate: DateOnly.FromDateTime(new DateTime(2025, 1, 1)), endDate: DateOnly.FromDateTime(new DateTime(2025, 12, 1)));
        var dynamicBadResult  = await applicationFacade.DynamicallyCreateDateRangeWithCompareTo(startDate: DateOnly.FromDateTime(new DateTime(2025, 7, 1)), endDate: DateOnly.FromDateTime(new DateTime(2025, 6, 1)));

        Console.WriteLine($"Dynamic good result: {dynamicGoodResult}\r\n");
        Console.WriteLine($"Dynamic bad result:  {dynamicBadResult}\r\n");
    }


    public static IContainer ConfigureAutofac()
    {
        var builder = new ContainerBuilder();

        var services = new ServiceCollection();
        services.AddHybridCache();


        var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Error);
        });

        builder.Populate(services);
        /*
            * The library does its best not to raise exceptions (especially with dynamic/config data) but if one does occur it will fail the validation with a cause of RuleConfigError or SystemError.
            * Instead of raising exceptions it will log them for you so register the logger factory if you want to see them.
        */

        builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();

        builder.RegisterModule<InfrastructureAutofacModule>();
        builder.RegisterModule<ApplicationAutofacModule>();
        builder.RegisterType<ValidatorFactoryProvider>().As<IValidatorFactoryProvider>().SingleInstance();
        builder.RegisterType<ApplicationFacade>().AsSelf().SingleInstance();


        return builder.Build();
    }
}
