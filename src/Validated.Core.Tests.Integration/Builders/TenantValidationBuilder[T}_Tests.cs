using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections;
using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Tests.SharedDataFixtures.Common.Validators;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.Tests.Integration.Builders;

public class TenantValidationBuilder_Tests
{

    private static TenantValidationBuilder<T> CreateTenantBuilder<T>(ImmutableList<ValidationRuleConfig> ruleConfigs) where T : notnull
    {
        var inMemoryLoggerFactory     = new InMemoryLoggerFactory();
        var validationFactoryProvider = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        return new TenantValidationBuilder<T>(ruleConfigs, validationFactoryProvider);
    }

    [Fact]
    public void Tenant_validation_builder_create_should_create_and_return_a_tenant_validation_builder()
    {
        var ruleConfigs               = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var inMemoryLoggerFactory     = new InMemoryLoggerFactory();
        var validationFactoryProvider = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validationFactoryProvider).Should().BeOfType<TenantValidationBuilder<ContactDto>>();
    }

    [Fact]
    public async Task Tenant_validation_builder_for_member_should_add_a_validator_for_the_member()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();

        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);
        /*
            * Given name rule config is a single regex requiring 2-50 chars starting with uppercase, no doubles spaces, dashes,apostrophes
            * Data hs name of John
        */
        var validator = builder.ForMember(c => c.GivenName).Build();
        var validated = await validator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task Tenant_validation_builder_for_nullable_member_should_add_a_validator_for_the_member()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();

        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        var validator = builder.ForNullableMember(c => c.NullableAge).Build();
        var validated = await validator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);

    }
    [Fact]
    public async Task Tenant_validation_builder_for_nullable_member_should_add_a_validator_for_the_member_amd_pass_validation_if_null()//nullable means optional
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();

        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        contact.NullableAge = null;

        var validator = builder.ForNullableMember(c => c.NullableAge).Build();
        var validated = await validator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);

    }

    [Fact]
    public async Task Tenant_validation_builder_for_nullable_string_member_should_be_able_to_add_a_validator_for_the_nullable_string_member()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();

        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);
        /*
            * Mobiles has a single regex requiring a UK formatted mobile number but data is set to 123456789, so should fail regex
        */
        var validator = builder.ForNullableStringMember(c => c.Mobile).Build();
        var validated = await validator(contact, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Mobile)}"
                                                            && i.FailureMessage == "Must be a valid UK mobile number format");
        }

    }

    [Fact]
    public async Task Tenant_validation_builder_for_nullable_string_member_should_be_able_to_add_a_validator_for_the_nullable_string_member_amd_pass_validation_if_null()//nullable means optional
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        contact.Mobile = null;

        var validator = builder.ForNullableStringMember(c => c.Mobile).Build();
        var validated = await validator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }


    [Fact]
    public async Task Tenant_validation_builder_for_nested_member_should_be_able_to_add_a_validator_for_the_nested_entity()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        var addressValidator = CreateTenantBuilder<AddressDto>(ruleConfigs)
                                    .ForMember(a => a.AddressLine)
                                        .ForMember(a => a.County)
                                            .ForNullableStringMember(a => a.Postcode)
                                                .ForMember(a => a.TownCity)
                                                    .Build();
        /*
            * The postcode is nullable but it does not make any difference for the test.
            * It has the value of "PostCode" which is not a valid UK formatted postcode so it fails and as such fails the overall validation 
        */

        var validator = builder.ForNestedMember<AddressDto>(c => c.Address!, addressValidator).Build();

        var validated = await validator(contact, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Address)}.{nameof(ContactDto.Address.Postcode)}"
                                                           && i.FailureMessage == "Must be a valid UK formatted postcode.");
        }

    }

    [Fact]
    public async Task Tenant_validation_builder_for_nullable_nested_member_should_be_able_to_add_a_validator_for_the_nested_entity()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        var addressValidator = CreateTenantBuilder<AddressDto>(ruleConfigs)
                                    .ForMember(a => a.AddressLine)
                                        .ForMember(a => a.County)
                                            .ForNullableStringMember(a => a.Postcode)
                                                .ForMember(a => a.TownCity)
                                                    .Build();
        /*
            * The postcode is nullable but has the value of "PostCode" which is not a valid UK formatted postcode so it fails and as such fails the overall validation 
        */

        var validator = builder.ForNullableNestedMember<AddressDto>(c => c.Address, addressValidator).Build();

        var validated = await validator(contact, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Address)}.{nameof(ContactDto.Address.Postcode)}"
                                                           && i.FailureMessage == "Must be a valid UK formatted postcode.");
        }

    }
    [Fact]
    public async Task Tenant_validation_builder_for_nullable_nested_member_should_be_able_to_add_a_validator_for_the_nested_entity_but_pass_if_nullable_is_null()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        var addressValidator = CreateTenantBuilder<AddressDto>(ruleConfigs)
                                    .ForMember(a => a.AddressLine)
                                        .ForMember(a => a.County)
                                            .ForNullableStringMember(a => a.Postcode)
                                                .ForMember(a => a.TownCity)
                                                    .Build();

        var validator = builder.ForNullableNestedMember<AddressDto>(c => c.Address, addressValidator).Build();

        contact.Address = null;

        var validated = await validator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);

    }


    [Fact]
    public async Task Tenant_validation_builder_for_comparison_with_member_should_add_a_validator_to_compare_members()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilderDOBCompare();//set to compare CompareDOB with equal to comparison
        var contact = StaticData.CreateContactObjectGraph();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        contact.DOB         = new DateOnly(1980, 6, 15);
        contact.CompareDOB  = new DateOnly(1980, 6, 15);

        var validator = builder.ForComparisonWith<DateOnly>(c => c.DOB).Build();
        var validated = await validator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true &&  v.Failures.Count == 0);
    }


    [Fact]
    public async Task Tenant_validation_builder_for_each_collection_member_should_add_a_validator_for_the_collection_entity_item()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var contact = StaticData.CreateContactObjectGraph();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);


        var methodValidator = CreateTenantBuilder<ContactMethodDto>(ruleConfigs)
                                    .ForMember(c => c.MethodType)
                                        .ForMember(c => c.MethodValue)
                                            .Build();

        var collectionValidator = builder.ForEachCollectionMember<ContactMethodDto>(c => c.ContactMethods, methodValidator).Build();

        var validated = await collectionValidator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count ==0);
    }


    [Fact]
    public async Task Tenant_validation_builder_for_each_collection_primitive_should_add_a_validator_for_the_collection_primitives()
    {
        var contact = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        contact.Entries = ["StringOne", "StringToLong"];

        var validator = builder.ForEachPrimitiveItem(c => c.Entries).Build();

        var validated = await validator(contact);

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Entries)}[1]" && i.PropertyName == nameof(ContactDto.Entries)
                                                           && i.FailureMessage == "Must be between 1 and 10 characters in length");
        }

    }

    [Fact]
    public async Task Tenant_validation_builder_for_collection_should_add_a_validator_for_the_collection_level()
    {
        var contact = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        contact.Entries = ["StringOne", "StringTwoLong"];//rule say between 1 and 5

        var validator = builder.ForCollection<IEnumerable>(c => c.Entries).Build();

        var validated = await validator(contact);

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);

    }
    [Fact]
    public async Task Tenant_validation_builder_for_collection_should_not_conflict_with_collection_Primitive_methods()
    {
        var contact = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        contact.Entries = ["StringOne", "StringTwo", "3", "4", "5", "BadIndexForCollectionAndBadStringLength"];//length rule for primitive is  between 1 and 10 chars, collection is between between 1 to 5


        var validator = builder.ForEachPrimitiveItem(c => c.Entries).ForCollection<IEnumerable>(c => c.Entries).Build();
        var validated = await validator(contact);

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Entries)}[5]" && i.PropertyName == nameof(ContactDto.Entries)
                                                            && i.FailureMessage =="Must be between 1 and 10 characters in length");


            validated.Failures[1].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Entries)}" && i.PropertyName == nameof(ContactDto.Entries)
                                                            && i.FailureMessage =="Must have at least 1 item but no more than 5");
        }



    }

    [Fact]
    public async Task Build_called_with_no_added_validators_should_return_a_valid_validated()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();//set to compare CompareDOB with equal to comparison
        var contact = StaticData.CreateContactObjectGraph();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        var validator = builder.Build();
        var validated = await validator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task Only_one_validator_per_member_should_be_added_to_the_builder_duplicated_should_be_ignored()
    {
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();//set to compare CompareDOB with equal to comparison
        var contact = StaticData.CreateContactObjectGraph();
        var builder = CreateTenantBuilder<ContactDto>(ruleConfigs);

        contact.GivenName = "J";

        var validator = builder.ForMember(c => c.GivenName).ForMember(c => c.GivenName).Build();
        var validated = await validator(contact, nameof(ContactDto));

        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);//there would be 2 if both validators were added
    }


    [Fact]
    public async Task For_recursive_entity_should_recurse_until_the_max_depth_is_reached_with_deep_graphs()
    {
        var nodeChain = StaticData.BuildNodeChain(10);

        ImmutableList<ValidationRuleConfig> ruleConfigs =
            [
                new(typeof(Node).FullName!, nameof(Node.Name),nameof(Node.Name), ValidatedConstants.RuleType_StringLength,"","","Should be between 3 and 10 characters in length",6,10)
            ];

        var builder     = CreateTenantBuilder<Node>(ruleConfigs);
        var builderTwo  = CreateTenantBuilder<Node>(ruleConfigs);
        var childNameValidator = builder.ForMember<string>(n => n.Name).Build();
        var validator = builderTwo.ForRecursiveEntity(n => n.Child!, childNameValidator).Build();

        var validated = await validator(nodeChain, "", new(new ValidationOptions { MaxRecursionDepth = 5 }));//5 + 1 max depth message

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<Node>>(v => v.IsValid == false && v.Failures.Count == 6);
            validated.Failures[5].FailureMessage.Should().Be(ErrorMessages.Validator_Max_Depth_Exceeded_User_Message);
        }
    }
}

    //    var nodeChain = StaticData.BuildNodeChain(100);


    //var childValidator = ValidationBuilder<Node>.Create()
    //                        .ForMember(n => n.Name, MemberValidators.CreateStringLengthValidator(6, 10, "Name", "Name", "Should be between 5 and 10 characters in length")).Build();

    //var validator = ValidationBuilder<Node>.Create()
    //                    .ForRecursiveEntity(c => c.Child, childValidator)
    //                        .Build();

    //var validated = await validator(nodeChain, "", new(new ValidationOptions { MaxRecursionDepth = 5 }));//5 + 1 max depth message