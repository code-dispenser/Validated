using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidators.ConsoleClient.Common.Data;
using Validated.TenantValidators.ConsoleClient.Common.Models;

namespace Validated.TenantValidators.ConsoleClient.Examples;

internal class String_Length_Validator_Factory
{
    private static ImmutableList<ValidationRuleConfig> GetRuleConfigs()
    {
        return //Bare minimum fields for the string length validator. Just using default for all other fields 
        [
            new(
                TypeFullName: typeof(AddressDto).FullName!,
                PropertyName: nameof(AddressDto.TownCity),
                DisplayName:  "Town/City",
                RuleType:     "RuleType_StringLength",  // ValidatedConstants.RuleType_StringLength
                MinMaxToValueType: "",
                Pattern:       "",
                FailureMessage:"Must be between 2 and 50 characters in length",
                MinLength: 2,
                MaxLength: 50)                          // Min and MaxLength are used by the StringLength validator.
        ];
    }
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var addressData = StaticData.CreateContactObjectGraph().Address;
        var ruleConfigs = GetRuleConfigs();

        var validator = TenantValidationBuilder<AddressDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForMember(a => a.TownCity)
                                    .Build();

        addressData.TownCity = "A";

        var validatedAddress = await validator(addressData);

        await Console.Out.WriteLineAsync($"Is address valid: {validatedAddress.IsValid} - Failures: {String.Join("\r\n", validatedAddress.Failures.Select(f => f))}\r\n");
    }
}
