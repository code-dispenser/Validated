using Microsoft.Extensions.Logging;
using System.Globalization;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Factory for creating validators that ensure values are convertible to decimal meeting the precision and scale requirements.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrecisionScaleValidatorFactory"/> interprets <see cref="ValidationRuleConfig"/> 
/// instances where <see cref="ValidationRuleConfig.Pattern"/> contains the allowable schemes.
/// </para>
/// <para>
/// Validators created by this factory check that the value can be converted to decimal and that the precision and 
/// and scale values are less than or equal to the maximums set.
/// </para>
/// <para>
/// If the type being validated is not a string or a number that can be converted to a decimal the factory treats this as a 
/// configuration error. Such errors are logged via the injected <see cref="ILogger"/> and 
/// surfaced as <see cref="CauseType.RuleConfigError"/> failures.
/// </para>
/// </remarks>
/// <param name="logger">
/// Logger instance used to record configuration errors. Logging ensures visibility into
/// tenant-specific rule issues without interrupting the validation pipeline.
/// </param>

internal sealed class PrecisionScaleValidatorFactory(ILogger<PrecisionScaleValidatorFactory> logger) : IValidatorFactory
{
    /// <summary>
    /// Creates a validator that checks whether values are convertible to decimal and are less than or 
    /// equal to both the configured precision and scale values.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value being validated. This must a string or a number that can be 
    /// converted to a decimal. The validator does not change the original type it only
    /// ensures that it can be converted to a decimal and that it would meet the precision
    /// and scale requirements set.
    /// </typeparam>
    /// <param name="ruleConfig">
    /// The validation rule configuration containing the minimum and maximum values,
    /// along with failure messages and property metadata.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates values against the configured
    /// precision and scale values. Produces <see cref="CauseType.Validation"/> failures if the value is outside
    /// the range, or <see cref="CauseType.RuleConfigError"/> failures if configuration
    /// is invalid.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (valueToValidate, path, context, CancellationToken) =>
        {
            try
            {
                var additionalInfo = ruleConfig.AdditionalInfo ?? new Dictionary<string,string>();

                var precision = additionalInfo.TryGetValue(ValidatedConstants.RuleDictKey_Precision, out var precisionValue) ? precisionValue : "-1";
                var scale     = additionalInfo.TryGetValue(ValidatedConstants.RuleDictKey_Scale, out var scaleValue) ? scaleValue : "-1";
                
                var maxPrecision = int.TryParse(precision, out var maxPrecisionValue) ? maxPrecisionValue : -1;
                var maxScale     = int.TryParse(scale, out var maxScaleValue) ? maxScaleValue : -1;

                if (maxPrecision == -1 || maxScale == -1) 
                    return CreateInvalidWithPSFormatting<T>(valueToValidate.ToString()!,maxPrecision.ToString(), maxScale.ToString(), "", "",path,ruleConfig.PropertyName, ruleConfig.DisplayName,ruleConfig.FailureMessage);

                CultureInfo cultureInfo = String.IsNullOrWhiteSpace(ruleConfig.CultureID) ? new CultureInfo(ValidatedConstants.Default_CultureID) : new CultureInfo(ruleConfig.CultureID);

                decimal? decimalValue = valueToValidate switch
                {
                    byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal _ => Convert.ToDecimal(valueToValidate),
                    string _ => decimal.TryParse(valueToValidate.ToString(), NumberStyles.Any, cultureInfo, out var asDecimal) ? asDecimal : null,
                    _ => null,
                };

                if (false == decimalValue.HasValue)
                    return CreateInvalidWithPSFormatting<T>(valueToValidate.ToString()!, maxPrecision.ToString(), maxScale.ToString(), "", "", path, ruleConfig.PropertyName, ruleConfig.DisplayName, ruleConfig.FailureMessage);

                int actualScale = decimalValue.Value.Scale;

                string absStr     = Math.Abs(decimalValue.Value).ToString(CultureInfo.InvariantCulture);
                string digitsOnly = absStr.Replace(".", "").Replace("-", "");

                return (digitsOnly.Length <= maxPrecision && actualScale <= maxScale)
                        ? Task.FromResult(Validated<T>.Valid(valueToValidate))
                            : CreateInvalidWithPSFormatting<T>(valueToValidate.ToString()!, maxPrecision.ToString(), maxScale.ToString(), digitsOnly.Length.ToString(), actualScale.ToString(), path, ruleConfig.PropertyName, ruleConfig.DisplayName, ruleConfig.FailureMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Configuration error causing precision scale validation failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} ValueToValidate:{ValueToValidate}",
                    ruleConfig?.TenantID        ?? "[Null]",
                    ruleConfig?.TypeFullName    ?? "[Null]",
                    ruleConfig?.PropertyName    ?? "[Null]",
                    valueToValidate?.ToString() ?? "[Null]"
                );

                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }
        };


    /// <summary>
    /// Internal helper method that creates a Task of an invalid validated with decimal precision and scale failure message replacement formatting
    /// </summary>
    private static Task<Validated<T>> CreateInvalidWithPSFormatting<T>(string valueToValidate, string precision, string scale, string actualPrecision, string actualScale, string path, string propertyName, string displayName, string failureMessage) where T : notnull

        => Task.FromResult(Validated<T>
                .Invalid(new InvalidEntry(FailureMessages.FormatDecimalPrecisionScaleMessage
                (
                    failureMessage, valueToValidate.ToString()!, displayName, precision, scale, actualPrecision, actualScale), path, propertyName, displayName)
                ));
}
