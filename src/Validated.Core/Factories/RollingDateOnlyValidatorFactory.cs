using Microsoft.Extensions.Logging;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Factory for creating validators that ensure <see cref="DateOnly"/> values
/// fall within a rolling date range relative to "today".
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="RollingDateOnlyValidatorFactory"/> interprets <see cref="ValidationRuleConfig"/>
/// instances where <see cref="ValidationRuleConfig.MinValue"/> and <see cref="ValidationRuleConfig.MaxValue"/>
/// are integer offsets (in days, months, or years) relative to the current date provided
/// by the injected <c>getToday</c> delegate.
/// </para>
/// <para>
/// Supported units are:
/// <list type="bullet">
/// <item><c>Day</c> — offset in days</item>
/// <item><c>Month</c> — offset in months</item>
/// <item><c>Year</c> — offset in years</item>
/// </list>
/// </para>
/// <para>
/// The factory ensures that only <see cref="DateOnly"/> values are validated. Configuration
/// errors (e.g., invalid units, non-integer offsets, or mismatched value types) are logged
/// via the injected <see cref="ILogger"/> and surfaced as <see cref="CauseType.RuleConfigError"/> 
/// failures.
/// </para>
/// </remarks>
/// <param name="getToday">
/// A function that returns the current date"/>).
/// This delegate allows unit testing and custom "today" definitions by controlling the source of truth
/// for the current date.
/// </param>
/// <param name="logger">
/// Logger instance used to record configuration errors such as invalid offsets, unsupported units,
/// or type mismatches. Logging provides visibility into tenant-specific validation issues without
/// interrupting the validation pipeline.
/// </param>
internal sealed class RollingDateOnlyValidatorFactory(Func<DateOnly> getToday, ILogger<RollingDateOnlyValidatorFactory> logger) : IValidatorFactory
{

    /// <summary>
    /// Creates a validator that checks whether a <see cref="DateOnly"/> value falls within
    /// a configured rolling date range relative to "today".
    /// </summary>
    /// <typeparam name="T">
    /// The type of value being validated. Must be <see cref="DateOnly"/> for validation
    /// to succeed; other types result in configuration errors.
    /// </typeparam>
    /// <param name="ruleConfig">
    /// The validation rule configuration specifying the minimum and maximum offsets,
    /// the unit of measure (<c>Day</c>, <c>Month</c>, or <c>Year</c>), and the failure message.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates <see cref="DateOnly"/> values
    /// against the rolling date range. Produces <see cref="CauseType.Validation"/> failures
    /// if the value lies outside the configured range, or <see cref="CauseType.RuleConfigError"/>
    /// if configuration is invalid.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull
        
        => (valueToValidate, path, _, _) =>
        {
            try
            {
                var (minDate, maxDate) = (ruleConfig.MinMaxToValueType.Split("_")[1]) switch
                {
                    "Day"   => (getToday().AddDays(int.Parse(ruleConfig.MinValue)), getToday().AddDays(int.Parse(ruleConfig.MaxValue))),
                    "Month" => (getToday().AddMonths(int.Parse(ruleConfig.MinValue)), getToday().AddMonths(int.Parse(ruleConfig.MaxValue))),
                    "Year"  => (getToday().AddYears(int.Parse(ruleConfig.MinValue)), getToday().AddYears(int.Parse(ruleConfig.MaxValue))),

                    _ => throw new ArgumentException("Rolling date unit Day, Month or Year not specified or the min max values were not convertible to integers")
                };
                
                if (valueToValidate is not DateOnly) throw new ArgumentException("Rolling date min max type should be int and the value should be date only");

                var valid = ((IComparable)valueToValidate).CompareTo(minDate) >= 0 &&
                            ((IComparable)valueToValidate).CompareTo(maxDate) <= 0;

                var failureMessage = valid ? "" : FailureMessages.FormatRollingDateMessage(ruleConfig.FailureMessage, DateOnly.Parse(valueToValidate.ToString()!).ToString("O"),ruleConfig.DisplayName,minDate.ToString("O"), maxDate.ToString("O"), getToday().ToString("O"));

                var result = valid ? Validated<T>.Valid(valueToValidate!)
                                        : Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig.PropertyName, ruleConfig.DisplayName));

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"Configuration error causing Rolling date validation failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} ValueToValidate:{ValueToValidate}",
                    ruleConfig?.TenantID        ?? "[Null]",
                    ruleConfig?.TypeFullName    ?? "[Null]",
                    ruleConfig?.PropertyName    ?? "[Null]",
                    valueToValidate?.ToString() ?? "[Null]"
                );


                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }
        };
}
