using System.Globalization;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Validators;

public static partial class MemberValidators
{
    /// <summary>
    /// Creates a validator that checks whether a string or number value is a numeric value that does
    /// not exceed the precision and scale bounds.
    /// </summary>
    /// <typeparam name="T">The type of value to validate.</typeparam>
    /// <param name="maxPrecision">The maximum number of digits</param>
    /// <param name="maxScale">The maximum number of digits to the right of the decimal point</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation messages.</param>
    /// <param name="failureMessage">The message returned when the value is not valid.</param>
    /// <param name="cultureInfo">Optional culture information when parsing strings that may contain comma separators</param>
    /// <returns>A member validator that checks precision and scale inputs expected to be decimal values.</returns>
    public static MemberValidator<T> CreatePrecisionScaleValidator<T>(int maxPrecision, int maxScale, string propertyName, string displayName, string failureMessage, CultureInfo? cultureInfo = null) where T: notnull

        => (valueToValidate, path, _, _) =>
        {

            if (valueToValidate is null) return CreateInvalidWithPSFormatting<T>("", maxPrecision.ToString(), maxScale.ToString(), "", "", path, propertyName,displayName,failureMessage);

            decimal? decimalValue = valueToValidate switch
            {
                byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal _ => Convert.ToDecimal(valueToValidate),
                string _ => decimal.TryParse(valueToValidate.ToString(),NumberStyles.Any, cultureInfo ?? CultureInfo.InvariantCulture, out var asDecimal) ? asDecimal : null,
                _        => null,
            };

            if (false == decimalValue.HasValue) return CreateInvalidWithPSFormatting<T>(valueToValidate.ToString()!, maxPrecision.ToString(), maxScale.ToString(),"","", path, propertyName, displayName, failureMessage);

            int actualScale = decimalValue.Value.Scale;

            string absStr     = Math.Abs(decimalValue.Value).ToString(CultureInfo.InvariantCulture);
            string digitsOnly = absStr.Replace(".", "").Replace("-", "");

            return (digitsOnly.Length <= maxPrecision && actualScale <= maxScale)
                    ? Task.FromResult(Validated<T>.Valid(valueToValidate))
                        : CreateInvalidWithPSFormatting<T>(valueToValidate.ToString()!, maxPrecision.ToString(), maxScale.ToString(), digitsOnly.Length.ToString(), actualScale.ToString(), path, propertyName, displayName, failureMessage);
        };

    /// <summary>
    /// Internal helper method that creates a Task of an invalid validated with decimal precision and scale failure message replacement formatting
    /// </summary>
    private static Task<Validated<T>> CreateInvalidWithPSFormatting<T>(string valueToValidate, string precision, string scale, string actualPrecision, string actualScale, string path, string propertyName, string displayName, string failureMessage) where T: notnull

        => Task.FromResult(Validated<T>
                .Invalid(new InvalidEntry(FailureMessages.FormatDecimalPrecisionScaleMessage
                (
                    failureMessage, valueToValidate.ToString()!, displayName, precision, scale,actualPrecision, actualScale), BuildPathFromParams(path, propertyName), propertyName, displayName)
                ));

}
