using FluentAssertions;
using FluentAssertions.Execution;
using System.Linq.Expressions;
using Validated.Core.Common.Utilities;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Xunit.Sdk;

namespace Validated.Core.Tests.Unit.Common.Utilities;

public class GeneralUtils_Tests
{
    [Fact]
    public void The_get_member_name_should_return_the_member_name_for_a_valid_expression()

        => GeneralUtils.GetMemberName<ContactDto, string>(c => c.FamilyName)
                            .Should().Be("FamilyName");

    [Fact]
    public void The_get_member_name_should_return_a_fallback_value_for_an_invalid_expression()

        => GeneralUtils.GetMemberName<ContactDto, string>(c => c.FamilyName.ToString()).Should().StartWith("Unknown_");


    [Fact]
    public void The_build_full_path_should_combine_the_path_and_property_name_separated_by_a_dot()

        => GeneralUtils.BuildFullPath("Path", "PropertyName").Should().Be("Path.PropertyName");

    [Fact]
    public void The_build_full_path_should_only_return_the_property_name_if_the_path_is_null_empty_or_whitespace()

        => GeneralUtils.BuildFullPath("", "PropertyName").Should().Be("PropertyName");

    /*
        * Below add to exercise each switch expression case in the ExtractMemberName method.
    */

    [Fact]
    public void Extract_member_name_should_return_member_name_for_unary_expression_with_member_operand()
    {
        Expression expr = ((Expression<Func<ContactDto, object>>)(c => (object)c.GivenName)).Body;
        GeneralUtils.ExtractMemberName(expr).Should().Be("GivenName");
    }
    [Fact]
    public void Extract_member_name_should_return_Item_for_method_call_expression_get_Item()
    {
        Expression expression = ((Expression<Func<ContactDto, string>>)(c => c.ContactMethods[0].MethodType)).Body;

        if (expression is MemberExpression memberExpression && memberExpression.Expression is MethodCallExpression methodExpression)
        {
            GeneralUtils.ExtractMemberName(methodExpression).Should().Be("Item");
        }
        else
        {
            throw new XunitException("Should not be here");
        }
    }
    [Fact]
    public void Extract_member_name_should_return_parameter_name_for_parameter_expression()
    {

        ParameterExpression expression = Expression.Parameter(typeof(ContactDto), nameof(ContactDto));
        GeneralUtils.ExtractMemberName(expression).Should().Be(nameof(ContactDto));
    }

    [Fact]
    public void Extract_member_info_should_return_a_member_info_for_simple_expressions()
    {
        Expression<Func<ContactDto, int>> selectorExpression = c => c.Age;

        var memberInfo = GeneralUtils.ExtractMemberInfo(selectorExpression.Body);

        using(new AssertionScope())
        {
            memberInfo.Should().NotBeNull();
            memberInfo.Name.Should().Be("Age");
        }
    }
    [Fact]
    public void Extract_member_info_should_return_null_a_non_member_or_unary_expressions()
    {
        Expression<Func<ContactDto, int>> selectorExpression = c => c.Age;

        var memberInfo = GeneralUtils.ExtractMemberInfo(selectorExpression);

        memberInfo.Should().BeNull();
    }
    [Fact]
    public void Extract_member_info_should_return_a_member_info_for_unary_expressions()
    {
        Expression<Func<ContactDto, int>> selectorExpression = c => (int)c.NullableAge!;

        var memberInfo = GeneralUtils.ExtractMemberInfo(selectorExpression.Body);

        using (new AssertionScope())
        {
            memberInfo.Should().NotBeNull();
            memberInfo.Name.Should().Be("NullableAge"); // The member info should still point to the original member
        }
    }

    [Fact]
    public void Guard_against_deep_member_access_should_not_throw_if_the_expression_is_null()
    
        => FluentActions.Invoking(() => GeneralUtils.GuardAgainstDeepMemberAccess<ContactDto,string>(null)).Should().NotThrow();

    [Fact]
    public void Guard_against_deep_member_access_should_throw_if_the_expression_is_nested()

        => FluentActions.Invoking(() => GeneralUtils.GuardAgainstDeepMemberAccess<ContactDto, string>(c => c.Address!.AddressLine))
                            .Should().ThrowExactly<InvalidOperationException>();


}
