<h1>
<img src="https://raw.github.com/code-dispenser/Validated/main/assets/logo-64.png" align="center" alt="Conditionals icon" /> Validated
</h1>

## Overview

This standalone solution using the Validated.Core NuGet package demonstrates validating N-level deep recursive structures and how to target collections for validation when using the `TenantValidationBuilder` with `ValidationRuleConfig` items.

## Validating Recursive Structures

The example shows how you can set a limit on the validation depth to prevent stack overflows on structures where you may not be able to control how deep they could grow.
By default, the validation process allows a depth of 100 before terminating the recursive validation process with a maximum depth validation failure. This failure is added to the list of validation failures that may have occurred prior to the maximum recursive depth being exceeded.
The examples demonstrate how you can validate recursive structures in three ways:

- Using extension methods directly
- Using the `ValidationBuilder` for static validations
- Using the `TenantValidationBuilder` for dynamic configuration-based validation

All approaches show the use of `ValidatedContext` to set the desired maximum recursion depth via the `ValidationOptions` parameter.


## Collections with TenantValidationBuilder

The process of validating collection items is similar to using the `ValidationBuilder`, as shown in the Validated.Core demo project within the main solution.
This example highlights an important distinction when working with collections: you must override the default TargetType property in the ValidationRuleConfig class when you want to validate the collection itself (such as its length) rather than the items within it.

### TargetType Configuration

- Default: TargetType_Item - validates individual items within the collection
- Collection validation: TargetType_Collection - validates the collection itself (e.g., length, count)


This override is necessary because the validation process uses the TargetType to determine whether to apply validation rules to the collection as a whole.

### Key Concepts Demonstrated

- Recursive validation depth control
- ValidationOptions and ValidatedContext usage
- Static vs dynamic validation approaches
- Collection vs item-level validation targeting
- Prevention of stack overflow in deep structures
