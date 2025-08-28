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