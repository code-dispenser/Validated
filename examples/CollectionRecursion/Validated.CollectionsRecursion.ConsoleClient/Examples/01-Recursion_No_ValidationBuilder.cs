using Validated.CollectionsRecursion.ConsoleClient.Common.Data;
using Validated.CollectionsRecursion.ConsoleClient.Common.Models;
using Validated.Core.Common.Constants;
using Validated.Core.Extensions;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.CollectionsRecursion.ConsoleClient.Examples;

public static class Recursion_No_ValidationBuilder
{
    public static async Task Scenario_One()
    {
        /*
            * Scenario is that you want to validate some tree structure that is N-Level deep, without the ValidationBuilder. 
            * Don't forget there is no short circuit validation will continue until all the nodes have been validated or the max recursion depth is reached.
        */
        var treeData = StaticData.BuildNodeTree(100);// << this is at the max limit of the allowable recursion depth unless changed via the ValidationContext see scenario two.

        /*
            * child nodes get named Child-index 
        */ 
        var nameValidator = MemberValidators.CreatePredicateValidator<string>(n => n.Contains("88") == false, "Name", "Name", $"Should not contain 88 but found {FailureMessageTokens.VALIDATED_VALUE}");
        
        var nodeNameValidator = ValidatorExtensions.ForEntityMember<Node, string>(nameValidator, n => n.Name);
        var validator         = ValidatorExtensions.ForRecursiveEntity<Node>(c => c.Child!, nodeNameValidator);
        
        var validated         = await validator(treeData);//<< default max recursion depth is 100 after that the validation exits with a failure.

        await Console.Out.WriteLineAsync($"Is Tree valid: {validated.IsValid} - Failures: {String.Join("\r\n",validated.Failures.Select(f => f.DisplayName + " - " + f.FailureMessage))}\r\n");

    }

    public static async Task Scenario_Two()
    {
        /*
            * Scenario is that you need to control the maximum recursion depth to stop any sort of stack overflow. 
        */

        var treeData = StaticData.BuildNodeTree(100);
        /*
            * child nodes get named Child-index, root is 1 (first node) so child-10 is beyond the allowable depth set below 
        */
        var nameValidator = MemberValidators.CreatePredicateValidator<string>(n => n.Contains("10") == false, "Name", "Name", $"Should not contain 10 but found {FailureMessageTokens.VALIDATED_VALUE}");

        var nodeNameValidator = ValidatorExtensions.ForEntityMember<Node, string>(nameValidator, n => n.Name);
        var validator         = ValidatorExtensions.ForRecursiveEntity<Node>(c => c.Child!, nodeNameValidator);
        
        var validatedContext  = new ValidatedContext(new ValidationOptions { MaxRecursionDepth = 10 });//<<Change to 11 to see two failures, child name and depth.

        var validated = await validator(treeData,"", validatedContext);//The root path of 'Node' will get filled in for you unless you want it renamed

        await Console.Out.WriteLineAsync($"Is Tree valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f.DisplayName + " - " + f.FailureMessage))}\r\n");
    }
}
