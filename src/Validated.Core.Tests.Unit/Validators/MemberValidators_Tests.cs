using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.Tests.Unit.Validators;

public class MemberValidators_Tests
{
    public class CreateRegexValidator
    {

        [Fact]
        public void Build_path_from_params_should_return_just_the_path_when_its_not_null_empty_or_whitespace()

            => MemberValidators.BuildPathFromParams("Path", "PropertyName").Should().Be("Path");

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null!)]
        public void Build_path_from_params_should_return_just_the_property_name_when_the_path_is_null_empty_or_whitespace(string? path)

            => MemberValidators.BuildPathFromParams(path!, "PropertyName").Should().Be("PropertyName");

        [Fact]
        public async Task Create_regex_validator_should_return_a_valid_validated_if_the_value_matches_the_pattern()
        {
            var validator = MemberValidators.CreateRegexValidator<int>("^[0-9]{2,}$", "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator(42, "Path");

            validated.Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        [Fact]
        public async Task Create_regex_validator_should_return_an_invalid_validated_when_the_value_fails_to_match_the_pattern()
        {
            var validator = MemberValidators.CreateRegexValidator<int>("^[A-Za-z]{2,}$", "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator(42);//no root object need for the path leave blank for the property name to be used

            using (new AssertionScope())
            {
                validator.Should().BeOfType<MemberValidator<int>>();
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "PropertyName" && i.PropertyName == "PropertyName" && i.DisplayName =="DisplayName" && i.FailureMessage=="FailureMessage");
            }
        }
        [Fact]
        public async Task Create_regex_validator_should_return_an_invalid_validated_when_the_value_is_null()
        {
            object valueToValidate = null!;

            var validator = MemberValidators.CreateRegexValidator<object>("^[A-Za-z]{2,}$", "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator(valueToValidate!, "Path");

            using (new AssertionScope())
            {
                validator.Should().BeOfType<MemberValidator<object>>();
                validated.Should().Match<Validated<object>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName =="DisplayName" && i.FailureMessage=="FailureMessage");
            }
        }
    }


    public class CreateNotNullOrEmptyValidator
    {

        [Theory]
        [InlineData("test")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Create_not_null_or_empty_validator_should_check_for_null_empty_or_whitespace(string? valueToValidate)
        {
            var validator = MemberValidators.CreateNotNullOrEmptyValidator<string>("PropertyName", "DisplayName", "FailureName");

            if (false == String.IsNullOrWhiteSpace(valueToValidate))
            {
                var validatedGood = (await validator(valueToValidate))
                                    .Should().Match<Validated<string>>(r => r.IsValid == true && r.Failures.Count == 0);

                return;
            }

            var validatedBad = (await validator(valueToValidate!)).Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);

        }

        [Fact]
        public async Task Create_not_null_or_empty_validator_should_return_a_valid_validated_for_value_types()
        {
            var validator = MemberValidators.CreateNotNullOrEmptyValidator<int>("PropertyName", "DisplayName", "FailureName");
            var validated = (await validator(1))
                                    .Should().Match<Validated<int>>(r => r.IsValid == true && r.Failures.Count == 0);
        }

        [Fact]
        public async Task Create_not_null_or_empty_validator_should_return_an_invalid_validated_for_empty_enumerable()
        {
            var validator = MemberValidators.CreateNotNullOrEmptyValidator<List<object>>("PropertyName", "DisplayName", "FailureName");
            var validated = (await validator([]))
                                .Should().Match<Validated<List<object>>>(r => r.IsValid == false && r.Failures.Count == 1);
        }

    }


    public class CreatePredicateValidator
    {

        [Fact]
        public async Task Create_predicate_validator_should_return_valid_when_predicate_returns_true()
        {

            var validator = MemberValidators.CreatePredicateValidator<string>(s => s.Length > 5, "PropertyName", "DisplayName", "Value must be longer than 5 characters");
            var validated = await validator("ValidString");

            validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);
        }

        [Fact]
        public async Task Create_predicate_validator_should_return_invalid_when_predicate_returns_false()
        {
            var validator = MemberValidators.CreatePredicateValidator<string>(s => s.Length > 10, "PropertyName", "DisplayName", "Value must be longer than 10 characters");
            var validated = await validator("Short");

            using (new AssertionScope())
            {
                validated.IsValid.Should().BeFalse();
                validated.Failures.Should().HaveCount(1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "PropertyName" && i.PropertyName == "PropertyName" &&  i.DisplayName == "DisplayName" && i.FailureMessage == "Value must be longer than 10 characters");
            }
        }
        [Fact]
        public async Task Create_predicate_validator_should_return_use_an_empty_string_for_the_value_in_failure_messages_replacements_if_null()
        {
            var validator = MemberValidators.CreatePredicateValidator<string>(s => s.Length > 10, "PropertyName", "DisplayName", "Value must be longer than 10 characters but the value was: {ValidatedValue}");
            var validated = await validator(null!);

            using (new AssertionScope())
            {
                validated.IsValid.Should().BeFalse();
                validated.Failures.Should().HaveCount(1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "PropertyName" && i.PropertyName == "PropertyName" &&  i.DisplayName == "DisplayName" && i.FailureMessage == "Value must be longer than 10 characters but the value was: ");
            }
        }
    }


    public class CreateRangeValidator
    {

        [Theory]
        [InlineData(5, -10, 10)]
        [InlineData(-10, -10, 10)]
        [InlineData(10, -10, 10)]
        public async Task Create_range_validator_should_return_a_valid_validated_for_values_within_range_inclusive(int value, int minValue, int maxValue)
        {
            var validator = MemberValidators.CreateRangeValidator(minValue, maxValue, "PropertyName", "DisplayName", "Should be within range");

            (await validator(value)).Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        [Theory]
        [InlineData(-11, -10, 10)]
        [InlineData(11, -10, 10)]
        public async Task Create_range_validator_should_return_an_invalid_validated_for_values_outside_the_range_inclusive(int value, int minValue, int maxValue)
        {
            var validator = MemberValidators.CreateRangeValidator(minValue, maxValue, "PropertyName", "DisplayName", "Should be within range");

            var validated = await validator(value, "Path");

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName" && i.FailureMessage == "Should be within range");
            }
        }

        [Fact]
        public async Task Create_range_validator_should_return_use_an_empty_string_for_the_value_in_failure_messages_replacements_if_null()
        {
            var validator = MemberValidators.CreateRangeValidator("1", "10", "PropertyName", "DisplayName", "Should be within range but found [{ValidatedValue}] which is not");
            var validated = await validator(null!);

            using (new AssertionScope())
            {
                validated.IsValid.Should().BeFalse();
                validated.Failures.Should().HaveCount(1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "PropertyName" && i.PropertyName == "PropertyName" &&  i.DisplayName == "DisplayName" && i.FailureMessage == "Should be within range but found [] which is not");
            }
        }
    }



    public class CreateStringLengthValidator
    {
        [Theory]
        [InlineData("between", 1, 10)]
        [InlineData("on maximum", 1, 10)]
        [InlineData("on minimum", 10, 15)]
        public async Task Create_string_length_validator_should_return_a_valid_validated_when_the_length_is_within_the_min_max_values_inclusive(string valueToValidate, int minLength, int maxLength)
        {
            var validator = MemberValidators.CreateStringLengthValidator(minLength, maxLength, "PropertyName", "DisplayName", "Outside of the min max lengths");
            var validated = await validator(valueToValidate);

            validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        [Theory]
        [InlineData("between", 1, 5)]
        [InlineData("outside maximum", 5, 10)]
        [InlineData("under minimum", 15, 30)]
        public async Task Create_string_length_validator_should_return_an_invalid_validated_when_the_length_is_outside_of_the_min_max_values(string valueToValidate, int minLength, int maxLength)
        {
            var validator = MemberValidators.CreateStringLengthValidator(minLength, maxLength, "PropertyName", "DisplayName", "Outside of the min max lengths");
            var validated = await validator(valueToValidate);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);

                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "PropertyName" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName" &&  i.FailureMessage == "Outside of the min max lengths");
            }
        }

        [Fact]
        public async Task Create_string_length_validator_should_return_an_invalid_validated_if_the_string_value_is_null()
        {
            var validator = MemberValidators.CreateStringLengthValidator(1, 10, "PropertyName", "DisplayName", "Outside of the min max lengths");
            var validated = await validator(null!, "Path");

            using (new AssertionScope())
            {

                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName =="DisplayName" && i.FailureMessage=="Outside of the min max lengths");
            }
        }
    }


    public class CreateStringRegexValidator
    {

        [Fact]
        public async Task Create_string_regex_validator_should_return_a_valid_validated_if_the_value_matches_the_pattern()
        {
            var validator = MemberValidators.CreateStringRegexValidator("^[0-9]{2,}$", "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator("42");

            validated.Should().Match<Validated<String>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        [Fact]
        public async Task Create_string_regex_validator_should_return_an_invalid_validated_when_the_value_fails_to_match_the_pattern()
        {
            var validator = MemberValidators.CreateStringRegexValidator("^[A-Za-z]{2,}$", "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator("42");

            using (new AssertionScope())
            {

                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "PropertyName" && i.PropertyName == "PropertyName" && i.DisplayName =="DisplayName" && i.FailureMessage=="FailureMessage");
            }
        }

        [Fact]
        public async Task Create_string_regex_validator_should_return_an_invalid_validated_when_the_value_is_null()
        {
            string valueToValidate = null!;

            var validator = MemberValidators.CreateStringRegexValidator("^[A-Za-z]{2,}$", "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator(valueToValidate!, "Path");

            using (new AssertionScope())
            {
                validator.Should().BeOfType<MemberValidator<string>>();
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName =="DisplayName" && i.FailureMessage=="FailureMessage");
            }
        }
    }


    public class CreateCollectionLengthValidator
    {

        [Fact]
        public async Task Create_collection_length_validator_should_return_an_invalid_validated_if_the_collection_is_null()
        {
            var validator = MemberValidators.CreateCollectionLengthValidator<List<int>>(1, 5, "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator(null!);

            validated.Should().Match<Validated<List<int>>>(v => v.IsValid == false && v.Failures.Count == 1);
        }
        [Fact]
        public async Task Create_collection_length_validator_should_return_an_invalid_validated_if_the_type_is_not_a_collection()
        {
            var validator = MemberValidators.CreateCollectionLengthValidator<int>(1, 5, "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator(42);

            validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
        }
        [Fact]
        public async Task Create_collection_length_validator_should_return_a_valid_validated_for_a_hash_set_if_length_is_valid()
        {
            var validator = MemberValidators.CreateCollectionLengthValidator<HashSet<int>>(1, 5, "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator([1, 2, 3]);

            validated.Should().Match<Validated<HashSet<int>>>(v => v.IsValid == true && v.Failures.Count == 0);
        }


        [Theory]
        [InlineData(1, 5, true)]
        [InlineData(0, 4, false)]
        [InlineData(6, 10, false)]
        [InlineData(5, 10, true)]
        [InlineData(-10, -5, false)]

        public async Task Create_collection_length_validator_should_return_a_valid_validated_if_valid(int minLength, int maxLength, bool shouldPass)
        {
            var validator = MemberValidators.CreateCollectionLengthValidator<List<int>>(minLength, maxLength, "PropertyName", "DisplayName", "FailureMessage");
            var validated = await validator([1, 2, 3, 4, 5]);

            if (true == shouldPass)
            {
                validated.Should().Match<Validated<List<int>>>(v => v.IsValid == true && v.Failures.Count == 0);
                return;
            }

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<List<int>>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "PropertyName" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName" && i.FailureMessage == "FailureMessage");
            }
        }
    }


    public class CreateCompareToValidator
    {
        [Theory]
        [InlineData(5, 5, CompareType.EqualTo)]
        [InlineData(6, 5, CompareType.NotEqualTo)]
        [InlineData(6, 5, CompareType.GreaterThan)]
        [InlineData(4, 5, CompareType.LessThan)]
        [InlineData(5, 5, CompareType.LessThanOrEqual)]
        [InlineData(5, 5, CompareType.GreaterThanOrEqual)]
        public async Task Create_compare_to_validator_should_return_a_valid_validated_for_a_correct_comparison_given_the_comparison_type(int valueToValidate, int compareTo, CompareType compareType)
        {
            var validator = MemberValidators.CreateCompareToValidator(compareTo, compareType, "PropertyName", "DisplayName", "Comparison failed");
            var validated = await validator(valueToValidate);

            validated.Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count == 0);
        }

        [Theory]
        [InlineData(4, 5, CompareType.EqualTo)]
        [InlineData(5, 5, CompareType.NotEqualTo)]
        [InlineData(4, 5, CompareType.GreaterThan)]
        [InlineData(6, 5, CompareType.LessThan)]
        [InlineData(6, 5, CompareType.LessThanOrEqual)]
        [InlineData(4, 5, CompareType.GreaterThanOrEqual)]
        public async Task Create_compare_to_validator_should_return_an_invalid_validated_for_an_incorrect_comparison_given_the_comparison_type(int valueToValidate, int compareTo, CompareType compareType)
        {
            var validator = MemberValidators.CreateCompareToValidator(compareTo, compareType, "PropertyName", "DisplayName", "Comparison failed");
            var validated = await validator(valueToValidate);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "PropertyName" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName" && i.FailureMessage == "Comparison failed");
            }
        }



        [Fact]
        public async Task Create_compare_to_validator_should_not_throw_exceptions_but_instead_return_an_invalid_validated_for_incorrect_compare_types()
        {
            var validator = MemberValidators.CreateCompareToValidator<string>("test", (CompareType)999, "PropertyName", "DisplayName", "Comparison failed");
            var validated = await validator("value", "Path");

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);

                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" &&  i.DisplayName == "DisplayName" && i.FailureMessage == "Comparison failed");
            }
        }


        [Fact]
        public async Task Non_comparable_types_should_return_an_invalid_validated()
        {
            int[] valueOne = [1, 2, 3];
            int[] valueTwo = [1, 2, 3];

            var validator = MemberValidators.CreateCompareToValidator<int[]>(valueOne, CompareType.EqualTo, "PropertyName", "DisplayName", "Comparison failed");
            var validated = await validator(valueTwo, "Path");

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int[]>>(v => v.IsValid == false && v.Failures.Count == 1);

                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" &&  i.DisplayName == "DisplayName" && i.FailureMessage == "Comparison failed");
            }
        }

        [Fact]
        public async Task If_two_nulls_are_compared_and_the_compare_type_is_equals_to_int_should_return_a_valid_validated()
        {
            string valueOne = null!, valueTwo = null!;

            var validator = MemberValidators.CreateCompareToValidator<string>(valueOne, CompareType.EqualTo, "PropertyName", "DisplayName", "Comparison failed");
            var validated = await validator(valueTwo, "Path");

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path=="Path" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName" && i.FailureMessage =="Comparison failed");
            }
        }

        [Fact]
        public async Task Create_compare_to_validator_should_return_an_invalid_validated_on_encountering_an_exception()
        {
            var throwingValue = new ThrowingComparable();

            var validator = MemberValidators.CreateCompareToValidator<ThrowingComparable>(throwingValue, CompareType.EqualTo, "PropertyName", "DisplayName", "Comparison failed");
            var validated = await validator(new ThrowingComparable(), nameof(ThrowingComparable));

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ThrowingComparable>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path==nameof(ThrowingComparable) && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName" && i.FailureMessage =="Comparison failed");

            }
        }
    }


    public class CreateMemberComparisonValidator
    {

        [Theory]
        [InlineData(CompareType.LessThan)]
        [InlineData(CompareType.LessThanOrEqual)]
        [InlineData(CompareType.NotEqualTo)]
        public async Task Create_member_comparison_validator_should_return_a_valid_validated_for_a_correct_comparison_given_the_comparison_type(CompareType compareType)
        {
            var dob = new DateOnly(1980, 1, 1);
            var olderDob = new DateOnly(1980, 1, 2);

            var contact = StaticData.CreateContactObjectGraph() with { DOB = dob, CompareDOB = olderDob };

            var validator = MemberValidators.CreateMemberComparisonValidator<ContactDto, DateOnly>(c => c.DOB, c => c.CompareDOB, compareType, "Date of birth", "Comparison failed");
            var validated = await validator(contact, nameof(ContactDto));

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }

        [Theory]
        [InlineData(CompareType.GreaterThan)]
        [InlineData(CompareType.GreaterThanOrEqual)]
        [InlineData(CompareType.EqualTo)]
        public async Task Create_member_comparison_validator_should_return_an_invalid_validated_for_a_correct_comparison_given_the_comparison_type(CompareType compareType)
        {
            var dob = new DateOnly(1980, 1, 1);
            var olderDob = new DateOnly(1980, 1, 2);

            var contact = StaticData.CreateContactObjectGraph() with { DOB = dob, CompareDOB = olderDob };

            var validator = MemberValidators.CreateMemberComparisonValidator<ContactDto, DateOnly>(c => c.DOB, c => c.CompareDOB, compareType, "Date of birth", "Comparison failed");
            var validated = await validator(contact, nameof(ContactDto));

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);

                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == "DOB" && i.DisplayName == "Date of birth" &&  i.FailureMessage == "Comparison failed");
            }

        }

        [Fact]
        public async Task Create_member_comparison_validator_should_return_a_valid_validated_for_an_equal_comparison()
        {
            var dob = new DateOnly(1980, 1, 1);
            var olderDob = new DateOnly(1980, 1, 1);

            var contact = StaticData.CreateContactObjectGraph() with { DOB = dob, CompareDOB = olderDob };

            var validator = MemberValidators.CreateMemberComparisonValidator<ContactDto, DateOnly>(c => c.DOB, c => c.CompareDOB, CompareType.EqualTo, "Date of birth", "Comparison failed");
            var validated = await validator(contact);

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
        }

        [Fact]
        public async Task Create_member_comparison_validator_should_return_an_invalid_validated_when_an_exception_is_encountered()
        {
            var validator = MemberValidators.CreateMemberComparisonValidator<ThrowingProperty, string>(x => x.GoodProperty, x => x.BadProperty, CompareType.EqualTo, "Good Property", "Comparison failed");
            var validated = await validator(new ThrowingProperty());

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ThrowingProperty>>(v => v.IsValid == false && v.Failures.Count == 1);

                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path =="GoodProperty" && i.PropertyName == "GoodProperty" && i.DisplayName == "Good Property" &&  i.FailureMessage == "Comparison failed");
            }
        }
    }

    public class CreateUrlValidator
    {
        [Theory]
        [InlineData(null, UrlSchemeTypes.Https)]
        [InlineData("badUrl", UrlSchemeTypes.Http)]
        public async Task Should_return_an_invalid_validated_when_value_is_null_or_url_try_parse_fails(string? valueEoValidate, UrlSchemeTypes allowableSchemes)
        {
            var validator = MemberValidators.CreateUrlValidator<string>(allowableSchemes, "Url", "Url", "Invalid format");

            var validated = await validator(valueEoValidate!, "Path");

            using(new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "Url" && i.DisplayName == "Url" && i.FailureMessage == "Invalid format");
            }
        }

        [Theory]
        [InlineData("http://www.google.com", UrlSchemeTypes.None)]
        [InlineData("ftps:123//www.google.com", UrlSchemeTypes.Http)]
        [InlineData("ppp://www.google.com", UrlSchemeTypes.Http)]
        public async Task Should_return_an_invalid_validated_when_the_url_scheme_is_not_allowed_or_allowable_scheme_is_none_or_the_host_is_empty(string? valueEoValidate, UrlSchemeTypes allowableSchemes)
        {
            var validator = MemberValidators.CreateUrlValidator<string>(allowableSchemes, "Url", "Url", "Invalid format");

            var validated = await validator(valueEoValidate!, "Path");

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "Url" && i.DisplayName == "Url" && i.FailureMessage == "Invalid format");
            }
        }
        [Theory]
        [InlineData("http://www.google.com", UrlSchemeTypes.Ftps | UrlSchemeTypes.Https, true)]
        [InlineData("http://www.google.com", UrlSchemeTypes.Ftps | UrlSchemeTypes.Http, false)]
        public async Task Should_return_an_invalid_validated_if_the_scheme_is_not_one_of_the_allowed_types(string valueToValidate, UrlSchemeTypes allowableSchemes, bool shouldFail)
        {
            var validator = MemberValidators.CreateUrlValidator<string>(allowableSchemes, "Url", "Url", "Invalid format");

            var validated = await validator(valueToValidate, "Path");

            if (true == shouldFail)
            {
                using (new AssertionScope())
                {
                    validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                    validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "Url" && i.DisplayName == "Url" && i.FailureMessage == "Invalid format");
                }
                return;
            }

            validated.GetValueOr("").Should().Be(valueToValidate);
        }

        public static IEnumerable<object[]> UriData

            =>
                [
                    new object[] { null!, UrlSchemeTypes.Https | UrlSchemeTypes.Ftps},
                    new object[] { new Uri("http://www.google.com"), UrlSchemeTypes.Https | UrlSchemeTypes.Ftps},
                ];

        [Theory]
        [MemberData(nameof(UriData))]
        public async Task Should_return_an_invalid_validated_when_uri_value_is_null_or_the_scheme_is_not_allowed(Uri? valueEoValidate, UrlSchemeTypes allowableSchemes)
        {
            var validator = MemberValidators.CreateUrlValidator<Uri>(allowableSchemes, "Url", "Url", "Invalid format");

            var validated = await validator(valueEoValidate!, "Path");

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<Uri>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "Url" && i.DisplayName == "Url" && i.FailureMessage == "Invalid format");
            }
        }
    }
}
