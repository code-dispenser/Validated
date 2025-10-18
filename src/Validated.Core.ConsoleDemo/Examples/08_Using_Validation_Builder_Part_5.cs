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
        * A scope remains in effect until closed with EndWhen. You can create as many scopes as you like.
        * 
        * Nested scopes are not currently supported.
    */
    public static async Task Run()
    {

        var contact = StaticData.CreateContactObjectGraph();

        var truePredicate = true;

        var contactValidator = ValidationBuilder<ContactDto>.Create()
                                    .DoWhen(_ => truePredicate)
                                        .ForMember(c => c.Age, GeneralFieldValidators.AgeValidator())
                                    .EndWhen()
                                    .DoWhen(c => c.FamilyName != null)
                                        .ForMember(c => c.Title, GeneralFieldValidators.TitleValidator())
                                        .ForMember(c => c.GivenName, GeneralFieldValidators.GivenNameValidator())
                                    .EndWhen()
                                    .Build();


        contact.Age        = 60;    //fails validation.
        contact.Title      = "D";   //fails validation but should not even run due to predicate.
        contact.FamilyName = null!; // force predicate to false


        var validated = contactValidator(contact);

        await WriteResult(await contactValidator(contact));
    }

    private static async Task WriteResult(Validated<ContactDto> validated)

        => await Console.Out.WriteLineAsync($"Is contact object valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f))}\r\n");
}

