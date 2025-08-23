using System.Collections.Immutable;
using Validated.Core.Common.Constants;

namespace Validated.Core.Types;
/// <summary>
/// Represents the result of a validation operation, encapsulating either a valid value or a collection of validation
/// failures.
/// </summary>
/// <remarks>Use the <see cref="Valid(T)"/> method to create a valid instance, or the <see
/// cref="Invalid(IEnumerable{InvalidEntry})"/> and <see cref="Invalid(InvalidEntry)"/> methods to create an invalid
/// instance with associated validation failures.</remarks>
/// <typeparam name="T">The type of the value being validated. Must be a non-nullable type.</typeparam>
public sealed record class Validated<T> where T : notnull
{
    private readonly ImmutableArray<InvalidEntry> _failures;
    private readonly T? _value;

    /// <summary>
    /// Gets a read-only list of invalid entries that represent the failures encountered during processing.
    /// </summary>
    public IReadOnlyList<InvalidEntry> Failures => _failures;
    
    /// <summary>
    /// Gets a value indicating whether the current state is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets a value indicating whether the current state is invalid.
    /// </summary>
    public bool IsInvalid => !IsValid;

    private Validated(T value)
        => (_value, _failures, IsValid) = value is null
            ? (default, [new("Value cannot be null.",nameof(value), nameof(value), nameof(value))], false)
                : (value, ImmutableArray<InvalidEntry>.Empty, true);

    private Validated(IEnumerable<InvalidEntry> failures)
    {
        var filteredFailures = failures?.Where(failure => false == string.IsNullOrEmpty(failure.FailureMessage)).ToList() ?? [];
        _failures            = filteredFailures.Count > 0 ? [.. filteredFailures] : [new InvalidEntry("No validation failures provided.", "Unknown", "Unknown", "Unknown", CauseType.SystemError)];
        _value               = default!;
        IsValid              = false;
    }
    /// <summary>
    /// Creates a new <see cref="Validated{T}"/> instance representing a valid value.
    /// </summary>
    /// <param name="value">The value to be wrapped in a <see cref="Validated{T}"/> instance. Must not be null.</param>
    /// <returns>A <see cref="Validated{T}"/> instance containing the specified valid value.</returns>
    public static Validated<T> Valid(T value)

        => new(value);

    /// <summary>
    /// Creates an instance of <see cref="Validated{T}"/> representing an invalid state with the specified validation
    /// failures.
    /// </summary>
    /// <param name="failures">A collection of <see cref="InvalidEntry"/> objects that describe the validation errors.</param>
    /// <returns>A <see cref="Validated{T}"/> instance in an invalid state containing the provided validation failures.</returns>
    public static Validated<T> Invalid(IEnumerable<InvalidEntry> failures)

        => new(failures);

    /// <summary>
    /// Creates a <see cref="Validated{T}"/> instance representing an invalid state with the specified failure.
    /// </summary>
    /// <param name="failure">The <see cref="InvalidEntry"/> describing the reason for the invalid state. Cannot be null.</param>
    /// <returns>A <see cref="Validated{T}"/> instance containing the specified failure.</returns>
    public static Validated<T> Invalid(InvalidEntry failure)

        => new([failure]);

    /// <summary>
    /// Retrieves the current value if it is valid; otherwise, returns the specified fallback value.
    /// </summary>
    /// <param name="fallback">The value to return if the current value is not valid.</param>
    /// <returns>The current value if it is valid; otherwise, the specified fallback value.</returns>
    public T GetValueOr(T fallback)

        => IsValid ? _value! : fallback;

    /// <summary>
    /// Executes one of the provided functions based on the validity of the current instance.
    /// </summary>
    /// <typeparam name="TOut">The type of the result returned by the provided functions.</typeparam>
    /// <param name="onInvalid">A function to execute if the instance is invalid. The function receives a collection of  <see
    /// cref="InvalidEntry"/> objects representing the validation failures.</param>
    /// <param name="onValid">A function to execute if the instance is valid. The function receives the valid value of type <typeparamref
    /// name="T"/>.</param>
    /// <returns>The result of invoking either <paramref name="onInvalid"/> or <paramref name="onValid"/>,  depending on the
    /// validity of the instance.</returns>
    public TOut Match<TOut>(Func<IEnumerable<InvalidEntry>, TOut> onInvalid, Func<T, TOut> onValid)

        => IsValid ? onValid(_value!) : onInvalid(Failures);

    /// <summary>
    /// Executes one of two actions based on the validity of the current value.
    /// </summary>
    /// <remarks>This method allows callers to handle both valid and invalid states of the value in a
    /// functional style. The appropriate action is invoked based on the result of the <see cref="IsValid"/>
    /// property.</remarks>
    /// <param name="act_onInvalid">An action to execute if the value is invalid. The action receives a collection of  <see cref="InvalidEntry"/>
    /// objects representing the validation failures.</param>
    /// <param name="act_onValid">An action to execute if the value is valid. The action receives the valid value of type <typeparamref
    /// name="T"/>.</param>
    public void Match(Action<IEnumerable<InvalidEntry>> act_onInvalid, Action<T> act_onValid)
    {
        if (true == IsValid) act_onValid(_value!); else act_onInvalid(Failures);
    }

    /// <summary>
    /// Transforms the current valid value to a new value of type <typeparamref name="TOut"/> using the specified
    /// mapping function. This operation preserves the validation state - if the current instance is invalid,
    /// the failures are carried forward without executing the transformation.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value. Must be a non-nullable type.</typeparam>
    /// <param name="onValid">
    /// A function to transform the current valid value to a new value of type <typeparamref name="TOut"/>.
    /// This function is only executed if the current <see cref="Validated{T}"/> instance is in a valid state.
    /// </param>
    /// <returns>
    /// A <see cref="Validated{TOut}"/> instance containing the transformed value if the current instance is valid;
    /// otherwise, an invalid <see cref="Validated{TOut}"/> instance with the same failures as the current instance.
    /// </returns>
    public Validated<TOut> Map<TOut>(Func<T, TOut> onValid) where TOut : notnull

        => IsValid ? Validated<TOut>.Valid(onValid(_value!)) : Validated<TOut>.Invalid(Failures);


    /// <summary>
    /// Transforms the current valid value to a new value of type <typeparamref name="TOut"/>  using the specified
    /// asynchronous mapping function.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value. Must be a non-nullable type.</typeparam>
    /// <param name="onValid">A function to asynchronously map the current valid value to a new value of type <typeparamref name="TOut"/>.</param>
    /// <returns>A <see cref="Validated{TOut}"/> instance containing the transformed value if the current instance is valid; 
    /// otherwise, an invalid <see cref="Validated{TOut}"/> instance with the same failures as the current instance.</returns>
    public async Task<Validated<TOut>> Map<TOut>(Func<T, Task<TOut>> onValid) where TOut : notnull
    
        => IsValid ? Validated<TOut>.Valid(await onValid(_value!).ConfigureAwait(false)) : Validated<TOut>.Invalid(Failures);

}

