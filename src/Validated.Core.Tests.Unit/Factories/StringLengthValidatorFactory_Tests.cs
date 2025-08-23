using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class StringLengthValidatorFactory_Tests
{
    [Fact]
    public async Task Create_from_configuration_should_return_a_valid_validated_if_it_passes_validation()
    {
        var contact    = StaticData.CreateContactObjectGraph();//family name is Doe
        var ruleConfig = StaticData.ValidationRuleConfigForStringLengthValidator(nameof(ContactDto), nameof(ContactDto.FamilyName), "Surname", 2, 50);
        var logger     = new InMemoryLoggerFactory().CreateLogger<StringLengthValidatorFactory>();
        var validator  = new StringLengthValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated = await validator(contact.FamilyName, nameof(ContactDto));

        validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);
        
    }
    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_validation()
    {
        var contact    = StaticData.CreateContactObjectGraph();//family name is Doe
        var ruleConfig = StaticData.ValidationRuleConfigForStringLengthValidator(nameof(ContactDto), nameof(ContactDto.FamilyName), "Surname", 5, 50) with {FailureMessage = "Should be between 5 and 50 characters"};
        var logger     = new InMemoryLoggerFactory().CreateLogger<StringLengthValidatorFactory>();
        var validator  = new StringLengthValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated  = await validator(contact.FamilyName, nameof(ContactDto));

        using(new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.FamilyName) && i.DisplayName == "Surname"
                                                            && i.FailureMessage == "Should be between 5 and 50 characters" & i.Cause == CauseType.Validation);
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_value_is_null()
    {
        var contact     = StaticData.CreateContactObjectGraph();//family name is Doe
        var ruleConfig  = StaticData.ValidationRuleConfigForStringLengthValidator(nameof(ContactDto), nameof(ContactDto.FamilyName), "Surname", 5, 50) with { FailureMessage = "Should be between 5 and 50 characters" };
        var logger      = new InMemoryLoggerFactory().CreateLogger<StringLengthValidatorFactory>();
        var validator   = new StringLengthValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated   = await validator(null!, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.FamilyName) && i.DisplayName == "Surname"
                                                            && i.FailureMessage == "Should be between 5 and 50 characters" & i.Cause == CauseType.Validation);
        }
    }


    [Fact]
    public async Task Create_from_configuration_should_log_an_error_and_return_an_invalid_validated_if_an_exception_is_encountered()
    {
        var contact   = StaticData.CreateContactObjectGraph();//family name is Doe
        var logger    = new InMemoryLoggerFactory().CreateLogger<StringLengthValidatorFactory>();
        var validator = new StringLengthValidatorFactory(logger).CreateFromConfiguration<string>(null!);

        var validated = await validator(contact.FamilyName, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.SystemError);//<< Normally RuleConfigError but as everything is null Validated makes it a system error

            ((InMemoryLogger<StringLengthValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(StringLengthValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception_with_null_Log_entry_replacements()
    {
        var logger = new InMemoryLoggerFactory().CreateLogger<StringLengthValidatorFactory>();
        var validator = new StringLengthValidatorFactory(logger).CreateFromConfiguration<string>(null!);

        var validated = await validator(null!, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<StringLengthValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(StringLengthValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
        }
    }
    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_and_log_an_error_if_the_value_is_not_a_string()
    {
        var ruleConfig = StaticData.ValidationRuleConfigForStringLengthValidator(nameof(ContactDto), nameof(ContactDto.FamilyName), "Surname", 5, 50) with { FailureMessage = "Should be between 5 and 50 characters" };
        var logger = new InMemoryLoggerFactory().CreateLogger<StringLengthValidatorFactory>();
        var validator = new StringLengthValidatorFactory(logger).CreateFromConfiguration<int>(ruleConfig);


        var validated = await validator(42, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Cause.Should().Be(CauseType.RuleConfigError);

            ((InMemoryLogger<StringLengthValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(StringLengthValidatorFactory).FullName && l.Exception != null);
        }
    }


}
