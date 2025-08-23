namespace Validated.Core.Common.Constants;

/// <summary>
/// Contains a set of commonly used constants for the Validated.Core project.
/// </summary>
/// <remarks>
/// These constants are used throughout the application to provide consistent
/// string and numeric values, such as default IDs, rule types, and comparison types.
/// </remarks>
public static class ValidatedConstants
{
    /// <summary>
    /// The default tenant ID, used when a specific tenant is not specified.
    /// </summary>
    public const string Default_TenantID = "ALL";

    /// <summary>
    /// The default culture ID, used when a specific culture is not specified.
    /// </summary>
    public const string Default_CultureID = "en-GB";

    /// <summary>
    /// Rule type for when a rule is not found.
    /// </summary>
    public const string RuleType_NotFound = "RuleType_NotFound";

    /// <summary>
    /// Rule type for regular expression validation.
    /// </summary>
    public const string RuleType_Regex = "RuleType_Regex";

    /// <summary>
    /// Rule type for string length validation.
    /// </summary>
    public const string RuleType_StringLength = "RuleType_StringLength";

    /// <summary>
    /// Rule type for range validation.
    /// </summary>
    public const string RuleType_Range = "RuleType_Range";

    /// <summary>
    /// Rule type for rolling date validation.
    /// </summary>
    public const string RuleType_RollingDate = "RuleType_RollingDate";

    /// <summary>
    /// Rule type for collection length validation.
    /// </summary>
    public const string RuleType_CollectionLength = "RuleType_CollectionLength";

    /// <summary>
    /// Rule type for member comparison.
    /// </summary>
    public const string RuleType_MemberComparison = "RuleType_MemberComparison";

    /// <summary>
    /// Rule type for comparison with a value.
    /// </summary>
    public const string RuleType_CompareTo = "RuleType_CompareTo";

    /// <summary>
    /// Rule type for value object comparison.
    /// </summary>
    public const string RuleType_VOComparison = "RuleType_VOComparison";

    /// <summary>
    /// Comparison type for equality check.
    /// </summary>
    public const string CompareType_EqualTo = "CompareType_EqualTo";

    /// <summary>
    /// Comparison type for inequality check.
    /// </summary>
    public const string CompareType_NotEqualTo = "CompareType_NotEqualTo";

    /// <summary>
    /// Comparison type for a greater than check.
    /// </summary>
    public const string CompareType_GreaterThan = "CompareType_GreaterThan";

    /// <summary>
    /// Comparison type for a less than check.
    /// </summary>
    public const string CompareType_LessThan = "CompareType_LessThan";

    /// <summary>
    /// Comparison type for a greater than or equal to check.
    /// </summary>
    public const string CompareType_GreaterThanOrEqual = "CompareType_GreaterThanOrEqual";

    /// <summary>
    /// Comparison type for a less than or equal to check.
    /// </summary>
    public const string CompareType_LessThanOrEqual = "CompareType_LessThanOrEqual";

    /// <summary>
    /// The value type for string comparisons.
    /// </summary>
    public const string MinMaxToValueType_String = "MinMaxToValueType_String";

    /// <summary>
    /// The value type for 32-bit integer comparisons.
    /// </summary>
    public const string MinMaxToValueType_Int32 = "MinMaxToValueType_Int32";

    /// <summary>
    /// The value type for decimal comparisons.
    /// </summary>
    public const string MinMaxToValueType_Decimal = "MinMaxToValueType_Decimal";

    /// <summary>
    /// The value type for date and time comparisons.
    /// </summary>
    public const string MinMaxToValueType_DateTime = "MinMaxToValueType_DateTime";

    /// <summary>
    /// The value type for date-only comparisons.
    /// </summary>
    public const string MinMaxToValueType_DateOnly = "MinMaxToValueType_DateOnly";

    /// <summary>
    /// The value type for GUID comparisons.
    /// </summary>
    public const string MinMaxToValueType_Guid = "MinMaxToValueType_Guid";

    /// <summary>
    /// The value type for time span comparisons.
    /// </summary>
    public const string MinMaxToValueType_TimeSpan = "MinMaxToValueType_TimeSpan";

    /// <summary>
    /// The value type for day comparisons.
    /// </summary>
    public const string MinMaxToValueType_Day = "MinMaxToValueType_Day";

    /// <summary>
    /// The value type for month comparisons.
    /// </summary>
    public const string MinMaxToValueType_Month = "MinMaxToValueType_Month";

    /// <summary>
    /// The value type for year comparisons.
    /// </summary>
    public const string MinMaxToValueType_Year = "MinMaxToValueType_Year";

    /// <summary>
    /// The target type for a single item.
    /// </summary>
    public const string TargetType_Item = "TargetType_Item";

    /// <summary>
    /// The target type for a collection of items.
    /// </summary>
    public const string TargetType_Collection = "TargetType_Collection";

    /// <summary>
    /// The maximum depth for validation options to prevent infinite loops.
    /// </summary>
    public const int ValidationOptions_MaxDepth = 100;
}