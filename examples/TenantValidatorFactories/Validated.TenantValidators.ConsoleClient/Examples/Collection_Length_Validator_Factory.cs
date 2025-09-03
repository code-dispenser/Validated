using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidators.ConsoleClient.Common.Data;
using Validated.TenantValidators.ConsoleClient.Common.Models;

namespace Validated.TenantValidators.ConsoleClient.Examples;

internal class Collection_Length_Validator_Factory
{
    private static ImmutableList<ValidationRuleConfig> GetRuleConfigs()
    {
        return //Bare minimum fields for the collection length validator. Just using default for all other fields 
        [
            new(
            TypeFullName: typeof(ContactDto).FullName!,
            PropertyName: nameof(ContactDto.Entries),
            DisplayName:  nameof(ContactDto.Entries),
            RuleType:     "RuleType_CollectionLength", // ValidatedConstants.RuleType_CollectionLength
            MinMaxToValueType: "",
            Pattern:       "",
            FailureMessage:"Must contain between 2 and 5 entries but found: {ActualLength}",// FailureMessageTokens.ACTUAL_LENGTH see docs for what you can use per validator.
            MinLength: 2,
            MaxLength: 5,                            // Min and MaxLength are used by the collection length validator.
            MinValue: "",
            MaxValue: "",
            CompareValue: "",
            ComparePropertyName: "",
            CompareType: "",
            TargetType:"TargetType_Collection")     // ValidatedConstants.TargetType_Collection.
        ];
        /*
            * The Target type has the default of TargetType_Item. Collection is needed to let the framework know
            * that we want to validate the collection not its contents.
        */ 

    }
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = GetRuleConfigs();

        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForMember(c => c.Entries)
                                    .Build();

        contactData.Entries = ["One Entry"];

        var validatedContact = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is contact valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}\r\n");
        
    }
}
