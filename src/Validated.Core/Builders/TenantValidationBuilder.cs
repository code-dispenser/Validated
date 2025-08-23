using System.Collections;
using System.Collections.Immutable;
using System.Linq.Expressions;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Extensions;
using Validated.Core.Factories;
using Validated.Core.Types;

namespace Validated.Core.Builders;

/// <summary>
/// Builder for composing entity validators using configuration-driven validation with tenant and culture support.
/// </summary>
/// <typeparam name="TEntity">The type of entity for which validation rules are being built.</typeparam>
/// <remarks>
/// <para>
/// The TenantValidationBuilder provides a fluent interface for creating entity validators from
/// validation rule configurations, with built-in support for multi-tenant and multi-culture
/// validation scenarios. Unlike <see cref="ValidationBuilder{TEntity}"/>, this builder automatically
/// creates validators from configuration rather than requiring explicit validator instances.
/// </para>
/// <para>
/// The builder uses the factory provider pattern to create appropriate validators based on
/// rule configurations, applying tenant and culture-specific rule resolution to support
/// customized validation behaviour across different organizational contexts.
/// </para>
/// <para>
/// Duplicate validator prevention ensures that the same member isn't validated multiple times
/// with the same context, optimizing performance while maintaining validation completeness.
/// </para>
/// </remarks>
public class TenantValidationBuilder<TEntity> where TEntity : notnull
{
    private readonly List<EntityValidator<TEntity>> _validators;
    private readonly HashSet<string>                _membersAdded;
    private readonly IValidatorFactoryProvider      _factoryProvider;
    private readonly string                         _entityTypeFullName;
    private readonly string                         _tenantID;
    private readonly string                         _cultureID;

    private readonly ImmutableList<ValidationRuleConfig> _ruleConfigs;

    /// <summary>
    /// Initializes a new instance of the TenantValidationBuilder with the specified configuration and tenant context.
    /// </summary>
    /// <param name="ruleConfigs">
    /// The complete set of validation rule configurations available for creating validators.
    /// These configurations are filtered based on entity type, property names, tenant, and culture.
    /// </param>
    /// <param name="factoryProvider">
    /// The factory provider responsible for creating validators from rule configurations.
    /// Provides access to all registered validator factories for different rule types.
    /// </param>
    /// <param name="tenantID">
    /// The tenant identifier for multi-tenant rule resolution. Defaults to the system default tenant.
    /// Used to apply tenant-specific validation rules and customizations.
    /// </param>
    /// <param name="cultureID">
    /// The culture identifier for localized validation rules and messages. Defaults to en-GB.
    /// Enables culture-specific validation behaviour and localized error messages.
    /// </param>
    /// <remarks>
    /// The constructor initializes all internal collections and captures the entity type information
    /// needed for rule configuration filtering and validator creation.
    /// </remarks>
    internal TenantValidationBuilder(ImmutableList<ValidationRuleConfig> ruleConfigs, IValidatorFactoryProvider factoryProvider, string tenantID = ValidatedConstants.Default_TenantID, 
        string cultureID = ValidatedConstants.Default_CultureID)
    {
        _validators         = [];
        _membersAdded       = [];
        _factoryProvider    = factoryProvider;
        _entityTypeFullName = typeof(TEntity).FullName!;
        _tenantID           = tenantID;
        _cultureID          = cultureID;
        _ruleConfigs        = ruleConfigs;
    }

    /// <summary>
    /// Adds an entity validator to the collection if it hasn't already been added for the specified member and context.
    /// </summary>
    /// <param name="memberName">
    /// The name of the member being validated. Used as part of the deduplication key
    /// to prevent duplicate validators for the same member.
    /// </param>
    /// <param name="validator">
    /// The entity validator to add to the collection. This validator will be executed
    /// as part of the composite validation when <see cref="Build"/> is called.
    /// </param>
    /// <param name="context">
    /// The validation context identifier used for deduplication. Defaults to "Item".
    /// Allows the same member to have different validators for different contexts.
    /// </param>
    /// <returns>
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method implements deduplication logic to prevent the same member from being
    /// validated multiple times with the same context, improving performance and avoiding
    /// redundant validation results.
    /// </remarks>
    private TenantValidationBuilder<TEntity> AddValidatorIfNotExists(string memberName, EntityValidator<TEntity> validator, string context = "Item")
    {
        string memberKey = $"{memberName}-{context}";

        if (_membersAdded.Contains(memberKey)) return this;

        _membersAdded.Add(memberKey);
        _validators.Add(validator);

        return this;
    }

    /// <summary>
    /// Configures validation for a non-nullable property using validators created from rule configurations.
    /// </summary>
    /// <typeparam name="TProperty">
    /// The type of the property to validate. Must be a non-null reference type.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the property to validate from the entity.
    /// Used to extract the property value and determine applicable validation rules.
    /// </param>
    /// <returns>
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method automatically creates appropriate validators for the selected property
    /// by querying the rule configurations for matching tenant, culture, entity type, and property name.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForMember<TProperty>(Expression<Func<TEntity, TProperty>> selectorExpression) where TProperty : notnull
    {
        var memberName = GeneralUtils.GetMemberName(selectorExpression);
        var validator = _factoryProvider.CreateValidator<TProperty>(_entityTypeFullName, memberName, _ruleConfigs, _tenantID, _cultureID);

        return AddValidatorIfNotExists(memberName, validator.ForEntityMember(selectorExpression));
    }

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
    /// <returns>
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method provides optional validation behaviour for nullable value types,
    /// automatically creating validators from configuration and treating null values as valid.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForNullableMember<TProperty>(Expression<Func<TEntity, TProperty?>> selectorExpression) where TProperty : struct
    {
        var memberName = GeneralUtils.GetMemberName(selectorExpression);
        var validator = _factoryProvider.CreateValidator<TProperty>(_entityTypeFullName, memberName, _ruleConfigs,_tenantID, _cultureID);

        return AddValidatorIfNotExists(memberName,validator.ForNullableEntityMember(selectorExpression));
    }

    /// <summary>
    /// Configures validation for a nullable string property, skipping validation if the value is null (think optional value).
    /// </summary>
    /// <param name="selectorExpression">
    /// Expression that selects the nullable string property to validate from the entity.
    /// The expression should return a nullable string.
    /// </param>
    /// <returns>
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method provides specialized handling for nullable strings, automatically creating
    /// string validators from configuration while treating null values as valid.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForNullableStringMember(Expression<Func<TEntity, string?>> selectorExpression)
    {
        var memberName = GeneralUtils.GetMemberName(selectorExpression);
        var validator = _factoryProvider.CreateValidator<string>(_entityTypeFullName, memberName, _ruleConfigs, _tenantID, _cultureID);

        return AddValidatorIfNotExists(memberName, validator.ForNullableStringEntityMember(selectorExpression));
    }

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
    /// <param name="validator">
    /// The entity validator to apply to the nested entity. This validator must be
    /// explicitly provided as it defines the validation logic for the nested object.
    /// </param>
    /// <returns>
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method enables hierarchical validation by applying entity-level validation
    /// to nested objects, maintaining proper validation paths and error context.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForNestedMember<TNested>(Expression<Func<TEntity, TNested>> selectorExpression, EntityValidator<TNested> validator) where TNested : notnull
    {
        var memberName = GeneralUtils.GetMemberName(selectorExpression);

        return AddValidatorIfNotExists(memberName, ValidatorExtensions.ForNestedEntityMember(selectorExpression, validator));
    }

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
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method provides optional validation for nested entities, treating null nested
    /// objects as valid while applying full entity validation when objects are present.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForNullableNestedMember<TNested>(Expression<Func<TEntity, TNested?>> selectorExpression, EntityValidator<TNested> nestedValidator) where TNested : class
    {
        var memberName = GeneralUtils.GetMemberName(selectorExpression);
        return AddValidatorIfNotExists(memberName, ValidatorExtensions.ForNullableNestedEntityMember(selectorExpression, nestedValidator));
    }

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
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method enables collection validation by applying entity-level validation
    /// to each item in the collection, maintaining indexed paths for error reporting.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForEachCollectionMember<TItem>(Expression<Func<TEntity, IEnumerable<TItem>>> selectorExpression, EntityValidator<TItem> itemValidator) where TItem : notnull
    {
        var memberName = GeneralUtils.GetMemberName(selectorExpression);
        return AddValidatorIfNotExists(memberName, ValidatorExtensions.ForCollectionEntityMember(selectorExpression, itemValidator));
    }

    /// <summary>
    /// Configures validation for each primitive item in a collection property using validators created from rule configurations.
    /// </summary>
    /// <typeparam name="TPrimitive">
    /// The primitive type of items contained in the collection. Must be a non-null type.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the collection property from the entity.
    /// The collection should contain primitive items of type TPrimitive.
    /// </param>
    /// <returns>
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method automatically creates validators for primitive collection items from
    /// rule configurations, applying tenant and culture-specific rules for each item.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForEachPrimitiveItem<TPrimitive>(Expression<Func<TEntity, IEnumerable<TPrimitive>>> selectorExpression) where TPrimitive : notnull
    {
        var memberName  = GeneralUtils.GetMemberName(selectorExpression);
        var validator   = _factoryProvider.CreateValidator<TPrimitive>(_entityTypeFullName, memberName, _ruleConfigs, _tenantID, _cultureID);

        return AddValidatorIfNotExists(memberName, ValidatorExtensions.ForEachPrimitiveItem(selectorExpression, validator));
    }

    /// <summary>
    /// Configures validation for a collection property as a whole using validators created from rule configurations.
    /// </summary>
    /// <typeparam name="TCollection">
    /// The type of the collection to validate. Must implement <see cref="IEnumerable"/> and be non-null.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the collection property from the entity.
    /// The entire collection is passed to the validator.
    /// </param>
    /// <returns>
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method automatically creates validators for collection-level validation from
    /// rule configurations, such as length constraints, using the "Collection" context for deduplication.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForCollection<TCollection>(Expression<Func<TEntity, TCollection>> selectorExpression) where TCollection : notnull, IEnumerable
    {
        var memberName  = GeneralUtils.GetMemberName(selectorExpression);
        var validator   = _factoryProvider.CreateValidator<TCollection>(_entityTypeFullName, memberName, _ruleConfigs, _tenantID, _cultureID);

        return AddValidatorIfNotExists(memberName, ValidatorExtensions.ForCollection(selectorExpression, validator), "Collection");
    }

    /// <summary>
    /// Configures comparison validation using validators created from rule configurations that operate on the entire entity.
    /// </summary>
    /// <typeparam name="TMember">
    /// The type of the member being used to provide comparison context.
    /// </typeparam>
    /// <param name="selectorExpression">
    /// Expression that selects the member to provide context for the comparison validation.
    /// This member's name is used for rule lookup and validation path construction.
    /// </param>
    /// <returns>
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method automatically creates comparison validators from configuration that can
    /// validate relationships between multiple entity properties, such as ensuring one
    /// property is greater than another within the same entity.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForComparisonWith<TMember>(Expression<Func<TEntity, TMember>> selectorExpression) where TMember : notnull
    {
        var memberName = GeneralUtils.GetMemberName(selectorExpression);
        var validator = _factoryProvider.CreateValidator<TEntity>(_entityTypeFullName, memberName, _ruleConfigs, _tenantID, _cultureID);

        return AddValidatorIfNotExists(memberName, validator.ToCompareEntityMember(selectorExpression));
    }

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
    /// The current TenantValidationBuilder instance to enable fluent method chaining.
    /// </returns>
    /// <remarks>
    /// This method enables validation of self-referencing entity structures while preventing
    /// infinite recursion through depth limiting and circular reference detection.
    /// </remarks>
    public TenantValidationBuilder<TEntity> ForRecursiveEntity(Expression<Func<TEntity, TEntity>> selectorExpression, EntityValidator<TEntity> baseValidator)
    {
        var memberName = GeneralUtils.GetMemberName(selectorExpression);
        return AddValidatorIfNotExists(memberName, ValidatorExtensions.ForRecursiveEntity(selectorExpression, baseValidator));
    }

    /// <summary>
    /// Builds and returns the composite entity validator from all configured validation rules.
    /// </summary>
    /// <returns>
    /// An <see cref="EntityValidator{TEntity}"/> that combines all validators created from
    /// rule configurations. If no validators were configured, returns a validator that
    /// considers all entities valid.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The returned validator executes all configured validators and combines their results.
    /// All validators must pass for the entity to be considered valid. Any validation failures
    /// are aggregated and returned together.
    /// </para>
    /// <para>
    /// This method should be called once all desired validation rules have been configured
    /// through the builder's fluent methods.
    /// </para>
    /// </remarks>
    public EntityValidator<TEntity> Build()
    {
        if (_validators.Count == 0) return ValidatedExtensions.Combine<TEntity>((input, _, context,_) => Task.FromResult(Validated<TEntity>.Valid(input)));

       return ValidatedExtensions.Combine(_validators.ToArray());
    }

    /// <summary>
    /// Creates a new instance of the TenantValidationBuilder with the specified configuration and tenant context.
    /// </summary>
    /// <param name="ruleConfigs">
    /// The complete set of validation rule configurations available for creating validators.
    /// These configurations define the validation behaviour for different entity types and properties.
    /// </param>
    /// <param name="factoryProvider">
    /// The factory provider responsible for creating validators from rule configurations.
    /// Must be properly initialized with all required validator factories.
    /// </param>
    /// <param name="tenantID">
    /// The tenant identifier for multi-tenant rule resolution. Defaults to the system default tenant.
    /// Determines which tenant-specific rules take precedence during validation.
    /// </param>
    /// <param name="cultureID">
    /// The culture identifier for localized validation rules and messages. Defaults to en-GB.
    /// Controls culture-specific validation behaviour and error message localization.
    /// </param>
    /// <returns>
    /// A new <see cref="TenantValidationBuilder{TEntity}"/> instance ready for configuration
    /// through its fluent API methods.
    /// </returns>
    /// <remarks>
    /// This factory method provides the entry point for using the TenantValidationBuilder
    /// and ensures consistent initialization with proper tenant and culture context.
    /// </remarks>
    public static TenantValidationBuilder<TEntity> Create(ImmutableList<ValidationRuleConfig> ruleConfigs, IValidatorFactoryProvider factoryProvider, string tenantID = ValidatedConstants.Default_TenantID, string cultureID = ValidatedConstants.Default_CultureID)

        => new(ruleConfigs, factoryProvider, tenantID, cultureID); 
}
