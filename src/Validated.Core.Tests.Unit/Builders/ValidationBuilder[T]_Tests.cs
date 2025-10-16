using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Tests.SharedDataFixtures.Common.Validators;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.Tests.Unit.Builders;

public class ValidationBuilder_Tests
{
    [Fact]
    public void The_create_method_should_create_and_return_a_validation_builder()
    
        => ValidationBuilder<ContactDto>.Create().Should().BeOfType<ValidationBuilder<ContactDto>>();
    
    [Fact]
    public async Task The_for_member_should_add_a_validator_for_the_member()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var memberValidator = StubbedValidators.CreatePassingMemberValidator<string>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForMember(c => c.GivenName, memberValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task The_for_nullable_member_should_be_able_to_add_a_validator_for_the_nullable_member()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var memberValidator = StubbedValidators.CreatePassingMemberValidator<int>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForNullableMember(c => c.NullableAge, memberValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task The_for_nullable_string_member_should_be_able_to_add_a_validator_for_the_nullable_string_member()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var memberValidator = StubbedValidators.CreatePassingMemberValidator<string>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForNullableStringMember(c => c.Mobile, memberValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task The_for_nested_member_should_be_able_to_add_a_validator_for_the_nested_entity()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var nestedValidator = StubbedValidators.CreatePassingEntityValidator<AddressDto>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForNestedMember(c => c.Address!, nestedValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task The_for_nullable_nested_member_should_be_able_to_add_a_validator_for_the_nullable_nested_entity()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var nestedValidator = StubbedValidators.CreatePassingEntityValidator<AddressDto>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForNullableNestedMember(c => c.Address, nestedValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task The_for_each_collection_member_should_be_able_to_add_a_validator_for_the_collection()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var nestedValidator = StubbedValidators.CreatePassingEntityValidator<ContactMethodDto>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForEachCollectionMember(c => c.ContactMethods, nestedValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task The_for_to_compare_member_should_add_a_validator_to_compare_members()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var memberValidator = StubbedValidators.CreatePassingMemberValidator<ContactDto>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForComparisonWithMember(c => c.DOB, memberValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task The_for_each_primitive_should_add_a_validator_to_validate_collection_primitives()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var memberValidator = StubbedValidators.CreatePassingMemberValidator<string>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForEachPrimitiveItem<string>(c => c.Entries, memberValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task The_for_collection_add_a_validator_to_validate_the_collection()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var memberValidator = StubbedValidators.CreatePassingMemberValidator<List<string>>();
        var builder         = ValidationBuilder<ContactDto>.Create().ForCollection(c => c.Entries, memberValidator);
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }


    [Fact]
    public async Task Build_called_with_no_added_validators_should_return_a_valid_validated()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var builder         = ValidationBuilder<ContactDto>.Create();
        var entityValidator = builder.Build();

        var validated = await entityValidator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task Build_called_with_fail_fast_on_null_set_to_true_should_return_an_invalid_validated_with_a_single_failure()
    {
        var listValidator   = StubbedValidators.CreatePassingMemberValidator<List<string>>();
        var memberValidator = StubbedValidators.CreateFailingMemberValidator<string>("GivenName", "First name", "Should not see this message");

        var entityValidator = ValidationBuilder<ContactDto>.Create()
                                .ForMember(c => c.GivenName, memberValidator)
                                .ForCollection(c => c.Entries, listValidator)
                                .Build(failFastOnNull: true);
        
        var validated = await entityValidator(null!);

        using(new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
        }
    }
  
    [Fact]
    public async Task Build_called_with_fail_fast_on_null_set_to_false_should_continue_causing_multiple_null_failures()
    {
        var listValidator   = StubbedValidators.CreateFailingMemberValidator<List<string>>(nameof(ContactDto.Entries), nameof(ContactDto.Entries), "Entries should contain items");
        var memberValidator = StubbedValidators.CreateFailingMemberValidator<string>(nameof(ContactDto.GivenName), "First name", "Should start with a capital");

        var entityValidator = ValidationBuilder<ContactDto>.Create()
                                .ForMember(c => c.GivenName, memberValidator)
                                .ForCollection(c => c.Entries, listValidator)
                                .Build(failFastOnNull: false);

        var validated = await entityValidator(null!);

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.PropertyName == nameof(ContactDto.GivenName) && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            validated.Failures[1].Should().Match<InvalidEntry>(i => i.PropertyName == nameof(ContactDto.Entries) && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
        }

    }

    [Fact]
    public async Task Build_called_with_fail_fast_on_null_set_to_true_run_as_normal_if_the_entity_is_not_null()
    {
        var contact         = StaticData.CreateContactObjectGraph();
        var listValidator   = StubbedValidators.CreateFailingMemberValidator<List<string>>(nameof(ContactDto.Entries), nameof(ContactDto.Entries), "Entries should contain items");
        var memberValidator = StubbedValidators.CreateFailingMemberValidator<string>(nameof(ContactDto.GivenName), "First name", "Should start with a capital");

        var entityValidator = ValidationBuilder<ContactDto>.Create()
                                .ForMember(c => c.GivenName, memberValidator)
                                .ForCollection(c => c.Entries, listValidator)
                                .Build(failFastOnNull: true);

        var validated = await entityValidator(contact);

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.PropertyName == nameof(ContactDto.GivenName) && i.FailureMessage == "Should start with a capital" && i.Cause == CauseType.Validation);
            validated.Failures[1].Should().Match<InvalidEntry>(i => i.PropertyName == nameof(ContactDto.Entries) && i.FailureMessage == "Entries should contain items" && i.Cause == CauseType.Validation);

        }
    }
}
