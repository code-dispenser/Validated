using Validated.Core.Common.Constants;

namespace Validated.Core.Types;


/// <summary>
/// Represents contextual information used during entity validation.
/// </summary>
/// <remarks>
/// Tracks state across validation operations, including recursion depth
/// and the set of entities currently being validated. This prevents
/// circular references and enforces recursion limits.
/// </remarks>
public record ValidatedContext
{
    private readonly int _depth = 0;
    private readonly HashSet<object> _validatingInstances = new(ReferenceEqualityComparer.Instance);

    private readonly ValidationOptions _validationOptions = default;

    /// <summary>
    /// Gets the set of entity instances currently being validated.
    /// </summary>
    /// <remarks>
    /// This collection is compared by reference equality to detect circular references
    /// and prevent infinite validation loops.
    /// </remarks>
    public HashSet<object> ValidatingInstances
    {
        get => _validatingInstances;
        init => _validatingInstances = value ?? new HashSet<object>(ReferenceEqualityComparer.Instance);
    }

    /// <summary>
    /// Gets the maximum allowed recursion depth for validation.
    /// </summary>
    public int MaxDepth { get; } 

    /// <summary>
    /// Gets the current recursion depth of validation.
    /// </summary>
    public int Depth    { get => _depth; init => _depth = value; }

    /// <summary>
    /// Determines whether the maximum allowed recursion depth has been exceeded.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the maximum recursion depth is exceeded; otherwise, <c>false</c>.
    /// </returns>
    public bool IsMaxDepthExceeded() => _depth >= MaxDepth;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatedContext"/> class.
    /// </summary>
    /// <param name="validationOptions">
    /// The validation options that control context behaviour, including
    /// the maximum recursion depth.
    /// </param>
    public ValidatedContext(ValidationOptions validationOptions = default)
    { 
        _validationOptions = validationOptions;
        MaxDepth = validationOptions.MaxRecursionDepth;
    }

    /// <summary>
    /// Determines whether the specified entity is already being validated.
    /// </summary>
    /// <param name="instance">The entity instance to check.</param>
    /// <returns>
    /// <c>true</c> if the entity is currently being validated;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool IsValidating(object instance) => _validatingInstances.Contains(instance);

    /// <summary>
    /// Returns a new context that includes the specified entity as being validated.
    /// </summary>
    /// <param name="instance">The entity to add to the validation context.</param>
    /// <returns>A new <see cref="ValidatedContext"/> instance.</returns>
    public ValidatedContext WithValidating(object instance)
    {
        var newInstances = new HashSet<object>(_validatingInstances, ReferenceEqualityComparer.Instance) { instance };

        return this with { ValidatingInstances = newInstances};
    }

    /// <summary>
    /// Returns a new context with the recursion depth incremented.
    /// </summary>
    /// <returns>A new <see cref="ValidatedContext"/> instance.</returns>
    public ValidatedContext WithIncrementedDepth()

        => this with { Depth = _depth + 1 };
}
