using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class UrlFormatValidatorFactory_Tests
{

    private static MemberValidator<T> CreateConfiguredValidatorFactory<T>(string typeFullName, string propertyName, string displayName, string allowableSchemes, string failureMessage = "Should be a valid Url") where T: notnull
    {
        var ruleConfig = StaticData.ValidationRuleConfigForUrlFormatValidator(typeFullName, propertyName,displayName, allowableSchemes,failureMessage);
        var logger     = new InMemoryLoggerFactory().CreateLogger<UrlFormatValidatorFactory>();
        
        return new UrlFormatValidatorFactory(logger).CreateFromConfiguration<T>(ruleConfig);
    }
    [Fact]
    public async Task Create_from_configuration_should_return_a_valid_validated_if_a_string_value_it_passes_validation()
    {
        var contact   = StaticData.CreateContactObjectGraph();
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), nameof(ContactDto.StringUrl), "Url", ValidatedConstants.SchemeTypes_All);

        contact.StringUrl = "https://www.google.com";

        var validated = await validator(contact.StringUrl, nameof(ContactDto));

        validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);

    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_allowable_scheme_pattern_is_not_correct()
    {
        var contact   = StaticData.CreateContactObjectGraph();
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), nameof(ContactDto.StringUrl), "Url", "bad|scheme|patterns");

        contact.StringUrl = "https://www.google.com";

        var validated = await validator(contact.StringUrl, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be a valid Url" && i.Cause == CauseType.Validation);
        }
    }
    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_scheme_is_not_allowed_defined_by_the_pattern()
    {
        var contact   = StaticData.CreateContactObjectGraph();
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), nameof(ContactDto.StringUrl), "Url", ValidatedConstants.SchemeTypes_Https);

        contact.StringUrl = "http://www.google.com";

        var validated = await validator(contact.StringUrl, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be a valid Url" && i.Cause == CauseType.Validation);
        }
    }
    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_scheme_is_missing()
    {
        var contact   = StaticData.CreateContactObjectGraph();
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), nameof(ContactDto.StringUrl), "Url", ValidatedConstants.SchemeTypes_Https);
  
        contact.StringUrl = "www.google.com";

        var validated = await validator(contact.StringUrl, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be a valid Url" && i.Cause == CauseType.Validation);
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_with_a_cause_of_config_error_if_the_type_validated_is_not_a_string_or_uri()
    {
        var validator = CreateConfiguredValidatorFactory<int>(nameof(ContactDto), "Url", "Url", ValidatedConstants.SchemeTypes_All);
        var validated = await validator(42, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be a valid Url" && i.Cause == CauseType.RuleConfigError);
        }
    }
    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_with_a_null_uri()
    {
        var validator = CreateConfiguredValidatorFactory<Uri>(nameof(ContactDto), "Url", "Url", ValidatedConstants.SchemeTypes_All);
        var validated = await validator(null!, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<Uri>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be a valid Url" && i.Cause == CauseType.Validation);
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_scheme_is_bad()
    {
        var contact = StaticData.CreateContactObjectGraph();
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), nameof(ContactDto.StringUrl), "Url", ValidatedConstants.SchemeTypes_Https);

        contact.StringUrl = "bad://www.google.com";

        var validated = await validator(contact.StringUrl, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be a valid Url" && i.Cause == CauseType.Validation);
        }
    }
    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_host_is_missing()
    {
        var contact = StaticData.CreateContactObjectGraph();
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), nameof(ContactDto.StringUrl), "Url", ValidatedConstants.SchemeTypes_Ftps);

        contact.StringUrl = "ftps:123//www.google.com";

        var validated = await validator(contact.StringUrl, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be a valid Url" && i.Cause == CauseType.Validation);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData(null!)]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_the_allowable_scheme_is_empty_whitespace_or_null(string? allowableSchemes)
    {
        var contact = StaticData.CreateContactObjectGraph();
        var validator = CreateConfiguredValidatorFactory<string>(nameof(ContactDto), nameof(ContactDto.StringUrl), "Url", allowableSchemes!);

        contact.StringUrl = "https://www.google.com";

        var validated = await validator(contact.StringUrl, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.FailureMessage == "Should be a valid Url" && i.Cause == CauseType.Validation);
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_log_an_error_and_return_an_invalid_validated_if_an_exception_is_encountered()
    {
        var contact   = StaticData.CreateContactObjectGraph();

        var logger    = new InMemoryLoggerFactory().CreateLogger<UrlFormatValidatorFactory>();
        var validator = new UrlFormatValidatorFactory(logger).CreateFromConfiguration<string>(null!);

        contact.StringUrl = "https://www.google.com";

        var validated = await validator(contact.FamilyName, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Cause == CauseType.SystemError);//<< Normally RuleConfigError but as everything is null Validated makes it a system error

            ((InMemoryLogger<UrlFormatValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(UrlFormatValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_due_to_an_exception_with_null_Log_entry_replacements()
    {
        var logger    = new InMemoryLoggerFactory().CreateLogger<UrlFormatValidatorFactory>();
        var validator = new UrlFormatValidatorFactory(logger).CreateFromConfiguration<string>(null!);

        var validated = await validator(null!, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            ((InMemoryLogger<UrlFormatValidatorFactory>)logger).LogEntries[0]
                        .Should().Match<LogEntry>(l => l.Category == typeof(UrlFormatValidatorFactory).FullName && l.Exception != null && l.Message.Contains("[Null]"));
        }
    }
}
