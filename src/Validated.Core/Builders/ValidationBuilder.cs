using System.Collections;
using System.Linq.Expressions;
using Validated.Core.Common.Constants;
using Validated.Core.Extensions;
using Validated.Core.Types;

namespace Validated.Core.Builders;

/// <summary>
/// Builder for composing entity validators using a fluent API with explicit validator specification.
/// </summary>
/// <typeparam name="TEntity">The type of entity for which validation rules are being built.</typeparam>
/// <remarks>
/// <para>
/// The ValidationBuilder provides a fluent interface for manually composing entity validation rules
/// where validators are explicitly provided rather than created from configuration. This approach
/// offers maximum flexibility for scenarios requiring custom validation logic or when configuration-driven
/// validation is not required.
/// </para>
/// <para>
/// Unlike <see cref="TenantValidationBuilder{TEntity}"/>, this builder requires explicit validator
/// instances to be provided for each property, enabling fine-grained control over validation behaviour
/// and supporting complex validation scenarios that may not be easily expressed through configuration.
/// </para>
/// <para>
/// The builder accumulates validators and combines them into a single composite entity validator
/// that validates all specified members when executed.
/// </para>
/// </remarks>
public class ValidationBuilder<TEntity> where TEntity : notnull
{
    private readonly List<EntityValidator<TEntity>> _validators   = [];

    private Stack<Func<TEntity, bool>> _predicateStack = [];
    private Func<TEntity, bool>?        _cachedCombinedPredicate;
    /// <summary>
    /// Initializes a new instance of the ValidationBuilder with an empty validator collection.
    /// </summary>
    /// <remarks>
    /// The constructor is marked internal as instances should be created through the
    /// <see cref="Create"/> factory method to maintain consistent builder initialization.
    /// </remarks>
    internal ValidationBuilder() { }

    /// <summary>
    /// Adds an entity validator to the internal collection of validators.
    /// </summary>
    /// <param name="validator">
    /// The entity validator to add to the collection. This validator will be executed
    /// as part of the composite validation when <see cref="Build"/> is called.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method provides the core accumulation functionality for the builder pattern,
    /// allowing multiple validators to be composed into a single validation pipeline.
    /// </remarks>
    private ValidationBuilder<TEntity> AddValidator(EntityValidator<TEntity> validator)
    {
        var finalValidator = validator;

        if (_predicateStack.Count > 0 && _cachedCombinedPredicate is not null) finalValidator = validator.When(_cachedCombinedPredicate);

        _validators.Add(finalValidator);

        return this;
    }
    private void UpdateCachedPredicate()
    {
        if (_predicateStack.Count == 0)
        {
            _cachedCombinedPredicate = null;
            return;
        }

        var predicates = _predicateStack.Reverse().ToList();
        _cachedCombinedPredicate = entity => predicates.All(p => p(entity));
    }
    /// <summary>
    /// Begins a conditional validation scope that applies subsequent validation rules 
    /// only when the specified predicate evaluates to <see langword="true"/> for the entity.
    /// </summary>
    /// <param name="predicate">
    /// A function that determines whether the subsequent validation rules should be applied
    /// for the given entity instance. If the predicate returns <see langword="false"/>, the rules
    /// defined after this call (until <see cref="EndWhen"/>) are skipped.
    /// </param>
    /// <returns>
    /// The current <see cref="ValidationBuilder{TEntity}"/> instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method enables conditional validation by allowing one or more validation rules 
    /// to be applied only when a specified condition holds true. It sets an internal predicate 
    /// that gates all following validation configurations until <see cref="EndWhen"/> is called.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// builder.DoWhen(c => c.IsActive)
    ///        .ForMember(c => c.Email, Validators.Required())
    ///        .EndWhen();
    /// </code>
    /// In this example, the <c>Email</c> property is validated only when the entity’s <c>IsActive</c>
    /// flag is set to true.
    /// </para>
    /// </remarks>
    public ValidationBuilder<TEntity> DoWhen(Func<TEntity, bool> predicate)
    {
        _predicateStack.Push(predicate);
        UpdateCachedPredicate();
        return this;
    }

    /// <summary>
    /// Ends a previously started conditional validation scope, returning validation behavior 
    /// to unconditional mode.
    /// </summary>
    /// <returns>
    /// The current <see cref="ValidationBuilder{TEntity}"/> instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method clears the predicate set by a preceding call to <see cref="DoWhen"/>,
    /// ensuring that subsequent validation rules are always applied regardless of entity state.
    /// </para>
    /// <para>
    /// It is typically used to close a conditional validation block, restoring normal validation flow:
    /// </para>
    /// <code>
    /// builder.DoWhen(c => c.HasChildren)
    ///        .ForEachCollectionMember(c => c.Children, childValidator)
    ///        .EndWhen()
    ///        .ForMember(c => c.Name, Validators.Required());
    /// </code>
    /// In this example, child validation occurs only when <c>HasChildren</c> is true,
    /// while the <c>Name</c> validation always runs.
    /// </remarks>
    public ValidationBuilder<TEntity> EndWhen()
    {
        if (_predicateStack.Count > 0)
        {  
            _predicateStack.Pop();
            UpdateCachedPredicate();
        }
           
        return this;
    }

    /// <summary>
    /// Configures validation for a non-nullable property using the provided member validator.
    /// </summary>
    /// <typeparam name="TProperty">
    /// The type of the property to validate. Must be a non-null reference type.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the property to validate from the entity.
    /// Used to extract the property value and determine the validation path.
    /// </param>
    /// <param name="validator">
    /// The member validator to apply to the selected property. This validator defines
    /// the specific validation logic for the property.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method converts the provided member validator into an entity validator that
    /// can extract and validate the specified property from the complete entity.
    /// </remarks>
    public ValidationBuilder<TEntity> ForMember<TProperty>(Expression<Func<TEntity, TProperty>> selectorExpression, MemberValidator<TProperty> validator) where TProperty : notnull
        
        =>  AddValidator(validator.ForEntityMember(selectorExpression));

    /// <summary>
    /// Configures validation for a nullable value type property, skipping validation if the value is null (think optional value).
    /// </summary>
    /// <typeparam name="TProperty">
    /// The value type of the property to validate. Must be a struct (value type).
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the nullable property to validate from the entity.
    /// The expression should return a nullable value type.
    /// </param>
    /// <param name="validator">
    /// The member validator to apply to the property value when it is not null.
    /// Validation is skipped entirely if the property value is null.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method provides optional validation behaviour for nullable value types,
    /// treating null values as valid and only applying validation when a value is present.
    /// </remarks>
    public ValidationBuilder<TEntity> ForNullableMember<TProperty>(Expression<Func<TEntity, TProperty?>> selectorExpression, MemberValidator<TProperty> validator) where TProperty : struct
    
        => AddValidator(validator.ForNullableEntityMember(selectorExpression));

    /// <summary>
    /// Configures validation for a nullable string property, skipping validation if the value is null (think optional value).
    /// </summary>
    /// <param name="selectorExpression">
    /// Expression that selects the nullable string property to validate from the entity.
    /// The expression should return a nullable string.
    /// </param>
    /// <param name="validator">
    /// The string validator to apply to the property value when it is not null.
    /// Validation is skipped entirely if the property value is null.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method provides specialized handling for nullable strings, which require different
    /// treatment (think optional value)
    /// </remarks>
    public ValidationBuilder<TEntity> ForNullableStringMember(Expression<Func<TEntity, string?>> selectorExpression, MemberValidator<string> validator)
        
        => AddValidator(validator.ForNullableStringEntityMember(selectorExpression));


    /// <summary>
    /// Configures validation for a nested entity property using the provided entity validator.
    /// </summary>
    /// <typeparam name="TNested">
    /// The type of the nested entity to validate. Must be a non-null reference type.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the nested entity property from the parent entity.
    /// Used to extract the nested object for validation.
    /// </param>
    /// <param name="nestedValidator">
    /// The entity validator to apply to the nested entity. This validator defines
    /// the complete validation logic for the nested object.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method enables hierarchical validation by applying entity-level validation
    /// to nested objects, maintaining proper validation paths and error context.
    /// </remarks>
    public ValidationBuilder<TEntity> ForNestedMember<TNested>(Expression<Func<TEntity, TNested>> selectorExpression, EntityValidator<TNested> nestedValidator) where TNested : notnull
    
        => AddValidator(ValidatorExtensions.ForNestedEntityMember(selectorExpression, nestedValidator));


    /// <summary>
    /// Configures validation for a nullable nested entity property, skipping validation if the value is null (think optional value).
    /// </summary>
    /// <typeparam name="TNested">
    /// The type of the nested entity to validate. Must be a reference type (class).
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the nullable nested entity property from the parent entity.
    /// The expression should return a nullable reference type.
    /// </param>
    /// <param name="nestedValidator">
    /// The entity validator to apply to the nested entity when it is not null.
    /// Validation is skipped entirely if the nested entity is null.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method provides optional validation for nested entities, treating null nested
    /// objects as valid while applying full entity validation when objects are present.
    /// </remarks>
    public ValidationBuilder<TEntity> ForNullableNestedMember<TNested>(Expression<Func<TEntity, TNested?>> selectorExpression, EntityValidator<TNested> nestedValidator) where TNested : class

        => AddValidator(ValidatorExtensions.ForNullableNestedEntityMember(selectorExpression, nestedValidator));

    /// <summary>
    /// Configures validation for each item in a collection property using the provided entity validator.
    /// </summary>
    /// <typeparam name="TItem">
    /// The type of items contained in the collection. Must be a non-null reference type.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the collection property from the entity.
    /// The collection should contain items of type TItem.
    /// </param>
    /// <param name="itemValidator">
    /// The entity validator to apply to each item in the collection.
    /// Each item is validated independently using this validator.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method enables collection validation by applying entity-level validation
    /// to each item in the collection, maintaining indexed paths for error reporting.
    /// </remarks>
    public ValidationBuilder<TEntity> ForEachCollectionMember<TItem>(Expression<Func<TEntity, IEnumerable<TItem>>> selectorExpression, EntityValidator<TItem> itemValidator) where TItem : notnull
    
        => AddValidator(ValidatorExtensions.ForCollectionEntityMember(selectorExpression, itemValidator));

    /// <summary>
    /// Configures validation for each primitive item in a collection property using the provided member validator.
    /// </summary>
    /// <typeparam name="TPrimitive">
    /// The primitive type of items contained in the collection. Must be a non-null type.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the collection property from the entity.
    /// The collection should contain primitive items of type TPrimitive.
    /// </param>
    /// <param name="primitiveValidator">
    /// The member validator to apply to each primitive item in the collection.
    /// Each item is validated independently using this validator.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method is optimized for collections of primitive types where member-level
    /// validation is sufficient rather than full entity validation.
    /// </remarks>
    public ValidationBuilder<TEntity> ForEachPrimitiveItem<TPrimitive>(Expression<Func<TEntity, IEnumerable<TPrimitive>>> selectorExpression, MemberValidator<TPrimitive> primitiveValidator) where TPrimitive : notnull

        => AddValidator(ValidatorExtensions.ForEachPrimitiveItem(selectorExpression, primitiveValidator));


    /// <summary>
    /// Configures validation for a collection property as a whole using the provided member validator.
    /// </summary>
    /// <typeparam name="TCollection">
    /// The type of the collection to validate. Must implement <see cref="IEnumerable"/> and be non-null.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the collection property from the entity.
    /// The entire collection is passed to the validator.
    /// </param>
    /// <param name="validator">
    /// The member validator to apply to the collection as a complete unit.
    /// This validates collection-level properties like length, rather than individual items.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method validates the collection itself (such as its length or other collection-level
    /// properties) rather than validating individual items within the collection.
    /// </remarks>
    public ValidationBuilder<TEntity> ForCollection<TCollection>(Expression<Func<TEntity, TCollection>> selectorExpression, MemberValidator<TCollection> validator) where TCollection : notnull, IEnumerable

        => AddValidator(ValidatorExtensions.ForCollection(selectorExpression, validator));

    /// <summary>
    /// Configures comparison validation using a validator that compares one entity member to another or to a provided value.
    /// </summary>
    /// <typeparam name="TMember">
    /// The type of the member being used to provide comparison context.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the member to provide context for the comparison validation.
    /// This member's name is used in the validation path but the validator receives the entire entity.
    /// </param>
    /// <param name="validator">
    /// The member validator that operates on the entity member to perform the desired comparison validation.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method enables complex validation scenarios where the entire entity is needed
    /// for comparison operations, such as validating that one property is greater than another
    /// within the same entity.
    /// </remarks>
    public ValidationBuilder<TEntity> ForComparisonWithMember<TMember>(Expression<Func<TEntity, TMember>> selectorExpression,MemberValidator<TEntity> validator) where TMember : notnull
    
       => AddValidator(validator.ToCompareEntityMember(selectorExpression));

    /// <summary>
    /// Configures recursive validation for entities that contain references to themselves.
    /// </summary>
    /// <param name="selectorExpression">
    /// Expression that selects the child entity property from the parent entity.
    /// This property should be of the same type as the parent entity.
    /// </param>
    /// <param name="baseValidator">
    /// The base entity validator to apply at each level of the recursive structure.
    /// This validator is applied to both the current entity and its recursive children.
    /// </param>
    /// <returns>
    /// The current ValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method enables validation of self-referencing entity structures while preventing
    /// infinite recursion through depth limiting and circular reference detection.
    /// </remarks>
    public ValidationBuilder<TEntity> ForRecursiveEntity(Expression<Func<TEntity, TEntity>> selectorExpression, EntityValidator<TEntity> baseValidator)

         => AddValidator(ValidatorExtensions.ForRecursiveEntity(selectorExpression, baseValidator));

    /// <param name="failFastOnNull">
    /// When set to true, the returned validator immediately fails with an invalid result
    /// if the entity being validated is null.
    /// When set to false (the default), the validator behaves normally, allowing individual
    /// validators to determine how null entities are handled.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when there are unbalanced DoWhen/EndWhen calls. Each DoWhen() must have a 
    /// corresponding EndWhen() before calling Build(). The exception message indicates 
    /// how many DoWhen() calls remain unclosed.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The returned validator executes all configured validators and combines their results.
    /// All validators must pass for the entity to be considered valid. Any validation failures
    /// are aggregated and returned together.
    /// </para>
    /// <para>
    /// When <paramref name="failFastOnNull"/> is set to true, the validator performs an early check
    /// for null entities before executing any validation logic. This ensures that validation halts
    /// immediately and returns a system error indicating that the entity cannot be null.
    /// This behavior is useful in scenarios where null entities represent an unrecoverable state.
    /// </para>
    /// <para>
    /// Build() validates that all conditional scopes opened with DoWhen() have been properly 
    /// closed with EndWhen(). This validation prevents malformed builder configurations that 
    /// would result in incorrect validation behavior at runtime.
    /// </para>
    /// </remarks>
    public EntityValidator<TEntity> Build(bool failFastOnNull = false)
    {
        if (_validators.Count == 0) return ValidatedExtensions.Combine<TEntity>((input, _, context, _) => Task.FromResult(Validated<TEntity>.Valid(input)));

        if (_predicateStack.Count > 0) throw new InvalidOperationException(String.Format(ErrorMessages.Validation_Builder_Unbalenced_DoWhen, _predicateStack.Count));

        var combinedValidator = ValidatedExtensions.Combine(_validators.ToArray());

        if (false == failFastOnNull) return combinedValidator;

        return async (entity, path, context, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, typeof(TEntity).Name, typeof(TEntity).Name, CauseType.SystemError));

            return await combinedValidator(entity, path, context, cancellationToken);
        };

    }

    /// <summary>
    /// Creates a new instance of the ValidationBuilder for the specified entity type.
    /// </summary>
    /// <returns>
    /// A new <see cref="ValidationBuilder{TEntity}"/> instance ready for configuration
    /// through its fluent API methods.
    /// </returns>
    /// <remarks>
    /// This factory method provides the entry point for using the ValidationBuilder
    /// and ensures consistent initialization of the builder instance.
    /// </remarks>
    public static ValidationBuilder<TEntity> Create()

        => new();
}
