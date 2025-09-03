using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidators.ConsoleClient.Common.Data;
using Validated.TenantValidators.ConsoleClient.Common.Models;

namespace Validated.TenantValidators.ConsoleClient.Examples;

internal class Rolling_DateOnly_Validator_Factory
{
    private static ImmutableList<ValidationRuleConfig> GetRuleConfigs()
    {
        return //Bare minimum fields for the rolling date only validator. Just using default for all other fields 
        [
            new(
                TypeFullName: typeof(ContactDto).FullName!,
                PropertyName: nameof(ContactDto.DOB),
                DisplayName:  "Date of Birth",
                RuleType:     "RuleType_RollingDate",        // ValidatedConstants.RuleType_RollingDate
                MinMaxToValueType: "MinMaxToValueType_Year", // ValidatedConstants.MinMaxToValueType_Day or .MinMaxToValueType_Month or .MinMaxToValueType_Year for DateOnly calculations.
                Pattern:       "",
                FailureMessage:"You must be over 18 to apply but you were born: {ValidatedValue}} which is outside the range of: {MinDate} to {MaxDate} based on todays date of: {Today}.",
                MinLength: 0,
                MaxLength: 0,                               // Min and MaxLength are not used by the rolling date validator.
                MinValue: "-125",                           // Min and MaxValue are required. These should only contain integer values only. 
                MaxValue: "-18")                            // Min and Max are always relative to todays date. And the validated value must be in this range (inclusive)
        ];
    }
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var ruleConfigs = GetRuleConfigs();
        var contactData = StaticData.CreateContactObjectGraph();

        contactData.DOB = DateOnly.Parse("2015-06-15");

        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs,validatorFactoryProvider)
                                .ForMember(c => c.DOB)
                                    .Build();

        var validatedContact = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is contact valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}\r\n");
    }
}
