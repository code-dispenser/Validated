using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Data;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Examples;

internal static class For_Collection
{
    /*
        * ForCollection is to validate the collection itself i.e its length 
        * IMPORTANT - The ValidationRuleConfig TargetType property must be set to TargetType_Collection
        * for validations at the collection level.
    */
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForMember(c => c.Title)
                                    .ForCollection(c => c.Entries)
                                        .Build();


        contactData.Entries = ["EntryOne", "EntryTwo", "EntryThree", "EntryFour", "EntryFive"];

        Console.WriteLine("Executing the validator with a contact that has a valid collection length 1 to 5 entries (inclusive).\r\n");

        var validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

        contactData.Entries.Add("EntrySix");

        Console.WriteLine("Executing the validator with a contact has collection of string entries with an invalid item. \r\n");

        validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

    }
}
