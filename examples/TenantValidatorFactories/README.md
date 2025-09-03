<h1>
<img src="https://raw.github.com/code-dispenser/Validated/main/assets/logo-64.png" align="center" alt="Conditionals icon" /> Validated
</h1>

## Overview

This standalone solution demonstrates how to use the **Validated.Core** NuGet package with built-in validation factories when working with multi-tenant, dynamic, or configuration-based validators.  
It highlights the specific properties of the `ValidationRuleConfig` record class that must be set for the built-in validators to function as expected.

The first example in `Program.cs` shows how to use validation factories directly, without the `TenantValidationBuilder`. It then demonstrates the builder, which is the **normal usage**.  
The builder essentially acts as a coordinator — you can achieve the same effect with a static class and `ValidatorExtensions` if you prefer.

> **Note**  
> This demo is **not** about the `TenantValidationBuilder`. Please refer to the solution itself for examples and additional details about the builder.


### Validator Factories

- **`RegexValidatorFactory`** – validates using regular expressions.  
- **`StringLengthValidatorFactory`** – validates that a string’s length is between min and max (inclusive).  
- **`RangeValidatorFactory`** – validates that a comparable type’s value is between min and max (inclusive).  
- **`CollectionLengthValidatorFactory`** – validates that an `IEnumerable`’s length is between min and max (inclusive).  
- **`RollingDateOnlyValidatorFactory`** – validates that a `DateOnly` value is between min and max dates relative to today’s date.  
- **`ComparisonValidatorFactory`** – validates by comparing members to other members or config values (e.g., equal to, less than, greater than, etc.).  


### Example: Setup and DI Registration for Configuration-Based Validators

```csharp
// For Microsoft.Extensions.DependencyInjection
// LoggerFactory is added automatically

services.AddLogging(logging =>
{
    logging.AddConsole(); // Your preferred sink
    logging.SetMinimumLevel(LogLevel.Error);
});

services.AddSingleton<IValidatorFactoryProvider, ValidatorFactoryProvider>();

// Example for Autofac:

var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole(); // Your preferred sink
    logging.SetMinimumLevel(LogLevel.Error);
});

builder.RegisterInstance(loggerFactory).As<ILoggerFactory>().SingleInstance();

/*
 * Validators for config data are created from factories
 * that are stored inside this class, so just register it as a singleton.
 */
builder.RegisterType<ValidatorFactoryProvider>().As<IValidatorFactoryProvider>().SingleInstance();
```
