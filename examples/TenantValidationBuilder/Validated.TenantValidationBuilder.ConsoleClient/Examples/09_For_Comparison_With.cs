using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Data;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Examples;

internal static class For_Comparison_With
{

    /*
        * ForComparisonWith is for a comparison validation between two members of the entity with the same data type
        * using a comparison type such as less than, greater than, equal to etc.
        * If either side of the comparison is null an invalid result is returned.
    */
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForMember(c => c.Title)
                                    .ForComparisonWith(c => c.CompareDOB)//This is the left hand-side, the config entry is the right-hand-side
                                        .Build();

        Console.WriteLine("Executing the validator with a contact that has a CompareDOB property that is greater than the DOB property\r\n");

        var validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

        contactData.CompareDOB = contactData.DOB.AddDays(-1);

        Console.WriteLine("Executing the validator with a contact that has a CompareDOB less than the DOB property\r\n");

        validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

    }
}
