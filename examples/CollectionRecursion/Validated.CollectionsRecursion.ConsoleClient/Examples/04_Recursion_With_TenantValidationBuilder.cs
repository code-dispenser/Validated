using System.Collections.Immutable;
using Validated.CollectionsRecursion.ConsoleClient.Common.Data;
using Validated.CollectionsRecursion.ConsoleClient.Common.Models;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;

namespace Validated.CollectionsRecursion.ConsoleClient.Examples;

public static class Recursion_With_TenantValidationBuilder
{
    /*
        * This data would be in your cache, most likely built from data retrieved from your apps database 
    */ 
    private static ImmutableList<ValidationRuleConfig> GetConfigData(string regexPattern, string failureMessage)//No predicate validator with the dynamic/config approach
    
        => 
        [
            new(typeof(Node).FullName!, nameof(Node.Name),nameof(Node.Name), ValidatedConstants.RuleType_Regex,"",regexPattern,failureMessage,0,0)
        ];
    
    public static async Task Scenario_One(IValidatorFactoryProvider validatorFactoryProvider)
    {
        /*
            * Scenario is that you want to validate some tree structure that is N-Level deep, using the TenantValidationBuilder. 
        */
        var treeData    = StaticData.BuildNodeTree(100);// << this is at the max limit of the allowable recursion depth unless changed via the ValidationContext see scenario two.
        var ruleConfigs = GetConfigData(@"^((?!88).)*$", $"Should not contain 88 but found {FailureMessageTokens.VALIDATED_VALUE}");


        var nodeValidator = TenantValidationBuilder<Node>.Create(ruleConfigs, validatorFactoryProvider)
                                                            .ForMember(node => node.Name)
                                                                .Build();

        var validator    = TenantValidationBuilder<Node>.Create(ruleConfigs, validatorFactoryProvider)
                                                            .ForRecursiveEntity(node => node.Child!, nodeValidator)
                                                                .Build();

        var validated = await validator(treeData);//<< default max recursion depth is 100 after that the validation exits with a failure.

        await Console.Out.WriteLineAsync($"Is Tree valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f.DisplayName + " - " + f.FailureMessage))}\r\n");
    }

    public static async Task Scenario_Two(IValidatorFactoryProvider validatorFactoryProvider)
    {
        /*
            * Scenario is that you need to control the maximum recursion depth to stop any sort of stack overflow. 
        */
        var treeData    = StaticData.BuildNodeTree(100);
        var ruleConfigs = GetConfigData(@"^((?!10).)*$", $"Should not contain 10 but found {FailureMessageTokens.VALIDATED_VALUE}");


        var nodeValidator = TenantValidationBuilder<Node>.Create(ruleConfigs, validatorFactoryProvider)
                                                            .ForMember(node => node.Name)
                                                                .Build();

        var validator = TenantValidationBuilder<Node>.Create(ruleConfigs, validatorFactoryProvider)
                                                            .ForRecursiveEntity(node => node.Child!, nodeValidator)
                                                                .Build();

        var validatedContext = new ValidatedContext(new ValidationOptions { MaxRecursionDepth = 10 });//<<Change to 11 to see two failures, child name and depth.
        var validated        = await validator(treeData,"",validatedContext);

        await Console.Out.WriteLineAsync($"Is Tree valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f.DisplayName + " - " + f.FailureMessage))}\r\n");
    }
}
