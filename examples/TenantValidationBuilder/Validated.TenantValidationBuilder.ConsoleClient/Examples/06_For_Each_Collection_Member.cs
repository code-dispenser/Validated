using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Data;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Examples;

internal static class For_Each_Collection_Member
{
    /*
        * ForEachCollectionMember is for a collection of complex types.  
        * Just like nested types the simplest way to add validation for a collection of complex types 
        * is to use a separate builder for the contained complex type. 
    */
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        var contactMethodValidator = TenantValidationBuilder<ContactMethodDto>.Create(ruleConfigs, validatorFactoryProvider)
                                        .ForMember(c => c.MethodType)
                                            .ForMember(c => c.MethodValue)
                                                .Build();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForMember(c => c.Title)
                                    .ForEachCollectionMember(c => c.ContactMethods, contactMethodValidator)
                                        .Build();

        Console.WriteLine("Executing the validator with a contact that a collection of contact methods that are valid.\r\n");

        var validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

        contactData.ContactMethods = [new ContactMethodDto("GoodType", "GoodValue"), new ContactMethodDto("GoodTypeTwo", "B")];

        Console.WriteLine("Executing the validator with a contact has collection of contact methods with invalid data. \r\n");

        validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

    }
}
