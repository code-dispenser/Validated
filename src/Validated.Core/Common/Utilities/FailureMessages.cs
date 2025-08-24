using System.Text;

using Token = Validated.Core.Common.Constants.FailureMessageTokens;

namespace Validated.Core.Common.Utilities;

/// <summary>
/// Centralized helpers for formatting validation failure messages from templates with tokens.
/// </summary>
/// <remarks>
/// Tokens (e.g., display name, validated value, lengths, dates) are replaced to produce
/// clear, consistent messages across validators.
/// </remarks>
internal static class FailureMessages
{
    /// <summary>
    /// Formats a general failure message by replacing standard tokens.
    /// </summary>
    /// <param name="messageTemplate">The message template that may contain tokens.</param>
    /// <param name="validatedValue">The value that was validated, as a string.</param>
    /// <param name="displayName">The user-friendly name of the member being validated.</param>
    /// <returns>The formatted message.</returns>
    public static string Format(string messageTemplate, string validatedValue, string displayName)

    => !RequiresTokenReplacement(messageTemplate)
           ? messageTemplate
                : new StringBuilder(messageTemplate).Replace(Token.DISPLAY_NAME, displayName).Replace(Token.VALIDATED_VALUE, validatedValue).ToString();


    /// <summary>
    /// Formats a failure message for string length validation.
    /// </summary>
    /// <param name="messageTemplate">The message template that may contain tokens.</param>
    /// <param name="validatedValue">The value that was validated, as a string.</param>
    /// <param name="displayName">The user-friendly name of the member being validated.</param>
    /// <param name="length">The actual length of the string being validated.</param>
    /// <returns>A formatted validation failure message.</returns>
    public static string FormatStringLengthMessage(string messageTemplate, string validatedValue, string displayName, string length)

        => !RequiresTokenReplacement(messageTemplate)
               ? messageTemplate
                    : new StringBuilder(messageTemplate).Replace(Token.DISPLAY_NAME, displayName).Replace(Token.VALIDATED_VALUE, validatedValue).Replace(Token.ACTUAL_LENGTH, length).ToString();

    /// <summary>
    /// Formats a failure message for collection length validation.
    /// </summary>
    /// <param name="messageTemplate">The message template that may contain tokens.</param>
    /// <param name="displayName">The user-friendly name of the member being validated.</param>
    /// <param name="length">The actual length of the string being validated.</param>
    /// <returns>A formatted validation failure message.</returns>
    public static string FormatCollectionLengthMessage(string messageTemplate,  string displayName, string length)

        => !RequiresTokenReplacement(messageTemplate)
               ? messageTemplate
                    : new StringBuilder(messageTemplate).Replace(Token.DISPLAY_NAME, displayName).Replace(Token.ACTUAL_LENGTH, length).ToString();

    /// <summary>
    /// Formats a failure message for value comparison validation.
    /// </summary>
    /// <param name="messageTemplate">The message template that may contain tokens.</param>
    /// <param name="validatedValue">The value that was validated, as a string.</param>
    /// <param name="displayName">The user-friendly name of the member being validated.</param>
    /// <param name="compareToValue">The value being compared against.</param>
    /// <returns>A formatted validation failure message.</returns>
    public static string FormatCompareValueMessage(string messageTemplate, string validatedValue, string displayName, string compareToValue)

        => !RequiresTokenReplacement(messageTemplate)
               ? messageTemplate
                    : new StringBuilder(messageTemplate) .Replace(Token.DISPLAY_NAME, displayName).Replace(Token.VALIDATED_VALUE, validatedValue) .Replace(Token.COMPARE_TO_VALUE, compareToValue).ToString();


    /// <summary>
    /// Formats a rolling-date failure message by replacing display name, value, min/max range, and current date tokens.
    /// </summary>
    /// <param name="messageTemplate">The message template that may contain tokens.</param>
    /// <param name="validatedValue">The date value that was validated, as a string.</param>
    /// <param name="displayName">The user-friendly name of the member being validated.</param>
    /// <param name="minDate">The minimum allowed date (formatted).</param>
    /// <param name="maxDate">The maximum allowed date (formatted).</param>
    /// <param name="todaysDate">The current date used for rolling calculations (formatted).</param>
    /// <returns>The formatted message.</returns>
    public static string FormatRollingDateMessage(string messageTemplate, string validatedValue, string displayName, string minDate, string maxDate, string todaysDate)

        => !RequiresTokenReplacement(messageTemplate)
               ? messageTemplate
                    : new StringBuilder(messageTemplate)
                        .Replace(Token.DISPLAY_NAME, displayName).Replace(Token.VALIDATED_VALUE, validatedValue).Replace (Token.MIN_DATE, minDate)
                            .Replace(Token.MAX_DATE, maxDate).Replace(Token.TODAY,todaysDate).ToString();


    /// <summary>
    /// Indicates whether the template contains tokens that require replacement.
    /// </summary>
    /// <param name="messageTemplate">The message template to inspect.</param>
    /// <returns><c>true</c> if token replacement is required; otherwise, <c>false</c>.</returns>
    public static bool RequiresTokenReplacement(string messageTemplate)

        => messageTemplate is not null && messageTemplate.Contains('{');
}
