using Validated.Core.Builders;
using Validated.Core.ConsoleDemo.Common.Data;
using Validated.Core.ConsoleDemo.Common.Models;
using Validated.Core.Extensions;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.ConsoleDemo.Examples;

public static class Using_Validation_Builder_Part_1
{
    /*
        * There are two builders. TenantValidationBuilder which is for multi-tenant apps and/or validators that should be built from configuration data.
        * And the one we will use here ValidationBuilder for non-multi-tenant/ static needs
        * Please see the examples folder in the GitHub Repo for separate dedicated solutions showing advanced scenarios. 
    
        * The ValidationBuilder is just a helper, You can do all of this manually, as everything the builder uses is available for you.
        * to manually create classes with all of the validators, which can be returned as a single validator, you just use the the Combine method found in the ValidatedExtension class.
        * 
        * I would put all of the following validators in static classes that can be used from anywhere, in this case for the builder to compose a single
        * validator, to validate an object graph - see the contents of the AllModels file in the Common - Models folder.
    */ 

    public static async Task Run()
    {
        var contact = StaticData.CreateContactObjectGraph();
        /*
            * Lets start simple with just a couple of non-complex properties. 
            * You need to supply the validator you want to use, these all get combined into a single validator.
            * This is why its good to create these in a shared project / location so they can be used from anywhere/ and/or for common fields.
            * I will just create these hee for the demonstration.
        */ 

        var titleValidator     = MemberValidators.CreateStringRegexValidator("^(Mr|Mrs|Ms|Dr|Prof)$", "Title", "Title", "Must be one of Mr, Mrs, Ms, Dr, Prof");
        var givenNameValidator = MemberValidators.CreateStringRegexValidator(@"^(?=.{2,50}$)[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$", "GivenName", "First name", "Must start with a capital letter and be between 2 and 50 characters in length");

        var familyNameValidator = MemberValidators.CreateStringRegexValidator(@"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", "FamilyName", "Surname", "Must start with a capital letter")
                                    .AndThen(MemberValidators.CreateStringLengthValidator(2, 50, "FamilyName", "Surname", "Must be between 2 and 50 characters in length"));

        var contactValidator = ValidationBuilder<ContactDto>.Create()
                                    .ForMember(c => c.Title, titleValidator)
                                        .ForMember(c => c.GivenName, givenNameValidator)
                                            .ForMember(c => c.FamilyName, familyNameValidator)
                                                .Build();//<< Don't forget to call build unless you want to continue building later

        
        await WriteResult(await contactValidator(contact));
        /*
            * Lets make it fail 
        */
        contact.Title = "Professor";
        await WriteResult(await contactValidator(contact));
    }

    private static async Task WriteResult(Validated<ContactDto> validatedContact)

        => await Console.Out.WriteLineAsync($"Is contact object valid: {validatedContact.IsValid} - Failures: {String.Join("\r\n", validatedContact.Failures.Select(f => f))}");
}
