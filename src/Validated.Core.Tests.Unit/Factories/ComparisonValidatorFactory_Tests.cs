using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class ComparisonValidatorFactory_Tests
{
    public class EntityComparison
    {
        private static async Task RunCompareProperty<T>(T valueToValidate, string propertyName, string comparePropertyName, string displayName, string minMaxToTypeValue, string compareType, bool shouldPass) where T : notnull
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator(typeof(T).FullName!, propertyName, "Date of birth", ValidatedConstants.RuleType_MemberComparison, "", comparePropertyName, minMaxToTypeValue, compareType) with { FailureMessage = "FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.EntityObject).CreateFromConfiguration<T>(ruleConfig);

            var validated = await validator(valueToValidate, typeof(T).Name);

            if (true == shouldPass)
            {
                validated.Should().Match<Validated<T>>(v => v.IsValid == true && v.Failures.Count == 0);
            }
            else
            {
                using (new AssertionScope())
                {
                    validated.Should().Match<Validated<T>>(v => v.IsValid == false && v.Failures.Count == 1);
                    validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == typeof(T).Name! && i.PropertyName == propertyName && i.DisplayName == displayName
                                                                   && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);


                    //validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{typeof(T).Name}.{propertyName}" && i.PropertyName == propertyName && i.DisplayName == displayName
                    //                               && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);

                }
            }
        }


        [Theory]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData("1980-06-15", "1980-07-15", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData("1980-06-15", "1980-05-15", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData("1980-06-15", "1979-05-15", ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData("1980-06-15", "1981-07-15", ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData("1980-06-15", "1981-06-15", ValidatedConstants.CompareType_LessThan, true)]
        [InlineData("1980-06-15", "1979-06-15", ValidatedConstants.CompareType_LessThan, false)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-06-15", "1979-06-15", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-06-15", "1981-06-14", ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-06-15", "1981-06-14", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-06-15", "1979-06-16", ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_date_only_property_to_date_only_property_returning_a_valid_or_invalid_validated_as_appropriate(string dobValue, string compareDobValue, string compareType, bool shouldPass)
        {
            var contact = StaticData.CreateContactObjectGraph() with { DOB = DateOnly.Parse(dobValue), CompareDOB = DateOnly.Parse(compareDobValue) };
            await RunCompareProperty<ContactDto>(contact, "DOB", "CompareDOB", "Date of birth", "MinMaxToValueType_DateOnly", compareType, shouldPass);
        }


        [Fact]
        public async Task Test()
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator(typeof(ContactDto).FullName!, "Age", "Age", ValidatedConstants.RuleType_MemberComparison, "", "NullableAge",
                                    ValidatedConstants.MinMaxToValueType_Int32, ValidatedConstants.CompareType_LessThan) with
            { FailureMessage = "Should fail due to null comparison value" };

            var contact = StaticData.CreateContactObjectGraph() with { NullableAge = null };

            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.EntityObject).CreateFromConfiguration<ContactDto>(ruleConfig);

            var validated = await validator(contact, nameof(ContactDto));

            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
        }


        [Fact]
        public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception_with_null_Log_entry_replacements()
        {
            var contact = StaticData.CreateContactObjectGraph();
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.EntityObject).CreateFromConfiguration<ContactDto>(null!);

            var validated = await validator(contact, nameof(ContactDto));

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                ((InMemoryLogger<ComparisonValidatorFactory>)logger).LogEntries[0]
                            .Should().Match<LogEntry>(l => l.Category == typeof(ComparisonValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
            }
        }


        [Fact]
        public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_a_bad_compare_property_name()
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator(typeof(ContactDto).FullName!, "DOB", "Date of birth", ValidatedConstants.RuleType_MemberComparison, "",
                                                                                     "BADCompareDOB", ValidatedConstants.MinMaxToValueType_DateOnly, ValidatedConstants.CompareType_EqualTo) with
            { FailureMessage = "FailureMessage" };
            var contact = StaticData.CreateContactObjectGraph();
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.EntityObject).CreateFromConfiguration<ContactDto>(ruleConfig);

            var validated = await validator(contact, nameof(ContactDto));

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "FailureMessage" && i.Cause == CauseType.RuleConfigError);
            }
        }
    }

    public class ValueComparison
    {
        private static async Task RunCompareValue<T>(T valueToValidate, string compareValue, string comparePropertyName, string minMaxToTypeValue, string compareType, bool shouldPass) where T : notnull
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator("TypeFullName", "PropertyName", "DisplayName", ValidatedConstants.RuleType_CompareTo, compareValue, comparePropertyName, minMaxToTypeValue, compareType) with { FailureMessage = "FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.Value).CreateFromConfiguration<T>(ruleConfig);

            var validated = await validator(valueToValidate, "TypeFullName");

            if (true == shouldPass)
            {
                validated.Should().Match<Validated<T>>(v => v.IsValid == true && v.Failures.Count == 0);
            }
            else
            {
                using (new AssertionScope())
                {
                    validated.Should().Match<Validated<T>>(v => v.IsValid == false && v.Failures.Count == 1);
                    validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "TypeFullName" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName"
                                                                   && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);

                }
            }
        }

        [Theory]
        [InlineData(15, "15", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData(10, "15", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData(16, "15", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData(15, "15", ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData(20, "15", ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData(15, "15", ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData(10, "15", ValidatedConstants.CompareType_LessThan, true)]
        [InlineData(15, "15", ValidatedConstants.CompareType_LessThan, false)]
        [InlineData(15, "15", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData(16, "15", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData(14, "15", ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData(15, "15", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData(14, "15", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData(16, "15", ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_int_property_to_int_value_returning_a_valid_or_invalid_validated_as_appropriate(int valueToValidate, string compareValue, string compareType, bool shouldPass)

            => await RunCompareValue<int>(valueToValidate, compareValue, "", ValidatedConstants.MinMaxToValueType_Int32, compareType, shouldPass);

        [Theory]
        [InlineData(15.50, "15.5", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData(15.40, "15.5", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData(15.60, "15.5", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData(15.50, "15.5", ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData(20.50, "15.5", ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData(15.50, "15.5", ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData(10.50, "15.5", ValidatedConstants.CompareType_LessThan, true)]
        [InlineData(15.50, "15.5", ValidatedConstants.CompareType_LessThan, false)]
        [InlineData(15.50, "15.5", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData(16.50, "15.5", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData(14.50, "15.5", ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData(15.50, "15.5", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData(14.50, "15.5", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData(16.50, "15.5", ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_decimal_property_to_decimal_value_returning_a_valid_or_invalid_validated_as_appropriate(decimal valueToValidate, string compareValue, string compareType, bool shouldPass)

            => await RunCompareValue<decimal>(valueToValidate, compareValue, "", ValidatedConstants.MinMaxToValueType_Decimal, compareType, shouldPass);


        [Theory]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData("1980-06-16", "1980-06-15", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData("1980-05-15", "1980-06-15", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData("1981-06-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData("1979-06-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData("1980-06-14", "1980-06-15", ValidatedConstants.CompareType_LessThan, true)]
        [InlineData("1980-06-16", "1980-06-15", ValidatedConstants.CompareType_LessThan, false)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-07-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-05-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-05-15", "1980-06-15", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-07-15", "1980-06-15", ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_date_only_property_to_date_only_value_returning_a_valid_or_invalid_validated_as_appropriate(string valueToValidate, string compareValue, string compareType, bool shouldPass)

            => await RunCompareValue<DateOnly>(DateOnly.Parse(valueToValidate), compareValue, "", ValidatedConstants.MinMaxToValueType_DateOnly, compareType, shouldPass);

        [Theory]
        [InlineData("1980-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData("1980-06-16 12:12:13", "1980-06-15 12:12:12", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData("1980-05-15 12:13:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData("1980-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData("1981-06-15 13:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData("1979-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData("1980-06-14 11:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThan, true)]
        [InlineData("1980-06-16 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThan, false)]
        [InlineData("1980-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-07-15 12:12:13", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-05-15 12:12:11", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData("1980-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-05-14 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-07-16 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_date_time_property_to_date_time_value_returning_a_valid_or_invalid_validated_as_appropriate(string valueToValidate, string compareValue, string compareType, bool shouldPass)

            => await RunCompareValue<DateTime>(DateTime.Parse(valueToValidate), compareValue, "", ValidatedConstants.MinMaxToValueType_DateTime, compareType, shouldPass);


        [Theory]
        [InlineData("c6d3a183-f6ae-46d8-8476-acaf9f15b7aa", "67bb38ec-b10f-45a7-a572-baeb5e850219", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData("c6d3a183-f6ae-46d8-8476-acaf9f15b7aa", "c6d3a183-f6ae-46d8-8476-acaf9f15b7aa", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData("c6d3a183-f6ae-46d8-8476-acaf9f15b7aa", "67bb38ec-b10f-45a7-a572-baeb5e850219", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData("c6d3a183-f6ae-46d8-8476-acaf9f15b7aa", "c6d3a183-f6ae-46d8-8476-acaf9f15b7aa", ValidatedConstants.CompareType_NotEqualTo, false)]
        public async Task Create_from_configuration_should_validate_comparing_guid_property_to_guid_value_returning_a_valid_or_invalid_validated_as_appropriate(string valueToValidate, string compareValue, string compareType, bool shouldPass)

            => await RunCompareValue<Guid>(Guid.Parse(valueToValidate), compareValue, "", ValidatedConstants.MinMaxToValueType_Guid, compareType, shouldPass);



        [Theory]
        [InlineData("12:12:12", "12:12:12", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData("12:12:13", "12:12:12", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData("02:13:12", "12:12:12", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData("02:12:12", "02:12:12", ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData("13:12:12", "10:12:12", ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData("10:12:12", "13:12:12", ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData("10:12:12", "12:12:12", ValidatedConstants.CompareType_LessThan, true)]
        [InlineData("12:12:12", "10:12:12", ValidatedConstants.CompareType_LessThan, false)]
        [InlineData("12:12:12", "12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("12:12:13", "12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("12:12:11", "12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData("12:12:12", "12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("12:12:11", "12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("12:12:22", "12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_time_span_property_to_time_span_value_returning_a_valid_or_invalid_validated_as_appropriate(string valueToValidate, string compareValue, string compareType, bool shouldPass)

            => await RunCompareValue<TimeSpan>(TimeSpan.Parse(valueToValidate), compareValue, "", ValidatedConstants.MinMaxToValueType_TimeSpan, compareType, shouldPass);


        [Fact]
        public async Task Create_from_configuration_should_log_an_error_and_return_an_invalid_validated_if_an_exception_is_encountered()
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator("TypeFullName", "PropertyName", "DisplayName", ValidatedConstants.RuleType_CompareTo, "", "", "BAD_MINMAX_TYPE", ValidatedConstants.CompareType_EqualTo) with { FailureMessage="FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.Value).CreateFromConfiguration<string>(ruleConfig!);

            var validated = await validator("test", nameof(ContactDto));

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.RuleConfigError);

                ((InMemoryLogger<ComparisonValidatorFactory>)logger).LogEntries[0]
                            .Should().Match<LogEntry>(l => l.Category == typeof(ComparisonValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
            }
        }

        [Fact]
        public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception_with_null_Log_entry_replacements()
        {
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.Value).CreateFromConfiguration<string>(null!);

            var validated = await validator("test", nameof(ContactDto));

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                ((InMemoryLogger<ComparisonValidatorFactory>)logger).LogEntries[0]
                            .Should().Match<LogEntry>(l => l.Category == typeof(ComparisonValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
            }
        }


        [Fact]
        public async Task Create_from_configuration_should_log_an_error_and_return_an_invalid_validated_if_its_not_a_recognised_compare_type()
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator("TypeFullName", "PropertyName", "DisplayName", ValidatedConstants.RuleType_CompareTo, "", "", ValidatedConstants.MinMaxToValueType_String, "BAD_COMPARE_TYPE") with { FailureMessage="FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.Value).CreateFromConfiguration<string>(ruleConfig);

            var validated = await validator("test", nameof(ContactDto));

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.RuleConfigError);

                ((InMemoryLogger<ComparisonValidatorFactory>)logger).LogEntries[0]
                            .Should().Match<LogEntry>(l => l.Category == typeof(ComparisonValidatorFactory).FullName && l.Exception == null && l.Message.StartsWith("Configuration error"));
            }
        }


        [Theory]
        [InlineData(5, 5, ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData(5, 6, ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData(4, 5, ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData(5, 5, ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData(5, 5, ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        public void Perform_comparison_should_use_the_fallback_equality_comparison_for_non_comparable_types_equal_to(int valueOne, int valueTwo, string compareType, bool shouldPass)//Added for coverage not possible currently
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator("TypeFullName", "PropertyName", "DisplayName", ValidatedConstants.RuleType_CompareTo, "", "", ValidatedConstants.MinMaxToValueType_String, compareType) with { FailureMessage="FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var factory = new ComparisonValidatorFactory(logger, ComparisonTypeFor.Value);

            var resultTuple = factory.PerformComparison(new NonComparable(valueOne), new NonComparable(valueTwo), compareType, ruleConfig);

            if (true == shouldPass) resultTuple.Should().Be((true, CauseType.Validation));
            else resultTuple.Should().Be((false, CauseType.Validation));
        }


        [Theory]
        [InlineData(1, null)]
        [InlineData(null, 1)]
        [InlineData(null, null)]
        public void Perform_comparison_should_return_false_with_the_cause_of_validation_if_any_value_is_null(object? leftValue, object? rightValue)
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator("TypeFullName", "PropertyName", "DisplayName", ValidatedConstants.RuleType_CompareTo, "", "", ValidatedConstants.MinMaxToValueType_String, ValidatedConstants.CompareType_EqualTo);
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var factory = new ComparisonValidatorFactory(logger, ComparisonTypeFor.Value);

            var resultTuple = factory.PerformComparison(leftValue, rightValue, ValidatedConstants.CompareType_EqualTo, ruleConfig);

            resultTuple.Should().Be((false, CauseType.Validation));
        }

        [Fact]
        public void Perform_comparison_should_return_false_with_the_cause_of_validation_if_the_compare_type_is_null()
        {
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var factory = new ComparisonValidatorFactory(logger, ComparisonTypeFor.Value);

            var resultTuple = factory.PerformComparison("LeftValue", "RightValue", null!, null!);

            resultTuple.Should().Be((false, CauseType.RuleConfigError));
        }



        [Fact]
        public async Task Create_from_configuration_if_given_an_unknown_compare_type_for_enum_value_should_just_try_and_use_the_compare_value_method()
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator("TypeFullName", "PropertyName", "DisplayName", ValidatedConstants.RuleType_CompareTo, "42", "", ValidatedConstants.MinMaxToValueType_Int32, ValidatedConstants.CompareType_EqualTo) with { FailureMessage = "FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, (ComparisonTypeFor)999).CreateFromConfiguration<int>(ruleConfig);

            var validated = await validator(42, "Path");

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count==0);
                validated.GetValueOr(84).Should().Be(42);
            }
        }

        [Fact]
        public async Task Create_from_configuration_should_return_an_invalid_validated_and_create_a_log_entry_on_a_bad_min_max_to_type()
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator("TypeFullName", "GivenName", "DisplayName", ValidatedConstants.RuleType_CompareTo, "CompareValue", "", "BadMinMaxToType", ValidatedConstants.CompareType_EqualTo) with { FailureMessage="FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.Value).CreateFromConfiguration<string>(ruleConfig);

            var validated = await validator(null!, nameof(ContactDto));//using null for value to exercise valueToValidate?.ToString() ?? ""

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count==1);
                ((InMemoryLogger<ComparisonValidatorFactory>)logger).LogEntries[0]
                            .Should().Match<LogEntry>(l => l.Category == typeof(ComparisonValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
            }
        }
    }

    public class ValueObjectComparison
    {
        private static async Task RunCompareValueObject<T>(T valueToValidate, T compareValue, string compareType, bool shouldPass) where T : notnull
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValueObjectValidator("TypeFullName", "PropertyName", "DisplayName", compareType) with { FailureMessage = "FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.ValueObject).CreateFromConfiguration<T>(ruleConfig);

            var validated = await validator(valueToValidate, "TypeFullName", compareValue);

            if (true == shouldPass)
            {
                validated.Should().Match<Validated<T>>(v => v.IsValid == true && v.Failures.Count == 0);
            }
            else
            {
                using (new AssertionScope())
                {
                    validated.Should().Match<Validated<T>>(v => v.IsValid == false && v.Failures.Count == 1);
                    validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "TypeFullName" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName"
                                                                   && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);

                }
            }
        }


        [Theory]
        [InlineData(15, 15, ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData(10, 15, ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData(16, 15, ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData(15, 15, ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData(20, 15, ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData(15, 15, ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData(10, 15, ValidatedConstants.CompareType_LessThan, true)]
        [InlineData(15, 15, ValidatedConstants.CompareType_LessThan, false)]
        [InlineData(15, 15, ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData(16, 15, ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData(14, 15, ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData(15, 15, ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData(14, 15, ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData(16, 15, ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_int_property_to_int_value_returning_a_valid_or_invalid_validated_as_appropriate(int valueToValidate, int compareValue, string compareType, bool shouldPass)

            => await RunCompareValueObject<int>(valueToValidate, compareValue, compareType, shouldPass);

        [Theory]
        [InlineData(15.50, 15.50, ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData(15.40, 15.50, ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData(15.60, 15.50, ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData(15.50, 15.50, ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData(20.50, 15.50, ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData(15.50, 15.50, ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData(10.50, 15.50, ValidatedConstants.CompareType_LessThan, true)]
        [InlineData(15.50, 15.50, ValidatedConstants.CompareType_LessThan, false)]
        [InlineData(15.50, 15.50, ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData(16.50, 15.50, ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData(14.50, 15.50, ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData(15.50, 15.50, ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData(14.50, 15.50, ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData(16.50, 15.50, ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_decimal_property_to_decimal_value_returning_a_valid_or_invalid_validated_as_appropriate(decimal valueToValidate, decimal compareValue, string compareType, bool shouldPass)

            => await RunCompareValueObject<decimal>(valueToValidate, compareValue, compareType, shouldPass);


        [Theory]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData("1980-06-16", "1980-06-15", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData("1980-05-15", "1980-06-15", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData("1981-06-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData("1979-06-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData("1980-06-14", "1980-06-15", ValidatedConstants.CompareType_LessThan, true)]
        [InlineData("1980-06-16", "1980-06-15", ValidatedConstants.CompareType_LessThan, false)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-07-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-05-15", "1980-06-15", ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData("1980-06-15", "1980-06-15", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-05-15", "1980-06-15", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-07-15", "1980-06-15", ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_date_only_property_to_date_only_value_returning_a_valid_or_invalid_validated_as_appropriate(string valueToValidate, string compareValue, string compareType, bool shouldPass)

            => await RunCompareValueObject<DateOnly>(DateOnly.Parse(valueToValidate), DateOnly.Parse(compareValue), compareType, shouldPass);

        [Theory]
        [InlineData("1980-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_EqualTo, true)]
        [InlineData("1980-06-16 12:12:13", "1980-06-15 12:12:12", ValidatedConstants.CompareType_EqualTo, false)]
        [InlineData("1980-05-15 12:13:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_NotEqualTo, true)]
        [InlineData("1980-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_NotEqualTo, false)]
        [InlineData("1981-06-15 13:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThan, true)]
        [InlineData("1979-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThan, false)]
        [InlineData("1980-06-14 11:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThan, true)]
        [InlineData("1980-06-16 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThan, false)]
        [InlineData("1980-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-07-15 12:12:13", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, true)]
        [InlineData("1980-05-15 12:12:11", "1980-06-15 12:12:12", ValidatedConstants.CompareType_GreaterThanOrEqual, false)]
        [InlineData("1980-06-15 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-05-14 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, true)]
        [InlineData("1980-07-16 12:12:12", "1980-06-15 12:12:12", ValidatedConstants.CompareType_LessThanOrEqual, false)]
        public async Task Create_from_configuration_should_validate_comparing_date_time_property_to_date_time_value_returning_a_valid_or_invalid_validated_as_appropriate(string valueToValidate, string compareValue, string compareType, bool shouldPass)

            => await RunCompareValueObject<DateTime>(DateTime.Parse(valueToValidate), DateTime.Parse(compareValue), compareType, shouldPass);


        [Fact]
        public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception_with_null_Log_entry_replacements()
        {
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.ValueObject).CreateFromConfiguration<string>(null!);

            var validated = await validator("test", "ValueObject", "test");

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
                ((InMemoryLogger<ComparisonValidatorFactory>)logger).LogEntries[0]
                            .Should().Match<LogEntry>(l => l.Category == typeof(ComparisonValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
            }
        }


        [Fact]
        public async Task Create_from_configuration_should_return_an_invalid_validated_and_create_a_log_entry_on_a_null_compare_type()
        {
            var ruleConfig = StaticData.ValidationRuleConfigForComparisonValidator("TypeFullName", "GivenName", "DisplayName", ValidatedConstants.RuleType_VOComparison, "", "", ValidatedConstants.MinMaxToValueType_String, null!) with { FailureMessage="FailureMessage" };
            var logger = new InMemoryLoggerFactory().CreateLogger<ComparisonValidatorFactory>();
            var validator = new ComparisonValidatorFactory(logger, ComparisonTypeFor.ValueObject).CreateFromConfiguration<string>(ruleConfig);

            var validated = await validator(null!, nameof(ContactDto), null);// value to validate set to null to also exercise valueToValidate?.ToString() ?? "" for code coverage.

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count==1);
                ((InMemoryLogger<ComparisonValidatorFactory>)logger).LogEntries[0]
                            .Should().Match<LogEntry>(l => l.Category == typeof(ComparisonValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
            }
        }
    }
}
