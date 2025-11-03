using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidators.ConsoleClient.Common.Data;
using Validated.TenantValidators.ConsoleClient.Common.Models;

namespace Validated.TenantValidators.ConsoleClient.Examples;

internal class Precision_Scale_Validator_Factory
{
    private static ImmutableList<ValidationRuleConfig> GetRuleConfigs()
    {
        return //Bare minimum fields for the url format validator. Just using default for all other fields 
        [
            new(
                TypeFullName: typeof(ContactDto).FullName!,
                PropertyName: nameof(ContactDto.StringAmount),
                DisplayName:  nameof(ContactDto.StringAmount),
                RuleType:     "RuleType_PrecisionScale",                                            // ValidatedConstants.RuleType_PrecisionScale
                MinMaxToValueType: "",
                Pattern:           "",               
                FailureMessage:"The amount of {ValidatedValue} is not valid. Precision and scale should be no more than of {MaxPrecision} and {MaxScale} but found {ActualPrecision} and {ActualScale}", //Using message replacement tokens see docs for which ones you can use per validator
                MinLength: 2,
                MaxLength: 12,                                                                      //Min and MaxLength not used by the url validator but you could use these to set control field lengths.
                AdditionalInfo:  new Dictionary<string, string>() {["Precision"]="7",["Scale"]="2"} // = ValidatedConstants.RuleDictKey_Precision and ValidatedConstants.RuleDictKey_Scale 

                ),
                new(
                TypeFullName: typeof(ContactDto).FullName!,
                PropertyName: nameof(ContactDto.StringAmount),
                DisplayName:  nameof(ContactDto.StringAmount),
                RuleType:     "RuleType_PrecisionScale",                                            // ValidatedConstants.RuleType_PrecisionScale
                MinMaxToValueType: "",
                Pattern:           "",
                FailureMessage:"The amount of {ValidatedValue} is not valid. Precision and scale should be no more than of {MaxPrecision} and {MaxScale} but found {ActualPrecision} and {ActualScale}", //Using message replacement tokens see docs for which ones you can use per validator
                MinLength: 2,
                MaxLength: 12,                                                                      //Min and MaxLength not used by the url validator but you could use these to set control field lengths.
                CultureID: "de-DE",// rule for all tenants wanting German culture
                AdditionalInfo:  new Dictionary<string, string>() {["Precision"]="7",["Scale"]="2"} // = ValidatedConstants.RuleDictKey_Precision and ValidatedConstants.RuleDictKey_Scale 

                )
        ];
    }

    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = GetRuleConfigs();

        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider) //just using the default TenantID ALL and default CultureID en-GB
                                .ForMember(c => c.StringAmount)                                           // This could be a decimal, string or any numeric type that is convertible to a decimal. Using string to demo cultures
                                    .Build();

        contactData.StringAmount = "10,123.555"; // Using UK formats. The default culture used in ValidationRuleConfig is en-GB unless specified.

        var validatedContact = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is UK contact data / StringAmount valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}\r\n");

        /*
            * In a multi-tenant app you would have/get the tenant ID and their specific culture from some context 
            * You would then pass this into the builder for it to get specific rules or use the fallback.
            * 
            * Its recommended that you build all rules using the defaults of ALL en-GB and then add specific tenant ids/culture rules which if not found can then use the fallback 
        */


        var tenantID = "ALL";    // = ValidatedConstants.Default_TenantID
        var cultureID = "de-DE"; // we want a German rule. As mentioned both of these values would be obtained from some context


        validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider, tenantID, cultureID) // Bot using a specific tenant id but want German culture. In reality the text would be in German as well etc.
                        .ForMember(c => c.StringAmount) 
                            .Build();


        contactData.StringAmount = "10.123,55"; // using German format with valid amount - comma and dot separators are reversed.

        validatedContact = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is German contact data / StringAmount valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}\r\n");
    }
}
