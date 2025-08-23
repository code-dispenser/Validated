using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Validated.Core.Common.Utilities;

/// <summary>
/// Provides caching utilities for compiled expression delegates to improve performance.
/// </summary>
/// <remarks>
/// This class avoids repeated compilation of member access expressions by storing
/// them in an internal cache keyed by expression signature.
/// </remarks>
internal static class InternalCache
{
    private static readonly ConcurrentDictionary<(Type, MemberInfo), Delegate> _memberSelectorCache   = new();
    private static readonly ConcurrentDictionary<(Type, string), Delegate>     _expressionStringCache = new();
    private static readonly ConcurrentDictionary<MemberInfo, string>            _memberNameCache      = new();

    /// <summary>
    /// Retrieves a compiled delegate for the given member expression from the cache,
    /// or compiles and adds it to the cache if not already present.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity containing the member.</typeparam>
    /// <typeparam name="TMember">The type of the member being accessed.</typeparam>
    /// <param name="selectorExpression">The member access expression to compile and cache.</param>
    /// <returns>A compiled delegate that evaluates the specified member on an entity.</returns>

    public static Func<TEntity, TMember> GetAddMemberExpression<TEntity, TMember>(Expression<Func<TEntity, TMember>> selectorExpression)
    {
        // Try to extract member info for optimal caching
        var memberInfo = GeneralUtils.ExtractMemberInfo(selectorExpression.Body);

        if (memberInfo != null)
        {
            var memberCacheKey = (typeof(TEntity), memberInfo);

            if (_memberSelectorCache.TryGetValue(memberCacheKey, out var cached)) return (Func<TEntity, TMember>)cached;

            var compiledSelector = selectorExpression.Compile();
            
            _memberSelectorCache.TryAdd(memberCacheKey, compiledSelector);

            return compiledSelector;
        }

        // Fallback to expression string caching for complex expressions
        var stringCacheKey = (typeof(TEntity), selectorExpression.ToString());

        if (_expressionStringCache.TryGetValue(stringCacheKey, out var stringCached)) return (Func<TEntity, TMember>)stringCached;

        var compiledFallback = selectorExpression.Compile();

        _expressionStringCache.TryAdd(stringCacheKey, compiledFallback);

        return compiledFallback;
    }

    public static string GetAddMemberName<TEntity, TMember>(Expression<Func<TEntity, TMember>> selectorExpression)
    {
        var memberInfo = GeneralUtils.ExtractMemberInfo(selectorExpression.Body);

        if (memberInfo != null) return _memberNameCache.GetOrAdd(memberInfo, m => m.Name);

        return GeneralUtils.ExtractMemberName(selectorExpression.Body) ?? GeneralUtils.GenerateFallbackName<TMember>();
    }

#if DEBUG
    public static void ClearCache()
    {
        _expressionStringCache.Clear();
        _memberNameCache.Clear();
        _memberSelectorCache.Clear();
    }

    public static int GetCacheItemCount(string cacheType)

        => cacheType switch
        {
            "MemberSelector"    => _memberSelectorCache.Count,
            "MemberName"        => _memberNameCache.Count,
            "ExpressionString"  => _expressionStringCache.Count,
            _ => 0
        };

#endif

}