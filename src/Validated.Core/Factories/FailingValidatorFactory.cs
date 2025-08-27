using Validated.Core.Common.Constants;
using Validated.Core.Types;

namespace Validated.Core.Factories;

/// <summary>
/// Fallback validator factory used when no matching factory is registered
/// for a given rule type.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="FailingValidatorFactory"/> is a sentinel implementation of 
/// <see cref="IValidatorFactory"/>. It is used by the <see cref="ValidatorFactoryProvider"/>
/// when a requested rule type cannot be resolved to a concrete factory.
/// </para>
/// <para>
/// Instead of throwing an exception, this factory always produces a validator
/// that returns a failure with <see cref="ErrorMessages.Validator_Factory_User_Failure_Message"/>.
/// This ensures that invalid or unknown rule types are handled gracefully and
/// surfaced as validation failures rather than runtime errors.
/// </para>
/// </remarks>
internal sealed class FailingValidatorFactory : IValidatorFactory
{
    /// <summary>
    /// Creates a validator for the given type <typeparamref name="T"/> that always fails.
    /// </summary>
    /// <typeparam name="T">The type of value being validated.</typeparam>
    /// <param name="ruleConfig">
    /// The rule configuration passed to the factory. This parameter is unused, but 
    /// included to satisfy the <see cref="IValidatorFactory"/> contract.
    /// </param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that always produces an invalid result
    /// with <see cref="ErrorMessages.Validator_Factory_User_Failure_Message"/>.
    /// </returns>
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (_, path, _, _)
        
            => Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ErrorMessages.Validator_Factory_User_Failure_Message, path, ruleConfig.PropertyName, ruleConfig.DisplayName, CauseType.SystemError)));
}
