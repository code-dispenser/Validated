using Microsoft.Extensions.Logging;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;

namespace Validated.Core.Tests.SharedDataFixtures.Common.ValidatorFactory;

public class CustomTenantIntValidatorFactory(ILogger<CustomTenantIntValidatorFactory> logger) : IValidatorFactory
{
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (valueToValidate, path, compareTo, _) =>
        {
            
            if (typeof(int) != typeof(T)) return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig.FailureMessage,path, ruleConfig.PropertyName, ruleConfig.DisplayName)));

            try
            {
                var valid = (int)(object)valueToValidate;

                var result = valid == 42 ? Validated<T>.Valid(valueToValidate) : Validated<T>.Invalid(new InvalidEntry(ruleConfig.FailureMessage,path, ruleConfig.PropertyName, ruleConfig.DisplayName));

                return Task.FromResult(result);

            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"Configuration error causing custom validation failure for Tenant:{ruleConfig?.TenantID ?? "[Null]"} - {ruleConfig?.TypeFullName ?? "[Null]"}.{ruleConfig?.PropertyName ?? "[Null]"} " +
                        $"ValueToValidate: {valueToValidate?.ToString() ?? "[Null]"}");

                return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig?.FailureMessage ?? "", path, ruleConfig?.PropertyName ?? "", ruleConfig?.DisplayName ?? "", CauseType.RuleConfigError)));
            }

        }; 


}
