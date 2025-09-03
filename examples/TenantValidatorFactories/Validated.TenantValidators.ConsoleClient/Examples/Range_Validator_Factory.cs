using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidators.ConsoleClient.Common.Data;
using Validated.TenantValidators.ConsoleClient.Common.Models;

namespace Validated.TenantValidators.ConsoleClient.Examples;

internal class Range_Validator_Factory
{
    private static ImmutableList<ValidationRuleConfig> GetRuleConfigs()
    {
        return //Bare minimum fields for the range validator. Just using default for all other fields 
        [
            new(
                TypeFullName: typeof(ContactDto).FullName!,
                PropertyName: nameof(ContactDto.Age),
                DisplayName:  nameof(ContactDto.Age),
                RuleType:     "RuleType_Range",               // ValidatedConstants.RuleType_Range
                MinMaxToValueType: "MinMaxToValueType_Int32", // ValidatedConstants.MinMaxToValueType_Int32
                Pattern:       "",
                FailureMessage:"Must be between 18 and 120",
                MinLength: 2,
                MaxLength: 3,                               // Min and MaxLengths are not used by the range validator.
                MinValue: "18",
                MaxValue: "120")                            // Min and MaxValue are required. As these are strings it uses the MinMaxToValueType for the conversion
        ];
    }
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData= StaticData.CreateContactObjectGraph();
        var ruleConfigs = GetRuleConfigs();

        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForMember(c => c.Age)
                                    .Build();

        contactData.Age = 17;

        var validatedContact = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is contact valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}\r\n");
   
    }
}
