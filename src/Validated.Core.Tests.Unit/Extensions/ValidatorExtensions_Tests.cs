using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Tracing;
using System.Security.Cryptography.X509Certificates;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Extensions;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Tests.SharedDataFixtures.Common.Validators;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.Tests.Unit.Extensions;

public class ValidatorExtensions_Tests
{

    public class ForEntityMember
    {

        [Fact]
        public async Task For_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<string>();
            var entityValidator = ValidatorExtensions.ForEntityMember<ContactDto, string>(memberValidator, c => c.Title);

            var validated = await entityValidator(null!);

            using(new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.Title) && i.DisplayName == nameof(ContactDto.Title)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }

        [Fact]
        public async Task For_entity_member_should_return_a_valid_validated_if_the_member_passes_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<string>();
            var entityValidator = ValidatorExtensions.ForEntityMember<ContactDto, string>(memberValidator, c => c.Title);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid && v.Failures.Count == 0);
        }

        [Fact]
        public async Task For_entity_member_should_return_an_invalid_validated_if_the_member_fails_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreateFailingMemberValidator<string>(nameof(ContactDto.Title), nameof(ContactDto.Title), "Failed validation");
            var entityValidator = ValidatorExtensions.ForEntityMember<ContactDto, string>(memberValidator, c => c.Title);

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Title)}" && i.DisplayName == nameof(ContactDto.Title)
                                                               && i.FailureMessage=="Failed validation" && i.Cause == CauseType.Validation);
            }
        }
    }


    public class ForNullableStringEntityMember
    {
        [Fact]
        public async Task For_nullable_string_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<string>();
            var entityValidator = ValidatorExtensions.ForNullableStringEntityMember<ContactDto>(memberValidator, c => c.Mobile);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.Mobile) && i.DisplayName == nameof(ContactDto.Mobile)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }
        [Fact]
        public async Task For_nullable_string_entity_member_should_return_a_valid_validated_if_the_member_passes_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<string>();
            var entityValidator = ValidatorExtensions.ForNullableStringEntityMember<ContactDto>(memberValidator, c => c.Mobile);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        [Fact]
        public async Task For_nullable_string_entity_member_should_return_a_valid_validated_if_the_member_to_validate_is_null_ie_its_optional()
        {
            var contact = StaticData.CreateContactObjectGraph() with { Mobile = null };
            var memberValidator = StubbedValidators.CreateFailingMemberValidator<string>(nameof(ContactDto.Mobile), nameof(ContactDto.Mobile), "Failed validations");
            var entityValidator = ValidatorExtensions.ForNullableStringEntityMember<ContactDto>(memberValidator, c => c.Mobile);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        [Fact]
        public async Task For_nullable_string_entity_member_should_return_an_invalid_validated_if_the_member_fails_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreateFailingMemberValidator<string>(nameof(ContactDto.Mobile), nameof(ContactDto.Mobile), "Failed validation");
            var entityValidator = ValidatorExtensions.ForNullableStringEntityMember<ContactDto>(memberValidator, c => c.Mobile);

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Mobile)}" && i.DisplayName == nameof(ContactDto.Mobile)
                                                               && i.FailureMessage=="Failed validation" && i.Cause == CauseType.Validation);
            }
        }
    }


    public class ForNullableEntityMember()
    {
        [Fact]
        public async Task For_nullable_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<int>();
            var entityValidator = ValidatorExtensions.ForNullableEntityMember<ContactDto, int>(memberValidator, c => c.NullableAge);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.NullableAge) && i.DisplayName == nameof(ContactDto.NullableAge)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }
        [Fact]
        public async Task For_nullable_entity_member_should_return_a_valid_validated_if_the_member_passes_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<int>();
            var entityValidator = ValidatorExtensions.ForNullableEntityMember<ContactDto, int>(memberValidator, c => c.NullableAge);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        [Fact]
        public async Task For_nullable_entity_member_should_return_a_valid_validated_if_the_value_to_validate_is_null_ie_its_optional()
        {
            var contact = StaticData.CreateContactObjectGraph() with { NullableAge = null };
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<int>();
            var entityValidator = ValidatorExtensions.ForNullableEntityMember<ContactDto, int>(memberValidator, c => c.NullableAge);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }

        [Fact]
        public async Task For_nullable_entity_member_should_return_an_invalid_validated_if_the_member_fails_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreateFailingMemberValidator<int>(nameof(ContactDto.NullableAge), nameof(ContactDto.NullableAge), "Failed validation");
            var entityValidator = ValidatorExtensions.ForNullableEntityMember<ContactDto, int>(memberValidator, c => c.NullableAge);

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.NullableAge)}" && i.DisplayName == nameof(ContactDto.NullableAge)
                                                               && i.FailureMessage == "Failed validation" && i.Cause == CauseType.Validation);
            }
        }
    }


    public class ForNestedEntityMember
    {
        [Fact]
        public async Task For_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var nestedValidator = StubbedValidators.CreatePassingEntityValidator<AddressDto>();
            var entityValidator = ValidatorExtensions.ForNestedEntityMember<ContactDto, AddressDto>(c => c.Address!, nestedValidator);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.Address) && i.DisplayName == nameof(ContactDto.Address)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }

        [Fact]
        public async Task For_nested_entity_member_should_return_a_valid_validated_if_everything_passes_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var nestedValidator = StubbedValidators.CreatePassingEntityValidator<AddressDto>();
            var entityValidator = ValidatorExtensions.ForNestedEntityMember<ContactDto, AddressDto>(c => c.Address!, nestedValidator);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }


        [Fact]
        public async Task For_nested_entity_member_should_return_an_invalid_validated_if_the_object_is_null()
        {
            var contact = StaticData.CreateContactObjectGraph() with { Address = null };
            var nestedValidator = StubbedValidators.CreatePassingEntityValidator<AddressDto>();
            var entityValidator = ValidatorExtensions.ForNestedEntityMember<ContactDto, AddressDto>(c => c.Address!, nestedValidator);

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Address)}" && i.DisplayName == nameof(ContactDto.Address)
                                                               && i.FailureMessage==$"{nameof(ContactDto.Address)} is required" && i.Cause == CauseType.Validation);
            }
        }

        [Fact]
        public async Task For_nested_entity_member_should_return_an_invalid_validated_if_anything_fails_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var nestedValidator = StubbedValidators.CreateFailingEntityValidator<AddressDto>(nameof(ContactDto.Address), nameof(ContactDto.Address), "Failed validation");
            var entityValidator = ValidatorExtensions.ForNestedEntityMember<ContactDto, AddressDto>(c => c.Address!, nestedValidator);

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Address)}" && i.DisplayName == nameof(ContactDto.Address)
                                                               && i.FailureMessage == "Failed validation" && i.Cause == CauseType.Validation);
            }
        }
    }


    public class ForNullableNestedEntityMember
    {
        [Fact]
        public async Task For_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var nestedValidator = StubbedValidators.CreatePassingEntityValidator<AddressDto>();
            var entityValidator = ValidatorExtensions.ForNullableNestedEntityMember<ContactDto, AddressDto>(c => c.Address, nestedValidator);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.Address) && i.DisplayName == nameof(ContactDto.Address)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }

        [Fact]
        public async Task For_nullable_nested_entity_member_should_return_a_valid_validated_if_everything_passes_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var nestedValidator = StubbedValidators.CreatePassingEntityValidator<AddressDto>();
            var entityValidator = ValidatorExtensions.ForNullableNestedEntityMember<ContactDto, AddressDto>(c => c.Address, nestedValidator);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }


        [Fact]
        public async Task For_nullable_nested_entity_member_should_return_a_valid_validated_if_the_object_is_null()//null means optional
        {
            var contact = StaticData.CreateContactObjectGraph() with { Address = null };
            var nestedValidator = StubbedValidators.CreateFailingEntityValidator<AddressDto>(nameof(ContactDto.Address), nameof(ContactDto.Address), "Failed validation");//if null no need to check as its optional
            var entityValidator = ValidatorExtensions.ForNullableNestedEntityMember<ContactDto, AddressDto>(c => c.Address!, nestedValidator);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }

        [Fact]
        public async Task For_nullable_nested_entity_member_should_return_an_invalid_validated_if_anything_fails_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var nestedValidator = StubbedValidators.CreateFailingEntityValidator<AddressDto>(nameof(ContactDto.Address), nameof(ContactDto.Address), "Failed validation");
            var entityValidator = ValidatorExtensions.ForNullableNestedEntityMember<ContactDto, AddressDto>(c => c.Address!, nestedValidator);

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Address)}" && i.DisplayName == nameof(ContactDto.Address)
                                                               && i.FailureMessage == "Failed validation" && i.Cause == CauseType.Validation);
            }
        }
    }


    public class ForCollectionEntityMember
    {
        [Fact]
        public async Task For_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var itemValidator = StubbedValidators.CreatePassingEntityValidator<ContactMethodDto>();
            var entityValidator = ValidatorExtensions.ForCollectionEntityMember<ContactDto, ContactMethodDto>(c => c.ContactMethods, itemValidator);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.ContactMethods) && i.DisplayName == nameof(ContactDto.ContactMethods)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }

        [Fact]
        public async Task For_collection_entity_member_should_return_a_valid_validated_if_everything_passes_validation()
        {
            var contact = StaticData.CreateContactObjectGraph() with { Address = null };
            var itemValidator = StubbedValidators.CreatePassingEntityValidator<ContactMethodDto>();
            var entityValidator = ValidatorExtensions.ForCollectionEntityMember<ContactDto, ContactMethodDto>(c => c.ContactMethods, itemValidator);

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }


        [Fact]
        public async Task For_collection_entity_member_should_return_an_invalid_validated_if_anything_fails_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var itemValidator = StubbedValidators.CreateFailingEntityValidator<ContactMethodDto>(nameof(ContactMethodDto.MethodType), nameof(ContactMethodDto.MethodType), "Failed validation");
            var entityValidator = ValidatorExtensions.ForCollectionEntityMember<ContactDto, ContactMethodDto>(c => c.ContactMethods, itemValidator);

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);//Two contact methods in contacts
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.ContactMethods)}[0]" && i.PropertyName == nameof(ContactMethodDto.MethodType)
                                                                && i.DisplayName == nameof(ContactMethodDto.MethodType) && i.FailureMessage=="Failed validation" && i.Cause == CauseType.Validation);

            }
        }

        [Fact]
        public async Task For_collection_entity_member_should_return_an_invalid_validated_if_the_collection_is_null()
        {
            var contact = StaticData.CreateContactObjectGraph() with { ContactMethods = null! };
            var itemValidator = StubbedValidators.CreatePassingEntityValidator<ContactMethodDto>();
            var entityValidator = ValidatorExtensions.ForCollectionEntityMember<ContactDto, ContactMethodDto>(c => c.ContactMethods, itemValidator);

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.ContactMethods)}" && i.PropertyName == nameof(ContactDto.ContactMethods)
                                                              &&  i.DisplayName == nameof(ContactDto.ContactMethods) && i.FailureMessage==$"{nameof(ContactDto.ContactMethods)} is required" && i.Cause == CauseType.Validation);
            }
        }
    }


    public class ToCompareEntityMember
    {
        [Fact]
        public async Task ToCompare_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<ContactDto>();
            var entityValidator = ValidatorExtensions.ToCompareEntityMember<ContactDto, DateOnly>(memberValidator, c => c.DOB);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.DOB) && i.DisplayName == nameof(ContactDto.DOB)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }

        [Fact]
        public async Task ToCompare_entity_member_should_return_a_valid_validated_if_the_comparison_passes_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<ContactDto>();

            var entityValidator = ValidatorExtensions.ToCompareEntityMember<ContactDto, DateOnly>(memberValidator, c => c.DOB);
            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }

        [Fact]
        public async Task ToCompare_entity_member_should_return_an_invalid_validated_if_the_comparison_fails_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreateFailingMemberValidator<ContactDto>(nameof(ContactDto.DOB), "Date of birth", "Comparison failed");

            var entityValidator = ValidatorExtensions.ToCompareEntityMember<ContactDto, DateOnly>(memberValidator, c => c.DOB);
            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.DOB)}" && i.PropertyName == nameof(ContactDto.DOB)
                                                              &&  i.DisplayName == "Date of birth" && i.FailureMessage=="Comparison failed" && i.Cause == CauseType.Validation);
            }
        }
    }


    public class ToCompareEntityValue
    {
        [Fact]
        public async Task To_compare_entity_value_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<DateOnly>();
            var entityValidator = ValidatorExtensions.ToCompareEntityValue<ContactDto, DateOnly>(memberValidator, c => c.DOB);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.DOB) && i.DisplayName == nameof(ContactDto.DOB)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }

        [Fact]
        public async Task ToCompare_entity_value_should_return_a_valid_validated_if_the_comparison_passes_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreatePassingMemberValidator<DateOnly>();

            var entityValidator = ValidatorExtensions.ToCompareEntityValue<ContactDto, DateOnly>(memberValidator, c => c.DOB);
            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }


        [Fact]
        public async Task ToCompare_entity_value_should_return_an_invalid_validated_if_the_comparison_fails_validation()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var memberValidator = StubbedValidators.CreateFailingMemberValidator<DateOnly>(nameof(ContactDto.DOB), "Date of birth", "Comparison failed");

            var entityValidator = ValidatorExtensions.ToCompareEntityValue<ContactDto, DateOnly>(memberValidator, c => c.DOB);
            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.DOB)}" && i.PropertyName == nameof(ContactDto.DOB)
                                                              &&  i.DisplayName == "Date of birth" && i.FailureMessage=="Comparison failed" && i.Cause == CauseType.Validation);
            }
        }
    }



    public class ForEachPrimitiveItem
    {
        [Fact]
        public async Task For_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var itemValidator = StubbedValidators.CreatePassingMemberValidator<string>();
            var entityValidator = ValidatorExtensions.ForEachPrimitiveItem<ContactDto, string>(c => c.Entries, itemValidator);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.Entries) && i.DisplayName == nameof(ContactDto.Entries)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }

        [Fact]
        public async Task For_each_primitive_item_should_return_an_invalid_validated_if_the_collection_is_null()
        {
            var contact = StaticData.CreateContactObjectGraph() with { ContactMethods = null! };
            var itemValidator = StubbedValidators.CreatePassingMemberValidator<string>();
            var entityValidator = ValidatorExtensions.ForEachPrimitiveItem<ContactDto, string>(c => c.Entries, itemValidator);

            contact.Entries = null!;

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Entries)}" && i.PropertyName == nameof(ContactDto.Entries)
                                                              &&  i.DisplayName == nameof(ContactDto.Entries) && i.FailureMessage==$"{nameof(ContactDto.Entries)} is required" && i.Cause == CauseType.Validation);
            }
        }

        [Fact]
        public async Task For_each_primitive_item_should_return_a_valid_validated_if_every_item_in_the_collection_is_valid()
        {
            var contact = StaticData.CreateContactObjectGraph() with { ContactMethods = null! };
            var itemValidator = StubbedValidators.CreatePassingMemberValidator<string>();
            var entityValidator = ValidatorExtensions.ForEachPrimitiveItem<ContactDto, string>(c => c.Entries, itemValidator);

            contact.Entries = ["NotChecked-PassingStubbedValidation"]!;

            var validated = await entityValidator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid && v.Failures.Count == 0);
        }

        [Fact]
        public async Task For_each_primitive_item_should_return_an_invalid_validated_if_any_item_the_collection_is_not_valid()
        {
            var contact = StaticData.CreateContactObjectGraph() with { ContactMethods = null! };
            var itemValidator = StubbedValidators.CreateFailingMemberValidator<string>(nameof(ContactDto.Entries), "Entries", "The entry is not valid");
            var entityValidator = ValidatorExtensions.ForEachPrimitiveItem<ContactDto, string>(c => c.Entries, itemValidator);

            contact.Entries = ["FailOne", "FailTwo"]!;

            var validated = await entityValidator(contact);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);
                validated.Failures[1].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Entries)}[1]" && i.PropertyName == nameof(ContactDto.Entries)
                                                              &&  i.DisplayName == nameof(ContactDto.Entries) && i.FailureMessage=="The entry is not valid" && i.Cause == CauseType.Validation);
            }
        }
    }

    public class ForCollection
    {
        [Fact]
        public async Task For_entity_member_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var itemValidator = StubbedValidators.CreatePassingMemberValidator<List<ContactMethodDto>>();
            var entityValidator = ValidatorExtensions.ForCollection<ContactDto, List<ContactMethodDto>>(c => c.ContactMethods, itemValidator);

            var validated = await entityValidator(null!);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.ContactMethods) && i.DisplayName == nameof(ContactDto.ContactMethods)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }
    }

    public class ForRecursiveNestedMember
    {
        [Fact]
        public async Task For_recursive_entity_should_return_an_invalid_validated_if_the_entity_is_null()
        {
            var nameValidator = MemberValidators.CreateStringLengthValidator(6, 10, "Name", "Name", "Should be between 5 and 10 characters in length");

            var nodeChain = StaticData.BuildNodeChain(10);

            var nodeNameValidator = ValidatorExtensions.ForEntityMember<Node, string>(nameValidator, n => n.Name);
            var validator = ValidatorExtensions.ForRecursiveEntity<Node>(c => c.Child!, nodeNameValidator);

            var validationOptions = new ValidationOptions { MaxRecursionDepth =5 };

            var validated = await validator(null!, "", new(validationOptions)); ;

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<Node>>(v => v.IsValid == false && v.Failures.Count == 2);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(Node) && i.PropertyName == nameof(Node.Name) && i.DisplayName == nameof(Node.Name)
                                                                && i.FailureMessage == ErrorMessages.Validator_Entity_Null_User_Message && i.Cause == CauseType.SystemError);
            }
        }

        [Fact]
        public async Task For_recursive_nested_member_should_recurse_until_validations_complete_or_max_depth_is_hit()
        {
            var nameValidator = MemberValidators.CreateStringLengthValidator(6, 10, "Name", "Name", "Should be between 5 and 10 characters in length");

            var nodeChain = StaticData.BuildNodeChain(10);

            var nodeNameValidator = ValidatorExtensions.ForEntityMember<Node, string>(nameValidator, n => n.Name);
            var validator         = ValidatorExtensions.ForRecursiveEntity<Node>(c => c.Child!, nodeNameValidator);

            var validationOptions = new ValidationOptions { MaxRecursionDepth =5 };

            var validated = await validator(nodeChain, "", new(validationOptions));

            validated.Should().Match<Validated<Node>>(v => v.IsValid == false && v.Failures.Count == 6);//5 + 1 max depth message
        }

        [Fact]
        public async Task For_recursive_nested_member_should_recurse_until_done_if_under_depth()
        {
            var nameValidator = MemberValidators.CreateStringLengthValidator(2, 10, "Name", "Name", "Should be between 5 and 10 characters in length");

            var nodeChain = StaticData.BuildNodeChain(10);

            var nodeNameValidator = ValidatorExtensions.ForEntityMember<Node, string>(nameValidator, n => n.Name);
            var validator = ValidatorExtensions.ForRecursiveEntity<Node>(c => c.Child!, nodeNameValidator);

            var validated = await validator(nodeChain);//default depth is 100;

            validated.Should().Match<Validated<Node>>(v => v.IsValid == true && v.Failures.Count == 0);
        }


        [Fact]
        public async Task Should_detect_and_exit_cyclic_refs_to_prevent_stack_overflows()
        {
            var ageValidator = MemberValidators.CreatePredicateValidator<int>(a => a >= 10, "Age", "Age", "Should be aged 10 or over");
            var nameValidator = MemberValidators.CreateStringLengthValidator(5, 10, "Name", "Name", "Should be between 5 and 10 characters");

            EntityValidator<Parent> parentValidator = null!;
            EntityValidator<Child> childValidator = null!;

            var childNameValidator = ValidatorExtensions.ForEntityMember<Child, string>(nameValidator, c => c.Name);
            var childAgeValidator = ValidatorExtensions.ForEntityMember<Child, int>(ageValidator, c => c.Age);

            Task<Validated<Child>> childParentValidatorLazy(Child child, string path = "", ValidatedContext? context = null, CancellationToken cancellationToken = default)
            {
                return ValidatorExtensions.ForNestedEntityMember<Child, Parent>(
                   c => c.Parent,
                   parentValidator // Captured by reference via closure
               )(child, path, context,CancellationToken.None);
            }

            childValidator = ValidatedExtensions.Combine(childNameValidator, childAgeValidator, childParentValidatorLazy);

            var parentNameValidator = ValidatorExtensions.ForEntityMember<Parent, string>(nameValidator, p => p.Name);
            var parentChildrenValidator = ValidatorExtensions.ForCollectionEntityMember<Parent, Child>(p => p.Children, childValidator);

            parentValidator = ValidatedExtensions.Combine(parentNameValidator, parentChildrenValidator);

            var parentChild = StaticData.BuildParentChildRelationships();

            var validated = await parentValidator(parentChild);

            validated.Should().Match<Validated<Parent>>(v => v.IsValid == false && v.Failures.Count == 12);

        }
    }

}