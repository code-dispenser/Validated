using System.Linq.Expressions;
using Validated.Core.Common.Constants;
using Validated.Core.Common.Utilities;
using Validated.Core.Types;

namespace Validated.Core.Validators;

public static partial class MemberValidators
{
    /// <summary>
    /// Creates a member validator that compares a value against a specified comparison value using the given comparison type.
    /// </summary>
    /// <typeparam name="T">The type of value to validate.</typeparam>
    /// <param name="compareTo">The value to compare against.</param>
    /// <param name="comparisonType">The type of comparison to perform (equal, greater than, etc.).</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that performs value comparison validation.</returns>
    public static MemberValidator<T> CreateCompareToValidator<T>(T compareTo, CompareType comparisonType, string propertyName, string displayName, string failureMessage) where T : notnull

        => (valueToValidate, path, _, _) =>
          {
              try
              {
                  //var result = PerformComparison(valueToValidate, compareTo, comparisonType)
                  //                  ? Validated<T>.Valid(valueToValidate)
                  //                      : Validated<T>.Invalid(new InvalidEntry(FailureMessages.FormatCompareValueMessage(failureMessage, valueToValidate.ToString()!, displayName, compareTo.ToString()!), BuildPathFromParams(path, propertyName), propertyName, displayName));


                  var isValid = PerformComparison(valueToValidate, compareTo, comparisonType);
                  
                  var (leftValue, rightValue) = ("", "");

                  if (false == isValid) (leftValue, rightValue) = (GeneralUtils.FromValue(valueToValidate), GeneralUtils.FromValue(compareTo));

                  var result = isValid ? Validated<T>.Valid(valueToValidate)
                                       : Validated<T>.Invalid(new InvalidEntry(FailureMessages.FormatCompareValueMessage(failureMessage, leftValue.ToString()!, displayName, rightValue.ToString()!), BuildPathFromParams(path, propertyName), propertyName, displayName));

                  return Task.FromResult(result);
              }
              catch
              {
                  return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(failureMessage, BuildPathFromParams(path, propertyName), propertyName, displayName, CauseType.SystemError)));
              }
          };

    /// <summary>
    /// Creates a member validator that compares two properties within the same entity using the specified comparison type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity containing the properties to compare.</typeparam>
    /// <typeparam name="TProperty">The type of the properties being compared.</typeparam>
    /// <param name="firstSelectorExpression">Expression that selects the first property to compare.</param>
    /// <param name="secondSelectorExpression">Expression that selects the second property to compare.</param>
    /// <param name="compareType">The type of comparison to perform between the two properties.</param>
    /// <param name="displayName">The display name used in validation error messages.</param>
    /// <param name="failureMessage">The error message to display when validation fails.</param>
    /// <returns>A member validator that compares two entity properties.</returns>
    public static MemberValidator<TEntity> CreateMemberComparisonValidator<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> firstSelectorExpression, Expression<Func<TEntity, TProperty>> secondSelectorExpression,
                                                                                               CompareType compareType, string displayName, string failureMessage)  where TEntity : notnull where TProperty : notnull
    {
        var firstCompiled  = firstSelectorExpression.Compile();
        var secondCompiled = secondSelectorExpression.Compile();
        var memberName     = GeneralUtils.GetMemberName(firstSelectorExpression);

        return (entity, path, _, _) =>
        {
            try
            {
                var firstValue  = firstCompiled(entity);
                var secondValue = secondCompiled(entity);

                var isValid = PerformComparison(firstValue, secondValue, compareType);

                var (leftValue, rightValue) = ("","");

                if (false == isValid) (leftValue, rightValue) = (GeneralUtils.FromValue(firstValue), GeneralUtils.FromValue(secondValue));

                var result  = isValid ? Validated<TEntity>.Valid(entity)
                                        : Validated<TEntity>.Invalid(new InvalidEntry(FailureMessages.FormatCompareValueMessage(failureMessage, leftValue, displayName, rightValue),BuildPathFromParams(path, memberName), memberName, displayName));
            
                return Task.FromResult(result);    
            }
            catch
            {
                return Task.FromResult(Validated<TEntity>.Invalid(new InvalidEntry(failureMessage,BuildPathFromParams(path, memberName), memberName, displayName, CauseType.SystemError) ));
            }
        };
    }

    /// <summary>
    /// Performs a comparison between two values using the specified comparison type.
    /// </summary>
    /// <param name="leftValue">The left-hand value in the comparison.</param>
    /// <param name="rightValue">The right-hand value in the comparison.</param>
    /// <param name="comparisonType">The type of comparison to perform.</param>
    /// <returns>True if the comparison succeeds, false otherwise.</returns>
    private static bool PerformComparison(object? leftValue, object? rightValue, CompareType comparisonType)
    {
        if (leftValue == null || rightValue == null) return false;

        if (leftValue is IComparable leftComparable && rightValue is IComparable)
        {
            var comparisonResult = leftComparable.CompareTo(rightValue);

            return comparisonType switch
            {
                CompareType.EqualTo             => comparisonResult == 0,
                CompareType.NotEqualTo          => comparisonResult != 0,
                CompareType.GreaterThan         => comparisonResult > 0,
                CompareType.LessThan            => comparisonResult < 0,
                CompareType.GreaterThanOrEqual  => comparisonResult >= 0,
                CompareType.LessThanOrEqual     => comparisonResult <= 0,
                _ => false
            };
        }

        return false;
    }


}
