using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Validated.Core.Common.Constants;

namespace Validated.Core.Common.Utilities
{

    /// <summary>
    /// Provides general utility methods for working with expressions, member metadata, 
    /// and validation path construction.
    /// </summary>
    /// <remarks>
    /// These helpers are used internally by builders, extensions, and factories 
    /// to extract property names, build validation paths, and safely convert values.
    /// </remarks>
    internal static class GeneralUtils
    {
        /// <summary>
        /// Gets the member name represented by a lambda expression.
        /// </summary>
        /// <typeparam name="TEntity">The type containing the member.</typeparam>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="selectorExpression">The member access expression.</param>
        /// <returns>The name of the member accessed in the expression.</returns>
        public static string GetMemberName<TEntity, TMember>(Expression<Func<TEntity, TMember>> selectorExpression)

            =>  ExtractMemberName(selectorExpression.Body) ?? GenerateFallbackName<TMember>();

        /// <summary>
        /// Builds a full validation path by combining a parent path with a member name.
        /// </summary>
        /// <param name="path">The root or parent path.</param>
        /// <param name="propertyName">The name of the member being validated.</param>
        /// <returns>A concatenated validation path.</returns>
        public static string BuildFullPath(string path, string propertyName)

            => String.IsNullOrWhiteSpace(path) ? propertyName : String.Concat(path,".", propertyName);

        /// <summary>
        /// Generates a fallback name for a member when a proper name cannot be resolved.
        /// </summary>
        /// <returns>A generated fallback name.</returns>
        public static string GenerateFallbackName<TMember>()
        
            =>  $"Unknown_{typeof(TMember).Name}_{Guid.NewGuid().ToString("N")[..8]}";


        /// <summary>
        /// Extracts the member name from a lambda expression.
        /// </summary>
        /// <param name="expression">The member access expression.</param>
        /// <returns>The name of the accessed member.</returns>
        public static string? ExtractMemberName(Expression expression)
        
            => expression switch
            {   
                MemberExpression member => member.Member.Name,
                UnaryExpression { Operand: MemberExpression member } => member.Member.Name,
                MethodCallExpression method when method.Method.Name == "get_Item" => "Item", // For indexers
                ParameterExpression param => param.Name, // For root parameter
                _ => null
            };

        /// <summary>
        /// Extracts the <see cref="MemberInfo"/> from a given expression.
        /// </summary>
        /// <param name="expression">The expression to extract the member information from.</param>
        /// <returns>
        /// The <see cref="MemberInfo"/> associated with the expression, or <c>null</c> if
        /// the expression does not represent a member access.
        /// </returns>
        public static MemberInfo? ExtractMemberInfo(Expression expression)
        
            => expression switch
            {
                MemberExpression memberExpr => memberExpr.Member,
                UnaryExpression { Operand: MemberExpression memberExpr } => memberExpr.Member,
                _ => null 
            };
        


        /// <summary>
        /// Converts a value to a string for use in validation messages.
        /// </summary>
        /// <param name="valueToValidate">The value to convert.</param>
        /// <returns>
        /// A string representation of the value, or an empty string if the value is null.
        /// </returns>
        public static string FromValue(object? valueToValidate)

            => valueToValidate switch
            {
                null => String.Empty,
                string s => s,
                DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateOnly d => d.ToString("O"),
                TimeSpan t => t.ToString(),
                _ => valueToValidate.ToString()!,

            };

        public static void GuardAgainstDeepMemberAccess<TEntity, TMember>(Expression<Func<TEntity, TMember>>? selectorExpression, [CallerMemberName] string methodName = "")
        {
            if (selectorExpression == null) return;

            if (selectorExpression!.Body is MemberExpression memberExpression)
            {
                // Check if the expression is chained (e.g., c.Address.Postcode)
                if (memberExpression.Expression is MemberExpression) throw new InvalidOperationException(String.Format(ErrorMessages.Validator_Nesting_Unsupported_Message, methodName, selectorExpression));
            }
        }
    }
}
