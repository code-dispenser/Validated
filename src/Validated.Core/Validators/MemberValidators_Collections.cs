using System.Collections;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Validators;

public static partial class MemberValidators
{
    /// <summary>
    /// Creates a member validator that validates the length of a collection is within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of collection to validate.</typeparam>
    /// <param name="minLength">The minimum allowed number of items in the collection.</param>
    /// <param name="maxLength">The maximum allowed number of items in the collection.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that checks collection length constraints.</returns>
    public static MemberValidator<T> CreateCollectionLengthValidator<T>(int minLength, int maxLength, string propertyName, string displayName, string failureMessage) where T : notnull

        => (valueToValidate, path, _, _) =>
        {

            if (valueToValidate == null  || typeof(T) == typeof(string) || !typeof(T).IsAssignableTo(typeof(IEnumerable))) 
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.FormatCollectionLengthMessage(failureMessage, displayName, "0"),BuildPathFromParams(path, propertyName), propertyName, displayName)));
            
            var count = -1;

            if (valueToValidate is ICollection collection) count = collection.Count;
            if (count == -1 && valueToValidate is IEnumerable enumerable) count = enumerable.Cast<object>().Count();

            var result = (count >= minLength && count <= maxLength && count > -1 )
                            ? Validated<T>.Valid(valueToValidate!)
                                : Validated<T>.Invalid(new InvalidEntry(FailureMessages.FormatCollectionLengthMessage(failureMessage, displayName, count.ToString()), BuildPathFromParams(path, propertyName), propertyName, displayName));

            return Task.FromResult(result);
        };

}

