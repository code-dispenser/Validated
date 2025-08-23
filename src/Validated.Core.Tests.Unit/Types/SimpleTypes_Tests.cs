using Validated.Core.Common.Constants;
using Validated.Core.Types;

using FluentAssertions;
using FluentAssertions.Execution;

namespace Validated.Core.Tests.Unit.Types;

public class SimpleTypes_Tests
{
    [Fact]
    public void Validation_rule_config_all_properties_just_be_settable_by_the_constructor()
    {
        var validationVersion = new ValidationVersion(1, 2, 3, DateTime.Now);
        var ruleConfig = new ValidationRuleConfig("TypeFullName", "PropertyName", "DisplayName", "RuleType", "MinMaxToValueType_Int32", "Pattern", "FailureMessage", 1, 10, "1", "10",
                                             "CompareValue", "ComparePropertyName", "CompareType_EqualTo",ValidatedConstants.TargetType_Item, "TenantID_ALL", "CultureID_en-GB", [], validationVersion);


        ruleConfig.Should().Match<ValidationRuleConfig>(r => r.TypeFullName == "TypeFullName" &&  r.PropertyName == "PropertyName" && r.DisplayName =="DisplayName" && r.RuleType == "RuleType" && r.MinMaxToValueType == "MinMaxToValueType_Int32"
                                                      && r.Pattern =="Pattern" && r.FailureMessage == "FailureMessage" && r.MinLength == 1 && r.MaxLength == 10 && r.MinValue == "1" && r.MaxValue == "10"
                                                      && r.CompareValue == "CompareValue" && r.ComparePropertyName == "ComparePropertyName" && r.CompareType == "CompareType_EqualTo" && r.TargetType == ValidatedConstants.TargetType_Item
                                                      && r.TenantID == "TenantID_ALL" && r.CultureID == "CultureID_en-GB" && r.AdditionalInfo!.Count == 0 && r.Version == validationVersion);

    }

    [Fact]
    public void Validation_rule_config_with_expression_can_set_all_properties()//Keep coverage happy as otherwise Set property is not covered.
    {

        var validationVersion = new ValidationVersion(1, 2, 3, DateTime.Now);
        var ruleConfig = new ValidationRuleConfig("TypeFullName", "PropertyName", "DisplayName", "RuleType", "MinMaxToValueType_Int32", "Pattern", "FailureMessage", 1, 10, "1", "10",
                                             "CompareValue", "ComparePropertyName", "CompareType_EqualTo",ValidatedConstants.TargetType_Item, ValidatedConstants.Default_TenantID, ValidatedConstants.Default_CultureID, [], validationVersion);

        var ruleConfigCopy = ruleConfig with
        {
            TypeFullName="TypeFullName",
            PropertyName="PropertyName",
            DisplayName="DisplayName",
            RuleType="RuleType",
            MinMaxToValueType = "MinMaxToValueType_Int32",
            Pattern ="Pattern",
            FailureMessage ="FailureMessage",
            MinLength=1,
            MaxLength=10,
            MinValue="1",
            MaxValue="10",
            CompareValue="CompareValue",
            ComparePropertyName="ComparePropertyName",
            CompareType="CompareType_EqualTo",
            TargetType = ValidatedConstants.TargetType_Item,
            TenantID=ValidatedConstants.Default_TenantID,
            CultureID=ValidatedConstants.Default_CultureID,
            AdditionalInfo = [],
            Version = validationVersion
        };

        ruleConfigCopy.Should().BeEquivalentTo<ValidationRuleConfig>(ruleConfig);

    }

    [Fact]
    public void Validated_version_to_string_should_output_only_the_major_minor_and_patch_values_separated_by_dots()
    {
        var validationVersion = new ValidationVersion(1, 2, 3, DateTime.Now);

        var toString = validationVersion.ToString();

        toString.Should().Be("1.2.3");
    }

    [Theory]
    [InlineData(1, 1, 1, 2, 1, 1)]
    [InlineData(1, 1, 1, 1, 2, 1)]
    [InlineData(1, 1, 1, 1, 1, 2)]
    public void Validated_version_compare_to_for_sorting_should_be_ordered_by_major_minor_and_then_patch(int majorOne, int minorOne, int patchOne, int majorTwo, int minorTwo, int patchTwo)
    {
        var version = new ValidationVersion(majorOne, minorOne, patchOne, new DateTime(2025, 1, 1));
        var versionToWinSort = new ValidationVersion(majorTwo, minorTwo, patchTwo, new DateTime(2025, 1, 2));

        using (new AssertionScope())
        {
            version.CompareTo(versionToWinSort).Should().Be(-1);
            versionToWinSort.CompareTo(version).Should().Be(1);
        }

    }
    [Theory]
    [InlineData(2, 1, 2, 2, 1, 2)]
    public void Validated_version_compare_to_for_sorting_should_be_ordered_by_major_minor_and_then_patch_date_time_for_tie_break(int majorOne, int minorOne, int patchOne, int majorTwo, int minorTwo, int patchTwo)
    {
        var version = new ValidationVersion(majorOne, minorOne, patchOne, new DateTime(2025, 1, 1));
        var versionToWinSort = new ValidationVersion(majorTwo, minorTwo, patchTwo, new DateTime(2025, 1, 2));

        using (new AssertionScope())
        {
            version.CompareTo(versionToWinSort).Should().Be(-1);
            versionToWinSort.CompareTo(version).Should().Be(1);
        }
    }

    [Fact]
    public void Invalid_entry_all_properties_should_be_settable_by_the_constructor()

        => new InvalidEntry("FailureMessage", "Path", "PropertyName", "DisplayName",  CauseType.SystemError)
                .Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName" && i.FailureMessage == "FailureMessage" 
                                           & i.Cause == CauseType.SystemError);

    [Fact]
    public void Invalid_entry_with_expression_can_set_all_properties()//Keep coverage happy as otherwise Set property is not covered.
    {
        var invalidEntry = new InvalidEntry("FailureMessage","Path", "PropertyName", "DisplayName");
        
        var invalidEntryCopy = invalidEntry with { Path = "Path", PropertyName = "PropertyName", DisplayName = "DisplayName", FailureMessage = "FailureMessage", Cause = CauseType.Validation };

        invalidEntryCopy.Should().BeEquivalentTo<InvalidEntry>(invalidEntry);
    }


    [Theory]
    [InlineData(25)]
    //[InlineData(null)]
    public void Validation_options_should_set_the_max_depth_if_provided_otherwise_it_should_use_the_library_default(int? maxDepth)
    {
        ValidationOptions options = default;  

        if (maxDepth.HasValue) options = new() {MaxRecursionDepth = maxDepth.Value};

        if (maxDepth.HasValue)
        {
            options.MaxRecursionDepth.Should().Be(maxDepth.Value);
            return;
        }

        options.MaxRecursionDepth.Should().Be(ValidatedConstants.ValidationOptions_MaxDepth);
    }
}

