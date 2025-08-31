<h1>
<img src="https://raw.github.com/code-dispenser/Validated/main/assets/logo-64.png" align="center" alt="Conditionals icon" /> Validated
</h1>

## Overview

This standalone solution using the Validated.Core NuGet package demonstrates how to create a custom validator for the multi-tenant / dynamic configuration based approach.

### Steps Involved
1. Implement the `IValidatorFactory` interface that has the single method `CreateFromConfiguration` that accepts a `ValidationRuleConfig` and returns a `MemberValidator<T>` delegate.
2. Register your custom validator with a DI container as a singleton for any of its dependencies.
3. Add it to the `ValidatorFactoryProvider` provided by Validated.Core which should also be registered as a singleton.

## Sample Implementation
The sample implements a business hours validator that checks if appointment times fall within configured working hours, excluding holidays and weekends.
It makes use of the `ValidationRuleConfig` `Dictionary<string,string> AdditionalInfo` property to store the dynamic data used by the validator which can be on a tenant per tenant basis.
This example is just using the default tenant of "ALL" as opposed to creating rules for specific tenants, covered in another example.


### Key Concepts
- Custom rule Type - each custom validator must define a unique rule type string identifier that is used to fetch from the ValidatorFactoryProvider`.
- Factory regisration - the custom validator must be added to the `ValidatorFactoryProvider` via its `AddOrUpdateFactory` method.
- Configuration data - the unique rule type identifier is added to the `ValidationRuleConfig` where appropriate. The validator can also make use of the
available properties and/or extend them via the use of the `Dictionary<string,string> AdditionalInfo` property. 
- Error handling - the library strives not to raise exceptions preferring to log them for inspection, returning invalid entries. Each `InvalidEntry` has a Cause propery that can be set to
`CauseType.Validation` (the default), `CauseType.RuleConfigError` or `CauseType.SystemError`.

## Custom Validator Dependencies 

The sample demonstrates creating a `BusinessHoursVaidatorFactory` that has an `ILoggerFactory` dependecy, what if you needed a dependency that could not be mainteined/re-used?

As your custom validator is added as a singleton/single-instance then any dependencies that you may want in your constructor would be instantiated on first resolution and only once. 

If you require an instance of the dependency per usage within the `CreateFromConfiguration<T>` method, then there are a couple of approaches you can take depending on what the dependency is. 

If the dependency has a factory such as Entity Frameworks `DbContext`, then you would be fine as you could inject the `IDbContextFactory<TContext>`. This could then be used to create an instance of the `DbContext` on demand. 
Nb. Just be aware of the number of connections that you may end up creating in case you need to take additional steps such as adding DbContextFactory pooling etc.

Likewise for a `HttpClient`you could injrect the IHttpClientFactory and create clients on demand.

If your dependency has no factory method then a simple solution is either to pass in configuration data that can be used to create the dependency or just simply use a delegate factory.

For example lets assume the `BusinessHoursValidatorFactory` needs to use some dependency type such as a `MyDependency` type. This type needs to be  a required parameter in the constructor of `BusinessHoursValidatorFactory`.

### Example setup and registration for dependencies of a custom tenant validator.
```c#
// In the BusinessHoursValidatorFactory - excludes the ILoggerFactory for brevity.

private readonly Func<MyDependency> _getMyDependency;
 
public BusinessHoursValidatorFactory(Func<MyDependency> getMyDependency)
{
    _getMyDependency = getMyDependency; 
}

public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

  => (valueToValidate, path, compareTo, cancellationToken)) =>
  {
      var dependencyInstance = _getMyDependency();
  };


// Registering this in Microsoft Dependency Injection

services.AddTransient<MyDependency>();

services.AddSingleton<BusinessHoursValidatorFactory>(sp =>
    new BusinessHoursValidatorFactory(() => sp.GetRequiredService<MyDependency>()));

// Registering this in Autofac

builder.RegisterType<MyDependency>().AsSelf().InstancePerDependency();
builder.Register(c =>
{
    var ctx = c.Resolve<IComponentContext>();
    return new BusinessHoursValidatorFactory(() => ctx.Resolve<MyDependency>());
})
.As<BusinessHoursValidatorFactory>().SingleInstance();

```
