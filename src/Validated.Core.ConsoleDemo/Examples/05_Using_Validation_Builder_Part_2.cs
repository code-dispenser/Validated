using Validated.Core.Builders;
using Validated.Core.ConsoleDemo.Common.Data;
using Validated.Core.ConsoleDemo.Common.Models;
using Validated.Core.ConsoleDemo.Common.SharedValidators;
using Validated.Core.Types;

namespace Validated.Core.ConsoleDemo.Examples;

public static class Using_Validation_Builder_Part_2
{
    /*
        * To save typing I have placed some general field validator in the Common\SharedValidators - GeneralFieldValidators static class 
    */ 
    public static async Task Run()
    {
        /*
            * What if we have a nested complex object. The contact object has an Address property that has holds a AddressDTO type.
            * If a property is complex and nullable (optional i.e Address?) then use the Nullable version of the builder methods. 
            * This is important. If the complex type is nullable and missing then it passes validation otherwise it is checked with the respective validator.
            * If its non-nullable but is null and you have assigned a validator it will fail validation as it was'nt classed as an optional member.
        
            * Simple types also have Nullable method.
            * 
            * For nested objects its simpler just to use another builder and use its returned validator - everything is composable.
            * 
            * Don't forget the return is not an object its just a function that we then execute to get the Task<Validated<T>> 

         */ 

        var contact = StaticData.CreateContactObjectGraph();

        contact.NullableAddress  = null;//this now will just pass do the same for address and it will fail
        contact.NullableAge      = 65;

        var addressValidator = ValidationBuilder<AddressDto>.Create()
                                    .ForMember(a => a.AddressLine, GeneralFieldValidators.AddressLineValidator())
                                        .ForMember(a => a.TownCity, GeneralFieldValidators.TownCityValidator())
                                            .ForMember(a => a.County, GeneralFieldValidators.CountyValidator())
                                               .ForNullableStringMember(a => a.Postcode, GeneralFieldValidators.UKPostcodeValidator())
                                                    .Build();

        var contactValidator = ValidationBuilder<ContactDto>.Create()
                                    .ForMember(c => c.Title, GeneralFieldValidators.TitleValidator())
                                        .ForMember(c => c.GivenName, GeneralFieldValidators.GivenNameValidator())
                                            .ForMember(c => c.FamilyName, GeneralFieldValidators.FamilyNameValidator())
                                                .ForNullableMember(c => c.NullableAge, GeneralFieldValidators.NullableAgeValidator())
                                                    .ForNullableNestedMember(c => c.NullableAddress, addressValidator)
                                                        .ForNestedMember(c => c.Address, addressValidator)
                                                            .Build();


        await WriteResult(await contactValidator(contact));

        contact.Address = null!;

        await WriteResult(await contactValidator(contact));

        /*
            * Remember everything is re-usable and composable
            * YWe can validate just the address part of the contact using the addressValidator directly
            * Or just the Family name using the FamilyNameValidator etc
        */

        await WriteResult(await addressValidator(StaticData.CreateContactObjectGraph().Address));//we set this to null above so we just get it again
    }


    private static async Task WriteResult(Validated<ContactDto> validated)

        => await Console.Out.WriteLineAsync($"Is contact object valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f))}\r\n");
    private static async Task WriteResult(Validated<AddressDto> validated)

        => await Console.Out.WriteLineAsync($"Is contact object valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f))}\r\n");
}
