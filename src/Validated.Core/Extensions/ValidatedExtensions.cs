using Validated.Core.Types;

namespace Validated.Core.Extensions;

/// <summary>
/// Provides a set of extension methods to simplify common validation-related tasks.
/// </summary>
/// <remarks>
/// These methods are designed to be chained with other validation components,
/// offering a fluent interface for configuring and executing validation rules.
/// They provide a clean and readable way to interact with the core validation
/// logic without exposing the underlying complexity.
/// </remarks>
public static class ValidatedExtensions
{
    /// <summary>
    /// Applies a validated function to a validated value, combining their validation states.
    /// This is the applicative functor operation for the <see cref="Validated{T}"/> type.
    /// </summary>
    /// <typeparam name="TIn">The input type that the function accepts.</typeparam>
    /// <typeparam name="TOut">The output type that the function produces.</typeparam>
    /// <param name="validatedFunc">A validated function that transforms values from <typeparamref name="TIn"/> to <typeparamref name="TOut"/>.</param>
    /// <param name="validatedItem">A validated value of type <typeparamref name="TIn"/> to which the function will be applied.</param>
    /// <returns>
    /// A <see cref="Validated{TOut}"/> containing the result of applying the function to the value if both are valid,
    /// or a combined set of failures if either or both are invalid.
    /// </returns>
    /// <remarks>
    /// If both the function and value are valid, the function is applied and the result is wrapped in a valid state.
    /// If either or both are invalid, all failures are collected and returned in an invalid state.
    /// This enables validation composition where multiple validation failures can be accumulated.
    /// </remarks>
    public static Validated<TOut> Apply<TIn, TOut>(this Validated<Func<TIn, TOut>> validatedFunc, Validated<TIn> validatedItem) where TIn : notnull where TOut : notnull
    {
        if (validatedFunc.IsValid && validatedItem.IsValid)
        {
            var func = validatedFunc.GetValueOr(default!);
            var value = validatedItem.GetValueOr(default!);
            var result = func(value);

            return Validated<TOut>.Valid(result);
        }

        var failures = new List<InvalidEntry>();

        if (validatedFunc.IsInvalid) failures.AddRange(validatedFunc.Failures);
        if (validatedItem.IsInvalid) failures.AddRange(validatedItem.Failures);

        return Validated<TOut>.Invalid(failures);
    }

    /// <summary>
    /// Asynchronously applies a validated function to a validated value, combining their validation states.
    /// This is the asynchronous version of the applicative functor operation.
    /// </summary>
    /// <typeparam name="TIn">The input type that the function accepts.</typeparam>
    /// <typeparam name="TOut">The output type that the function produces.</typeparam>
    /// <param name="validatedFunc">A task containing a validated function.</param>
    /// <param name="validatedItem">A task containing a validated value to which the function will be applied.</param>
    /// <returns>
    /// A task containing a <see cref="Validated{TOut}"/> with the result of applying the function to the value if both are valid,
    /// or a combined set of failures if either or both are invalid.
    /// </returns>
    /// <remarks>
    /// Both tasks are awaited concurrently, then the same logic as the synchronous version is applied.
    /// This enables efficient asynchronous validation composition.
    /// </remarks>
    public static async Task<Validated<TOut>> Apply<TIn, TOut>(this Task<Validated<Func<TIn, TOut>>> validatedFunc, Task<Validated<TIn>> validatedItem) where TIn : notnull where TOut : notnull
    {
        var funcResult = await validatedFunc;
        var itemResult = await validatedItem;

        if (funcResult.IsValid && itemResult.IsValid)
        {
            var func    = funcResult.GetValueOr(default!);
            var value   = itemResult.GetValueOr(default!);
            var result  = func(value);

            return Validated<TOut>.Valid(result);
        }

        var failures = new List<InvalidEntry>();

        if (funcResult.IsInvalid) failures.AddRange(funcResult.Failures);
        if (itemResult.IsInvalid) failures.AddRange(itemResult.Failures);

        return Validated<TOut>.Invalid(failures);
    }

    /// <summary>
    /// Combines two member validators using logical AND semantics, where both validators must pass for the overall validation to succeed.
    /// </summary>
    /// <typeparam name="T">The type of value being validated.</typeparam>
    /// <param name="thisValidator">The first validator to execute.</param>
    /// <param name="nextFunc">The second validator to execute after the first.</param>
    /// <returns>
    /// A <see cref="MemberValidator{T}"/> that executes both validators and returns success only if both pass,
    /// or combines all failures if either or both fail.
    /// </returns>
    /// <remarks>
    /// Both validators are always executed regardless of the result of the first validator, allowing for
    /// comprehensive failure collection. The input value is returned unchanged if both validations pass.
    /// </remarks>
    public static MemberValidator<T> AndThen<T>(this MemberValidator<T> thisValidator, MemberValidator<T> nextFunc) where T : notnull

         => async (input, path, compareTo, cancellationToken) =>
         {
             var firstResult  = await thisValidator(input, path, compareTo, cancellationToken).ConfigureAwait(false); 
             var secondResult = await nextFunc(input, path, compareTo, cancellationToken).ConfigureAwait(false); 

             return (firstResult.IsValid && secondResult.IsValid)
                         ? Validated<T>.Valid(input)
                             : Validated<T>.Invalid([.. firstResult.Failures, .. secondResult.Failures]);
         };


    /// <summary>
    /// Combines multiple entity validators into a single validator that executes all validators and collects their results.
    /// </summary>
    /// <typeparam name="T">The type of entity being validated.</typeparam>
    /// <param name="validators">An array of entity validators to combine.</param>
    /// <returns>
    /// An <see cref="EntityValidator{T}"/> that executes all provided validators and returns success only if all pass,
    /// or returns all collected failures if any validator fails.
    /// </returns>
    /// <remarks>
    /// All validators are executed sequentially and their results are combined. This allows for comprehensive
    /// validation where multiple aspects of an entity are validated independently and all failures are reported.
    /// The original entity is returned unchanged if all validations pass.
    /// </remarks>
    public static EntityValidator<T> Combine<T>(params EntityValidator<T>[] validators) where T : notnull

        => async (input, path, context, cancellationToken) =>
        {
            var allFailures = new List<InvalidEntry>();

            foreach (var validator in validators)
            {
                var result = await validator(input, path, context, cancellationToken).ConfigureAwait(false);
                
                if (result.IsInvalid) allFailures.AddRange(result.Failures); // single growing list
            }

            return allFailures.Count == 0 ? Validated<T>.Valid(input) : Validated<T>.Invalid(allFailures);
        };


    /// <summary>
    /// Combines two validated values using a builder function, succeeding only if both values are valid.
    /// </summary>
    /// <typeparam name="T1">The type of the first validated value.</typeparam>
    /// <typeparam name="T2">The type of the second validated value.</typeparam>
    /// <typeparam name="TOut">The type of the combined result.</typeparam>
    /// <param name="validations">A tuple containing the two validated values to combine.</param>
    /// <param name="builder">A function that combines the two valid values into a result of type <typeparamref name="TOut"/>.</param>
    /// <returns>
    /// A <see cref="Validated{TOut}"/> containing the combined result if both inputs are valid,
    /// or all collected failures if either or both inputs are invalid.
    /// </returns>
    /// <remarks>
    /// The builder function is only executed if both input validations are valid. If either input is invalid,
    /// their failures are collected and returned without executing the builder function.
    /// </remarks>
    public static Validated<TOut> Combine<T1, T2, TOut>(this (Validated<T1> first, Validated<T2> second) validations, Func<T1, T2, TOut> builder) where T1 : notnull where T2 : notnull where TOut : notnull
    {
        var (first, second)   = validations;
        var validationEntries = new List<InvalidEntry>();

        if (true == first.IsInvalid)  validationEntries.AddRange(first.Failures);
        if (true == second.IsInvalid) validationEntries.AddRange(second.Failures);

        return validationEntries.Count == 0 ? Validated<TOut>.Valid(builder(first.GetValueOr(default!), second.GetValueOr(default!)))
                                            : Validated<TOut>.Invalid(validationEntries);

    }


    /// <summary>
    /// Combines three validated values using a builder function, succeeding only if all three values are valid.
    /// </summary>
    /// <typeparam name="T1">The type of the first validated value.</typeparam>
    /// <typeparam name="T2">The type of the second validated value.</typeparam>
    /// <typeparam name="T3">The type of the third validated value.</typeparam>
    /// <typeparam name="TOut">The type of the combined result.</typeparam>
    /// <param name="validations">A tuple containing the three validated values to combine.</param>
    /// <param name="builder">A function that combines the three valid values into a result of type <typeparamref name="TOut"/>.</param>
    /// <returns>
    /// A <see cref="Validated{TOut}"/> containing the combined result if all inputs are valid,
    /// or all collected failures if any input is invalid.
    /// </returns>
    /// <remarks>
    /// The builder function is only executed if all three input validations are valid. If any input is invalid,
    /// their failures are collected and returned without executing the builder function.
    /// </remarks>
    public static Validated<TOut> Combine<T1, T2, T3, TOut>(this (Validated<T1> first, Validated<T2> second, Validated<T3> third) validations, Func<T1, T2, T3, TOut> builder) where T1 : notnull where T2 : notnull where T3 : notnull where TOut : notnull
    {
        var (first, second, third)   = validations;
        var validationEntries        = new List<InvalidEntry>();

        if (true == first.IsInvalid) validationEntries.AddRange(first.Failures);
        if (true == second.IsInvalid) validationEntries.AddRange(second.Failures);
        if (true == third.IsInvalid) validationEntries.AddRange(third.Failures);

        return validationEntries.Count == 0 ? Validated<TOut>.Valid(builder(first.GetValueOr(default!), second.GetValueOr(default!), third.GetValueOr(default!)))
                                            : Validated<TOut>.Invalid(validationEntries);
    }


    /// <summary>
    /// Combines four validated values using a builder function, succeeding only if all four values are valid.
    /// </summary>
    /// <typeparam name="T1">The type of the first validated value.</typeparam>
    /// <typeparam name="T2">The type of the second validated value.</typeparam>
    /// <typeparam name="T3">The type of the third validated value.</typeparam>
    /// <typeparam name="T4">The type of the fourth validated value.</typeparam>
    /// <typeparam name="TOut">The type of the combined result.</typeparam>
    /// <param name="validations">A tuple containing the four validated values to combine.</param>
    /// <param name="builder">A function that combines the four valid values into a result of type <typeparamref name="TOut"/>.</param>
    /// <returns>
    /// A <see cref="Validated{TOut}"/> containing the combined result if all inputs are valid,
    /// or all collected failures if any input is invalid.
    /// </returns>
    /// <remarks>
    /// The builder function is only executed if all four input validations are valid. If any input is invalid,
    /// their failures are collected and returned without executing the builder function.
    /// </remarks>
    public static Validated<TOut> Combine<T1, T2, T3, T4, TOut>(this (Validated<T1> first, Validated<T2> second, Validated<T3> third, Validated<T4> forth) validations, 
                                                                     Func<T1, T2, T3, T4, TOut> builder) where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull where TOut : notnull
    {
        var (first, second, third, forth) = validations;
        var validationEntries             = new List<InvalidEntry>();

        if (true == first.IsInvalid) validationEntries.AddRange(first.Failures);
        if (true == second.IsInvalid) validationEntries.AddRange(second.Failures);
        if (true == third.IsInvalid) validationEntries.AddRange(third.Failures);
        if (true == forth.IsInvalid) validationEntries.AddRange(forth.Failures);

        return validationEntries.Count == 0 ? Validated<TOut>.Valid(builder(first.GetValueOr(default!), second.GetValueOr(default!), third.GetValueOr(default!), forth.GetValueOr(default!)))
                                            : Validated<TOut>.Invalid(validationEntries);
    }

}

