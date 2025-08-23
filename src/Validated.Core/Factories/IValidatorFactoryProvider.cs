using System.Collections.Immutable;
using Validated.Core.Common.Constants;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Defines a contract for managing and retrieving <see cref="IValidatorFactory"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IValidatorFactoryProvider"/> acts as the central registry for validator factories. 
/// It is responsible for:
/// <list type="bullet">
/// <item>Resolving factories by rule type (via <see cref="GetValidatorFactory"/>)</item>
/// <item>Adding or updating factories at runtime (via <see cref="AddOrUpdateFactory"/>)</item>
/// <item>Creating validators for specific entity members based on tenant and culture-specific rule configurations</item>
/// </list>
/// </para>
/// <para>
/// This abstraction allows tenant-driven validation rules to be mapped to concrete validator factories,
/// enabling flexible and extensible validation logic without modifying core validation components.
/// </para>
/// </remarks>
public interface IValidatorFactoryProvider
{
    /// <summary>
    /// Retrieves a registered <see cref="IValidatorFactory"/> for the specified rule type.
    /// </summary>
    /// <param name="ruleType">The rule type identifier, typically matching <see cref="ValidationRuleConfig.RuleType"/>.</param>
    /// <returns>
    /// The <see cref="IValidatorFactory"/> registered for the given rule type.
    /// If no factory is found, the provider should return a fallback (e.g., <c>FailingValidatorFactory</c>).
    /// </returns>
    IValidatorFactory GetValidatorFactory(string ruleType);

    /// <summary>
    /// Registers or replaces a validator factory for the specified rule type.
    /// </summary>
    /// <param name="ruleType">The rule type identifier used to resolve the factory at runtime.</param>
    /// <param name="validatorFactory">The validator factory to be added or updated.</param>
    void AddOrUpdateFactory(string ruleType, IValidatorFactory validatorFactory);

    /// <summary>
    /// Creates a <see cref="MemberValidator{T}"/> for a given entity member,
    /// using tenant and culture-specific configurations.
    /// </summary>
    /// <typeparam name="T">The type of the member to be validated. Must be non-nullable.</typeparam>
    /// <param name="typeFullName">The full type name of the entity containing the member.</param>
    /// <param name="propertyName">The name of the property/member being validated.</param>
    /// <param name="configurations">
    /// A list of available validation rule configurations that may apply to the member.
    /// </param>
    /// <param name="tenantID">
    /// The tenant identifier. If not specified, defaults to <see cref="ValidatedConstants.Default_TenantID"/>.
    /// </param>
    /// <param name="cultureID">
    /// The culture identifier. If not specified, defaults to <see cref="ValidatedConstants.Default_CultureID"/>.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> built from the applicable rule configurations,
    /// or a validator that fails gracefully if no valid rules are found.
    /// </returns>
    MemberValidator<T> CreateValidator<T>(string typeFullName, string propertyName, ImmutableList<ValidationRuleConfig> configurations,
                                          string tenantID = ValidatedConstants.Default_TenantID, string cultureID = ValidatedConstants.Default_CultureID) where T : notnull;



}