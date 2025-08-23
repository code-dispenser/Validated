using Validated.Core.Factories;
using Validated.Core.Types;

namespace Validated.Core.Tests.SharedDataFixtures.Common.ValidatorFactory
{
    public class ErrorThrowingValidatorFactory : IValidatorFactory
    {
        public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull
        
            => throw new NotImplementedException();
        
    }
}
