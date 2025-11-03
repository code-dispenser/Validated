using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Validated.Core.Common.Constants;
using Validated.Core.Extensions;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Central registry and provider for validator factories, managing the creation and retrieval of validators based on rule configurations.
/// </summary>
/// <remarks>
/// <para>
/// The ValidatorFactoryProvider serves as the main orchestrator for the validation system's factory pattern.
/// It maintains a registry of validator factories keyed by rule type and provides methods for creating
/// validators from configurations with tenant and culture-specific rule resolution.
/// </para>
/// <para>
/// The provider includes built-in factories for all standard validation types (regex, range, string length,
/// collection length, comparisons, and rolling dates) and supports extensibility through the
/// <see cref="AddOrUpdateFactory"/> method for custom validation scenarios.
/// </para>
/// <para>
/// Rule resolution follows a hierarchical fallback strategy: specific tenant and culture combinations
/// take precedence, followed by tenant-specific defaults, and finally system-wide defaults.
/// </para>
/// </remarks>
public sealed class ValidatorFactoryProvider : IValidatorFactoryProvider
{
    private readonly ILogger<ValidatorFactoryProvider> _logger;
    private readonly ConcurrentDictionary<string, IValidatorFactory> _validationFactories;


    /// <summary>
    /// Initializes a new instance of the ValidatorFactoryProvider with all built-in validator factories pre-registered.
    /// </summary>
    /// <param name="loggerFactory">
    /// The logger factory used to create logger instances for each validator factory type.
    /// Each factory receives its own typed logger for proper error tracking and diagnostics.
    /// </param>
    /// <remarks>
    /// <para>
    /// The constructor pre-registers all standard validator factories including regex, string length,
    /// range, rolling date, comparison validators, and collection length validators. It also registers
    /// a failing validator factory as the default fallback for unknown rule types.
    /// </para>
    /// <para>
    /// All factories are initialized with their respective logger instances to ensure proper
    /// error tracking and diagnostics throughout the validation process.
    /// </para>
    /// </remarks>
    public ValidatorFactoryProvider(ILoggerFactory? loggerFactory = null)
    {
        _logger = loggerFactory?.CreateLogger<ValidatorFactoryProvider>() ?? NullLogger<ValidatorFactoryProvider>.Instance;


        _validationFactories = new()
        {
            [ValidatedConstants.RuleType_NotFound]         = new FailingValidatorFactory(),
            [ValidatedConstants.RuleType_Regex]            = new RegexValidatorFactory(loggerFactory?.CreateLogger<RegexValidatorFactory>() ?? NullLogger<RegexValidatorFactory>.Instance),
            [ValidatedConstants.RuleType_StringLength]     = new StringLengthValidatorFactory(loggerFactory?.CreateLogger<StringLengthValidatorFactory>() ?? NullLogger<StringLengthValidatorFactory>.Instance),
            [ValidatedConstants.RuleType_Range]            = new RangeValidatorFactory(loggerFactory?.CreateLogger<RangeValidatorFactory>() ?? NullLogger<RangeValidatorFactory>.Instance),
            [ValidatedConstants.RuleType_RollingDate]      = new RollingDateOnlyValidatorFactory(() => DateOnly.FromDateTime(DateTime.UtcNow), loggerFactory?.CreateLogger<RollingDateOnlyValidatorFactory>() ?? NullLogger<RollingDateOnlyValidatorFactory>.Instance),
            [ValidatedConstants.RuleType_MemberComparison] = new ComparisonValidatorFactory(loggerFactory?.CreateLogger<ComparisonValidatorFactory>() ?? NullLogger<ComparisonValidatorFactory>.Instance, ComparisonTypeFor.EntityObject),
            [ValidatedConstants.RuleType_CompareTo]        = new ComparisonValidatorFactory(loggerFactory?.CreateLogger<ComparisonValidatorFactory>() ?? NullLogger<ComparisonValidatorFactory>.Instance, ComparisonTypeFor.Value),
            [ValidatedConstants.RuleType_VOComparison]     = new ComparisonValidatorFactory(loggerFactory?.CreateLogger<ComparisonValidatorFactory>() ?? NullLogger<ComparisonValidatorFactory>.Instance, ComparisonTypeFor.ValueObject),
            [ValidatedConstants.RuleType_CollectionLength] = new CollectionLengthValidatorFactory(loggerFactory?.CreateLogger<CollectionLengthValidatorFactory>() ?? NullLogger<CollectionLengthValidatorFactory>.Instance),
            [ValidatedConstants.RuleType_UrlFormat]        = new UrlFormatValidatorFactory(loggerFactory?.CreateLogger<UrlFormatValidatorFactory>() ?? NullLogger<UrlFormatValidatorFactory>.Instance),
            [ValidatedConstants.RuleType_PrecisionScale]   = new PrecisionScaleValidatorFactory(loggerFactory?.CreateLogger<PrecisionScaleValidatorFactory>() ?? NullLogger<PrecisionScaleValidatorFactory>.Instance)
        };
    }

    /// <summary>
    /// Retrieves the appropriate validator factory for the specified rule type.
    /// </summary>
    /// <param name="ruleType">
    /// The rule type identifier that determines which factory to return. Should match one of the
    /// constants defined in <see cref="ValidatedConstants"/> (e.g., RuleType_Regex, RuleType_Range).
    /// </param>
    /// <returns>
    /// The <see cref="IValidatorFactory"/> instance registered for the specified rule type.
    /// If no factory is found for the rule type, an error is logged and the method returns the default 
    /// failing validator factory which produces validation errors indicating the rule type is not supported.
    /// </returns>
    /// <remarks>
    /// This method provides a safe fallback mechanism - it never throws exceptions for unknown
    /// rule types, instead returning a factory that creates validators which fail with appropriate
    /// error messages.
    /// </remarks>
    public IValidatorFactory GetValidatorFactory(string ruleType)
    {
        if (true == _validationFactories.TryGetValue(ruleType, out var validationFactory)) return validationFactory;

        _logger.LogError(ErrorMessages.Validator_Factory_Not_Found,ruleType);

        return _validationFactories[ValidatedConstants.RuleType_NotFound];

    }
    /// <summary>
    /// Registers or updates a validator factory for the specified rule type.
    /// </summary>
    /// <param name="ruleType">
    /// The rule type identifier to associate with the factory. This key is used by
    /// <see cref="GetValidatorFactory"/> to retrieve the appropriate factory.
    /// </param>
    /// <param name="validatorFactory">
    /// The validator factory implementation to register. This factory will be responsible
    /// for creating validators when the specified rule type is encountered.
    /// </param>
    /// <remarks>
    /// This method enables extensibility by allowing custom validator factories to be registered
    /// for new rule types or to override existing rule types with custom implementations.
    /// The operation is thread-safe using concurrent dictionary operations.
    /// </remarks>
    public void AddOrUpdateFactory(string ruleType, IValidatorFactory validatorFactory)

        => _validationFactories[ruleType] = validatorFactory;


    private MemberValidator<T> BuildValidator<T>(List<ValidationRuleConfig> buildFor, string typeFullName, string propertyName) where T : notnull
    {
        if (buildFor.Count == 0) return (memberValue, path, _, _) =>
        {
            _logger.LogError(null, ErrorMessages.Validator_No_Rules_Error_Message, String.Concat(typeFullName, ".", propertyName));
            return Task.FromResult(Validated<T>.Valid(memberValue));//function just to say its valid given no rules, validated will fail it if null
        };

        List<MemberValidator<T>> validators = [];
        ValidationRuleConfig ruleConfig = default!;

        try
        {
            foreach (var rule in buildFor)
            {
                ruleConfig = rule;
                validators.Add(this.GetValidatorFactory(rule.RuleType).CreateFromConfiguration<T>(rule));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.Validator_Creation_Error_Message, ruleConfig);
            return (_, path, _, _) => Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ErrorMessages.Validator_Creation_Failure_User_Message, path, ruleConfig.PropertyName, ruleConfig.DisplayName, CauseType.SystemError)));
        }

        return validators.Aggregate((current, next) => current.AndThen(next));

    }

    /// <summary>
    /// Creates a composite member validator from rule configurations, applying tenant and culture-specific rule resolution.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value to validate. Must be a non-null reference type and should be compatible
    /// with the validation rules defined in the configurations.
    /// </typeparam>
    /// <param name="typeFullName">
    /// The full type name of the entity containing the property being validated.
    /// Used for rule matching and configuration lookup.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property being validated within the specified type.
    /// Combined with typeFullName to uniquely identify validation rules.
    /// </param>
    /// <param name="configurations">
    /// The complete set of validation rule configurations available to the system.
    /// These are filtered and processed to find applicable rules for the specified property.
    /// </param>
    /// <param name="tenantID">
    /// The tenant identifier for multi-tenant rule resolution. Defaults to the system default tenant.
    /// Enables tenant-specific validation behaviour and rule customization.
    /// </param>
    /// <param name="cultureID">
    /// The culture identifier for localized validation rules and messages. Defaults to en-GB.
    /// Supports culture-specific validation rules and localized error messages.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that combines all applicable validation rules for the
    /// specified property. If no rules are found, returns a validator that logs the absence
    /// but allows the value to pass validation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method orchestrates the entire validator creation process: filtering configurations,
    /// resolving tenant/culture precedence, creating individual validators from factories,
    /// and combining them into a single composite validator using the AndThen pattern.
    /// </para>
    /// <para>
    /// Rule resolution follows a priority hierarchy ensuring the most specific applicable
    /// rule is selected while maintaining fallback behaviour for broader configurations.
    /// </para>
    /// </remarks>
    public MemberValidator<T> CreateValidator<T>(string typeFullName, string propertyName, ImmutableList<ValidationRuleConfig> configurations,
                                                        string tenantID = ValidatedConstants.Default_TenantID, string cultureID = ValidatedConstants.Default_CultureID) where T : notnull

        => GetTenantAndCultureConfigs<T>(typeFullName, propertyName, configurations, tenantID, cultureID)
                .Pipe(ruleConfig => BuildValidator<T>(ruleConfig, typeFullName, propertyName));


    /// <summary>
    /// Filters and prioritizes validation rule configurations based on tenant, culture, and version precedence.
    /// </summary>
    /// <typeparam name="T">
    /// The type being validated, used to determine the appropriate target type classification
    /// (item vs collection) for rule filtering.
    /// </typeparam>
    /// <param name="typeFullName">
    /// The full type name of the entity containing the property. Used as the primary filter
    /// criterion for matching applicable validation rules.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property within the type. Combined with typeFullName to uniquely
    /// identify the validation target.
    /// </param>
    /// <param name="configurations">
    /// The complete set of available validation rule configurations to filter and process.
    /// </param>
    /// <param name="tenantID">
    /// The tenant identifier for rule resolution. Used to find tenant-specific overrides
    /// of default validation rules.
    /// </param>
    /// <param name="cultureID">
    /// The culture identifier for localized rule selection. Enables culture-specific
    /// validation behaviour and error messages.
    /// </param>
    /// <returns>
    /// A filtered and prioritized list of <see cref="ValidationRuleConfig"/> instances that apply
    /// to the specified property. Rules are deduplicated and the most specific applicable
    /// version is selected for each unique rule pattern.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The filtering process first matches on type and property, then groups rules by their
    /// functional content (pattern, ranges, comparison values) to avoid duplicates. Within
    /// each rule group, the latest version is selected, then tenant/culture precedence is applied.
    /// </para>
    /// <para>
    /// Precedence order: Specific tenant + culture → Tenant + default culture → Default tenant + default culture.
    /// This ensures maximum flexibility for tenant customization while maintaining reliable fallbacks.
    /// </para>
    /// </remarks>
    internal List<ValidationRuleConfig> GetTenantAndCultureConfigs<T>(string typeFullName, string propertyName, ImmutableList<ValidationRuleConfig> configurations,
                                                                  string tenantID = ValidatedConstants.Default_TenantID, string cultureID = ValidatedConstants.Default_CultureID)
    {

        var targetType = DetermineTargetType<T>();
 
        return configurations.Where(c => c.TypeFullName == typeFullName && c.PropertyName == propertyName && c.TargetType == targetType)
                .GroupBy(c => new { c.RuleType, c.Pattern, c.MinLength, c.MaxLength, c.MinValue, c.MaxValue, c.CompareValue, c.ComparePropertyName, c.CompareType })  // Group by uniquely identifying rule content (NOT by tenant/culture)
                .Select(ruleGroup =>
                {
                    var latestVersionConfigs = ruleGroup.GroupBy(c => c.Version).OrderByDescending(versionGroup => versionGroup.Key).First().ToList(); // First: Get the latest version within this rule group

                    var specificTenantAndCulture = latestVersionConfigs.FirstOrDefault(config => config.TenantID == tenantID && !String.IsNullOrWhiteSpace(tenantID)  && config.CultureID == cultureID && !String.IsNullOrEmpty(cultureID));

                    if (specificTenantAndCulture != null) return specificTenantAndCulture;

                    var tenantSpecificDefaultCulture = latestVersionConfigs.FirstOrDefault(config => config.TenantID == tenantID && !String.IsNullOrWhiteSpace(tenantID) && config.CultureID == ValidatedConstants.Default_CultureID);

                    if (tenantSpecificDefaultCulture != null) return tenantSpecificDefaultCulture;

                    return latestVersionConfigs.FirstOrDefault(config => config.TenantID == ValidatedConstants.Default_TenantID && config.CultureID == ValidatedConstants.Default_CultureID);

                })
                .Where(config => config is not null).ToList()!;

    }


    private static string DetermineTargetType<T>()
    {
        if (typeof(T).IsAssignableTo(typeof(IEnumerable)) && typeof(T) != typeof(string)) return ValidatedConstants.TargetType_Collection;

        return ValidatedConstants.TargetType_Item;
    }
}



















