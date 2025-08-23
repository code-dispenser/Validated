using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class RegexValidatorFactory_Tests
{

    [Fact]
    public async Task Create_from_configuration_should_return_a_valid_validated_if_it_passes_validation()
    {
        var contact     = StaticData.CreateContactObjectGraph();//family name is Doe
        var ruleConfig  = StaticData.ValidationRuleConfigForRegexValidator(nameof(ContactDto), nameof(ContactDto.FamilyName), "Surname", @"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", 2,50);
        var logger      = new InMemoryLoggerFactory().CreateLogger<RegexValidatorFactory>();
        var validator   = new RegexValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated  = await validator(contact.FamilyName, nameof(ContactDto));

        validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);

    }
    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_validation()
    {
        var contact    = StaticData.CreateContactObjectGraph();//family name is Doe
        var ruleConfig = StaticData.ValidationRuleConfigForRegexValidator(nameof(ContactDto), nameof(ContactDto.FamilyName), "Surname", @"^[A]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", 2, 50) with { FailureMessage= "Should start with an A" };
        var logger     = new InMemoryLoggerFactory().CreateLogger<RegexValidatorFactory>();
        var validator  = new RegexValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated  = await validator(contact.FamilyName, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.FamilyName) && i.DisplayName == "Surname"
                                                            && i.FailureMessage == "Should start with an A" & i.Cause == CauseType.Validation);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_value_is_null_empty_or_whitespace(string? valueToValidate)
    {

        var ruleConfig  = StaticData.ValidationRuleConfigForRegexValidator(nameof(ContactDto), nameof(ContactDto.FamilyName), "Surname", @"^[A]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", 2, 50) with { FailureMessage= "Should start with an A" };
        var logger      = new InMemoryLoggerFactory().CreateLogger<RegexValidatorFactory>();
        var validator   = new RegexValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated = await validator(valueToValidate!, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.FamilyName) && i.DisplayName == "Surname"
                                                            && i.FailureMessage == "Should start with an A" & i.Cause == CauseType.Validation);
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_log_an_error_and_return_an_invalid_validated_if_an_exception_is_encountered()
    {
        var contact     = StaticData.CreateContactObjectGraph();//family name is Doe
        var ruleConfig  = StaticData.ValidationRuleConfigForRegexValidator(nameof(ContactDto), nameof(ContactDto.FamilyName), "Surname", null!, 2, 50) with { FailureMessage= "Should start with an A" };
        var logger      = new InMemoryLoggerFactory().CreateLogger<RegexValidatorFactory>();
        var validator   = new RegexValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated = await validator(contact.FamilyName, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.RuleConfigError);
            ((InMemoryLogger<RegexValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(RegexValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception_with_null_Log_entry_replacements()
    {

        var logger    = new InMemoryLoggerFactory().CreateLogger<RegexValidatorFactory>();
        var validator = new RegexValidatorFactory(logger).CreateFromConfiguration<string>(null!);

        var validated = await validator("test", nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<RegexValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(RegexValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
        }
    }
}
