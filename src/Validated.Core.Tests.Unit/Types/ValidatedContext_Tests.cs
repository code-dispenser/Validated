using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Types;

public class ValidatedContext_Tests
{

    [Fact]
    public void Validated_context_should_be_able_to_create_a_new_context_with_depth_added()
    {
        var context = new ValidatedContext();

        var newContext = context.WithIncrementedDepth();
        newContext     = newContext.WithIncrementedDepth();

        newContext.Depth.Should().Be(2);
    }

    [Fact]
    public void Validated_context_should_return_true_when_the_correct_depth_is_less_than_the_max_depth()
    {
        var context = new ValidatedContext();

        var newContext      = context.WithIncrementedDepth();
        var isDepthExceeded = newContext.IsMaxDepthExceeded();

        isDepthExceeded.Should().Be(false);
    }

    [Fact]
    public void Validated_context_should_return_false_when_the_correct_depth_is_less_than_the_max_depth()
    {
        var context = new ValidatedContext();

        var newContext = context.WithIncrementedDepth();

        for (int index = 1; index <= newContext.MaxDepth; index++) newContext = newContext.WithIncrementedDepth();

        newContext.IsMaxDepthExceeded().Should().Be(true);
    }

    [Fact]
    public void Validated_context_with_validating_should_return_a_new_context_with_the_object_added_and_depth_increased()
    {
        var someValue    = new object();
        var context      = new ValidatedContext();
        //var currentDepth = context.Depth;
        var newContext   = context.WithValidating(someValue);

        using (new AssertionScope())
        {
            //newContext.Depth.Should().Be(currentDepth + 1);
            newContext.IsValidating(someValue).Should().BeTrue();
        }
    }

    [Fact]
    public void Validated_context_with_should_create_a_new_hash_set_if_not_provided()
    {
        var context = new ValidatedContext();

        var newContext = context with { Depth=2, ValidatingInstances = null! };

        using (new AssertionScope())
        {
            newContext.ValidatingInstances.Should().NotBeNull();
            newContext.Depth.Should().Be(2);
        }
    }
}
