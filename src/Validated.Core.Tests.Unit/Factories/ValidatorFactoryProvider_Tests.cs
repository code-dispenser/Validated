using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Immutable;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Tests.SharedDataFixtures.Common.ValidatorFactory;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class ValidatorFactoryProvider_Tests
{
    [Theory]
    [InlineData(ValidatedConstants.RuleType_Range, typeof(RangeValidatorFactory))]
    [InlineData(ValidatedConstants.RuleType_Regex, typeof(RegexValidatorFactory))]
    [InlineData(ValidatedConstants.RuleType_StringLength, typeof(StringLengthValidatorFactory))]

    [InlineData(ValidatedConstants.RuleType_CompareTo, typeof(ComparisonValidatorFactory))]
    [InlineData(ValidatedConstants.RuleType_MemberComparison, typeof(ComparisonValidatorFactory))]
    [InlineData(ValidatedConstants.RuleType_VOComparison, typeof(ComparisonValidatorFactory))]

    [InlineData(ValidatedConstants.RuleType_RollingDate, typeof(RollingDateOnlyValidatorFactory))]
    [InlineData(ValidatedConstants.RuleType_NotFound, typeof(FailingValidatorFactory))]
    [InlineData("NOT_A_RULE_TYPE", typeof(FailingValidatorFactory))]

    public void Get_validator_factory_should_return_the_correct_factory_for_the_rule_type_or_the_failing_validator_factory_for_no_matching_rule_type(string ruleType, Type expectedFactoryType)
    {
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var factory = validatorProviderFactory.GetValidatorFactory(ruleType);

        factory.Should().BeOfType(expectedFactoryType);
      
    }

    [Fact]
    public void Should_be_able_to_add_validator_update_without_exception()
    {
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);
        var testValidatorFactory     = new ErrorThrowingValidatorFactory();

        validatorProviderFactory.AddOrUpdateFactory("NewRuleType", testValidatorFactory);

        var addedFactory = validatorProviderFactory.GetValidatorFactory("NewRuleType");

        addedFactory.Should().BeOfType<ErrorThrowingValidatorFactory>();

    }
    [Fact]
    public void Should_be_able_to_update_an_existing_validator_without_exception()
    {
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);
        var testValidatorFactory     = new ErrorThrowingValidatorFactory();

        var currentFactory = validatorProviderFactory.GetValidatorFactory(ValidatedConstants.RuleType_NotFound);

        validatorProviderFactory.AddOrUpdateFactory(ValidatedConstants.RuleType_NotFound, testValidatorFactory);
   
        var updatedFactory = validatorProviderFactory.GetValidatorFactory(ValidatedConstants.RuleType_NotFound);

        using(new AssertionScope())
        {
            currentFactory.Should().BeOfType<FailingValidatorFactory>();
            updatedFactory.Should().BeOfType<ErrorThrowingValidatorFactory>();
        }

    }


    [Theory]
    [InlineData("Smith", 0)]
    [InlineData("sm", 1)]
    [InlineData("s", 2)]

    public async Task Create_validator_should_build_a_composed_validator_comprised_of_one_or_more_validator_based_on_the_rule_config(string valueToValidate, int failureCount)
    {
        var ruleConfig               = StaticData.ValidationRuleConfigsForComposedBuildValidator(); //two validators, regex needs uppercase first character,string length needs min 2 chars (yes I know regex can do length)
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var validator = validatorProviderFactory.CreateValidator<string>(typeof(ContactDto).FullName!, nameof(ContactDto.FamilyName),ruleConfig);

        var validated = await validator(valueToValidate, nameof(ContactDto));

        if (failureCount == 0)
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == failureCount);
            return;
        }

        validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == failureCount);

    }

    [Fact]
    public async Task Create_validator_should_return_a_valid_validated_when_there_are_no_rules_but_with_a_log_entry_to_alert_of_issue()
    {
        var ruleConfig               = StaticData.ValidationRuleConfigsForComposedBuildValidator(); //(ContactDto FamilyName) two validators, regex needs uppercase first character,string length needs min 2 chars (yes I know regex can do length)
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var validator = validatorProviderFactory.CreateValidator<string>(typeof(ContactDto).FullName!, "Fake_Property", ruleConfig);
        var validated = await validator("Fake Value", nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == true && v.Failures.Count == 0);
            validated.GetValueOr("").Should().Be("Fake Value");

            var factoryLogger = inMemoryLoggerFactory.GetTestLogger(typeof(ValidatorFactoryProvider).FullName!)!;
            factoryLogger.LogEntries.Count.Should().Be(1);

        }
    }


    [Fact]
    public async Task Create_validator_should_return_an_invalid_validated_if_an_error_is_encountered()
    {
        ImmutableList<ValidationRuleConfig> ruleConfig = [new (typeof(ContactDto).FullName!,"PropertyName","DisplayName","Error_Throwing_Validator","","","FailureMessage",0,0)]; 
        var inMemoryLoggerFactory                      = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var errorThrowingValidatorFactory = new ErrorThrowingValidatorFactory();
        validatorProviderFactory.AddOrUpdateFactory("Error_Throwing_Validator", errorThrowingValidatorFactory);

        var validator = validatorProviderFactory.CreateValidator<string>(typeof(ContactDto).FullName! ,"PropertyName",ruleConfig);

        var validated = await validator("SomeValue",nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            var factoryLogger = inMemoryLoggerFactory.GetTestLogger(typeof(ValidatorFactoryProvider).FullName!)!;
            factoryLogger.LogEntries.Count.Should().Be(1);
        }
      

    }

    [Fact]
    public void Get_tenant_and_culture_configs_should_get_the_latest_version()
    {
        var baseConfig               = StaticData.BaseValidationRuleConfigsForGetConfigTests();
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var versionConfig = baseConfig with { Version = new ValidationVersion(1, 1, 1, DateTime.Now)}; 
        
        var ruleConfigs   = validatorProviderFactory.GetTenantAndCultureConfigs<int>("TypeFullName", "PropertyName", [baseConfig, versionConfig], "NoTenantID", "BadCulture");

        using (new AssertionScope())
        {
            ruleConfigs.Count.Should().Be(1);
            ruleConfigs[0].Should().Match<ValidationRuleConfig>(v => v.TenantID == ValidatedConstants.Default_TenantID && v.CultureID == ValidatedConstants.Default_CultureID && v.Version.ToString() == "1.1.1");
        }
    }

    [Fact]
    public void Get_tenant_and_culture_configs_should_get_the_config_that_matches_the_tenant_id_and_the_culture()
    {
        var baseConfig                  = StaticData.BaseValidationRuleConfigsForGetConfigTests();// has "TypeFullName", "PropertyName
        var inMemoryLoggerFactory       = new InMemoryLoggerFactory();
        var validatorProviderFactory    = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var tenantCultureConfig = baseConfig with { TenantID = "TenantID", CultureID = "CultureID" };

        var ruleConfigs = validatorProviderFactory.GetTenantAndCultureConfigs<string>("TypeFullName", "PropertyName", [baseConfig, tenantCultureConfig], "TenantID", "CultureID");

        using(new AssertionScope())
        {
            ruleConfigs.Count.Should().Be(1);
            ruleConfigs[0].Should().Match<ValidationRuleConfig>(v => v.TenantID == "TenantID" && v.CultureID == "CultureID");
        }
    }

    [Fact]
    public void Get_tenant_and_culture_configs_should_get_the_config_that_matches_the_tenant_but_with_the_default_culture_when_the_culture_does_not_match()
    {
        var baseConfig = StaticData.BaseValidationRuleConfigsForGetConfigTests();// has "TypeFullName", "PropertyName
        var inMemoryLoggerFactory = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var tenantCultureConfig = baseConfig with { TenantID = "TenantID" };

        var ruleConfigs = validatorProviderFactory.GetTenantAndCultureConfigs<decimal>("TypeFullName", "PropertyName", [baseConfig, tenantCultureConfig], "TenantID", "CultureID");

        using (new AssertionScope())
        {
            ruleConfigs.Count.Should().Be(1);
            ruleConfigs[0].Should().Match<ValidationRuleConfig>(v => v.TenantID == "TenantID" && v.CultureID == ValidatedConstants.Default_CultureID);
        }
    }

    [Fact]
    public void Get_tenant_and_culture_configs_should_get_the_default_configs_if_the_combo_of_default_tenant_and_specific_culture_do_not_match()
    {
        var baseConfig               = StaticData.BaseValidationRuleConfigsForGetConfigTests();// has "TypeFullName", "PropertyName
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var tenantCultureConfig = baseConfig with {TenantID = "", CultureID = "CultureID" };

        var ruleConfigs = validatorProviderFactory.GetTenantAndCultureConfigs<bool>("TypeFullName", "PropertyName", [baseConfig, tenantCultureConfig], "BadTenantID", "CultureID");

        using (new AssertionScope())
        {
            ruleConfigs.Count.Should().Be(1);
            ruleConfigs[0].Should().Match<ValidationRuleConfig>(v => v.TenantID == ValidatedConstants.Default_TenantID && v.CultureID == ValidatedConstants.Default_CultureID);
        }
    }

    [Theory]
    [InlineData("TenantID", "CultureID")]
    [InlineData("", "CultureID")]
    [InlineData("TenantID", "")]
    [InlineData("", "")]
    [InlineData("TenantID", null)]
    [InlineData(null, "CultureID")]
    [InlineData(null, null)]
    public void Get_tenant_and_culture_configs_should_get_the_configs_for_the_default_tenant_and_culture_if_there_are_no_match(string? tenantID, string? cultureID)
    {
        var baseConfig               = StaticData.BaseValidationRuleConfigsForGetConfigTests();// has "TypeFullName", "PropertyName
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var tenantCultureConfig = baseConfig with {TenantID="WrongTenantID", CultureID = "WrongCultureID" };

        var ruleConfigs = validatorProviderFactory.GetTenantAndCultureConfigs<string>("TypeFullName", "PropertyName", [baseConfig, tenantCultureConfig], tenantID!, cultureID!);

        using (new AssertionScope())
        {
            ruleConfigs.Count.Should().Be(1);
            ruleConfigs[0].Should().Match<ValidationRuleConfig>(v => v.TenantID == ValidatedConstants.Default_TenantID && v.CultureID == ValidatedConstants.Default_CultureID);
        }
    }

    [Fact]
    public void Get_tenant_and_culture_configs_should_return_an_empty_list_if_there_are_no_matched_on_the_config_defaults()
    {
        var baseConfig               = StaticData.BaseValidationRuleConfigsForGetConfigTests();// has "TypeFullName", "PropertyName
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var noDefaultsConfigs = baseConfig with { TenantID="SomeID", CultureID="en-GB" };

        var ruleConfigs = validatorProviderFactory.GetTenantAndCultureConfigs<byte>("TypeFullName", "PropertyName", [noDefaultsConfigs], "", "");
        
        ruleConfigs.Should().BeEmpty();
    }


}


