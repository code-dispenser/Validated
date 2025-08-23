using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Types;

namespace Validated.Core.Tests.Integration.Factories;

public class ValidatorFactoryProvider_Tests
{
    [Fact]
    public async Task Validator_provider_factory_should_provide_a_rolling_date_validator_factory_with_a_working_get_today_function()
    {
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);
        var rulesConfig             = StaticData.ValidationRuleConfigForRollingDateValidator("TypeFullName", "PropertyName", "Date","-5","5");
        var rollingDateValidator    = validatorProviderFactory.GetValidatorFactory(ValidatedConstants.RuleType_RollingDate);

        var validator = rollingDateValidator.CreateFromConfiguration<DateOnly>(rulesConfig);

        var validated = await validator(DateOnly.FromDateTime(DateTime.Now.AddYears(-6)), "Path");

        using(new AssertionScope())
        {
            validated.Should().Match<Validated<DateOnly>>(v => v.IsValid == false && v.Failures.Count == 1);
        }
    }
}
