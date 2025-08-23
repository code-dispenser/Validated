using System.Collections;
using System.Text.RegularExpressions;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;


namespace Validated.Core.Validators;

/// <summary>
/// Provides factory methods for creating member validators that validate individual properties or values.
/// </summary>
/// <remarks>
/// This static class contains methods for creating common validation scenarios including regex matching,
/// null/empty checking, predicate-based validation, and range validation. All validators return
/// <see cref="MemberValidator{T}"/> delegates that can be used independently or composed together
/// using the validation builder classes.
/// </remarks>
public static partial class MemberValidators
{

    /// <summary>
    /// Builds a validation path from the provided path parameter and property name.
    /// </summary>
    /// <param name="path">The current validation path.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <returns>The property name if path is null or whitespace, otherwise returns the trimmed path.</returns>
    internal static string BuildPathFromParams(string path, string propertyName)

        => String.IsNullOrWhiteSpace(path) ? propertyName : path.Trim();


    /// <summary>
    /// Creates a member validator that validates a value matches the specified regular expression pattern.
    /// </summary>
    /// <typeparam name="T">The type of value to validate.</typeparam>
    /// <param name="pattern">The regular expression pattern to match against.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that checks if the value matches the specified regex pattern.</returns>
    public static MemberValidator<T> CreateRegexValidator<T>(string pattern, string propertyName, string displayName, string failureMessage) where T : notnull

        => (valueToValidate, path, _, _) =>
        {
            if (String.IsNullOrWhiteSpace(valueToValidate?.ToString())) 
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, "", displayName), BuildPathFromParams(path, propertyName), propertyName, displayName)));
            
            var result = Regex.IsMatch(valueToValidate.ToString()!, pattern)
                                ? Validated<T>.Valid(valueToValidate!)
                                    : Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, valueToValidate.ToString()!, displayName),BuildPathFromParams(path, propertyName), propertyName, displayName));
            
            return Task.FromResult(result);

        };


    /// <summary>
    /// Creates a member validator that validates a value is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of value to validate.</typeparam>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that checks if the value is not null or empty.</returns>
    public static MemberValidator<T> CreateNotNullOrEmptyValidator<T>(string propertyName, string displayName, string failureMessage) where T : notnull

        => (valueToValidate, path, _, _) =>
        {

            if (true == typeof(T).IsValueType) return Task.FromResult(Validated<T>.Valid(valueToValidate!));

            if (valueToValidate is null) 
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, "", displayName),BuildPathFromParams(path, propertyName), propertyName, displayName)));

            if (valueToValidate is string stringValue && String.IsNullOrWhiteSpace(stringValue.Trim())) 
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, "", displayName),BuildPathFromParams(path, propertyName), propertyName, displayName)));

            if (valueToValidate is not string && valueToValidate is IEnumerable enumerable && !enumerable.Cast<object>().Any()) 
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, "", displayName),BuildPathFromParams(path, propertyName), propertyName, displayName))); 

            return Task.FromResult(Validated<T>.Valid(valueToValidate!));

        };

    /// <summary>
    /// Creates a member validator that validates a value using a custom predicate function.
    /// </summary>
    /// <typeparam name="T">The type of value to validate.</typeparam>
    /// <param name="predicate">A function that returns true if the value is valid, false otherwise.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that uses the provided predicate for validation.</returns>
    public static MemberValidator<T> CreatePredicateValidator<T>(Func<T, bool> predicate, string propertyName, string displayName, string failureMessage) where T : notnull

        => (valueToValidate, path, _, _) =>
        {

            if (valueToValidate is null) return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, "", displayName), BuildPathFromParams(path, propertyName), propertyName, displayName)));

            var result = predicate(valueToValidate) ? Validated<T>.Valid(valueToValidate)
                                                        : Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, valueToValidate.ToString()!, displayName),BuildPathFromParams(path, propertyName), propertyName, displayName));

            return Task.FromResult(result);
        };

    /// <summary>
    /// Creates a member validator that validates a value is within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of value to validate. Must implement <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="minValue">The minimum allowed value (inclusive).</param>
    /// <param name="maxValue">The maximum allowed value (inclusive).</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that checks if the value falls within the specified range.</returns>
    public static MemberValidator<T> CreateRangeValidator<T>(T minValue, T maxValue, string propertyName, string displayName, string failureMessage) where T : notnull, IComparable<T>
       
        => (valueToValidate, path, _, _) =>
        {
            if (valueToValidate is null) 
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, "", displayName), BuildPathFromParams(path, propertyName), propertyName, displayName)));

            var result = (valueToValidate.CompareTo(minValue) >= 0 && valueToValidate.CompareTo(maxValue) <= 0) 
                                            ? Validated<T>.Valid(valueToValidate)
                                                : Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(failureMessage, valueToValidate.ToString()!, displayName), BuildPathFromParams(path, propertyName), propertyName, displayName));

            return Task.FromResult(result);
        };


}
