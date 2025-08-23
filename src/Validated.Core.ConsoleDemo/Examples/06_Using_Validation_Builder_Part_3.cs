using Validated.Core.Builders;
using Validated.Core.ConsoleDemo.Common.Data;
using Validated.Core.ConsoleDemo.Common.Models;
using Validated.Core.ConsoleDemo.Common.SharedValidators;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.ConsoleDemo.Examples;

public static class Using_Validation_Builder_Part_3
{
    /*
        * Collections
        *
        * There are three types of types of collections from the point of view of the builder.
        * A collection of primitives    - ForEachPrimitiveItem
        * A collection of complex types - ForEachCollectionMember
        * The collection its self for validators such as the collections length (count) - ForCollection
    */ 
    public static async Task Run()
    {
        /*
            * Its simpler just to use another builder for complex types 
        */ 
        var contact = StaticData.CreateContactObjectGraph();

        contact.ContactMethods[0] = new ContactMethodDto("Mobile", "");
        contact.Entries           = ["EntryOne","EntryTwo", ""];

        var contactMethodValidator = ValidationBuilder<ContactMethodDto>.Create()
                                        .ForMember(c => c.MethodType, GeneralFieldValidators.MethodTypeValidator())
                                            .ForMember(c => c.MethodValue, GeneralFieldValidators.MethodValueValidator())
                                                .Build();

        var contactValidator = ValidationBuilder<ContactDto>.Create()
                                    .ForEachPrimitiveItem(c => c.Entries, GeneralFieldValidators.EntryValidator())
                                        .ForEachCollectionMember(c => c.ContactMethods, contactMethodValidator)
                                            .ForCollection(c => c.Entries, GeneralFieldValidators.EntryCountValidator())
                                                .Build();


        await WriteResult(await contactValidator(contact));

        contact.Entries = ["EntryOne", "EntryTwo", "", "EntryFour"];

        await WriteResult(await contactValidator(contact));

    }
    private static async Task WriteResult(Validated<ContactDto> validated)

        => await Console.Out.WriteLineAsync($"Is contact object valid: {validated.IsValid} - Failures: {String.Join("\r\n", validated.Failures.Select(f => f))}\r\n");

}
