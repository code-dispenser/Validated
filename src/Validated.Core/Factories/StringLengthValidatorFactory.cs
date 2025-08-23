using Microsoft.Extensions.Logging;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Factory for creating validators that ensure string values meet length constraints.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="StringLengthValidatorFactory"/> interprets <see cref="ValidationRuleConfig"/> 
/// instances where <see cref="ValidationRuleConfig.MinLength"/> and 
/// <see cref="ValidationRuleConfig.MaxLength"/> define the inclusive length boundaries.
/// </para>
/// <para>
/// Validators created by this factory check whether the length of the string value falls within
/// the specified range. <c>null</c> values are treated as length <c>0</c>.
/// </para>
/// <para>
/// If the type being validated is not <see cref="string"/>, the factory treats this as a 
/// configuration error. Such errors are logged via the injected <see cref="ILogger"/> and 
/// surfaced as <see cref="CauseType.RuleConfigError"/> failures.
/// </para>
/// </remarks>
/// <param name="logger">
/// Logger instance used to record configuration errors, such as applying string length validation
/// to non-string types or misconfigured min/max values. Logging ensures visibility into
/// tenant-specific rule issues without interrupting the validation pipeline.
/// </param>
internal sealed class StringLengthValidatorFactory(ILogger<StringLengthValidatorFactory> logger) : IValidatorFactory
{

    /// <summary>
    /// Creates a validator that checks whether a string value meets configured length constraints.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value being validated. Must be <see cref="string"/>; other types
    /// result in configuration errors.
    /// </typeparam>
    /// <param name="ruleConfig">
    /// The validation rule configuration specifying the minimum and maximum string lengths,
    /// along with the failure message and property metadata.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates string lengths against the configured
    /// constraints. Produces <see cref="CauseType.Validation"/> failures if the value length 
    /// is outside the range, or <see cref="CauseType.RuleConfigError"/> if configuration is invalid.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (valueToValidate, path, _, _) =>
        {
            try
            {
                //It was either throw or a goto statement with code modification to get 100% code coverage due to conditional null checks as nothing throws other than a null rule config which is only one half of the conditionals.

                if (typeof(T) != typeof(string)) throw new ArgumentException("The value passed in must be of type string");

                var value = valueToValidate is null ? 0 : valueToValidate.ToString()!.Length;
                var valid = value >= ruleConfig.MinLength && value <= ruleConfig.MaxLength;

                var failureMessage = valid ? "" : FailureMessages.FormatStringLengthMessage(ruleConfig.FailureMessage,valueToValidate?.ToString() ?? "", ruleConfig.DisplayName, value.ToString());

                var result = valid ? Validated<T>.Valid(valueToValidate!)
                                    : Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig.PropertyName, ruleConfig.DisplayName));

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"Configuration error causing String length validation failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} ValueToValidate:{ValueToValidate}",
                    ruleConfig?.TenantID        ?? "[Null]",
                    ruleConfig?.TypeFullName    ?? "[Null]",
                    ruleConfig?.PropertyName    ?? "[Null]",
                    valueToValidate?.ToString() ?? "[Null]"
                );


                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }

        };

}
