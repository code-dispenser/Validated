using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidators.ConsoleClient.Common.Data;
using Validated.TenantValidators.ConsoleClient.Common.Models;

namespace Validated.TenantValidators.ConsoleClient.Examples;

internal static class Comparison_Validator_Factory
{
    /*
        * This validator has the logic to compare:
        * 1. An entity member to another entity member of the same data type
        * 2. An entity member to a value in the config data.
        *
        * 3. A value object value compared to the config data entry - See the ValueObjects Demo for this type of comparison.
    */

    private static ImmutableList<ValidationRuleConfig> GetRuleConfigsForCompareWith()
    {
        return //Bare minimum fields for the range validator. Just using default for all other fields 
        [
            new(
                TypeFullName: typeof(ContactDto).FullName!,
                PropertyName: nameof(ContactDto.DOB),
                DisplayName:  "Date of birth",
                RuleType:     "RuleType_MemberComparison",      // ValidatedConstants.RuleType_MemberComparison
                MinMaxToValueType: "",                          // Not needed as the types need to match 
                Pattern:       "",
                FailureMessage:"Dates must be equal but the value: {ValidatedValue} did not match: {CompareToValue}",
                MinLength: 2,
                MaxLength: 3,                                  // Min and MaxLength are not used by the range validator.
                MinValue: "",
                MaxValue: "",                                  // Min and MaxValue are not used by the comparison validator
                CompareValue: "",                              // Not needed for member to member comparisons
                ComparePropertyName: nameof(ContactDto.CompareDOB),
                CompareType: "CompareType_EqualTo")           // ValidatedConstants.CompareType_EqualTo
            
        ];
    }
    private static ImmutableList<ValidationRuleConfig> GetRuleConfigsForCompareTo()
    {
        return //Bare minimum fields for the range validator. Just using default for all other fields 
        [
            new(
                TypeFullName: typeof(ContactDto).FullName!,
                PropertyName: nameof(ContactDto.Age),
                DisplayName:  nameof(ContactDto.Age),
                RuleType:     "RuleType_CompareTo",             // ValidatedConstants.RuleType_CompareTo
                MinMaxToValueType: "MinMaxToValueType_Int32",   // ValidatedConstants.MinMaxToValueType_Int32 this sets the datatype for the 
                Pattern:       "",
                FailureMessage:"Must be 18 or over to apply for credit",
                MinLength: 2,
                MaxLength: 3,                                  // Min and MaxLength are not used by the range validator.
                MinValue: "",
                MaxValue: "",                                  // Min and MaxValue are not for the comparison validator
                CompareValue: "18",                            // The MinMaxToValueType is used to determine the conversion from the string data
                ComparePropertyName: "",
                CompareType: "CompareType_GreaterThanOrEqual") // ValidatedConstants.CompareType_GreaterThanOrEqual

        ];
    }

    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = GetRuleConfigsForCompareWith();

        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                            .ForComparisonWith(c => c.DOB)
                                   .Build();

        var validatedContact = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is contact valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}\r\n");

        await Console.Out.WriteLineAsync("Now switching to using member to value comparison using Age\r\n");

        ruleConfigs = GetRuleConfigsForCompareTo();

        contactData.Age = 17;

        validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                            .ForComparisonWithValue(c => c.Age)
                                   .Build();

        validatedContact = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is contact valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}\r\n");


    }
}
