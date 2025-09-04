using Validated.Core.Builders;
using Validated.Core.Factories;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Data;
using Validated.TenantValidationBuilder.ConsoleClient.Common.Models;

namespace Validated.TenantValidationBuilder.ConsoleClient.Examples;

internal static class For_Nullable_Member
{
    /*
        * ForNullableMember is for members that might be nullable (int?)/optional.  
     * If the value is null validation is skipped and a valid result is returned otherwise its validated as normal
    */
    public static async Task Run(IValidatorFactoryProvider validatorFactoryProvider)
    {
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.GetValidationRuleConfigs();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider)
                                .ForNullableMember(c => c.NullableAge)
                                    .Build();

        contactData.NullableAge = null;

        Console.WriteLine("Executing the validator with a contact that has no nullable aga.\r\n");

        var validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

        contactData.NullableAge = 9;

        Console.WriteLine("Executing the validator with a contact that has a nullable age that does not meet the criteria\r\n");

        validatedContact = await contactValidator(contactData);

        Console.WriteLine($"Is the contact data valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}  \r\n");

    }
}