using Microsoft.Extensions.Logging;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Factory for creating validators that ensure values fall within a specified range.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="RangeValidatorFactory"/> interprets <see cref="ValidationRuleConfig"/> instances
/// where the <see cref="ValidationRuleConfig.MinValue"/> and <see cref="ValidationRuleConfig.MaxValue"/>
/// properties define inclusive range boundaries.
/// </para>
/// <para>
/// Validators created by this factory check whether the value being validated lies within
/// the specified range. Supported types include numeric types, dates (<see cref="DateOnly"/>, <see cref="DateTime"/>),
/// and any type that implements <see cref="IComparable"/>.
/// </para>
/// <para>
/// Configuration errors (e.g., invalid type conversions or non-comparable types) are logged
/// via the injected <see cref="ILogger"/> and surfaced as validation failures with
/// <see cref="CauseType.RuleConfigError"/>.
/// </para>
/// </remarks>
/// <param name="logger">
/// Logger instance used to record configuration errors such as invalid min/max values,
/// type conversion issues, or misconfigured range rules. Logging provides visibility into
/// tenant-specific validation issues without interrupting the validation pipeline.
/// </param>
internal sealed class RangeValidatorFactory(ILogger<RangeValidatorFactory> logger) : IValidatorFactory
{

    /// <summary>
    /// Creates a validator that checks whether values fall within the configured range.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value being validated. Must implement <see cref="IComparable"/>
    /// and be compatible with the configured <see cref="ValidationRuleConfig.MinValue"/> 
    /// and <see cref="ValidationRuleConfig.MaxValue"/>.
    /// </typeparam>
    /// <param name="ruleConfig">
    /// The validation rule configuration containing the minimum and maximum values,
    /// along with failure messages and property metadata.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates values against the configured
    /// range. Produces <see cref="CauseType.Validation"/> failures if the value is outside
    /// the range, or <see cref="CauseType.RuleConfigError"/> failures if configuration
    /// is invalid.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull
       
        => (valueToValidate, path, _, _) =>
        {
            try
            {
                var systemType = Type.GetType($"System.{ruleConfig.MinMaxToValueType.Split("_")[1]}")!;

                IComparable minValue, maxValue;
                if (systemType == typeof(DateOnly))
                {
                    minValue = DateOnly.Parse(ruleConfig.MinValue);
                    maxValue = DateOnly.Parse(ruleConfig.MaxValue);
                }
                else
                {
                    minValue = (IComparable)Convert.ChangeType(ruleConfig.MinValue, systemType);
                    maxValue = (IComparable)Convert.ChangeType(ruleConfig.MaxValue, systemType);
                }

                var valid = minValue.CompareTo(valueToValidate) <= 0 && maxValue.CompareTo(valueToValidate) >= 0;

                var failureMessage = valid ? String.Empty : FailureMessages.Format(ruleConfig.FailureMessage, valueToValidate?.ToString() ?? "", ruleConfig.DisplayName);

                var result = valid ? Validated<T>.Valid(valueToValidate!)
                                    : Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig.PropertyName, ruleConfig.DisplayName, CauseType.Validation));

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"Configuration error causing range validation failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} - MinValue:{MinValue} MaxValue:{MaxValue} - MinMaxToValueType:{MinMaxToValueType} ValueToValidate:{ValueToValidate}",
                    ruleConfig?.TenantID          ?? "[Null]",
                    ruleConfig?.TypeFullName      ?? "[Null]",
                    ruleConfig?.PropertyName      ?? "[Null]",
                    ruleConfig?.MinValue          ?? "[Null]",
                    ruleConfig?.MaxValue          ?? "[Null]",
                    ruleConfig?.MinMaxToValueType ?? "[Null]",
                    valueToValidate?.ToString()   ?? "[Null]"
                );

                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }
        };
}
