using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Factory for creating validators that check values against a regular expression pattern.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="RegexValidatorFactory"/> interprets <see cref="ValidationRuleConfig"/> instances
/// where the <see cref="ValidationRuleConfig.Pattern"/> property defines a regular expression.
/// </para>
/// <para>
/// Validators created by this factory ensure that string representations of values
/// match the configured regex pattern. If the value is <c>null</c> or whitespace,
/// the validator produces a failure unless explicitly allowed by the configuration.
/// </para>
/// <para>
/// Configuration errors (e.g., invalid regex patterns) are logged via the injected
/// <see cref="ILogger"/> and surfaced as validation failures with 
/// <see cref="CauseType.RuleConfigError"/>.
/// </para>
/// </remarks>
/// <param name="logger">
/// Logger instance used to record configuration errors, such as invalid regular
/// expression patterns or misconfigured rule settings. Logging provides visibility
/// into tenant-specific validation misconfigurations without interrupting runtime flow.
/// </param>
internal sealed class RegexValidatorFactory(ILogger<RegexValidatorFactory> logger) : IValidatorFactory
{
    /// <summary>
    /// Creates a validator that checks whether values match the configured regex pattern.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value being validated. Typically <see cref="string"/>, but any type 
    /// will be converted to a string for regex evaluation.
    /// </typeparam>
    /// <param name="ruleConfig">
    /// The validation rule configuration containing the regex pattern, failure message, 
    /// and property metadata.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates values against the configured 
    /// regex pattern. Produces <see cref="CauseType.Validation"/> failures if the 
    /// value does not match, or <see cref="CauseType.RuleConfigError"/> failures 
    /// if the regex is invalid.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (valueToValidate, path, _, _) =>
        {
            try
            {
                var failureMessage = ErrorMessages.Default_Failure_Message;

                if (String.IsNullOrWhiteSpace(valueToValidate?.ToString()))
                {
                    failureMessage = FailureMessages.Format(ruleConfig.FailureMessage, String.Empty, ruleConfig.DisplayName);
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig.PropertyName, ruleConfig.DisplayName, CauseType.Validation)));
                }

                var isValid = Regex.IsMatch(valueToValidate.ToString()!, ruleConfig.Pattern);
                
                failureMessage = isValid ? String.Empty : FailureMessages.Format(ruleConfig.FailureMessage, valueToValidate.ToString()!,ruleConfig.DisplayName); 

                var result = isValid ? Validated<T>.Valid(valueToValidate!)
                                           : Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig.PropertyName, ruleConfig.DisplayName));

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"Configuration error causing Regex validation failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} - Pattern:{Pattern} ValueToValidate:{ValueToValidate}",
                    ruleConfig?.TenantID        ?? "[Null]",
                    ruleConfig?.TypeFullName    ?? "[Null]",
                    ruleConfig?.PropertyName    ?? "[Null]",
                    ruleConfig?.Pattern         ?? "[Null]",
                    valueToValidate.ToString()
                );

                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }
        };
    
}
