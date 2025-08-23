using FluentAssertions;
using Validated.Core.Common.Utilities;
using Validated.Core.Extensions;

namespace Validated.Core.Tests.Unit.Extensions;

public class GeneralFunctional_Tests
{
    [Fact]
    public void Pipe_should_apply_a_function_to_the_value()
    
        => GeneralFunctional.Pipe(42, x => x * 2).Should().Be(84);

    [Fact]
    public void Pipe_should_apply_a_function_to_the_value_and_change_type()

        => GeneralFunctional.Pipe(42, x => (x * 2).ToString()).Should().Be("84");
}
