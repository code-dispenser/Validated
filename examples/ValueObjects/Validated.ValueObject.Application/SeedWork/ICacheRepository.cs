using System.Collections.Immutable;
using Validated.Core.Common.Constants;
using Validated.Core.Types;

namespace Validated.ValueObject.Application.SeedWork;

public interface ICacheRepository
{
    Task<ImmutableList<ValidationRuleConfig>> GetRuleConfigurations();
}
