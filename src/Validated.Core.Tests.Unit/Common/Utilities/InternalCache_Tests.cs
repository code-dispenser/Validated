using FluentAssertions;
using FluentAssertions.Execution;
using System.Linq.Expressions;
using Validated.Core.Common.Utilities;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;

namespace Validated.Core.Tests.Unit.Common.Utilities;

[Collection("NonParallelCollection")]// Use collection to force xunit to run sequentially for cache count
[CollectionDefinition("NonParallelCollection", DisableParallelization = true)]
public class InternalCache_Tests
{

    public InternalCache_Tests()
    {
        InternalCache.ClearCache();
    }
    [Fact]
    public void Get_add_member_expression_should_return_a_compiled_expression_adding_it_to_cache_if_not_present()
    {
        var contactDto        = StaticData.CreateContactObjectGraph();
        contactDto.FamilyName = "Working";

        InternalCache.ClearCache();
        Expression<Func<ContactDto, string>> selectorExpression = c => c.FamilyName;
        var initialCacheCount = InternalCache.GetCacheItemCount("MemberSelector");

        var compiledSelector     = InternalCache.GetAddMemberExpression(selectorExpression);
        var compiledSelectorTwo  = InternalCache.GetAddMemberExpression(selectorExpression);

        var familyName = compiledSelector(contactDto);

        using(new AssertionScope())
        {
            initialCacheCount.Should().Be(0);
            compiledSelector.Should().BeEquivalentTo(compiledSelectorTwo);
            familyName.Should().Be("Working");
            InternalCache.GetCacheItemCount("MemberSelector").Should().Be(1);
        }
    }

    [Fact]
    public void Get_add_member_expression_should_return_a_compiled_expression_adding_it_to_cache_if_not_present_for_complex_expressions()
    {
        Expression<Func<ContactDto, object>> selectorExpression = c => new { c.Age, c.GivenName };

        InternalCache.ClearCache();
        var initialCacheCount = InternalCache.GetCacheItemCount("ExpressionString");

        var compiledSelector    = InternalCache.GetAddMemberExpression(selectorExpression);
        var compiledSelectorTwo = InternalCache.GetAddMemberExpression(selectorExpression);

        using (new AssertionScope())
        {
            initialCacheCount.Should().Be(0);
            compiledSelector.Should().BeEquivalentTo(compiledSelectorTwo);
            InternalCache.GetCacheItemCount("ExpressionString").Should().Be(1);
        }
    }



    [Fact]
    public void Get_add_member_name_should_return_a_member_name_adding_it_to_cache_if_not_present()
    {
        InternalCache.ClearCache();
        Expression<Func<ContactDto, string>> selectorExpression = c => c.FamilyName;

        var initialCacheCount = InternalCache.GetCacheItemCount("MemberName");
        var familyName        = InternalCache.GetAddMemberName(selectorExpression);
        var familyNameTwo     = InternalCache.GetAddMemberName(selectorExpression);

        using (new AssertionScope())
        {
            initialCacheCount.Should().Be(0);
            familyName.Should().BeEquivalentTo(familyNameTwo);
            familyName.Should().Be(nameof(ContactDto.FamilyName));
            InternalCache.GetCacheItemCount("MemberName").Should().Be(1);
        }
    }

    [Fact]
    public void Get_add_member_name_should_return_a_fallback_if_it_cant_be_extracted_and_should_not_be_added_to_cache()
    {
        InternalCache.ClearCache();
        Expression<Func<ContactDto, string>> selectorExpression = c => "wrong";

        var initialCacheCount = InternalCache.GetCacheItemCount("MemberName");
        var wrongName         = InternalCache.GetAddMemberName(selectorExpression);

        using (new AssertionScope())
        {
            initialCacheCount.Should().Be(0);
            wrongName.Should().StartWith("Unknown");
            InternalCache.GetCacheItemCount("MemberName").Should().Be(0);
        }
    }

    [Fact]
    public void Get_cache_item_count_should_be_zero_for_an_in_correct_type()//just for code coverage / testing purposes not actually needed.

        => InternalCache.GetCacheItemCount("IncorrectCacheType").Should().Be(0);

    
}
