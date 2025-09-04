using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Data;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Examples;

internal class For_Each_Primitive_Item
{
    /*
        * ForEachPrimitiveItem is for a collection of primitive types.  
    */
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForMember(c => c.Title)
                                    .ForEachPrimitiveItem(c => c.Entries)    
                                        .Build();


        contactData.Entries = ["EntryOne", "EntryTwo", "EntryThree"];

        Console.WriteLine("Executing the validator with a contact that a collection of string entries are valid.\r\n");

        var validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

        contactData.Entries = ["EntryOne", "EntryTwoLong", "EntryThree"];

        Console.WriteLine("Executing the validator with a contact has collection of string entries with an invalid item. \r\n");

        validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

    }
}
