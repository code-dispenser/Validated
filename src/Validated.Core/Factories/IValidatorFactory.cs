using Validated.Core.Common.Constants;
using Validated.Core.Types;

namespace Validated.Core.Factories;


/// <summary>
/// Defines a contract for creating member validators from validation rule configurations.
/// Implementations of this interface are responsible for translating declarative validation
/// rules into executable validation logic for specific validation types.
/// </summary>
/// <remarks>
/// <para>
/// The IValidatorFactory interface is the core abstraction that enables the validation system's
/// extensibility and configuration-driven approach. Each factory implementation specializes in
/// creating validators for a specific type of validation rule (e.g., regex, range, string length).
/// </para>
/// <para>
/// Factory implementations are registered with the <see cref="IValidatorFactoryProvider"/> and
/// are selected based on the <see cref="ValidationRuleConfig.RuleType"/> property. This design
/// allows for:
/// </para>
/// <para>
/// Implementations should handle configuration errors gracefully by returning validators that
/// produce <see cref="InvalidEntry"/> instances with <see cref="CauseType.RuleConfigError"/> to
/// distinguish configuration problems from validation failures.
/// </para>
/// </remarks>
/// <seealso cref="IValidatorFactoryProvider"/>
/// <seealso cref="ValidationRuleConfig"/>
/// <seealso cref="MemberValidator{T}"/>
public interface IValidatorFactory
{
    /// <summary>
    /// Creates a member validator of type <typeparamref name="T"/> based on the provided validation rule configuration.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to be validated. This must be a non-null reference type and should
    /// be compatible with the validation logic defined in the rule configuration.
    /// </typeparam>
    /// <param name="ruleConfig">
    /// The validation rule configuration that defines the validation behaviour. This includes
    /// all necessary parameters such as patterns, ranges, comparison values, failure messages,
    /// and localization information.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> delegate that can validate values of type <typeparamref name="T"/>.
    /// The returned validator should handle all validation logic defined in the <paramref name="ruleConfig"/>
    /// and return appropriate <see cref="Validated{T}"/> results.
    /// </returns>
    MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull;
}
