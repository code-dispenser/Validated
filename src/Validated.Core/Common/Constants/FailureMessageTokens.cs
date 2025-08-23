namespace Validated.Core.Common.Constants;

/// <summary>
/// Contains a set of tokens used as placeholders in validation failure messages.
/// </summary>
/// <remarks>
/// These tokens are dynamically replaced at runtime with specific values related to the validation
/// failure, providing a more informative and contextual error message.
/// </remarks>
public static class FailureMessageTokens
{
    /// <summary>
    /// Placeholder for the display name of the property being validated.
    /// </summary>
    public const string DISPLAY_NAME = "{DisplayName}";

    /// <summary>
    /// Placeholder for the value that failed validation.
    /// </summary>
    public const string VALIDATED_VALUE = "{ValidatedValue}";

    /// <summary>
    /// Placeholder for the value being compared against.
    /// </summary>
    public const string COMPARE_TO_VALUE = "{CompareToValue}";

    /// <summary>
    /// Placeholder for the actual length of a string or collection that failed a length check.
    /// </summary>
    public const string ACTUAL_LENGTH = "{ActualLength}";

    /// <summary>
    /// Placeholder for the minimum date in a range validation.
    /// </summary>
    public const string MIN_DATE = "{MinDate}";

    /// <summary>
    /// Placeholder for the maximum date in a range validation.
    /// </summary>
    public const string MAX_DATE = "{MaxDate}";

    /// <summary>
    /// Placeholder for the current date in a rolling date validation.
    /// </summary>
    public const string TODAY = "{Today}";
}