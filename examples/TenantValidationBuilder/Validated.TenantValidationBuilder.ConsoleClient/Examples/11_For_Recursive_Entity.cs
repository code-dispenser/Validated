using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Data;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Examples;

internal static class For_Recursive_Entity
{
    /*
        * ForRecursiveEntity is to validate recursive structures. And just like nested types its simpler to 
        * use two builders. To stop stack overflows you can set a maximum depth by using the ValidatedContext 
        * and ValidationOptions. The default maximum recursion depth is 100

    */
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var nodeData    = StaticData.BuildNodeTree(100);
        var ruleConfigs = StaticData.GetValidationRuleConfigs();


        var childNodeValidator = TenantValidationBuilder<Node>.Create(ruleConfigs, validatorFactoryProvider)
                                    .ForMember(n => n.Name)
                                        .Build();

        var nodeValidator = TenantValidationBuilder<Node>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForRecursiveEntity(c => c.Child!, childNodeValidator)
                                    .Build();

        Console.WriteLine("Executing the validator with a node tree that has 100 items, all valid.\r\n");

        var validatedContact = await nodeValidator(nodeData);

        Console.WriteLine($"Is the node tree data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

        Console.WriteLine("Executing the validator with a node tree with a depth of 100 but with a max recursion depth of 99\r\n");

        var context = new ValidatedContext(new ValidationOptions() { MaxRecursionDepth = 99 });

        validatedContact = await nodeValidator(nodeData, "", context);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

    }
}
