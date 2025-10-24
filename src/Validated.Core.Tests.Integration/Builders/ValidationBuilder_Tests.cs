using FluentAssertions;
using FluentAssertions.Execution;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
        var contact = StaticData.CreateContactObjectGraph();
        var memberValidator = MemberValidators.CreateCollectionLengthValidator<List<string>>(1, 5, nameof(ContactDto.Entries), nameof(ContactDto.Entries), "Must have between 1 and 5 entries");

        contact.Entries = null!;
        var validator = ValidationBuilder<ContactDto>.Create().ForCollection(c => c.Entries, memberValidator).Build();
        var validated = await validator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
    }

    [Fact]
    public async Task For_each_primitive_item_should_return_an_invalid_validated_if_any_primitive_is_invalid_with_the_path_to_the_failing_item()
    {
        var contact = StaticData.CreateContactObjectGraph();
        var memberValidator = MemberValidators.CreateStringLengthValidator(1, 10, nameof(ContactDto.Entries), nameof(ContactDto.Entries), "Must have between 1 and 10 characters in length");

        contact.Entries = ["StringOne", "StringToLong"];
        var validator = ValidationBuilder<ContactDto>.Create().ForEachPrimitiveItem(c => c.Entries, memberValidator).Build();
        var validated = await validator(contact);

        using (new AssertionScope())
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

        using (new AssertionScope())
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


    public class DoWhen
    {
        [Theory]
        [InlineData("Paul")]
        [InlineData("John")]
        public async Task Should_add_a_predicate_to_each_validator_until_cleared_with_end_when(string givenName)
        {
            var contact = StaticData.CreateContactObjectGraph();

            var validator = ValidationBuilder<ContactDto>.Create()
                                .DoWhen(c => c.GivenName == "Paul")
                                    .ForMember(c => c.Title, MemberValidators.CreateStringRegexValidator("^(Mr|Mrs|Ms|Dr|Prof)$", "Title", "Title", "Must be one of Mr, Mrs, Ms, Dr, Prof"))
                                    .ForMember(c => c.Age, MemberValidators.CreateRangeValidator(10, 50, "Age", "Age", "Must be between 10 and 50"))
                                    .EndWhen()
                                .Build();

            contact.GivenName = givenName;
            contact.Title     = "D";
            contact.Age       = 55;

            var validated = await validator(contact, nameof(ContactDto));

            if (givenName == "Paul")//Passes condition so runs validation
            {
                using (new AssertionScope())
                {
                    validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);

                    validated.Failures[1].Should().Match<InvalidEntry>(i => i.FailureMessage == "Must be between 10 and 50");

                    return;
                }
            }

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        [Fact]
        public async Task Should_throw_invalidate_operation_exception_if_scopes_are_not_closed()
        {
            var givenName = "Paul";
            var contact = StaticData.CreateContactObjectGraph();

            var builder = ValidationBuilder<ContactDto>.Create()
                                .DoWhen(c => c.GivenName == givenName)
                                    .ForMember(c => c.Title, MemberValidators.CreateStringRegexValidator("^(Mr|Mrs|Ms|Dr|Prof)$", "Title", "Title", "Must be one of Mr, Mrs, Ms, Dr, Prof"))
                                    .DoWhen(c => c.GivenName != null)
                                        .ForMember(c => c.Age, MemberValidators.CreateRangeValidator(10, 50, "Age", "Age", "Must be between 10 and 50"));


            FluentActions.Invoking(() => builder.Build()).Should().ThrowExactly<InvalidOperationException>().WithMessage("The DoWhen method(s) are not closed, missing 2 EndWhen(S). Please add the missing EndWhen(s)");

        }

        [Theory]
        [InlineData("Paul", "Kent", "London")]
        [InlineData("Paul", "Smith", "Edinburgh")]
        public async Task Should_work_with_nested_scopes(string givenName, string familyName, string location)
        {
            var contact = StaticData.CreateContactObjectGraph();

            contact.GivenName         = givenName;
            contact.FamilyName        = familyName;
            contact.Title             = "D";
            contact.Address!.Postcode = "ABC";
            contact.Address.TownCity  = location;

            var addressValidator = ValidationBuilder<AddressDto>.Create()
                                        .DoWhen(a => a.TownCity == "London")
                                            .ForNullableStringMember(a => a.Postcode, MemberValidators.CreateStringLengthValidator(7, 8, "Post code", "Post code", "Should be 7 or 8 characters"))
                                        .EndWhen()
                                        .Build();

            var validator = ValidationBuilder<ContactDto>.Create()
                                .DoWhen(c => c.GivenName == "Paul")
                                    .ForMember(c => c.Title, MemberValidators.CreateStringRegexValidator("^(Mr|Mrs|Ms|Dr|Prof)$", "Title", "Title", "Must be one of Mr, Mrs, Ms, Dr, Prof"))
                                    .DoWhen(c => c.FamilyName == "Kent")
                                        .ForNestedMember(a => a.Address!, addressValidator)
                                    .EndWhen()
                                .EndWhen()
                                .Build();



            var validated = await validator(contact, nameof(ContactDto));

            if (givenName == "Paul" && familyName == "Kent")//Passes both conditions so runs validation
            {
                using (new AssertionScope())
                {
                    validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);

                    validated.Failures[1].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be 7 or 8 characters");

                    return;
                }
            }
            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage =="Must be one of Mr, Mrs, Ms, Dr, Prof");
            }

        }

    }

    public class EndWhen
    {

        [Fact]
        public async Task Should_close_the_block()
        {
            var contact = StaticData.CreateContactObjectGraph();

            var validator = ValidationBuilder<ContactDto>.Create()
                                .DoWhen(c => c.GivenName == "Paul")
                                    .ForMember(c => c.Title, MemberValidators.CreateStringRegexValidator("^(Mr|Mrs|Ms|Dr|Prof)$", "Title", "Title", "Must be one of Mr, Mrs, Ms, Dr, Prof"))
                                .EndWhen()
                                    .ForMember(c => c.Age, MemberValidators.CreateRangeValidator(10, 50, "Age", "Age", "Must be between 10 and 50"))
                                .Build();

            contact.GivenName = "John"; //Causes condition to be skipped, closed before the Age validation so that runs.
            contact.Title     = "D";
            contact.Age       = 55;

            var validated = await validator(contact, nameof(ContactDto));

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);

                validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Must be between 10 and 50");

                return;
            }

        }
    }

}
