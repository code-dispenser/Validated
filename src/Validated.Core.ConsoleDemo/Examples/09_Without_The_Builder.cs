using Validated.Core.ConsoleDemo.Common.Data;
using Validated.Core.ConsoleDemo.Common.Models;
using Validated.Core.ConsoleDemo.Common.SharedValidators;
using Validated.Core.Extensions;
using Validated.Core.Types;

namespace Validated.Core.ConsoleDemo.Examples;

public static class Without_The_Builder
{
    /*
        * Using the Validation builder is the easiest route but you can do this manually 
    */ 
    public static async Task Run()
    {
        /*
            * You would most likely put the following in a class/static class dependent on your needs 
            * Lets create a small validator to validate some of the ContactDto members
            * 
            * These use the next building block EntityValidator<T> delegate with a signature that returns a Task<Validated<T>> but they just consume/extend the MemberValidator that we have been using.
        */


        EntityValidator<ContactDto> familyNameValidator  = GeneralFieldValidators.FamilyNameValidator().ForEntityMember<ContactDto, string>(c => c.FamilyName);// << we just use the extensions methods to set the property name for the path
        /*
            * or the other way round 
        */ 
        EntityValidator<ContactDto> givenNameValidator   = ValidatorExtensions.ForEntityMember<ContactDto,string>(GeneralFieldValidators.GivenNameValidator(), c => c.GivenName);
        
        EntityValidator<ContactDto> ageValidator         = GeneralFieldValidators.NullableAgeValidator().ForNullableEntityMember<ContactDto, int>(c => c.NullableAge);

        EntityValidator<AddressDto> addressLineValidator = GeneralFieldValidators.AddressLineValidator().ForEntityMember<AddressDto, string>(a => a.AddressLine);

        EntityValidator<AddressDto> countyValidator      = GeneralFieldValidators.CountyValidator().ForEntityMember<AddressDto, string>(a => a.County);

        var addressDtoValidator = ValidatedExtensions.Combine(addressLineValidator, countyValidator);

        EntityValidator<ContactDto> addressValidator = ValidatorExtensions.ForNestedEntityMember<ContactDto, AddressDto>(c => c.Address, addressDtoValidator);

        /*
            * Now we combine them all to get a single entity validator for the contact dto 
        */

        var contactValidator = ValidatedExtensions.Combine(familyNameValidator, givenNameValidator, ageValidator, addressValidator);

        var contact = StaticData.CreateContactObjectGraph();

        contact.Address.County = "T";
        contact.GivenName = "J";

        await WriteResult(await contactValidator(contact));
    }

    private static async Task WriteResult(Validated<ContactDto> validated)

        => await Console.Out.WriteLineAsync($"Is contact object valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f))}\r\n");
}

