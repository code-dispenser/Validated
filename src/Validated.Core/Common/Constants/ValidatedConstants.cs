using Validated.Core.Types;
using Validated.Core.Factories;

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
    /// The default tenant ID, used when a specific tenant is not specified - used in config based validations.
    /// </summary>
    public const string Default_TenantID = "ALL";

    /// <summary>
    /// The default culture ID, used when a specific culture is not specified - used in config based validations.
    /// </summary>
    public const string Default_CultureID = "en-GB";

    /// <summary>
    /// Rule type for when a rule is not found - used in config based validations.
    /// </summary>
    public const string RuleType_NotFound = "RuleType_NotFound";

    /// <summary>
    /// Rule type for regular expression validation - used in config based validations.
    /// </summary>
    public const string RuleType_Regex = "RuleType_Regex";

    /// <summary>
    /// Rule type for string length validation - used in config based validations.
    /// </summary>
    public const string RuleType_StringLength = "RuleType_StringLength";

    /// <summary>
    /// Rule type for range validation - used in config based validations.
    /// </summary>
    public const string RuleType_Range = "RuleType_Range";

    /// <summary>
    /// Rule type for rolling date validation - used in config based validations.
    /// </summary>
    public const string RuleType_RollingDate = "RuleType_RollingDate";

    /// <summary>
    /// Rule type for collection length validation - used in config based validations.
    /// </summary>
    public const string RuleType_CollectionLength = "RuleType_CollectionLength";

    /// <summary>
    /// Rule type for member comparison - used in config based validations.
    /// Informs the validator to use a member to member comparison method.
    /// </summary>
    public const string RuleType_MemberComparison = "RuleType_MemberComparison";

    /// <summary>
    /// Rule type for comparison with a value - used in config based validations.
    /// Informs the validator to compare the member value to that included in the config entry. 
    /// </summary>
    public const string RuleType_CompareTo = "RuleType_CompareTo";

    /// <summary>
    /// Rule type for value object comparison - used in config based validations.
    /// Informs the validator that this is a comparison of two values for a ValueObject
    /// and as such it will use the MemberValidator compareTo value for the right-hand side of the comparison.
    /// 
    /// </summary>
    public const string RuleType_VOComparison = "RuleType_VOComparison";

    /// <summary>
    /// Rule type for Url format validation - used in config based validations.
    /// </summary>
    public const string RuleType_UrlFormat = "RuleType_UrlFormat";

    /// <summary>
    /// Rule type for decimal precision and scale validation - used in config based validations.
    /// </summary>
    public const string RuleType_PrecisionScale = "RuleType_PrecisionScale";

    /// <summary>
    /// Comparison type for equality check - used in config based validations.
    /// </summary>
    public const string CompareType_EqualTo = "CompareType_EqualTo";

    /// <summary>
    /// Comparison type for inequality check - used in config based validations.
    /// </summary>
    public const string CompareType_NotEqualTo = "CompareType_NotEqualTo";

    /// <summary>
    /// Comparison type for a greater than check.
    /// </summary>
    public const string CompareType_GreaterThan = "CompareType_GreaterThan";

    /// <summary>
    /// Comparison type for a less than check - used in config based validations.
    /// </summary>
    public const string CompareType_LessThan = "CompareType_LessThan";

    /// <summary>
    /// Comparison type for a greater than or equal to check.
    /// </summary>
    public const string CompareType_GreaterThanOrEqual = "CompareType_GreaterThanOrEqual";

    /// <summary>
    /// Comparison type for a less than or equal to check - used in config based validations.
    /// </summary>
    public const string CompareType_LessThanOrEqual = "CompareType_LessThanOrEqual";

    /// <summary>
    /// The value type for string comparisons - used in config based validations.
    /// </summary>
    public const string MinMaxToValueType_String = "MinMaxToValueType_String";

    /// <summary>
    /// The value type for 32-bit integer comparisons - used in config based validations.
    /// </summary>
    public const string MinMaxToValueType_Int32 = "MinMaxToValueType_Int32";

    /// <summary>
    /// The value type for decimal comparisons - used in config based validations.
    /// Informs the validator of the data type it should use for the MinValue, MaxValue or CompareValue property 
    /// values used in <see cref="ValidationRuleConfig"/>.
    /// </summary>
    public const string MinMaxToValueType_Decimal = "MinMaxToValueType_Decimal";

    /// <summary>
    /// The value type for date and time comparisons - used in config based validations.
    /// Informs the validator of the data type it should use for the MinValue, MaxValue or CompareValue property 
    /// values used in <see cref="ValidationRuleConfig"/>.
    /// </summary>
    public const string MinMaxToValueType_DateTime = "MinMaxToValueType_DateTime";

    /// <summary>
    /// The value type for date-only comparisons - used in config based validations.
    /// Informs the validator of the data type it should use for the MinValue, MaxValue or CompareValue property 
    /// values used in <see cref="ValidationRuleConfig"/>.
    /// </summary>
    public const string MinMaxToValueType_DateOnly = "MinMaxToValueType_DateOnly";

    /// <summary>
    /// The value type for GUID comparisons - used in config based validations.
    /// Informs the validator of the data type it should use for the MinValue, MaxValue or CompareValue property 
    /// values used in <see cref="ValidationRuleConfig"/>.
    /// </summary>
    public const string MinMaxToValueType_Guid = "MinMaxToValueType_Guid";

    /// <summary>
    /// The value type for time span comparisons - used in config based validations.
    /// Informs the validator of the data type it should use for the MinValue, MaxValue or CompareValue property 
    /// values used in <see cref="ValidationRuleConfig"/>.
    /// </summary>
    public const string MinMaxToValueType_TimeSpan = "MinMaxToValueType_TimeSpan";

    /// <summary>
    /// The value type for the day unit - currently only used in the <see cref="RollingDateOnlyValidatorFactory" />.
    /// Informs the validator of the data type it should use for the MinValue and MaxValue property 
    /// values used in <see cref="ValidationRuleConfig"/>.
    /// </summary>
    public const string MinMaxToValueType_Day = "MinMaxToValueType_Day";

    /// <summary>
    /// The value type for the month unit - currently only used in the <see cref="RollingDateOnlyValidatorFactory" />  - used in config based validations..
    /// Informs the validator of the data type it should use for the MinValue and MaxValue property 
    /// values used in <see cref="ValidationRuleConfig"/>.
    /// </summary>
    public const string MinMaxToValueType_Month = "MinMaxToValueType_Month";

    /// <summary>
    /// The value type for the year unit - currently only used in the <see cref="RollingDateOnlyValidatorFactory" />.
    /// Informs the validator of the data type it should use for the MinValue and MaxValue property 
    /// values used in <see cref="ValidationRuleConfig"/>.
    /// </summary>
    public const string MinMaxToValueType_Year = "MinMaxToValueType_Year";

    /// <summary>
    /// The target type for a single item i.e the rules is not about counting items in a collection - used in config based validations.
    /// </summary>
    public const string TargetType_Item = "TargetType_Item";

    /// <summary>
    /// The target type for a collection of items where the validation is about the collection itself such as the count of item and not the content - used in config based validations.
    /// </summary>
    public const string TargetType_Collection = "TargetType_Collection";

    /// <summary>
    /// Scheme type for all allowable schemes - used in config based validations.
    /// </summary>
    public const string SchemeTypes_All = "Https|Http|Ftps|Ftp";
    /// <summary>
    /// Scheme type for Https - used in config based validations.
    /// </summary>
    public const string SchemeTypes_Https = "Https";
    /// <summary>
    /// Scheme type for Http - used in config based validations.
    /// </summary>
    public const string SchemeTypes_Http = "Http";
    /// <summary>
    /// Scheme type for Ftps - used in config based validations.
    /// </summary>
    public const string SchemeTypes_Ftps = "Ftps";
    /// <summary>
    /// Scheme type for Https - used in config based validations.
    /// </summary>
    public const string SchemeTypes_Ftp = "Ftp";

    /// <summary>
    /// Rule dictionary key for the integer precision value - used in config based validations.
    /// </summary>
    public const string RuleDictKey_Precision = "Precision";
    /// <summary>
    /// Rule dictionary key for the scale value - used in config based validations.
    /// </summary>
    public const string RuleDictKey_Scale = "Scale";





    /// <summary>
    /// The default maximum depth for validation options to prevent infinite loops in recursive validations. Used in <see cref="ValidationOptions" />.
    /// </summary>
    public const int ValidationOptions_MaxDepth = 100;
}