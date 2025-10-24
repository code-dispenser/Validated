using System.Security.Authentication.ExtendedProtection;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.ConsoleDemo.Common.Data;
using Validated.Core.ConsoleDemo.Common.Models;
using Validated.Core.ConsoleDemo.Common.SharedValidators;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.ConsoleDemo.Examples;

public static class Using_Validation_Builder_Part_5
{
    /*
        * You can create conditional scopes using DoWen and EndWhen. Validators inside the scope only get executed if the predicate in DoWhen is true.
        * Nested scopes are now supported.
        * Please ensure you close each DoWhen scope with EndWhen. Failure to do so will cause an InvalidOperationException to be raised when calling the
        * Build method on the ValidationBuilder.
    */
    public static async Task Run()
    {

        var contact = StaticData.CreateContactObjectGraph();
        var truePredicate = true;

        var addressValidator = ValidationBuilder<AddressDto>.Create()
                                    .ForMember(a => a.AddressLine, GeneralFieldValidators.AddressLineValidator()) //should pass if run
                                        .DoWhen(a => a.AddressLine.Length > 2)
                                            .ForNullableStringMember(a => a.Postcode, GeneralFieldValidators.UKPostcodeValidator()) //should fail if run
                                        .EndWhen()
                                    .Build();

        var contactValidator = ValidationBuilder<ContactDto>.Create()
                                    .DoWhen(_ => truePredicate)
                                        .ForMember(c => c.Age, GeneralFieldValidators.AgeValidator()) //should fail
                                    .EndWhen()
                                    .DoWhen(c => c.FamilyName != null)
                                        .ForMember(c => c.Title, GeneralFieldValidators.TitleValidator()) //should fail
                                        .ForMember(c => c.GivenName, GeneralFieldValidators.GivenNameValidator()) //should pass
                                            .DoWhen(c => c.Title == "D")
                                                .ForNestedMember(c => c.Address, addressValidator)
                                            .EndWhen()
                                    .EndWhen()
                                    .Build();


        contact.Age        = 60;    //fails validation.
        contact.Title      = "D";   //fails validation but the values makes the DoWhen pass for the address validation
        contact.FamilyName = "Smith"; 


        var validated = contactValidator(contact);

        await WriteResult(await contactValidator(contact));
    }

    private static async Task WriteResult(Validated<ContactDto> validated)

        => await Console.Out.WriteLineAsync($"Is contact object valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f))}\r\n");
}

