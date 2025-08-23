using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.Tests.Integration.Builders;

public class ValidationBuilder_Tests
{
    [Fact]
    public async Task For_collection_should_return_an_invalid_validated_if_the_collection_is_null()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var memberValidator = MemberValidators.CreateCollectionLengthValidator<List<string>>(1, 5, nameof(ContactDto.Entries), nameof(ContactDto.Entries), "Must have between 1 and 5 entries");

        contact.Entries = null!;
        var validator   = ValidationBuilder<ContactDto>.Create().ForCollection(c => c.Entries, memberValidator).Build();
        var validated   = await validator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
    }

    [Fact]
    public async Task For_each_primitive_item_should_return_an_invalid_validated_if_any_primitive_is_invalid_with_the_path_to_the_failing_item()
    {
        var contact = StaticData.CreateContactObjectGraph();
        var memberValidator = MemberValidators.CreateStringLengthValidator(1, 10, nameof(ContactDto.Entries), nameof(ContactDto.Entries), "Must have between 1 and 10 characters in length");

        contact.Entries = ["StringOne","StringToLong"];
        var validator = ValidationBuilder<ContactDto>.Create().ForEachPrimitiveItem(c => c.Entries, memberValidator).Build();
        var validated = await validator(contact);

        using(new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Entries)}[1]" && i.PropertyName == nameof(ContactDto.Entries) && i.DisplayName == nameof(ContactDto.Entries)
                                                           && i.FailureMessage == "Must have between 1 and 10 characters in length");
        }
       
    }

    [Fact]
    public async Task For_recursive_entity_should_recurse_until_the_max_depth_is_reached_with_deep_graphs()
    {
        var nodeChain = StaticData.BuildNodeChain(100);


        var childValidator = ValidationBuilder<Node>.Create()
                                .ForMember(n => n.Name, MemberValidators.CreateStringLengthValidator(6, 10, "Name", "Name", "Should be between 5 and 10 characters in length")).Build();

        var validator = ValidationBuilder<Node>.Create()
                            .ForRecursiveEntity(c => c.Child!, childValidator)
                                .Build();

        var validated = await validator(nodeChain, "", new(new ValidationOptions { MaxRecursionDepth = 5 }));//5 + 1 max depth message

        using(new AssertionScope())
        {
            validated.Should().Match<Validated<Node>>(v => v.IsValid == false && v.Failures.Count == 6);
            validated.Failures[5].FailureMessage.Should().Be(ErrorMessages.Validator_Max_Depth_Exceeded_User_Message);    
        }
    }

    [Fact]
    public async Task For_recursive_entity_should_recurse_until_all_are_validated_is_less_than_the_max_depth()
    {
        var nodeChain = StaticData.BuildNodeChain(100);


        var childValidator = ValidationBuilder<Node>.Create()
                                .ForMember(n => n.Name, MemberValidators.CreateStringLengthValidator(6, 10, "Name", "Name", "Should be between 5 and 10 characters in length")).Build();

        var validator = ValidationBuilder<Node>.Create()
                            .ForRecursiveEntity(c => c.Child!, childValidator)
                                .Build();

        var validated = await validator(nodeChain, "", new(new ValidationOptions { MaxRecursionDepth = 10 }));//5 + 1 max depth message
    }

}
