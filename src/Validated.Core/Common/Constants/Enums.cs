namespace Validated.Core.Common.Constants;

/// <summary>
/// Specifies the type of comparison a validator should perform.
/// </summary>
public enum ComparisonTypeFor
{
    /// <summary>
    /// Compares a single value against a constant defined in the rule configuration.
    /// </summary>
    Value = 0,

    /// <summary>
    /// Compares one prospective value to another.
    /// </summary>
    ValueObject = 1,

    /// <summary>
    /// Compares the values of two members on the same entity instance.
    /// </summary>
    EntityObject = 2,


}
/// <summary>
/// Defines the types of comparison operations that can be performed.
/// </summary>
/// <remarks>
/// This enumeration provides a clear and type-safe way to specify
/// the comparison logic for validation rules.
/// </remarks>
public enum CompareType
{
    /// <summary>
    /// Specifies an equality comparison.
    /// </summary>
    EqualTo,

    /// <summary>
    /// Specifies an inequality comparison.
    /// </summary>
    NotEqualTo,

    /// <summary>
    /// Specifies a "greater than" comparison.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Specifies a "less than" comparison.
    /// </summary>
    LessThan,

    /// <summary>
    /// Specifies a "greater than or equal to" comparison.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Specifies a "less than or equal to" comparison.
    /// </summary>
    LessThanOrEqual
}

/// <summary>
/// Indicates the cause or source of a validation failure.
/// </summary>
public enum CauseType
{
    /// <summary>
    /// Validation failed due to user input or data not meeting requirements.
    /// </summary>
    Validation,

    /// <summary>
    /// Validation failed because of a misconfiguration in the rule definition.
    /// </summary>
    RuleConfigError,

    /// <summary>
    /// Validation failed due to a system error, such as exceeding recursion depth.
    /// </summary>
    SystemError
}

/// <summary>
///  Allowable url bit flag schemes for the URL validator.
/// </summary>
[Flags]
public enum UrlSchemeTypes : int
{
    /// <summary>
    /// no scheme.
    /// </summary>
    None = 0,
    /// <summary>
    /// allows the http scheme.
    /// </summary>
    Http = 1,
    /// <summary>
    /// allows the https scheme.
    /// </summary>
    Https = 2,
    /// <summary>
    /// allows the ftp scheme.
    /// </summary>
    Ftp = 4,
    /// <summary>
    /// allows the ftps scheme.
    /// </summary>
    Ftps = 8

    
}