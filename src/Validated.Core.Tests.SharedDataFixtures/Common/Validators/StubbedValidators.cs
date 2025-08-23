using Validated.Core.Types;

namespace Validated.Core.Tests.SharedDataFixtures.Common.Validators;

public static class StubbedValidators
{
    public static MemberValidator<T> CreatePassingMemberValidator<T>() where T : notnull

        => (value, path, compareTo, _) => Task.FromResult(Validated<T>.Valid(value));
    public static MemberValidator<T> CreateFailingMemberValidator<T>(string propertyName, string displayName, string failureMessage) where T : notnull

        => (value, path, compareTo, _) => Task.FromResult(Validated<T>.Invalid(new InvalidEntry(failureMessage, path, propertyName, displayName)));

    public static EntityValidator<TEntity> CreatePassingEntityValidator<TEntity>() where TEntity : notnull

        => (entity, path, context, _) => Task.FromResult(Validated<TEntity>.Valid(entity));

    public static EntityValidator<TEntity> CreateFailingEntityValidator<TEntity>(string propertyName, string displayName, string failureMessage) where TEntity : notnull

        => (entity, path, context, _) => Task.FromResult(Validated<TEntity>.Invalid(new InvalidEntry(failureMessage,path, propertyName, displayName)));
}
