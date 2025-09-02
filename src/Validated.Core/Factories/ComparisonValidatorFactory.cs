using Microsoft.Extensions.Logging;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Factory responsible for creating validators that perform comparison-based validation.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ComparisonValidatorFactory"/> supports three types of comparisons:
/// <list type="bullet">
/// <item><see cref="ComparisonTypeFor.EntityObject"/> - compares values of two entity members.</item>
/// <item><see cref="ComparisonTypeFor.Value"/> - compares a single value against a configured constant.</item>
/// <item><see cref="ComparisonTypeFor.ValueObject"/> - compares value objects directly.</item>
/// </list>
/// </para>
/// <para>
/// The factory interprets comparison rules from <see cref="ValidationRuleConfig"/> and produces
/// <see cref="MemberValidator{T}"/> instances that implement the appropriate logic. Configuration
/// errors are logged and converted into validation failures with <see cref="CauseType.RuleConfigError"/>.
/// </para>
/// </remarks>
/// <param name="logger">
/// Logger instance used to record configuration errors or unexpected failures encountered during 
/// validator creation or execution. This ensures tenant-specific misconfigurations are traceable
/// without interrupting validation flow.
/// </param>
/// <param name="forType">
/// Indicates the comparison strategy to apply. Determines whether the validator will compare:
/// <list type="bullet">
/// <item>Two members of the same entity (<see cref="ComparisonTypeFor.EntityObject"/>)</item>
/// <item>A value against a constant (<see cref="ComparisonTypeFor.Value"/>)</item>
/// <item>A value object against another value (<see cref="ComparisonTypeFor.ValueObject"/>)</item>
/// </list>
/// </param>
internal sealed class ComparisonValidatorFactory(ILogger<ComparisonValidatorFactory> logger, ComparisonTypeFor forType) : IValidatorFactory 
{

    /// <summary>
    /// Creates a validator for the given type <typeparamref name="T"/> based on the specified rule configuration.
    /// </summary>
    /// <typeparam name="T">The type of the value or entity being validated.</typeparam>
    /// <param name="ruleConfig">The validation rule configuration that defines the comparison type, target values, and error messages.</param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates values using the configured comparison rules.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => forType switch
        {
            ComparisonTypeFor.EntityObject  => CompareMemberValues<T>(ruleConfig),
            ComparisonTypeFor.Value         => CompareValues<T>(ruleConfig),
            ComparisonTypeFor.ValueObject   => CompareVOValues<T>(ruleConfig),
            _                               => CompareValues<T>(ruleConfig),
        };


    /// <summary>
    /// Creates a validator that compares two members of the same entity instance.
    /// </summary>
    /// <typeparam name="T">The entity type whose members will be compared.</typeparam>
    /// <param name="ruleConfig">The validation rule configuration specifying the two members and comparison type.</param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates whether the comparison between the two members satisfies the rule.
    /// </returns>
    internal MemberValidator<T> CompareMemberValues<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (entity, path, _, _) =>
        {
            object valueToValidate = default!;

            try
            {
                var entityType        = typeof(T);
                var valueProperty     = entityType.GetProperty(ruleConfig.PropertyName);
                
                valueToValidate       = valueProperty!.GetValue(entity)!;

                var compareToProperty = entityType.GetProperty(ruleConfig.ComparePropertyName);
                var compareToValue    = compareToProperty!.GetValue(entity);

                var (result, cause) = PerformComparison(valueToValidate, compareToValue, ruleConfig.CompareType, ruleConfig);

                var failureMessage = result ? "" : FailureMessages.FormatCompareValueMessage(ruleConfig.FailureMessage, GeneralUtils.FromValue(valueToValidate), ruleConfig.DisplayName, GeneralUtils.FromValue(compareToValue));

                var validated = result ? Validated<T>.Valid(entity)
                                            : Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig.PropertyName, ruleConfig.DisplayName,  CauseType.Validation));
                
                return Task.FromResult(validated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"Configuration error causing comparison failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} - ComparePropertyName:{ComparePropertyName} ValueToValidate:{ValueToValidate}",
                    ruleConfig?.TenantID            ?? "[Null]",
                    ruleConfig?.TypeFullName        ?? "[Null]",
                    ruleConfig?.PropertyName        ?? "[Null]",
                    ruleConfig?.ComparePropertyName ?? "[Null]",
                    valueToValidate?.ToString()     ?? "[Null]"
                );


                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "",  CauseType.RuleConfigError)));
            }
        };

    /// <summary>
    /// Creates a validator that compares a value against a fixed comparison value defined in the rule configuration.
    /// </summary>
    /// <typeparam name="T">The type of the value being validated.</typeparam>
    /// <param name="ruleConfig">The validation rule configuration specifying the comparison value and type.</param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates the value against the configured comparison value.
    /// </returns>
    internal MemberValidator<T> CompareValues<T>(ValidationRuleConfig ruleConfig) where T : notnull
        
        => (entity, path,_, _) =>
        {
            object valueToValidate = default!;

            try
            {
                var entityType    = typeof(T);
                var valueProperty = entityType.GetProperty(ruleConfig.PropertyName);

                valueToValidate  = valueProperty!.GetValue(entity)!;

                var systemType   = Type.GetType($"System.{ruleConfig.MinMaxToValueType.Split("_")[1]}")!;

                IComparable compareTo = systemType switch
                {
                    _ when systemType == typeof(DateOnly)   => (IComparable)DateOnly.Parse(ruleConfig.CompareValue),
                    _ when systemType == typeof(DateTime)   => DateTime.Parse(ruleConfig.CompareValue),
                    _ when systemType == typeof(Guid)       => Guid.Parse(ruleConfig.CompareValue),
                    _ when systemType == typeof(TimeSpan)   => TimeSpan.Parse(ruleConfig.CompareValue),

                    _ => (IComparable)Convert.ChangeType(ruleConfig.CompareValue, systemType)
                };

                var (result, cause) = PerformComparison(valueToValidate, compareTo, ruleConfig.CompareType, ruleConfig);

                var failureMessage = result ? "" : FailureMessages.FormatCompareValueMessage(ruleConfig.FailureMessage, GeneralUtils.FromValue(valueToValidate), ruleConfig.DisplayName, GeneralUtils.FromValue(compareTo));

                var validated = result ? Validated<T>.Valid(entity)
                                            : Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig.PropertyName, ruleConfig.DisplayName,  cause));

                return Task.FromResult(validated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"Configuration error causing comparison failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} - CompareValue:{CompareValue} MinMaxToValueType:{MinMaxToValueType} ValueToValidate:{ValueToValidate}",
                    ruleConfig?.TenantID          ?? "[Null]",
                    ruleConfig?.TypeFullName      ?? "[Null]",
                    ruleConfig?.PropertyName      ?? "[Null]",
                    ruleConfig?.CompareValue      ?? "[Null]",
                    ruleConfig?.MinMaxToValueType ?? "[Null]",
                    valueToValidate?.ToString()   ?? "[Null]"
                );

                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }
        };


    /// <summary>
    /// Creates a validator that compares value objects against a provided comparison value.
    /// </summary>
    /// <typeparam name="T">The value object type being validated.</typeparam>
    /// <param name="ruleConfig">The validation rule configuration specifying the comparison rules.</param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates value objects according to the configured comparison rules.
    /// </returns>
    internal MemberValidator<T> CompareVOValues<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (valueToValidate, path, compareTo, _) =>
        {
            try
            {
                if (compareTo is null) throw new ArgumentNullException(nameof(compareTo), "The compare to value should not be null.");

                var (result, cause) = PerformComparison(valueToValidate, compareTo, ruleConfig.CompareType, ruleConfig);

                var failureMessage = result ? "" : FailureMessages.FormatCompareValueMessage(ruleConfig.FailureMessage, GeneralUtils.FromValue(valueToValidate),ruleConfig.DisplayName, GeneralUtils.FromValue(compareTo));

                var validated = result ? Validated<T>.Valid(valueToValidate)
                                            : Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig.PropertyName, ruleConfig.DisplayName, cause));
                
                return Task.FromResult(validated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Configuration error causing comparison failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} - CompareType:{CompareType} ValueToValidate:{ValueToValidate} CompareToValue:{CompareToValue}",
                    ruleConfig?.TenantID        ?? "[Null]",
                    ruleConfig?.TypeFullName    ?? "[Null]",
                    ruleConfig?.PropertyName    ?? "[Null]",
                    ruleConfig?.CompareType     ?? "[Null]",
                    valueToValidate?.ToString() ?? "[Null]",
                    compareTo?.ToString()       ?? "[Null]"
                );

                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }
        };



    /// <summary>
    /// Performs the actual comparison between two values using the specified comparison type.
    /// </summary>
    /// <param name="leftValue">The left-hand value in the comparison.</param>
    /// <param name="rightValue">The right-hand value in the comparison.</param>
    /// <param name="compareType">The comparison type (e.g., equal, greater than, less than).</param>
    /// <param name="ruleConfig">The rule configuration for logging and context.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><c>result</c>: A boolean indicating whether the comparison succeeded.</item>
    /// <item><c>cause</c>: A <see cref="CauseType"/> indicating whether failure was due to validation or configuration.</item>
    /// </list>
    /// </returns>
    internal (bool result, CauseType cause) PerformComparison(object? leftValue, object? rightValue, string? compareType, ValidationRuleConfig ruleConfig)//Only way to cover every path was to have the compareType as nullable.
    {
        if (leftValue == null || rightValue == null) return (false, CauseType.Validation);

        if (leftValue is IComparable leftComparable && rightValue is IComparable)
        {
            var comparisonResult = leftComparable.CompareTo(rightValue);

            return compareType switch
            {
                ValidatedConstants.CompareType_EqualTo              => (comparisonResult == 0, CauseType.Validation),
                ValidatedConstants.CompareType_NotEqualTo           => (comparisonResult != 0, CauseType.Validation),
                ValidatedConstants.CompareType_GreaterThan          => (comparisonResult > 0, CauseType.Validation),
                ValidatedConstants.CompareType_LessThan             => (comparisonResult < 0, CauseType.Validation),
                ValidatedConstants.CompareType_GreaterThanOrEqual   => (comparisonResult >= 0, CauseType.Validation),
                ValidatedConstants.CompareType_LessThanOrEqual      => (comparisonResult <= 0, CauseType.Validation),

                _ => LogErrorAndReturnFalse(compareType, ruleConfig)
            };
        }

        return compareType switch
        {
            ValidatedConstants.CompareType_EqualTo      => (leftValue.Equals(rightValue), CauseType.Validation),
            ValidatedConstants.CompareType_NotEqualTo   => (!leftValue.Equals(rightValue), CauseType.Validation),
            _ => (false, CauseType.Validation) // Can't do ordering comparisons without IComparable
        };

        (bool, CauseType) LogErrorAndReturnFalse(string? compareType, ValidationRuleConfig ruleConfig)
        {

            logger.LogError("Configuration error causing comparison failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} InvalidComparisonType:{CompareType}",
                ruleConfig?.TenantID     ?? "[Null]",
                ruleConfig?.TypeFullName ?? "[Null]",
                ruleConfig?.PropertyName ?? "[Null]",
                compareType
            );

            return (false,CauseType.RuleConfigError);
        }
    }
    
}
