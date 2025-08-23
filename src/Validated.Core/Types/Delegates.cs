using Validated.Core.Extensions;
using Validated.Core.Builders;

namespace Validated.Core.Types;

/// <summary>
/// Represents an asynchronous validator for a member value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the member value to validate. Must be a non-null reference type.</typeparam>
/// <param name="memberValue">The value to validate.</param>
/// <param name="path">
/// The validation path for error reporting. When empty, the validator should use the property name as the path.
/// For nested validations, this represents the full path to the current member (e.g., "User.Address.Street").
/// </param>
/// <param name="compareTo">
/// An optional comparison value used in comparative validations. This parameter is used when validating
/// that one value relates to another (e.g., password confirmation, date ranges). For most validations,
/// this will be the default value.
/// </param>
/// <param name="cancellationToken">The cancellation token used to signify any cancellation requests.</param>
/// <returns>
/// A task that represents the asynchronous validation operation. The task result contains a 
/// <see cref="Validated{T}"/> indicating whether the validation passed or failed, along with 
/// any failure details.
/// </returns>
/// <remarks>
/// <para>
/// Member validators are the building blocks of the validation system. They validate individual
/// values and can be composed together using the <see cref="ValidatedExtensions.AndThen{T}"/> method.
/// </para>
/// <para>
/// When implementing custom member validators, ensure that:
/// <list type="bullet">
/// <item>The path parameter is used correctly for error reporting</item>
/// <item>Null values are handled appropriately for the validation logic</item>
/// <item>The compareTo parameter is used only when needed for comparative validations</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating a simple member validator
/// MemberValidator&lt;int&gt; positiveValidator = async (value, path, _) =>
/// {
///     return value > 0 
///         ? Validated&lt;int&gt;.Valid(value)
///         : Validated&lt;int&gt;.Invalid(new InvalidEntry(path, "Value", "Value", "Must be positive"));
/// };
/// 
/// // Using the validator
/// var result = await positiveValidator(5, "MyProperty");
/// </code>
/// </example>
public delegate Task<Validated<T>> MemberValidator<T>(T memberValue, string path= "", T? compareTo = default, CancellationToken cancellationToken = default) where T : notnull;


/// <summary>
/// Represents an asynchronous validator for an entire entity of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to validate. Must be a non-null reference type.</typeparam>
/// <param name="entity">The entity instance to validate.</param>
/// <param name="rootPath">
/// The root path for validation error reporting. This is typically the entity type name or a custom
/// identifier. All member validation paths will be built relative to this root path.
/// When empty, the entity type name is used as the default root path.
/// </param>
/// <param name="context">
/// The validation context that tracks validation state across the validation tree. This includes
/// circular reference detection, recursion depth tracking, and validation options. If null,
/// a new context will be created with default settings.
/// </param>
/// <param name="cancellationToken">The cancellation token used to signify any cancellation requests.</param>
/// <returns>
/// A task that represents the asynchronous validation operation. The task result contains a 
/// <see cref="Validated{TEntity}"/> indicating whether the validation passed or failed, along with 
/// any failure details from all validated members.
/// </returns>
/// <remarks>
/// <para>
/// Entity validators orchestrate the validation of complex objects by coordinating multiple
/// member validators and handling nested object validation. They are created using the
/// <see cref="ValidationBuilder{TEntity}"/> or <see cref="TenantValidationBuilder{TEntity}"/> classes.
/// </para>
/// <para>
/// The validator handles:
/// <list type="bullet">
/// <item>Circular reference detection to prevent infinite validation loops</item>
/// <item>Recursion depth limiting to prevent stack overflow</item>
/// <item>Path building for nested error reporting</item>
/// <item>Collection validation with indexed error paths</item>
/// </list>
/// </para>
/// <para>
/// Entity validators can be combined using <see cref="ValidatedExtensions.Combine{T}"/> to create
/// composite validators that run multiple validation rules against the same entity.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating an entity validator using the builder
/// var userValidator = ValidationBuilder&lt;User&gt;.Create()
///     .ForMember(u => u.Name, MemberValidators.CreateNotNullOrEmptyValidator("Name", "Name", "Name is required"))
///     .ForMember(u => u.Age, MemberValidators.CreateRangeValidator(0, 120, "Age", "Age", "Age must be between 0 and 120"))
///     .Build();
/// 
/// // Using the validator
/// var user = new User { Name = "John", Age = 25 };
/// var result = await userValidator(user, "User");
/// </code>
/// </example>
public delegate Task<Validated<TEntity>> EntityValidator<TEntity>(TEntity entity, string rootPath = "", ValidatedContext? context = default, CancellationToken cancellationToken = default) where TEntity : notnull;

