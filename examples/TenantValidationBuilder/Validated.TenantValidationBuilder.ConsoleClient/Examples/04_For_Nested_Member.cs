using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Data;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Examples;

internal static class For_Nested_Member
{
    /*
        * ForNestedMember is for non null members that are complex types.  
        * A null returns an invalid Validated<ContactDto> with a 'member name' is required message.
        * For the builder the simplest way to add nested members is to use a separate builder for the nested member. 
        * Both the addressValidator and contactValidator are just functions that can be executed.
    */
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        var addressValidator = TenantValidationBuilder<AddressDto>.Create(ruleConfigs, validatorFactoryProvider)
                                  .ForMember(a => a.AddressLine)
                                    .ForMember(a => a.TownCity)
                                        .Build();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForMember(c => c.Title)
                                    .ForNestedMember(c => c.Address, addressValidator)
                                        .Build();

        
        Console.WriteLine("Executing the validator with a contact that has an address with a valid address line and town/city .\r\n");

        var validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

        contactData.Address.TownCity = "T";

        Console.WriteLine("Executing the validator with a contact that has an address with an invalid town/city\r\n");

        validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

    }
}
