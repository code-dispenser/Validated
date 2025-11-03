using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Extensions;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidators.ConsoleClient.Common.Data;
using Validated.TenantValidators.ConsoleClient.Common.Models;

namespace Validated.TenantValidators.ConsoleClient.Examples;

internal static class Regex_Validator_Factory
{
    /*
        * You would be getting a list from cache most likely populated from your database. 
    */ 
    private static ImmutableList<ValidationRuleConfig> GetRuleConfigs()
    {
        return //Bare minimum fields for the regex validator. Just using default for all other fields 
        [
            new(
                TypeFullName: typeof(ContactDto).FullName!, 
                PropertyName: nameof(ContactDto.Title),
                DisplayName:  nameof(ContactDto.Title),
                RuleType:     "RuleType_Regex",         // ValidatedConstants.RuleType_Regex
                MinMaxToValueType: "",
                Pattern:       "^(Mr|Mrs|Ms|Dr|Prof)$", 
                FailureMessage:"Must be one of Mr, Mrs, Ms, Dr, Prof",
                MinLength: 2,
                MaxLength: 4)                           //Min and MaxLength not used by the regex but you could use these to set control field lengths.
        ];
    }
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        /*
            * Although it is possible to do: validatorFactoryProvider.GetValidatorFactory(ValidatedConstants.RuleType_Regex);
            * the validatorFactoryProvider will do this if the rule in the config data is RuleType_Regex.
            * You would also more than likely use the TenantValidationBuilder to create rules for an entire object 
            * but you do not have to.
         */

        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = GetRuleConfigs();
        /*
            * Not setting the other params as we are not using specific tenant or culture data. 
            * This returns a member validator which can be combined using AndThen to chain multiple validator for the single string property.
            * This is done for you is the rule configs had multiple entries for the same typeFullName + PropertyName entries.
        */ 
        var validator = validatorFactoryProvider.CreateValidator<string>(typeof(ContactDto).FullName!, nameof(ContactDto.Title), ruleConfigs);

        contactData.Title = "D";//will fail validation as its not in the list (regex alternation)
        /*
            *  Path optional this would get populated for you using the builder which uses the Validator.Extensions that work with objects not primitive values
        */ 
          
        var validatedBadTitle = await validator(contactData.Title);//This returns an invalid Validated<string> with an empty path
        await Console.Out.WriteLineAsync($"Is Title valid: {validatedBadTitle.IsValid} - Failures: {String.Join("\r\n", validatedBadTitle.Failures.Select(f => f))}\r\n");

        /*
            * Using the Validator.Extensions 
        */
        var entityTitleValidator    = ValidatorExtensions.ForEntityMember<ContactDto,string>(validator,c  => c.Title); // invalid with path
        var validatedEntityBadTitle = await entityTitleValidator(contactData);//This returns Validated<ContactDto>

        await Console.Out.WriteLineAsync($"Is Contact valid: {validatedEntityBadTitle.IsValid} - Failures: {String.Join("\r\n", validatedEntityBadTitle.Failures.Select(f => f))}\r\n");


        /*
            * Using the TenantValidationBuilder for the single property 
        */

        var builderValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider) // again invalid with path.
                                        .ForMember(c => c.Title)
                                            .Build();

        validatedEntityBadTitle = await builderValidator(contactData);
        await Console.Out.WriteLineAsync($"Is Contact valid: {validatedEntityBadTitle.IsValid} - Failures: {String.Join("\r\n", validatedEntityBadTitle.Failures.Select(f => f))}\r\n");

     
       
    }

}
