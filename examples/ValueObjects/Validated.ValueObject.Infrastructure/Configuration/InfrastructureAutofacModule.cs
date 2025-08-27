using Autofac;
using Validated.ValueObject.Application.SeedWork;
using Validated.ValueObject.Infrastructure.Common.Caching;

namespace Validated.ValueObject.Infrastructure.Configuration;

public class InfrastructureAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CacheRepository>().As<ICacheRepository>().InstancePerDependency();
        builder.RegisterType<CacheProvider>().AsSelf().SingleInstance();

        base.Load(builder);
    }
}

