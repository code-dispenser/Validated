using Autofac;
using Validated.ValueObject.Application.DomainServices;
using Validated.ValueObject.Domain.SeedWork;

namespace Validated.ValueObject.Application.Configuration;

public class ApplicationAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ValueObjectService>().As<ValueObjectServiceBase>().SingleInstance();

        base.Load(builder);
    }
}