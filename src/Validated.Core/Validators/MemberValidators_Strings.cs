using System.Text.RegularExpressions;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Validators;

public static partial class MemberValidators
{

    /// <summary>
    /// Creates a member validator that validates string length is within the specified range.
    /// </summary>
    /// <param name="minLength">The minimum allowed length for the string.</param>
    /// <param name="maxLength">The maximum allowed length for the string.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that checks string length constraints.</returns>
    public static MemberValidator<string> CreateStringLengthValidator(int minLength, int maxLength, string propertyName, string displayName, string failureMessage)

        => (valueToValidate, path, _, _) =>
        {
            if (valueToValidate == null) return Task.FromResult(Validated<string>.Invalid(new InvalidEntry(FailureMessages.FormatStringLengthMessage(failureMessage, "", displayName, "0"),BuildPathFromParams(path, propertyName), propertyName, displayName)));
            int value = valueToValidate.Length;

            var isValid = (value >= minLength && value <= maxLength);

            var result = isValid ? Validated<string>.Valid(valueToValidate!)
                                    : Validated<string>.Invalid(new InvalidEntry(FailureMessages.FormatStringLengthMessage(failureMessage, valueToValidate, displayName, value.ToString()),BuildPathFromParams(path, propertyName), propertyName, displayName));

            return Task.FromResult(result);
        };

    /// <summary>
    /// Creates a member validator that validates a string against a regular expression pattern.
    /// </summary>
    /// <param name="pattern">The regular expression pattern to match against.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that checks if the string matches the specified regex pattern.</returns>
    public static MemberValidator<string> CreateStringRegexValidator(string pattern, string propertyName, string displayName, string failureMessage)

        => (valueToValidate, path, _, _) =>
        {
            if (String.IsNullOrWhiteSpace(valueToValidate?.ToString())) return CreateInvalidWithDefaultFormatting<string>("", path, propertyName, displayName, failureMessage);

            if (Regex.IsMatch(valueToValidate.Trim(), pattern)) return Task.FromResult(Validated<string>.Valid(valueToValidate!.Trim()));
            
            return CreateInvalidWithDefaultFormatting<string>("", path, propertyName, displayName, failureMessage);
        };


}