using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.RuleConfigurations.ConsoleClient.Common.Data;
using Validated.RuleConfigurations.ConsoleClient.Common.Models;

namespace Validated.RuleConfigurations.ConsoleClient.Examples;

internal class Just_Different_Version
{
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        /*
            * Before even trying to match on Tenant/Culture the engine groups on the latest version of a rule.
            * So if versions are used the tenant and culture in the rule with the version must match otherwise there is no rule i.e there is no fallback.
        */

        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        /*
            * The Family name rule is comprised of two rules one for characters and one for length, lets in effect override the length one by adding a rule for a specific version
        */

        var version = new ValidationVersion(1, 1, 1, DateTime.Now);//the datetime is not used in matching versions.

        ruleConfigs = ruleConfigs.Add(new ValidationRuleConfig("Validated.RuleConfigurations.ConsoleClient.Common.Models.ContactDto", "FamilyName", "Surname", "RuleType_StringLength", "", "",
                                                               "Versioned - Must be between 2 and 50 characters long", 2, 50, Version: version));

        /*
            * By default the engine will always try and get the latest version of a rule 
        */ 
        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                            .ForMember(c => c.FamilyName)
                                .Build();


        var contactData = StaticData.CreateContactObjectGraph();
        contactData.FamilyName = "p";// fails both parts of the two part validator that will get built for FamilyName;

         var validated = await validator(contactData);

        Console.WriteLine($"Is the contact data valid: {validated.IsValid} - Failures: \r\n{String.Join("\r\n", validated.Failures.Select(f => f))}  \r\n\r\n");
    }
}
