using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class RollingDateOnlyValidatorFactory_Tests
{
    private static async Task RollingDateOnlyValidation(DateOnly valueToValidate, string minValue, string maxValue, string timeUnit, bool shouldPass)
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRollingDateValidator("TypeFullName", "PropertyName", "DisplayName", minValue, maxValue) with { FailureMessage = "FailureMessage", MinMaxToValueType = timeUnit };
        var logger     = new InMemoryLoggerFactory().CreateLogger<Core.Factories.RollingDateOnlyValidatorFactory>();
        var validator  = new RollingDateOnlyValidatorFactory(() => DateOnly.FromDateTime(DateTime.Now), logger).CreateFromConfiguration<DateOnly>(ruleConfig);

        var validated = await validator(valueToValidate, "TypeFullName");

        if (true == shouldPass)
        {
            validated.Should().Match<Validated<DateOnly>>(v => v.IsValid == true && v.Failures.Count == 0);
        }
        else
        {
            using (new AssertionScope())
            {
                validated.Should().Match<Validated<DateOnly>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "TypeFullName" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName"
                                                               && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);

            }
        }
    }

    [Theory]
    [InlineData("2025-06-15", "-20", "20",ValidatedConstants.MinMaxToValueType_Year, true)]
    [InlineData("2005-06-15", "-20", "20", ValidatedConstants.MinMaxToValueType_Year, false)]
    [InlineData("2085-06-15", "-20", "20", ValidatedConstants.MinMaxToValueType_Year, false)]

    [InlineData("2025-06-15", "-200", "200", ValidatedConstants.MinMaxToValueType_Month, true)]
    [InlineData("2005-06-15", "-20", "200", ValidatedConstants.MinMaxToValueType_Month, false)]
    [InlineData("2085-06-15", "-200", "20", ValidatedConstants.MinMaxToValueType_Month, false)]
    
    [InlineData("2025-06-15", "-2000", "2000", ValidatedConstants.MinMaxToValueType_Day, true)]
    [InlineData("2005-06-15", "-200", "2000", ValidatedConstants.MinMaxToValueType_Day, false)]
    [InlineData("2085-06-15", "-200", "200", ValidatedConstants.MinMaxToValueType_Day, false)]
    public async Task Create_rolling_date_validator_should_return_a_valid_validated_if_it_passes_validation(string valueToValidate, string minValue, string maxValue,string minMaxToType, bool shouldPass)

        => await RollingDateOnlyValidation(DateOnly.Parse(valueToValidate),minValue,maxValue, minMaxToType, shouldPass);


    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception()//MinMaxToValueType should be int for AddYear
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRangeValidator("TypeFullName", "PropertyName", "DisplayName", "MinMaxToValueType_DateOnly", "-20", "20") with { FailureMessage = "Wrong conversion type" };
        var logger     = new InMemoryLoggerFactory().CreateLogger<Core.Factories.RollingDateOnlyValidatorFactory>();
        var validator  = new Core.Factories.RollingDateOnlyValidatorFactory(() => DateOnly.FromDateTime(DateTime.Now), logger).CreateFromConfiguration<DateOnly>(ruleConfig);

        var validated = await validator(DateOnly.FromDateTime(DateTime.Now), "TypeFullName");

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<DateOnly>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "TypeFullName" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName"
                                               && i.FailureMessage == "Wrong conversion type" && i.Cause == CauseType.RuleConfigError);
        }
    }


    [Fact]
    public async Task Create_from_configuration_should_log_an_error_and_return_an_invalid_validated_if_an_exception_is_encountered()
    {
        var ruleConfig  = StaticData.ValidationRuleConfigForRangeValidator("TypeFullName", "PropertyName", "DisplayName", "MinMaxToValueType_DateOnly", "10", "20") with { FailureMessage = "Wrong conversion type" };
        var logger      = new InMemoryLoggerFactory().CreateLogger<Core.Factories.RollingDateOnlyValidatorFactory>();
        var validator   = new Core.Factories.RollingDateOnlyValidatorFactory(() => DateOnly.FromDateTime(DateTime.Now), logger).CreateFromConfiguration<int>(ruleConfig);

        var validated = await validator(15, "TypeFullName");

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
            
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.RuleConfigError);

            ((InMemoryLogger<Core.Factories.RollingDateOnlyValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(Core.Factories.RollingDateOnlyValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception_with_null_Log_entry_replacements()
    {

        var logger    = new InMemoryLoggerFactory().CreateLogger<Core.Factories.RollingDateOnlyValidatorFactory>();
        var validator = new Core.Factories.RollingDateOnlyValidatorFactory(() => DateOnly.FromDateTime(DateTime.Now), logger).CreateFromConfiguration<DateOnly>(null!);

        var validated = await validator(DateOnly.FromDateTime(DateTime.Now), nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<DateOnly>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<Core.Factories.RollingDateOnlyValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(Core.Factories.RollingDateOnlyValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_handle_a_null_value_to_validate_and_a_bad_ruleConfig_during_exception_logging()
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRollingDateValidator("TypeFullName", "PropertyName", "DisplayName", "10", "20") with { FailureMessage = "Bad setup to force error" };

        var logger    = new InMemoryLoggerFactory().CreateLogger<RollingDateOnlyValidatorFactory>();
        var validator = new RollingDateOnlyValidatorFactory(() => DateOnly.FromDateTime(DateTime.Now), logger).CreateFromConfiguration<string>(ruleConfig!);

        var validated = await validator(null!, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<RollingDateOnlyValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(RollingDateOnlyValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_log_an_error_and_return_an_invalid_validated_if_the_the_value_is_not_a_date_only()
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRollingDateValidator("TypeFullName", "PropertyName", "DisplayName", "-10", "20") with { FailureMessage = "Should be between @MINDATE and @MAXDATE"};

        var logger    = new InMemoryLoggerFactory().CreateLogger<RollingDateOnlyValidatorFactory>();
        var validator = new RollingDateOnlyValidatorFactory(() => DateOnly.FromDateTime(DateTime.Now), logger).CreateFromConfiguration<string>(ruleConfig!);

        var validated = await validator("2000-01-01", nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<RollingDateOnlyValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(RollingDateOnlyValidatorFactory).FullName && l.Exception != null);
        }
    }
    [Fact]
    public async Task Create_from_configuration_should_use_replace_the_min_max_tokens_on_a_normal_validation_failure_if_present()
    {
        var ruleConfig = StaticData.ValidationRuleConfigForRollingDateValidator("TypeFullName", "PropertyName", "DisplayName", "-20", "60") with { FailureMessage = "Should be between {MinDate} and {MaxDate}" };

        var logger = new InMemoryLoggerFactory().CreateLogger<RollingDateOnlyValidatorFactory>();
        var validator = new RollingDateOnlyValidatorFactory(() => DateOnly.FromDateTime(DateTime.Now), logger).CreateFromConfiguration<DateOnly>(ruleConfig!);

        var validated = await validator(DateOnly.Parse("2000-01-01"), nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<DateOnly>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].FailureMessage.Should().NotContain("{MinDate}");
            validated.Failures[0].FailureMessage.Should().NotContain("{MaxDate}");
        }
    }
}
