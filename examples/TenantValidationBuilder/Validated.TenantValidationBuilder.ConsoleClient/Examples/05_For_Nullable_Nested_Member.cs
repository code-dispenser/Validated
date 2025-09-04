using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Data;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Examples;

internal static class For_Nullable_Nested_Member
{
    /*
        * ForNullableNestedMember is for nullable/optional complex types members that are complex types.  
        * If the value is null validation is skipped and a valid result is returned otherwise its validated as normal
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
                                    .ForNullableNestedMember(c => c.NullableAddress, addressValidator)
                                        .Build();


        Console.WriteLine("Executing the validator with a contact that has a AddressDto? NullableAddress that is null .\r\n");

        contactData.NullableAddress = null;

        var validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

        contactData.NullableAddress = new AddressDto() { AddressLine = "Address line", TownCity = "T" };

        Console.WriteLine("Executing the validator with a contact that has a NullableAddress with an invalid town/city\r\n");

        validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

    }
}
