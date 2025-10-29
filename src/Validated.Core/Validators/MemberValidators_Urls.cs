using System.Data;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Validators;

public static partial class MemberValidators
{
    /// <summary>
    /// Creates a validator that checks whether a string value is a valid URL using the specified allowable schemes.
    /// </summary>
    /// <param name="allowableSchemes">The URL schemes (e.g., HTTP, HTTPS) allowed for validation.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation messages.</param>
    /// <param name="failureMessage">The message returned when the value is not a valid URL.</param>
    /// <returns>A member validator that checks URL format, host presence and allowed scheme.</returns>

    public static MemberValidator<String> CreateUrlValidator(UrlSchemeTypes allowableSchemes, string propertyName, string displayName, string failureMessage)

        => (valueToValidate, path, _, _) =>
        {
            if (valueToValidate is null) return CreateInvalidWithDefaultFormatting<string>(valueToValidate ?? "", path, propertyName, displayName, failureMessage);

            var absoluteUrl = Uri.TryCreate(valueToValidate,UriKind.Absolute, out var url) ? url : null;

            if (absoluteUrl is null) return CreateInvalidWithDefaultFormatting<string>(valueToValidate, path, propertyName, displayName, failureMessage);

            var schemeType = Enum.TryParse<UrlSchemeTypes>(absoluteUrl.Scheme, true, out var urlScheme) ? urlScheme : UrlSchemeTypes.None;

            if (schemeType is UrlSchemeTypes.None || allowableSchemes is UrlSchemeTypes.None || String.IsNullOrWhiteSpace(absoluteUrl.Host)) return CreateInvalidWithDefaultFormatting<string>(valueToValidate, path, propertyName, displayName, failureMessage);

            if (false == allowableSchemes.HasFlag(schemeType)) return CreateInvalidWithDefaultFormatting<string>(valueToValidate, path, propertyName, displayName, failureMessage);

            return Task.FromResult(Validated<string>.Valid(valueToValidate));
        };

}
