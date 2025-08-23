using System.Collections;
using System.Linq.Expressions;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Extensions;

/// <summary>
/// Provides a set of extension methods to aid in the creation of Entity validators.
/// </summary>
/// <remarks>
/// These methods offer a convenient and fluent way to help build the different validators used to validate
/// entity members.
/// </remarks>
public static class ValidatorExtensions
{

    /// <summary>
    /// Ensures that the root path is set to the entity type name if the provided path is null or whitespace.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being validated.</typeparam>
    /// <param name="path">The path to validate and potentially modify.</param>
    /// <returns>The original path if valid, otherwise the entity type name.</returns>
    private static string EnsureRootPath<TEntity>(string path)

        => string.IsNullOrWhiteSpace(path) ? typeof(TEntity).Name : path;

    /// <summary>
    /// Creates an entity validator that validates a specific member of an entity using the provided member validator.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity containing the member.</typeparam>
    /// <typeparam name="TMember">The type of the member to validate.</typeparam>
    /// <param name="validator">The member validator to apply to the selected member.</param>
    /// <param name="selectorExpression">Expression that selects the member to validate from the entity.</param>
    /// <returns>
    /// An <see cref="EntityValidator{TEntity}"/> that applies the specified member validator to the selected member.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ForEntityMember<TEntity, TMember>(this MemberValidator<TMember> validator, Expression<Func<TEntity, TMember>> selectorExpression) where TEntity : notnull where TMember : notnull
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var compiledSelector = InternalCache.GetAddMemberExpression(selectorExpression);//Partially apply these for performance. Caching the compiled selector is the 35x faster win.
        var memberName       = InternalCache.GetAddMemberName(selectorExpression);
        
        return async (entity, path, _, cancellationToken) =>
        {
  
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name,memberName,memberName,CauseType.SystemError));

            var rootPath = EnsureRootPath<TEntity>(path);
            var value    = compiledSelector(entity);
            var fullPath = GeneralUtils.BuildFullPath(rootPath, memberName);

            return (await validator(value, fullPath,default, cancellationToken).ConfigureAwait(false)).Map(_ => entity);
        };
    }

    /// <summary>
    /// Creates an entity validator that validates a nullable string member of an entity, skipping validation if the value is null.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity containing the member.</typeparam>
    /// <param name="validator">The string validator to apply to the selected member.</param>
    /// <param name="selectorExpression">Expression that selects the nullable string member to validate from the entity.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ForNullableStringEntityMember<TEntity>(this MemberValidator<string> validator, Expression<Func<TEntity, string?>> selectorExpression) where TEntity : notnull
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var compiledSelector = InternalCache.GetAddMemberExpression(selectorExpression);
        var memberName       = InternalCache.GetAddMemberName(selectorExpression);

        return  async (entity, path, _, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, memberName, memberName, CauseType.SystemError));

            var rootPath = EnsureRootPath<TEntity>(path);
            var value    = compiledSelector(entity);
            var fullPath = GeneralUtils.BuildFullPath(rootPath, memberName);

            if (value is null) return Validated<TEntity>.Valid(entity);

            return (await validator(value, fullPath,default, cancellationToken).ConfigureAwait(false)).Map(_ => entity);
        };
    }

    /// <summary>
    /// Creates an entity validator that validates a nullable value type member of an entity, skipping validation if the value is null (think optional value).
    /// </summary>
    /// <typeparam name="TEntity">The type of entity containing the member.</typeparam>
    /// <typeparam name="TMember">The value type of the member to validate.</typeparam>
    /// <param name="validator">The member validator to apply to the selected member.</param>
    /// <param name="selectorExpression">Expression that selects the nullable member to validate from the entity.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ForNullableEntityMember<TEntity, TMember>(this MemberValidator<TMember> validator, Expression<Func<TEntity, TMember?>> selectorExpression) where TEntity : notnull where TMember : struct
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var compiledSelector = InternalCache.GetAddMemberExpression(selectorExpression);
        var memberName       = InternalCache.GetAddMemberName(selectorExpression);

        return async (entity, path, _, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, memberName, memberName, CauseType.SystemError));

            var rootPath = EnsureRootPath<TEntity>(path);
            var value    = compiledSelector(entity);
            var fullPath = GeneralUtils.BuildFullPath(rootPath, memberName);

            if (false == value.HasValue) return Validated<TEntity>.Valid(entity);

            return (await validator(value.Value, fullPath,default, cancellationToken).ConfigureAwait(false)).Map(_ => entity);
        };
    }

    /// <summary>
    /// Creates an entity validator that validates a nested entity member using the provided entity validator.
    /// </summary>
    /// <typeparam name="TEntity">The type of the parent entity.</typeparam>
    /// <typeparam name="TProperty">The type of the nested entity to validate.</typeparam>
    /// <param name="selectorExpression">Expression that selects the nested entity from the parent entity.</param>
    /// <param name="validator">The entity validator to apply to the nested entity.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ForNestedEntityMember<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> selectorExpression, EntityValidator<TProperty> validator) where TEntity : notnull where TProperty : notnull
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var compiledSelector = InternalCache.GetAddMemberExpression(selectorExpression);
        var memberName       = InternalCache.GetAddMemberName(selectorExpression);

        return  async (entity, path, context, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, memberName, memberName, CauseType.SystemError));

            var rootPath = EnsureRootPath<TEntity>(path);
            var value    = compiledSelector(entity);
            var fullPath = GeneralUtils.BuildFullPath(rootPath, memberName);

            context ??= new ValidatedContext();

            if (context.IsValidating(value)) return Validated<TEntity>.Valid(entity);
            
            var newContext = context.WithValidating(value);

            if (value is null) return Validated<TEntity>.Invalid(new InvalidEntry($"{memberName} is required",fullPath, memberName, memberName));

            var result = await validator(value!, fullPath, newContext, cancellationToken).ConfigureAwait(false); 

            return result.IsValid ? Validated<TEntity>.Valid(entity) : Validated<TEntity>.Invalid(result.Failures);
        };
    }

    /// <summary>
    /// Creates an entity validator that validates a nullable nested entity member, skipping validation if the value is null (think optional value).
    /// </summary>
    /// <typeparam name="TEntity">The type of the parent entity.</typeparam>
    /// <typeparam name="TProperty">The type of the nested entity to validate.</typeparam>
    /// <param name="selectorExpression">Expression that selects the nullable nested entity from the parent entity.</param>
    /// <param name="validator">The entity validator to apply to the nested entity.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ForNullableNestedEntityMember<TEntity, TProperty>(Expression<Func<TEntity, TProperty?>> selectorExpression, EntityValidator<TProperty> validator) where TEntity : notnull where TProperty : class
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var compiledSelector = InternalCache.GetAddMemberExpression(selectorExpression);
        var memberName       = InternalCache.GetAddMemberName(selectorExpression);

        return async (entity, path, context, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, memberName, memberName, CauseType.SystemError));

            var value = compiledSelector(entity);

            context ??= new ValidatedContext();

            if (value is null || context.IsValidating(value)) return Validated<TEntity>.Valid(entity);

            var newContext = context.WithValidating(value);

            var rootPath = EnsureRootPath<TEntity>(path);
            var fullPath = GeneralUtils.BuildFullPath(rootPath, memberName);

            var result = await validator(value, fullPath, newContext, cancellationToken).ConfigureAwait(false);

            return result.IsValid ? Validated<TEntity>.Valid(entity) : Validated<TEntity>.Invalid(result.Failures);
        };
}

    /// <summary>
    /// Creates an entity validator that validates each item in a collection member using the provided entity validator.
    /// </summary>
    /// <typeparam name="TEntity">The type of the parent entity containing the collection.</typeparam>
    /// <typeparam name="TProperty">The type of items in the collection.</typeparam>
    /// <param name="selectorExpression">Expression that selects the collection from the parent entity.</param>
    /// <param name="itemValidator">The entity validator to apply to each item in the collection.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ForCollectionEntityMember<TEntity, TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> selectorExpression, EntityValidator<TProperty> itemValidator) where TEntity : notnull where TProperty : notnull
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var compiledSelector = InternalCache.GetAddMemberExpression(selectorExpression);
        var memberName       = InternalCache.GetAddMemberName(selectorExpression);

        return async (entity, path, context, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, memberName, memberName,  CauseType.SystemError));

            var collection       = compiledSelector(entity);
            var rootPath         = EnsureRootPath<TEntity>(path);
            var fullPath         = GeneralUtils.BuildFullPath(rootPath, memberName);

            context ??= new ValidatedContext();

            if (collection is null) return Validated<TEntity>.Invalid(new InvalidEntry($"{memberName} is required",fullPath, memberName, memberName));

            var failures = new List<InvalidEntry>();
            var index = 0;

            foreach (var item in collection)
            {
                var itemPath = $"{fullPath}[{index}]";
                var itemResult = await itemValidator(item, itemPath, context, cancellationToken).ConfigureAwait(false);
                if (itemResult.IsInvalid) failures.AddRange(itemResult.Failures);
                index++;
            }

            return failures.Count == 0 ? Validated<TEntity>.Valid(entity) : Validated<TEntity>.Invalid(failures);
        };
        }

    /// <summary>
    /// Creates an entity validator that validates each primitive item in a collection member using the provided member validator.
    /// </summary>
    /// <typeparam name="TEntity">The type of the parent entity containing the collection.</typeparam>
    /// <typeparam name="TPrimitive">The primitive type of items in the collection.</typeparam>
    /// <param name="selectorExpression">Expression that selects the collection from the parent entity.</param>
    /// <param name="primitiveValidator">The member validator to apply to each primitive item in the collection.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ForEachPrimitiveItem<TEntity, TPrimitive>(Expression<Func<TEntity, IEnumerable<TPrimitive>>> selectorExpression, MemberValidator<TPrimitive> primitiveValidator) where TEntity : notnull where TPrimitive : notnull
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var compiledSelector = InternalCache.GetAddMemberExpression(selectorExpression);
        var memberName       = InternalCache.GetAddMemberName(selectorExpression);

        return async (entity, path, _, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, memberName, memberName, CauseType.SystemError));

            var collection       = compiledSelector(entity);
            var rootPath         = EnsureRootPath<TEntity>(path);
            var fullPath         = GeneralUtils.BuildFullPath(rootPath, memberName);

            if (collection is null)   return Validated<TEntity>.Invalid(new InvalidEntry($"{memberName} is required",fullPath, memberName, memberName));

            var failures = new List<InvalidEntry>();
            var index = 0;

            foreach (var item in collection)
            {
                var itemPath = $"{fullPath}[{index}]";
                var itemResult = await primitiveValidator(item, itemPath,default, cancellationToken).ConfigureAwait(false);
                if (itemResult.IsInvalid) failures.AddRange(itemResult.Failures);
                index++;
            }

            return failures.Count == 0 ? Validated<TEntity>.Valid(entity) : Validated<TEntity>.Invalid(failures);
        };
    }

    /// <summary>
    /// Creates an entity validator that validates a collection member as a whole using the provided member validator.
    /// </summary>
    /// <typeparam name="TEntity">The type of the parent entity containing the collection.</typeparam>
    /// <typeparam name="TCollection">The type of the collection to validate.</typeparam>
    /// <param name="selectorExpression">Expression that selects the collection from the parent entity.</param>
    /// <param name="validator">The member validator to apply to the collection as a whole.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ForCollection<TEntity, TCollection>(Expression<Func<TEntity, TCollection>> selectorExpression, MemberValidator<TCollection> validator) where TEntity : notnull where TCollection : notnull, IEnumerable
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var memberName = InternalCache.GetAddMemberName(selectorExpression);

        return async (entity, path, _, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, memberName, memberName, CauseType.SystemError));

            var compiledSelector = selectorExpression.Compile();
            var collection      = compiledSelector(entity);
            var rootPath        = EnsureRootPath<TEntity>(path);
            var fullPath        = GeneralUtils.BuildFullPath(rootPath, memberName);

            if (collection is null) return Validated<TEntity>.Invalid(new InvalidEntry($"{memberName} is required",fullPath, memberName, memberName));

            return (await validator(collection, fullPath, default, cancellationToken).ConfigureAwait(false)).Map(_ => entity);
        };
}

    /// <summary>
    /// Creates an entity validator that uses a member validator for comparison operations on the entire entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to validate.</typeparam>
    /// <typeparam name="TMember">The type of the member being used for comparison context.</typeparam>
    /// <param name="validator">The member validator to apply to the entity.</param>
    /// <param name="selectorExpression">Expression that selects the member for comparison context.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="selectorExpression"/> attempts to perform nested
    /// member access (e.g. <c>c => c.Address.Postcode</c>), which is not supported.
    /// Use <see cref="ForNestedEntityMember"/> instead.
    /// </exception>
    public static EntityValidator<TEntity> ToCompareEntityMember<TEntity, TMember>(this MemberValidator<TEntity> validator, Expression<Func<TEntity, TMember>> selectorExpression) where TEntity : notnull where TMember : notnull
    {
        GeneralUtils.GuardAgainstDeepMemberAccess(selectorExpression);

        var memberName = InternalCache.GetAddMemberName(selectorExpression);

        return async (entity, path, _, cancellationToken) =>
        {
            if (entity is null) return Validated<TEntity>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(TEntity).Name, memberName, memberName, CauseType.SystemError));

            var rootPath = EnsureRootPath<TEntity>(path);
            var fullPath = GeneralUtils.BuildFullPath(rootPath, memberName);

            return (await validator(entity, fullPath, default, cancellationToken).ConfigureAwait(false)).Map(_ => entity);
        };
    }

    /// <summary>
    /// Creates a recursive entity validator that can validate entities containing references to themselves.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity that contains recursive references.</typeparam>
    /// <param name="childSelector">Expression that selects the child entity from the parent entity.</param>
    /// <param name="baseValidator">The base entity validator to apply at each level of recursion.</param>
    /// <returns>An entity validator capable of handling recursive entity structures.</returns>
    public static EntityValidator<TEntity> ForRecursiveEntity<TEntity>(Expression<Func<TEntity, TEntity>> childSelector, EntityValidator<TEntity> baseValidator) where TEntity : notnull
    {
        EntityValidator<TEntity> recursiveValidator = null!;

        recursiveValidator = async (entity, path, context, cancellationToken) =>
        {
            context ??= new ValidatedContext();

            var newContext = context.WithIncrementedDepth();

            var combined = ValidatedExtensions.Combine(baseValidator, ForRecursiveEntityMember(childSelector, recursiveValidator));

            return await combined(entity, path, newContext, cancellationToken);
        };

        return recursiveValidator;

        static EntityValidator<T> ForRecursiveEntityMember<T, TProperty>(Expression<Func<T, TProperty>> selectorExpression, EntityValidator<TProperty> validator) where T : notnull where TProperty : notnull

            => async (entity, path, context, cancellationToken) =>
            {
                var memberName = GeneralUtils.GetMemberName(selectorExpression);

                if (entity is null) return Validated<T>.Invalid(new InvalidEntry(ErrorMessages.Validator_Entity_Null_User_Message, typeof(T).Name, memberName, memberName,  CauseType.SystemError));

                var compiledSelector = selectorExpression.Compile();
                var value            = compiledSelector(entity);
                var rootPath         = EnsureRootPath<T>(path);
                var fullPath         = GeneralUtils.BuildFullPath(rootPath, memberName);

                if (value is null) return Validated<T>.Valid(entity);

                if (context!.IsMaxDepthExceeded()) return Validated<T>.Invalid(new InvalidEntry(ErrorMessages.Validator_Max_Depth_Exceeded_User_Message, fullPath, memberName, memberName,CauseType.SystemError));

                var result = await validator(value!, fullPath, context, cancellationToken).ConfigureAwait(false);

                return result.IsValid ? Validated<T>.Valid(entity) : Validated<T>.Invalid(result.Failures);
            };

        }

}
