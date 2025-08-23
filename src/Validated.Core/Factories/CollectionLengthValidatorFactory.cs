using Microsoft.Extensions.Logging;
using System.Collections;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Factories;


/// <summary>
/// Factory for creating validators that check collection length constraints.
/// </summary>
/// <remarks>
/// <para>
/// The CollectionLengthValidatorFactory creates validators that ensure collections contain
/// a number of items within specified minimum and maximum bounds. This factory supports
/// any type that implements <see cref="IEnumerable"/> except for strings, which are 
/// handled by the string length validator.
/// </para>
/// <para>
/// The factory optimizes collection counting by first checking if the collection implements
/// <see cref="ICollection"/> for direct count access, falling back to enumeration if needed.
/// Configuration errors are handled gracefully with appropriate logging and error responses.
/// </para>
/// </remarks>
/// <param name="logger">Logger instance for recording validation errors and configuration issues.</param>
internal sealed class CollectionLengthValidatorFactory(ILogger<CollectionLengthValidatorFactory> logger) : IValidatorFactory
{

    /// <summary>
    /// Creates a member validator that validates collection length is within the specified range defined in the rule configuration.
    /// </summary>
    /// <typeparam name="T">
    /// The collection type to validate. Must implement <see cref="IEnumerable"/> and cannot be a string type.
    /// The type constraint ensures only valid collection types are processed.
    /// </typeparam>
    /// <param name="ruleConfig">
    /// The validation rule configuration containing the minimum and maximum length constraints,
    /// along with failure messages and property identification information.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates the collection contains a number of items
    /// between the specified minimum and maximum values (inclusive). Returns validation errors
    /// for collections outside the specified range or for invalid collection types.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull
         
        => (valueToValidate, path, _, _) =>
         {
            
             try
             {
                 //It was either throw or a goto statement with code modification to get 100% code coverage due to conditional null checks as nothing throws other than a null rule config which is only one half of the conditionals.

                 if (typeof(T) == typeof(string) || !typeof(T).IsAssignableTo(typeof(IEnumerable))) throw new ArgumentException("The value must be a collection");

                 var count = -1;//done like this for code coverage
                 
                 if (valueToValidate is ICollection collection) count = collection.Count;
                 if (count == -1 && valueToValidate is IEnumerable enumerable) count = enumerable.Cast<object>().Count();

                 var valid = count >= ruleConfig.MinLength && count <= ruleConfig.MaxLength && count > -1;

                 var failureMessage = valid ? "" : FailureMessages.FormatCollectionLengthMessage(ruleConfig.FailureMessage, ruleConfig.DisplayName, count.ToString());

                 var result = valid ? Validated<T>.Valid(valueToValidate!)
                                     : Validated<T>.Invalid(new InvalidEntry(failureMessage,path, ruleConfig.PropertyName, ruleConfig.DisplayName));

                 return Task.FromResult(result);
             }
             catch (Exception ex)
             {
                 logger.LogError(ex,"Configuration error causing String length validation failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName}",
                     ruleConfig?.TenantID     ?? "[Null]",
                     ruleConfig?.TypeFullName ?? "[Null]",
                     ruleConfig?.PropertyName ?? "[Null]"
                 );



                 return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
             }

         };
}
