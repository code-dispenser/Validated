using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidators.ConsoleClient.Common.Data;
using Validated.TenantValidators.ConsoleClient.Common.Models;

namespace Validated.TenantValidators.ConsoleClient.Examples;


internal class Url_Format_Validator_Factory
{
    /*
        * You would be getting a list from cache most likely populated from your database. 
    */
    private static ImmutableList<ValidationRuleConfig> GetRuleConfigs()
    {
        return //Bare minimum fields for the url format validator. Just using default for all other fields 
        [
           
            new(
                TypeFullName: typeof(ContactDto).FullName!,
                PropertyName: nameof(ContactDto.NullableStringUrl),
                DisplayName:  nameof(ContactDto.NullableStringUrl),
                RuleType:     "RuleType_UrlFormat",         // = ValidatedConstants.RuleType_UrlFormat
                MinMaxToValueType: "",
                Pattern:       "Http|Https",               // Pipe separated list of allowable schemes, currently these are only Http | Https | Ftp | Ftps
                FailureMessage:"Must be a valid http or https formatted Url",
                MinLength: 15,
                MaxLength: 250                           //Min and MaxLength not used by the url validator but you could use these to set control field lengths.
                )                          
        ];
    }
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider) 
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = GetRuleConfigs();

        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForNullableStringMember(c => c.NullableStringUrl)
                                    .Build();

        contactData.NullableStringUrl = "ftp://www.goggle.com"; // not an allowed scheme

        var validatedContact = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is contact data / url valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}\r\n");
    }
}
