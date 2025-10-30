using Microsoft.Extensions.Logging;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Factory for creating validators that ensure string values meet Url constraints.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="UrlFormatValidatorFactory"/> interprets <see cref="ValidationRuleConfig"/> 
/// instances where <see cref="ValidationRuleConfig.Pattern"/> contains the allowable schemes.
/// </para>
/// <para>
/// Validators created by this factory check that the url scheme is one of the allowable defined scheme types
/// contained in the Pattern property. If the scheme is not valid or the host is empty then the url is invalid
/// </para>
/// <para>
/// If the type being validated is not <see cref="string"/> or <see cref="Uri"/>, the factory treats this as a 
/// configuration error. Such errors are logged via the injected <see cref="ILogger"/> and 
/// surfaced as <see cref="CauseType.RuleConfigError"/> failures.
/// </para>
/// </remarks>
/// <param name="logger">
/// Logger instance used to record configuration errors. Logging ensures visibility into
/// tenant-specific rule issues without interrupting the validation pipeline.
/// </param>
internal sealed class UrlFormatValidatorFactory(ILogger<UrlFormatValidatorFactory> logger) : IValidatorFactory
{

    /// <summary>
    /// Creates a validator that checks whether the value meets the configured constraints.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value being validated. Must be <see cref="string"/>  or <see cref="Uri"/>; other types
    /// result in configuration errors.
    /// </typeparam>
    /// <param name="ruleConfig">
    /// The validation rule configuration specifying the allowable Url scheme
    /// along with the failure message and property metadata.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that validates string and Uri values against the configured
    /// constraints. Produces <see cref="CauseType.Validation"/> for normal invalid data 
    /// or <see cref="CauseType.RuleConfigError"/> if the configuration data is invalid.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (valueToValidate, path, _, _) =>
        {
            try
            {

                if (typeof(T) != typeof(string) && typeof(T) != typeof(Uri)) throw new ArgumentException("The value passed in must be of type string or Uri");

                var failureMessage = string.Empty; 

                if (valueToValidate is null) 
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(ruleConfig.FailureMessage, "", ruleConfig.DisplayName),path,ruleConfig.PropertyName,ruleConfig.DisplayName)));

                var stringUrl   = valueToValidate.ToString()!;
                var absoluteUrl = Uri.TryCreate(stringUrl, UriKind.Absolute, out var url) ? url : null;

                if(absoluteUrl is null)
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(ruleConfig.FailureMessage, stringUrl, ruleConfig.DisplayName), path, ruleConfig.PropertyName, ruleConfig.DisplayName)));

                var schemeType = Enum.TryParse<UrlSchemeTypes>(absoluteUrl.Scheme, true, out var urlScheme) ? urlScheme : UrlSchemeTypes.None;
                
                if (schemeType == UrlSchemeTypes.None)
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(ruleConfig.FailureMessage, stringUrl, ruleConfig.DisplayName), path, ruleConfig.PropertyName, ruleConfig.DisplayName)));

                if (String.IsNullOrWhiteSpace(absoluteUrl.Host))
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(ruleConfig.FailureMessage, stringUrl, ruleConfig.DisplayName), path, ruleConfig.PropertyName, ruleConfig.DisplayName)));

                var schemes = String.IsNullOrWhiteSpace(ruleConfig.Pattern) ? UrlSchemeTypes.None.ToString() : ruleConfig.Pattern;

                var splitSchemes = schemes.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                var allowableSchemes = UrlSchemeTypes.None;

                foreach(var scheme in splitSchemes)
                {
                    allowableSchemes |= Enum.TryParse<UrlSchemeTypes>(scheme, true, out var schemeFlag) ? schemeFlag : UrlSchemeTypes.None;
                }

                if (false == allowableSchemes.HasFlag(schemeType))
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(FailureMessages.Format(ruleConfig.FailureMessage, stringUrl, ruleConfig.DisplayName), path, ruleConfig.PropertyName, ruleConfig.DisplayName)));

                return Task.FromResult(Validated<T>.Valid(valueToValidate));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Configuration error causing Url validation failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName} ValueToValidate:{ValueToValidate}",
                    ruleConfig?.TenantID        ?? "[Null]",
                    ruleConfig?.TypeFullName    ?? "[Null]",
                    ruleConfig?.PropertyName    ?? "[Null]",
                    valueToValidate?.ToString() ?? "[Null]"
                );


                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }

        };

}
