using FluentAssertions;
using FluentAssertions.Execution;
using System.Globalization;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class RangeValidatorFactory_Tests
{

    private static async Task RunRangeValidation<T>(T valueToValidate, string minValue, string maxValue, string minMaxTypeValue, bool shouldPass) where T : notnull
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRangeValidator("TypeFullName", "PropertyName", "DisplayName", minMaxTypeValue, minValue, maxValue) with { FailureMessage = "FailureMessage" };
        var logger = new InMemoryLoggerFactory().CreateLogger<RangeValidatorFactory>();
        var validator = new RangeValidatorFactory(logger).CreateFromConfiguration<T>(ruleConfig);

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
    [InlineData(15, "10", "20", true)]
    [InlineData(10, "10", "20", true)]
    [InlineData(20, "10", "20", true)]
    [InlineData(9, "10", "20", false)]
    [InlineData(21, "10", "20", false)]
    public async Task Create_from_configuration_should_validate_integers_returning_a_valid_or_invalid_validated_as_appropriate(int valueToValidate, string minValue, string maxValue, bool shouldPass)

        => await RunRangeValidation<int>(valueToValidate, minValue, maxValue, "MinMaxToValueType_Int32", shouldPass);


    [Theory]
    [InlineData("1990-01-01", "1980-01-01", "2020-01-01", true)]
    [InlineData("1980-01-01", "1980-01-01", "2020-01-01", true)]
    [InlineData("2020-01-01", "1980-01-01", "2020-01-01", true)]
    [InlineData("1979-12-31", "1980-01-01", "2020-01-01", false)]
    [InlineData("2020-01-02", "1980-01-01", "2020-01-01", false)]
    public async Task Create_from_configuration_should_validate_date_only_returning_a_valid_or_invalid_validated_as_appropriate(string valueToValidate, string minValue, string maxValue, bool shouldPass)

        => await RunRangeValidation<DateOnly>(DateOnly.Parse(valueToValidate), minValue, maxValue, "MinMaxToValueType_DateOnly", shouldPass);


    [Theory]
    [InlineData(15.55, "10.10", "20.20", true)]
    [InlineData(10.10, "10.10", "20.20", true)]
    [InlineData(20.20, "10.10", "20.20", true)]
    [InlineData(10.05, "10.10", "20.20", false)]
    [InlineData(20.25, "10.10", "20.20", false)]
    public async Task Create_from_configuration_should_validate_decimal_returning_a_valid_or_invalid_validated_as_appropriate(decimal valueToValidate, string minValue, string maxValue, bool shouldPass)

    => await RunRangeValidation<decimal>(valueToValidate, minValue, maxValue, "MinMaxToValueType_Decimal", shouldPass);


    [Theory]
    [InlineData("1990-01-01T01:01:01", "1980-01-01T10:10:10", "2020-01-01T20:20:20", true)]
    [InlineData("1980-01-01T10:10:10", "1980-01-01T10:10:10", "2020-01-01T20:20:20", true)]
    [InlineData("2020-01-01T20:20:20", "1980-01-01T10:10:10", "2020-01-01T20:20:20", true)]
    [InlineData("1979-12-31T01:01:01", "1980-01-01T10:10:10", "2020-01-01T20:20:20", false)]
    [InlineData("2020-01-12T20:20:21", "1980-01-01T10:10:10", "2020-01-01T20:20:20", false)]
    public async Task Create_from_configuration_should_validate_datetime_returning_a_valid_or_invalid_validated_as_appropriate(string valueToValidate, string minValue, string maxValue, bool shouldPass)

        => await RunRangeValidation<DateTime>(DateTime.ParseExact(valueToValidate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture), minValue, maxValue, "MinMaxToValueType_DateTime", shouldPass);


    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_value_is_null()
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRangeValidator("TypeFullName", "PropertyName", "DisplayName", ValidatedConstants.MinMaxToValueType_String, "10", "20") with { FailureMessage = "Should fail due to the null" };
        var logger     = new InMemoryLoggerFactory().CreateLogger<RangeValidatorFactory>();
        var validator  = new RangeValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated = await validator(null!, "Path");

        validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count ==1);
    }



    [Fact]
    public async Task Create_from_configuration_should_log_an_error_and_return_an_invalid_validated_if_an_exception_is_encountered()
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRangeValidator("TypeFullName", "PropertyName", "DisplayName", "MinMaxToValueType_DateOnly", "10", "20") with { FailureMessage = "Wrong conversion type" };
        var logger = new InMemoryLoggerFactory().CreateLogger<RangeValidatorFactory>();
        var validator = new RangeValidatorFactory(logger).CreateFromConfiguration<int>(ruleConfig);


        var validated = await validator(15, "TypeFullName");

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.RuleConfigError);
            ((InMemoryLogger<RangeValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(RangeValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception_with_null_Log_entry_replacements()
    {

        var logger = new InMemoryLoggerFactory().CreateLogger<RangeValidatorFactory>();
        var validator = new RangeValidatorFactory(logger).CreateFromConfiguration<int>(null!);

        var validated = await validator(42, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<RangeValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(RangeValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_handle_a_null_value_to_validate_and_a_bad_ruleConfig_during_exception_logging()
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRangeValidator("TypeFullName", "PropertyName", "DisplayName", "MinMaxToValueType_DateOnly", "10", "20") with { FailureMessage = "Wrong conversion type" };

        var logger    = new InMemoryLoggerFactory().CreateLogger<RangeValidatorFactory>();
        var validator = new RangeValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig!);

        var validated = await validator(null!, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<RangeValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(RangeValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
        }
    }

}