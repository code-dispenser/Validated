using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;
using static System.Formats.Asn1.AsnWriter;

namespace Validated.Core.Tests.Unit.Factories;
public class PrecisionScaleValidatorFactory_Tests
{
    private static MemberValidator<T> CreateConfiguredValidatorFactory<T>(string typeFullName, string propertyName, string displayName, Dictionary<string, string>? additionalInfo = null, 
                                                                         string cultureID = ValidatedConstants.Default_CultureID,  string failureMessage = "Should be a valid decimal") where T : notnull
    {
        var ruleConfig = StaticData.ValidationRuleConfigForPrecisionScaleValidator(typeFullName, propertyName, displayName, failureMessage, additionalInfo, cultureID); ;
        var logger = new InMemoryLoggerFactory().CreateLogger<PrecisionScaleValidatorFactory>();

        return new PrecisionScaleValidatorFactory(logger).CreateFromConfiguration<T>(ruleConfig);
    }
    [Fact]
    public async Task Create_from_configuration_should_return_a_valid_validated_if_a_string_value_it_passes_validation()
    {
        Dictionary<string, string> additionalInfo = new() { [ValidatedConstants.RuleDictKey_Precision]="9", [ValidatedConstants.RuleDictKey_Scale]="4" };

        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), "Amount", "Amount", additionalInfo);

        var validated = await validator("1234.5679", nameof(ContactDto));

        validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "2")]
    [InlineData("9", "")]
    [InlineData("", "")]
    [InlineData("bad", "bad")]

    public async Task Create_from_configuration_should_return_an_invalid_validated_if_precision_or_scale_are_missing_or_not_integers(string? precision, string? scale)
    {
        Dictionary<string, string> additionalInfo = new() { [ValidatedConstants.RuleDictKey_Precision]=precision!, [ValidatedConstants.RuleDictKey_Scale]=scale! };

        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), "Amount", "Amount", additionalInfo);

        var validated = await validator("1234.56789", nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "ContactDto" && i.PropertyName == "Amount" && i.FailureMessage == "Should be a valid decimal" && i.Cause == CauseType.Validation);
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_a_invalid_validated_if_additional_info_is_null()
    {
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), "Amount", "Amount",null);

        var validated = await validator("1234.56789", nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "ContactDto" && i.PropertyName == "Amount" && i.FailureMessage == "Should be a valid decimal" && i.Cause == CauseType.Validation);
        }
    }

    [Theory]
    [InlineData("£123.45", "en-GB",true)]
    [InlineData("£123.45", null, true)]
    [InlineData("£123.45", "", true)]
    [InlineData("£123.45", "  ", true)]
    [InlineData("$123.45", "en-GB",false)]//using dollar symbol so as its not en-GB it should fail

    public async Task Create_from_configuration_should_use_the_en_gb_culture_if_the_culture_is_null_empty_or_whitespace(string amount, string? cultureID, bool shouldPass)
    {
        Dictionary<string, string> additionalInfo = new() { [ValidatedConstants.RuleDictKey_Precision]="9", [ValidatedConstants.RuleDictKey_Scale]="5" };
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), "Amount", "Amount",additionalInfo,cultureID!);

        var validated = await validator(amount, nameof(ContactDto));

        if (true == shouldPass)
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);
            return;
        }

        validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);

    }

    [Fact]
    public async Task Create_from_configuration_should_use_the_correct_culture_if_specified()
    {
        Dictionary<string, string> additionalInfo = new() { [ValidatedConstants.RuleDictKey_Precision]="9", [ValidatedConstants.RuleDictKey_Scale]="5" };
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), "Amount", "Amount", additionalInfo, "de-DE");

        var validated = await validator("1.230,45", nameof(ContactDto));//using German separators so dot and commas reversed.

        validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);

    }


    [Theory]
    [InlineData("23.455", 6, 3, true)]
    [InlineData("23.456000", 6, 3, false)]
    [InlineData("23.4561", 6, 3, false)]
    [InlineData("123.4561", 6, 3, false)]
    [InlineData("-123456789.123456789", 18, 9, true)]
    [InlineData("123456789123456789.1234", 22, 6, true)]
    [InlineData("123456789123456789.1234567899", 28, 12, true)]
    [InlineData("-123456789123456789.12345678999", 28, 14, false)]
    public async Task Should_return_a_valid_validated_if_the_decimal_value_Is_within_precision_and_scale_bounds(string valueToValidate, int precision, int scale, bool shouldPass)
    {
        Dictionary<string, string> additionalInfo = new() { [ValidatedConstants.RuleDictKey_Precision]=precision.ToString(), [ValidatedConstants.RuleDictKey_Scale]= scale.ToString()};
        var validator = CreateConfiguredValidatorFactory<decimal>(nameof(ContactDto), "Amount", "Amount", additionalInfo);

        decimal decimalValue = decimal.Parse(valueToValidate, CultureInfo.InvariantCulture);

        var validated = await validator(decimalValue, "Path");

        if (true == shouldPass)
        {
            validated.Should().Match<Validated<decimal>>(v => v.IsValid == true && v.Failures.Count == 0);
            return;
        }

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<decimal>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "Amount" && i.DisplayName == "Amount" && i.FailureMessage == "Should be a valid decimal");
        }

    }


    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_type_is_not_a_string_or_convertible_to_a_decimal()
    {
        Dictionary<string, string> additionalInfo = new() { [ValidatedConstants.RuleDictKey_Precision]="9", [ValidatedConstants.RuleDictKey_Scale]="5" };
        var validator = CreateConfiguredValidatorFactory<DateTime>(nameof(ContactDto), "Amount", "Amount", additionalInfo);

        var validated = await validator(DateTime.UtcNow, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<DateTime>>(v => v.IsValid == false && v.Failures.Count == 1);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "ContactDto" && i.PropertyName == "Amount" && i.FailureMessage == "Should be a valid decimal" && i.Cause == CauseType.Validation);
        }

    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_a_null_rule_config_with_null_Log_entry_replacements()
    {
        var logger    = new InMemoryLoggerFactory().CreateLogger<PrecisionScaleValidatorFactory>();
        var validator = new PrecisionScaleValidatorFactory(logger).CreateFromConfiguration<string>(null!);

        var validated = await validator("123.456", nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<PrecisionScaleValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(PrecisionScaleValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
        }
    }

    //[Fact]
    //public async Task Create_from_configuration_should_handle_exception_with_valid_ruleConfig_and_log_actual_values()
    //{
    //    var logger = new InMemoryLoggerFactory().CreateLogger<PrecisionScaleValidatorFactory>();
    //    Dictionary<string, string> additionalInfo = new() { [ValidatedConstants.RuleDictKey_Precision]="9", [ValidatedConstants.RuleDictKey_Scale]="5" };

    //    var ruleConfig = StaticData.ValidationRuleConfigForPrecisionScaleValidator(nameof(ContactDto), "Amount", "Amount", "Should be a valid decimal", additionalInfo, "invalid-culture-xyz");
    //    var validator  = new PrecisionScaleValidatorFactory(logger).CreateFromConfiguration<decimal>(ruleConfig);

    //    var validated = await validator(123.45M, nameof(ContactDto));

    //    using (new AssertionScope())
    //    {
    //        validated.Should().Match<Validated<decimal>>(v => v.IsValid == false && v.Failures.Count == 1);
    //        validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.RuleConfigError && i.FailureMessage == "Should be a valid decimal");

    //        var logEntry = ((InMemoryLogger<PrecisionScaleValidatorFactory>)logger).LogEntries[0];

    //        logEntry.Should().Match<LogEntry>(l => l.Category == typeof(PrecisionScaleValidatorFactory).FullName  && l.Exception != null && l.Message.Contains("Configuration error"));
    //    }
    //}


    [Fact]
    public async Task Create_from_configuration_should_handle_exception_with_null_valueToValidate_and_log_null_placeholder()
    {
        var logger = new InMemoryLoggerFactory().CreateLogger<PrecisionScaleValidatorFactory>();
        Dictionary<string, string> additionalInfo = new() { [ValidatedConstants.RuleDictKey_Precision]="9", [ValidatedConstants.RuleDictKey_Scale]="5" };

        var ruleConfig = StaticData.ValidationRuleConfigForPrecisionScaleValidator(nameof(ContactDto), "Amount", "Amount", "Should be a valid decimal", additionalInfo);
        var validator  = new PrecisionScaleValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);
        var validated  = await validator(null!, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.RuleConfigError);

            ((InMemoryLogger<PrecisionScaleValidatorFactory>)logger).LogEntries[0]
                .Should().Match<LogEntry>(l => l.Exception != null && l.Message.Contains("[Null]")); 
        }
    }
}
