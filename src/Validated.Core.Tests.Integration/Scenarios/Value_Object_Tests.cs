using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;
using Validated.Core.Validators;

namespace Validated.Core.Tests.Integration.Scenarios;

public class Value_Object_Tests
{
    [Fact]
    public async Task Should_be_able_to_perform_static_comparisons_on_values_for_value_objects()
    {
        var minDate   = DateOnly.FromDateTime(new DateTime(2000, 5, 15));
        var maxDate   = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var startDate = DateOnly.FromDateTime(new DateTime(2000, 6, 15));
        var endDate   = DateOnly.FromDateTime(new DateTime(2000, 7, 15));

        var startDateValidator = MemberValidators.CreateCompareToValidator<DateOnly>(endDate, CompareType.LessThan, nameof(DateRange.StartDate), "Start date", "Should be less than the end date");
        var endDateValidator   = MemberValidators.CreateRangeValidator<DateOnly>(minDate, maxDate, nameof(DateRange.EndDate), "End date", $"Should be between {minDate} and {maxDate}");

        var validatedStartDate = await startDateValidator(startDate);
        var validatedEndDate   = await endDateValidator(endDate);

        var validatedDateRange = DateRange.Create(validatedStartDate, validatedEndDate);
        

        using(new AssertionScope())
        {
            validatedStartDate.IsValid.Should().BeTrue();
            validatedEndDate.IsValid.Should().BeTrue();

            validatedDateRange.Should().Match<Validated<DateRange>>(v => v.IsValid == true && v.Failures.Count == 0);
            validatedDateRange.GetValueOr(null!).StartDate.Should().Be(startDate);
        }
    }

    [Fact]
    public async Task Should_be_able_to_perform_a_value_comparison_against_the_value_in_the_rule_config()
    {
        var startDate                = DateOnly.FromDateTime(new DateTime(2000, 6, 15));
        var endDate                  = DateOnly.FromDateTime(new DateTime(2000, 7, 15));
        var ruleConfigs              = StaticData.ValidationRuleConfigsForValueObjectCompareTo();
        var inMemoryLoggerFactory    = new InMemoryLoggerFactory();
        var validatorProviderFactory = new ValidatorFactoryProvider(inMemoryLoggerFactory);

        var startDateValidator      = validatorProviderFactory.CreateValidator<DateOnly>(typeof(DateRange).FullName!, nameof(DateRange.StartDate), ruleConfigs); //uses compare to config entry
        var endDateValidator        = validatorProviderFactory.CreateValidator<DateOnly>(typeof(DateRange).FullName!,nameof(DateRange.EndDate), ruleConfigs);   // uses compare to other value

        var validatedStartDate = await startDateValidator(startDate);
        var validatedEndDate   = await endDateValidator(endDate,"",startDate);

        var validatedDateRange = DateRange.Create(validatedStartDate, validatedEndDate);

        using (new AssertionScope())
        {
            validatedStartDate.IsValid.Should().BeFalse();//rule is less than the config value of 2000-06-15 (but they are the same) 
            validatedEndDate.IsValid.Should().BeTrue();   //rule is set for the end date to be greater than the start date

            validatedDateRange.Should().Match<Validated<DateRange>>(v => v.IsValid == false && v.Failures.Count == 1);
            validatedDateRange.GetValueOr(null!).Should().Be(null);
        }

    }
}
