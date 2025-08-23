namespace Validated.Core.Extensions;

/// <summary>
/// Provides general-purpose functional extension methods for fluent composition.
/// </summary>
/// <remarks>
/// <para>
/// These extensions enable a functional programming style within C#, allowing values
/// to be passed through transformation pipelines in a concise and expressive manner.
/// </para>
/// <para>
/// The methods in this class are designed to be simple building blocks that improve
/// readability and composability when working with functional patterns.
/// </para>
/// </remarks>
public static class GeneralFunctional
{
    /// <summary>
    /// Applies a function to a value in a fluent, functional style.
    /// </summary>
    /// <typeparam name="TIn">
    /// The type of the input value that will be passed into the function.
    /// </typeparam>
    /// <typeparam name="TOut">
    /// The type of the output value produced by the function.
    /// </typeparam>
    /// <param name="pipedValue">
    /// The input value to be passed into the function. This value flows through
    /// the pipeline as the subject of the transformation.
    /// </param>
    /// <param name="pipeFunc">
    /// The function to apply to <paramref name="pipedValue"/>. This function
    /// transforms the input into the resulting output value.
    /// </param>
    /// <returns>
    /// The transformed value produced by applying <paramref name="pipeFunc"/> 
    /// to <paramref name="pipedValue"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method enables functional-style chaining by allowing values to be
    /// passed directly into functions in a readable, left-to-right manner.
    /// It is particularly useful when composing multiple operations into a pipeline.
    /// </para>
    /// </remarks>
    public static TOut Pipe<TIn,TOut>(this TIn pipedValue, Func<TIn, TOut> pipeFunc) 
        
        => pipeFunc(pipedValue);
}
