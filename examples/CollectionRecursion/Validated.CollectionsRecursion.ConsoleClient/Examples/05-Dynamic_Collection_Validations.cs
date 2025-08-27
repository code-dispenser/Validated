using System.Collections.Immutable;
using Validated.CollectionsRecursion.ConsoleClient.Common.Data;
using Validated.CollectionsRecursion.ConsoleClient.Common.Models;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;

namespace Validated.CollectionsRecursion.ConsoleClient.Examples;

public static class Dynamic_Collection_Validations
{
    /*
        * The process for validating collections is pretty much the same as using the ValidationBuilder as shown in the Main repo demo.
        * 
        * The main difference when using the dynamic configuration approach is that you have to add the type of validation in the config data.
        * 
        * By default the config data will assume you are validating an item/entity not the collection its self i.e the length of a collection.
    */


    private static ImmutableList<ValidationRuleConfig> GetConfigData()

        =>
        [
            new(typeof(ContactDto).FullName!,nameof(ContactDto.Entries),nameof(ContactDto.Entries), "RuleType_StringLength", "", "",
                                                                        $"Must be between 1 and 10 characters in length but found the entry '{FailureMessageTokens.VALIDATED_VALUE}' with a length of {FailureMessageTokens.ACTUAL_LENGTH}", 1, 10, "", ""),

            new(typeof(ContactDto).FullName!,nameof(ContactDto.ContactMethods), "Contact methods", ValidatedConstants.RuleType_CollectionLength, "", "", $"Must have at least 1 item but no more than 3, found {FailureMessageTokens.ACTUAL_LENGTH}"
                ,1, 3, "", "", "", "", "", ValidatedConstants.TargetType_Collection)
            // 1 and 3 are min and max lengths

            //Note the use of the ValidatedConstants.TargetType_Collection which lets the process know we are targeting the collection its self not its contents, by default this is set to TargetType_Item.
        ];
        
    public static async Task Scenario_One(IValidatorFactoryProvider validatorFactoryProvider)
    {
        /*
            * Scenario checking the length of a collection. 
        */ 
        var ruleConfigs = GetConfigData(); 
        var contactData = StaticData.CreateContactObjectGraph();

        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                                                .ForCollection(c => c.ContactMethods)
                                                                    .Build();
        var validated = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is collection length valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f.DisplayName + " - " + f.FailureMessage))}\r\n");
        /*
            * Lets make it fail by adding a couple more entries to take it over the limit 
        */

        contactData.ContactMethods = [.. contactData.ContactMethods, new ContactMethodDto("Type Three", "Value Three"), new ContactMethodDto("Type Four", "Value Four")];

        validated = await validator(contactData);

        await Console.Out.WriteLineAsync($"Is collection length valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f.DisplayName + " - " + f.FailureMessage))}\r\n");
    }

    public static async Task Scenario_Two(IValidatorFactoryProvider validatorFactoryProvider)
    {
        /*
            * Scenario checking the contents of a collection of primitives
        */
        var ruleConfigs = GetConfigData();
        var contactData = StaticData.CreateContactObjectGraph();

        var validator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                                                .ForEachPrimitiveItem(e => e.Entries)
                                                                    .Build();
        /*
            * Each entry should be between 1 and 10 characters in length, we will create one good and one bad. 
        */

        contactData.Entries = ["GoodEntry", "Failing Entry"];


        var validated = await validator(contactData);

        await Console.Out.WriteLineAsync($"Are all items valid:  {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f.DisplayName + " - " + f.FailureMessage))}\r\n");

    }
}
