using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Types;
using Xunit.Sdk;
namespace Validated.Core.Tests.Unit.Types;

public class Validated_Tests
{
    [Fact]
    public void The_static_valid_method_should_create_a_valid_validated_when_the_value_is_not_null()

        => Validated<int>.Valid(42).Should().Match<Validated<int>>(v => v.IsValid == true && v.IsInvalid == false && v.Failures.Count == 0);

    [Fact]
    public void The_static_valid_method_should_not_create_a_valid_validated_when_the_value_is_null()

        => Validated<string>.Valid(null!).Should().Match<Validated<string>>(v => v.IsValid == false && v.IsInvalid == true && v.Failures.Count == 1);

    [Fact]
    public void The_static_valid_method_if_given_a_null_will_add_a_failure_to_its_collection()

        => Validated<string>.Valid(null!).Failures[0].Should()
                .Match<InvalidEntry>(f => f.Path=="value" && f.PropertyName == "value" && f.DisplayName== "value" && f.FailureMessage == "Value cannot be null.");

    [Fact]
    public void The_static_invalid_method_will_add_an_invalid_entry_to_its_list_and_mark_the_validated_as_invalid()
    {
        var invalidEntry = new InvalidEntry("Path", "PropertyName", "DisplayName", "FailureMessage");

        var validated = Validated<int>.Invalid(invalidEntry);

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<int>>(v => v.IsInvalid ==true && v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().BeEquivalentTo<InvalidEntry>(invalidEntry);
        }
    }
    [Fact]
    public void The_static_invalid_method_can_accept_an_array_of_invalid_entries()
    {
        var invalidEntryOne = new InvalidEntry("Path", "PropertyName", "DisplayName", "FailureMessage");
        var invalidEntryTwo = invalidEntryOne with { };

        var validated = Validated<int>.Invalid([invalidEntryOne, invalidEntryTwo]);

        validated.Failures.Count.Should().Be(2);
    }


    [Fact]
    public void The_static_invalid_method_if_given_a_null_will_produce_an_invalid_validated()
    {
        var validated = Validated<int>.Invalid(null!);

        using (new AssertionScope())
        {
            validated.Failures.Count.Should().Be(1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Unknown" && i.PropertyName == "Unknown" && i.DisplayName == "Unknown" && i.FailureMessage == "No validation failures provided.");
        }
    }
    [Fact]
    public void The_static_invalid_method_if_given_an_invalid_entry_with_empty_or_null_value_should_produce_an_invalid_validated()
    {
        var invalidEntry = new InvalidEntry("", "", "", "");

        var validated = Validated<int>.Invalid(invalidEntry);

        using (new AssertionScope())
        {
            validated.Failures.Count.Should().Be(1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Unknown" && i.PropertyName == "Unknown" && i.DisplayName == "Unknown" && i.FailureMessage == "No validation failures provided.");
        }
    }

    [Fact]
    public void The_get_value_or_method_if_valid_should_return_the_valid_value_otherwise_it_should_return_the_provided_value()

        => Validated<int>.Valid(42).GetValueOr(24).Should().Be(42);

    [Fact]
    public void The_get_value_or_method_if_invalid_should_return_provided_value()

        => Validated<int>.Invalid(new InvalidEntry("Path", "PropertyName", "DisplayName", "FailureMessage"))
                .GetValueOr(42).Should().Be(42);


    [Fact]
    public void The_match_method_for_a_valid_validated_should_apply_the_on_valid_function_to_the_valid_value_and_return_the_result()

        => Validated<int>.Valid(42).Match(_ => throw new XunitException("Should not be here"), onValid => onValid * 2).Should().Be(84);

    [Fact]
    public void The_match_method_for_an_invalid_validated_should_apply_the_invalid_function_to_the_invalid_values_when_invalid()
    {
        var invalidEntry = new InvalidEntry("Path", "PropertyName", "DisplayName", "FailureMessage");

        var invalidEntries = Validated<int>.Invalid(invalidEntry).Match(invalid => invalid.Append(invalidEntry), _ => throw new XunitException("Should not be here"));

        invalidEntries.Count().Should().Be(2);
    }

    [Fact]
    public void The_match_method_should_invoke_the_valid_action_when_validated_is_valid()
    {
        string actionTaken = String.Empty;

        Validated<int>.Valid(42).Match(_ => { }, valid => actionTaken = valid.ToString());

        actionTaken.Should().Be("42");
    }

    [Fact]
    public void The_match_method_should_invoke_the_invalid_action_when_validated_is_invalid()
    {
        string actionTaken = "42";

        Validated<int>.Invalid(new InvalidEntry("Path", "PropertyName", "DisplayName", "FailureMessage"))
                        .Match(failure => actionTaken = String.Empty, _ => { });

        actionTaken.Should().BeEmpty();
    }

    [Fact]
    public void The_map_method_should_apply_a_transformation_function_when_validated_is_valid()
    {
        Validated<int>.Valid(42).Map<string>(valid => (valid * 2).ToString())
            .GetValueOr("42").Should().Be("84");
    }

    [Fact]
    public void The_map_method_should_not_apply_a_transformation_and_return_the_invalid_validated_when_invalid()
    {
        var invalidEntry = new InvalidEntry("Path", "PropertyName", "DisplayName", "FailureMessage");

        var validated = Validated<int>.Invalid(invalidEntry)
                            .Map(valid => valid * 2);

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.IsInvalid == true && v.Failures.Count == 1);
            validated.Failures[0].Should().BeEquivalentTo<InvalidEntry>(invalidEntry);
        }
    }

    [Fact]
    public async Task The_map_method_should_apply_an_async__transformation_function_when_validated_is_valid()
    {
        var validated = await Validated<int>.Valid(42).Map<string>(valid => Task.FromResult((valid * 2).ToString()));

        validated.GetValueOr("42").Should().Be("84");

    }
    [Fact]
    public async Task The_map_method_should_not_apply_an_async_transformation_and_return_the_invalid_validated_when_invalid()
    {
        var invalidEntry = new InvalidEntry("Path", "PropertyName", "DisplayName", "FailureMessage");

        var validated = await Validated<int>.Invalid(invalidEntry).Map<string>(valid => Task.FromResult((valid * 2).ToString()));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.IsInvalid == true && v.Failures.Count == 1);

            validated.Failures[0].Should().BeEquivalentTo<InvalidEntry>(invalidEntry);
        }

    }

    [Fact]
    public void The_with_expression_should_create_a_copy()
    {
        var validated = Validated<int>.Valid(42);

        var validatedCopy = validated with { };

        validatedCopy.Should().BeEquivalentTo(validated);
    }


}









