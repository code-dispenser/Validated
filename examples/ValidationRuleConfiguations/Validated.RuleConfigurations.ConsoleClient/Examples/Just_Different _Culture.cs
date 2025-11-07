using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.RuleConfigurations.ConsoleClient.Common.Data;
using Validated.RuleConfigurations.ConsoleClient.Common.Models;

namespace Validated.RuleConfigurations.ConsoleClient.Examples;

internal class Just_Different_Culture
{
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        /*
            * Just getting our base rules which we add to for different tenants/cultures so there is always a rule to fall back on.
        */

        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        /*
            * The Family name rule is comprised of two rules one for characters and one for length, lets in effect override the length one by adding a French Culture 
        */

        ruleConfigs = ruleConfigs.Add(new ValidationRuleConfig("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactDto", "FamilyName", "Surname", "RuleType_StringLength", "", "",
                                      "French - Must be between 2 and 50 characters long", 2, 50, CultureID: "fr-FR"));

        var getCultureFromSomeContext = "fr-FR";
        var getTenantIDFromSomeContext = ValidatedConstants.Default_TenantID; //Using ALL



        /*
            * We want rules for the TenantID of ALL but in French. If there are no French rules then it will fall back to en-GB the default. 
        */
        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider, getTenantIDFromSomeContext, getCultureFromSomeContext)
                            .ForMember(c => c.FamilyName)
                                .Build();


        var contactData = StaticData.CreateContactObjectGraph();
        contactData.FamilyName = "p";// fails both parts of the two part validator that will get built for FamilyName;

        var validated = await validator(contactData);

        Console.WriteLine($"Is the contact data valid: {validated.IsValid} - Failures: \r\n{String.Join("\r\n", validated.Failures.Select(f => f))}  \r\n\r\n");

        /*
            * Lets make a typo in the French culture - Voila no French (like the pun)
        */

        getCultureFromSomeContext = "fr-RF";
        validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider, getTenantIDFromSomeContext, getCultureFromSomeContext)
                    .ForMember(c => c.FamilyName)
                        .Build();

        validated = await validator(contactData);
        
        Console.WriteLine($"Is the contact data valid: {validated.IsValid} - Failures: \r\n{String.Join("\r\n", validated.Failures.Select(f => f))}  \r\n");

    }
}
