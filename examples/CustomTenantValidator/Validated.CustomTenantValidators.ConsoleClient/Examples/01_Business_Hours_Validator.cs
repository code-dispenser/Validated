using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Extensions;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.CustomTenantValidators.ConsoleClient.Common.Models;
using Validated.CustomTenantValidators.ConsoleClient.CustomValidators;

namespace Validated.CustomTenantValidators.ConsoleClient.Examples;

public static class Business_Hours_Validator
{

    /*
        * This data would be in your cache, most likely built from data retrieved from your apps database.
    */
    private static ImmutableList<ValidationRuleConfig> GetConfigData()//You would return other rules as well
    {
        Dictionary<string, string> additionalInfo = new()
        { 
            ["OpeningTime"] = "09:00",
            ["ClosingTime"] = "17:30",
            ["WorkingDays"] = "Monday, Tuesday, Wednesday, Thursday, Friday",
            ["Holidays"]    = "2025-05-05, 2025-05-26, 2025-08-25, 2025-12-25"    
        };//<< for your custom rule data
        return
        [   /*
                * As this is for a custom validator you could also add your own failure message replacement tokens for your validator - I added {Reason}
             */
            new(typeof(AppointmentRequest).FullName!, nameof(AppointmentRequest.RequestedDateTime), "Appointment", BusinessHoursValidatorFactory.RuleType_BusinessHours, "", "", "Requested date time is not available. {Reason}", 0, 0, "", "", "", ""
                , "", ValidatedConstants.TargetType_Item, ValidatedConstants.Default_TenantID, ValidatedConstants.Default_CultureID, additionalInfo
                )
        ];

    }
    /*
        * The custom business hours validator has been registered in the DI and was then added to the concrete ValidatorFactoryProvider
    */
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var ruleConfigs = GetConfigData();
        var validator   = TenantValidationBuilder<AppointmentRequest>.Create(ruleConfigs, validatorFactoryProvider)
                                                                       .ForMember(a => a.RequestedDateTime)
                                                                            .Build();

        var appointmentRequestOne   = new AppointmentRequest("Project meeting", new DateTime(2025, 6, 1, 10, 30, 0)); // Sunday 10:30am (closed on that day)
        var appointmentRequestTwo   = new AppointmentRequest("Project meeting", new DateTime(2025, 6, 2, 08, 30, 0)); // Monday 08:30am (closed at that time)
        var appointmentRequestThree = new AppointmentRequest("Project meeting", new DateTime(2025, 8, 25, 10, 30, 0)); // Monday 10:30am 2025-08-25 is a bank holiday
        var appointmentRequestFour  = new AppointmentRequest("Project meeting", new DateTime(2025, 9, 3, 14, 00, 0)); // Wednesday  14:00 good date time.

        await PrintResult(await validator(appointmentRequestOne));
        await PrintResult(await validator(appointmentRequestTwo));
        await PrintResult(await validator(appointmentRequestThree));
        await PrintResult(await validator(appointmentRequestFour));


        static async Task PrintResult(Validated<AppointmentRequest> validated)
        {
            var failures = validated.IsValid ? "N/A" : String.Join("\r\n", validated.Failures.Select(f => f.DisplayName + " - " + f.FailureMessage));

            await Console.Out.WriteLineAsync($"Is requested date valid: {validated.IsValid} - Failures:{failures}\r\n");
        }

        /*
            * You can also use your validators for simple primitives as long as the type FullName and PropertyName match the config
            * 
            * i.e  var primitiveValidator = validatorFactoryProvider.CreateValidator<DateTime>(typeof(AppointmentRequest).FullName!, nameof(AppointmentRequest.RequestedDateTime), ruleConfigs);
            * The path will be blank but you can provide it if needed when executing the validator or take it a step further and use
        */

    }
}
