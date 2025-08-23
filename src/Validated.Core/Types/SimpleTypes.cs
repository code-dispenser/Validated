using Validated.Core.Common.Constants;
using Validated.Core.Factories;

namespace Validated.Core.Types;

/// <summary>
/// Represents an invalid entry encountered during a validation or processing operation.
/// </summary>
/// <remarks>This structure encapsulates details about an invalid entry, including a failure message, its location, the property
/// involved, a user-friendly display name,  and the cause of the failure.</remarks>
/// <param name="FailureMessage"></param>
/// <param name="Path"></param>
/// <param name="PropertyName"></param>
/// <param name="DisplayName"></param>

/// <param name="Cause"></param>
public readonly record struct InvalidEntry(string FailureMessage, string Path ="", string PropertyName="", string DisplayName="", CauseType Cause = CauseType.Validation);


/// <summary>
/// Represents a version identifier for validation purposes, consisting of major, minor, and patch components, along
/// with a creation timestamp.
/// </summary>
/// <remarks>This struct is immutable and provides a way to compare versions based on their components in the
/// order of major, minor, patch, and creation timestamp. It is useful for scenarios where versioning is required to
/// track changes or updates in a validation process.</remarks>
public readonly record struct ValidationVersion : IComparable<ValidationVersion>
{
    /// <summary>
    /// Gets the major version number.
    /// </summary>
    public int Major { get; }

    /// <summary>
    /// Gets the minor version number.
    /// </summary>
    public int Minor { get; }

    /// <summary>
    /// Gets the patch version number.
    /// </summary>
    public int Patch { get; }

    /// <summary>
    /// Gets the timestamp when this version was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationVersion struct with the specified version components and creation timestamp.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number.</param>
    /// <param name="patch">The patch version number.</param>
    /// <param name="createdAt">The timestamp when this version was created.</param>
    public ValidationVersion(int major, int minor, int patch, DateTime createdAt)

        => (Major, Minor, Patch, CreatedAt) = (major,minor,patch,createdAt);

    /// <summary>
    /// Compares this version with another ValidationVersion instance.
    /// </summary>
    /// <param name="other">The ValidationVersion to compare with this instance.</param>
    /// <returns>A value less than zero if this version is earlier, zero if equal, or greater than zero if this version is later.</returns>
    public int CompareTo(ValidationVersion other)
    {
        int result;

        if ((result = Major.CompareTo(other.Major)) != 0) return result;
        if ((result = Minor.CompareTo(other.Minor)) != 0) return result;
        if ((result = Patch.CompareTo(other.Patch)) != 0) return result;

        return CreatedAt.CompareTo(other.CreatedAt);//else return tie breaker
    }

    /// <summary>
    /// Returns a string representation of the version in the format "Major.Minor.Patch".
    /// </summary>
    /// <returns>A string representing the version number.</returns>
    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}

/// <summary>
/// Represents a configuration rule that defines validation behaviour for a specific property of an entity type.
/// This record encapsulates all the information needed to create and execute validation rules in a multi-tenant,
/// multi-culture environment with versioning support.
/// </summary>
/// <param name="TypeFullName">
/// The full type name (including namespace) of the entity that owns the property being validated.
/// Example: "MyApp.Models.User" or "System.String".
/// </param>
/// <param name="PropertyName">
/// The name of the property to be validated. This should match exactly with the property name in the entity.
/// For nested properties or special cases, this represents the immediate property being validated.
/// </param>
/// <param name="DisplayName">
/// A human-readable name for the property used in validation error messages and user interfaces.
/// This allows for localized or more user-friendly property names (e.g., "Email Address" instead of "EmailAddress").
/// </param>
/// <param name="RuleType">
/// Identifies the type of validation rule to be applied. This corresponds to constants in <see cref="ValidatedConstants"/>
/// such as RuleType_Regex, RuleType_StringLength, RuleType_Range, etc. The rule type determines which
/// <see cref="IValidatorFactory"/> will be used to create the actual validator.
/// </param>
/// <param name="MinMaxToValueType">
/// Specifies the data type context for validation rules that involve type-specific operations.
/// Used by range validators, comparison validators, and rolling date validators to determine how to
/// parse and compare values. Examples include "MinMaxToValueType_Int32", "MinMaxToValueType_DateTime", etc.
/// </param>
/// <param name="Pattern">
/// The regular expression pattern used by regex validators. For non-regex validation rules,
/// this should be an empty string. The pattern should be a valid .NET regular expression.
/// </param>
/// <param name="FailureMessage">
/// The error message template displayed when validation fails. This message can contain tokens
/// (defined in <see cref="FailureMessageTokens"/>) that will be replaced with actual values
/// during validation failure reporting. Example: "{DisplayName} must be between {MinValue} and {MaxValue}".
/// </param>
/// <param name="MinLength">
/// The minimum allowed length for string or collection validators. For other validator types,
/// this value is ignored. Used by string length and collection length validators.
/// </param>
/// <param name="MaxLength">
/// The maximum allowed length for string or collection validators. For other validator types,
/// this value is ignored. Used by string length and collection length validators.
/// </param>
/// <param name="MinValue">
/// The minimum value for range validators or the minimum offset for rolling date validators.
/// The interpretation depends on the <paramref name="MinMaxToValueType"/>. For rolling dates,
/// this represents an offset (e.g., "-30" for 30 days in the past).
/// </param>
/// <param name="MaxValue">
/// The maximum value for range validators or the maximum offset for rolling date validators.
/// The interpretation depends on the <paramref name="MinMaxToValueType"/>. For rolling dates,
/// this represents an offset (e.g., "90" for 90 days in the future).
/// </param>
/// <param name="CompareValue">
/// A literal value to compare against when using comparison validators (RuleType_CompareTo).
/// The value should be provided as a string and will be converted to the appropriate type
/// based on <paramref name="MinMaxToValueType"/> during validation.
/// </param>
/// <param name="ComparePropertyName">
/// The name of another property in the same entity to compare against when using member
/// comparison validators (RuleType_MemberComparison). Example: "ConfirmPassword" when
/// validating a "Password" property.
/// </param>
/// <param name="CompareType">
/// Specifies the type of comparison to perform for comparison validators. Values correspond
/// to constants like "CompareType_EqualTo", "CompareType_GreaterThan", etc. from <see cref="ValidatedConstants"/>.
/// </param>
/// <param name="TargetType">
/// Indicates whether the validation applies to individual items or collections.
/// Values are "TargetType_Item" for single values or "TargetType_Collection" for collections.
/// This helps the factory provider determine the appropriate validation behaviour.
/// </param>
/// <param name="TenantID">
/// Identifies the tenant for which this validation rule applies in multi-tenant applications.
/// Uses <see cref="ValidatedConstants.Default_TenantID"/> ("ALL") for rules that apply to all tenants.
/// Tenant-specific rules take precedence over default rules.
/// </param>
/// <param name="CultureID">
/// Specifies the culture/locale for which this validation rule applies, enabling localized
/// validation messages and culture-specific validation behaviour. Uses <see cref="ValidatedConstants.Default_CultureID"/>
/// ("en-GB") as the default. Culture-specific rules take precedence over default culture rules.
/// </param>
/// <param name="AdditionalInfo">
/// A dictionary for storing custom metadata or configuration values specific to certain
/// validation scenarios. This extensibility mechanism allows for custom validator factories
/// to access additional configuration without modifying the core record structure.
/// </param>
/// <param name="Version">
/// The version of this validation rule configuration, used for rule evolution and rollback scenarios.
/// When multiple versions of the same rule exist, the system will use the latest version.
/// The version includes major, minor, patch numbers, and creation timestamp for precise ordering.
/// </param>
/// <remarks>
/// <para>
/// This record serves as the central configuration unit for the validation system's rule engine.
/// It supports sophisticated scenarios including:
/// </para>
/// <list type="bullet">
/// <item><strong>Multi-tenancy:</strong> Different validation rules per tenant using <paramref name="TenantID"/></item>
/// <item><strong>Internationalization:</strong> Culture-specific rules and messages via <paramref name="CultureID"/></item>
/// <item><strong>Versioning:</strong> Evolution of validation rules over time with <paramref name="Version"/></item>
/// <item><strong>Type safety:</strong> Strongly-typed configuration through <paramref name="MinMaxToValueType"/></item>
/// <item><strong>Extensibility:</strong> Custom metadata support through <paramref name="AdditionalInfo"/></item>
/// </list>
/// <para>
/// The rule selection algorithm prioritizes configurations in the following order:
/// </para>
/// <list type="number">
/// <item>Specific tenant + specific culture (highest priority)</item>
/// <item>Specific tenant + default culture</item>
/// <item>Default tenant + default culture (lowest priority)</item>
/// </list>
/// <para>
/// Within each priority level, the latest version (as determined by <paramref name="Version"/>) is selected.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Basic string length validation rule
/// var emailLengthRule = new ValidationRuleConfig(
///     TypeFullName: "MyApp.Models.User",
///     PropertyName: "Email",
///     DisplayName: "Email Address",
///     RuleType: ValidatedConstants.RuleType_StringLength,
///     MinMaxToValueType: ValidatedConstants.MinMaxToValueType_String,
///     Pattern: "",
///     FailureMessage: "{DisplayName} must be between {MinLength} and {MaxLength} characters",
///     MinLength: 5,
///     MaxLength: 100
/// );
/// 
/// // Range validation with comparison
/// var ageRangeRule = new ValidationRuleConfig(
///     TypeFullName: "MyApp.Models.User",
///     PropertyName: "Age",
///     DisplayName: "Age",
///     RuleType: ValidatedConstants.RuleType_Range,
///     MinMaxToValueType: ValidatedConstants.MinMaxToValueType_Int32,
///     Pattern: "",
///     FailureMessage: "{DisplayName} must be between {MinValue} and {MaxValue}",
///     MinLength: 0,
///     MaxLength: 0,
///     MinValue: "0",
///     MaxValue: "120"
/// );
/// 
/// // Tenant-specific rule with versioning
/// var tenantSpecificRule = new ValidationRuleConfig(
///     TypeFullName: "MyApp.Models.Product",
///     PropertyName: "Price",
///     DisplayName: "Price",
///     RuleType: ValidatedConstants.RuleType_Range,
///     MinMaxToValueType: ValidatedConstants.MinMaxToValueType_Decimal,
///     Pattern: "",
///     FailureMessage: "Price must be between {MinValue} and {MaxValue} for premium customers",
///     MinLength: 0,
///     MaxLength: 0,
///     MinValue: "10.00",
///     MaxValue: "10000.00",
///     TenantID: "PREMIUM_TENANT",
///     CultureID: "en-US",
///     Version: new ValidationVersion(1, 2, 0, DateTime.UtcNow)
/// );
/// </code>
/// </example>
public record class ValidationRuleConfig(string TypeFullName, string PropertyName, string DisplayName, string RuleType, string MinMaxToValueType, string Pattern, string FailureMessage,
                                        int MinLength, int MaxLength, string MinValue = "", string MaxValue = "", string CompareValue = "", string ComparePropertyName = "", string CompareType = "",
                                        string TargetType = ValidatedConstants.TargetType_Item, string TenantID = ValidatedConstants.Default_TenantID, string CultureID = ValidatedConstants.Default_CultureID, 
                                         Dictionary<string,string>? AdditionalInfo = null, ValidationVersion Version = default);


/// <summary>
/// Represents configuration options for validation operations, including recursion depth limits.
/// </summary>
/// <remarks>This struct provides options to control the behaviour of validation processes, such as limiting the
/// maximum recursion depth to prevent excessive nesting. If <see cref="MaxRecursionDepth"/> is not explicitly set, a
/// default value is used.</remarks>
public readonly record struct ValidationOptions
{
    private readonly int _maxDepth;

    /// <summary>
    /// Gets or sets the maximum recursion depth allowed during validation.
    /// </summary>
    /// <remarks>This property determines the maximum depth to which recursive operations are allowed to
    /// proceed. Setting this value to 0 will use the default depth defined by the system.</remarks>
    public int MaxRecursionDepth 
    { 
      get => _maxDepth == 0 ? ValidatedConstants.ValidationOptions_MaxDepth : _maxDepth; 
      init => _maxDepth = value; 
    }

    /// <summary>
    /// Represents configuration options for validation operations.
    /// </summary>
    /// <remarks>This class provides a way to configure settings that influence how validation is
    /// performed.</remarks>
    public ValidationOptions() { }
}