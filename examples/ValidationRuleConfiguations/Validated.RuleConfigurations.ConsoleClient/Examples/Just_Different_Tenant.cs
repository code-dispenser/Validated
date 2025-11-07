using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.RuleConfigurations.ConsoleClient.Common.Data;
using Validated.RuleConfigurations.ConsoleClient.Common.Models;

namespace Validated.RuleConfigurations.ConsoleClient.Examples;

internal class Just_Different_Tenant
{
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        /*
            * Just getting our base rules which we add to for different tenants/cultures so there is always a rule to fall back on.
        */

        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        /*
            * The Family name rule is comprised of two rules one for characters and one for length, lets in effect override the length one by adding a rule for a specific TenantID 
        */

        ruleConfigs = ruleConfigs.Add(new ValidationRuleConfig("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactDto", "FamilyName", "Surname", "RuleType_StringLength", "", "",
                                      "MyTenant - Must be between 2 and 50 characters long", 2, 50, TenantID: "MyTenant"));

        var getCultureFromSomeContext = ValidatedConstants.Default_CultureID;// this is the default en-GB so we could ommit it
        var getTenantIDFromSomeContext = "MyTenant";

        /*
            * We want rules for the TenantID of MyTenant but using the default culture of UK. If there is not tenant match it would fallback to the tenant of ALL 
        */
        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider, getTenantIDFromSomeContext, getCultureFromSomeContext)
                            .ForMember(c => c.FamilyName)
                                .Build();


        var contactData = StaticData.CreateContactObjectGraph();
        contactData.FamilyName = "p";// fails both parts of the two part validator that will get built for FamilyName;

        var validated = await validator(contactData);

        Console.WriteLine($"Is the contact data valid: {validated.IsValid} - Failures: \r\n{String.Join("\r\n", validated.Failures.Select(f => f))}  \r\n\r\n");

        /*
            * Lets make a typo in the TenantID
        */

        getTenantIDFromSomeContext = "NotMyTenant";

        validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider, getTenantIDFromSomeContext, getCultureFromSomeContext)
                    .ForMember(c => c.FamilyName)
                        .Build();

        validated = await validator(contactData);

        Console.WriteLine($"Is the contact data valid: {validated.IsValid} - Failures: \r\n{String.Join("\r\n", validated.Failures.Select(f => f))}  \r\n");

    }
}
