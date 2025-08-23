using FluentAssertions;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class FailingValidatorFactory_Tests
{
    [Fact]
    public async Task Create_from_configuration_should_always_return_an_invalid_validated()
    {
        var ruleConfig = StaticData.ValidationRuleConfigForFailedValidator("TypeFullName", "PropertyName", "DisplayName", "Always Fail");
        var validator  = new FailingValidatorFactory().CreateFromConfiguration<string>(ruleConfig);

        var validated = await validator("test", "TypeFullName");

        validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count ==1);
    }
}
