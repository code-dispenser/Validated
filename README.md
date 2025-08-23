<h1>
<img src="https://raw.github.com/code-dispenser/Validated/main/assets/logo-64.png" align="center" alt="Conditionals icon" /> Validated
</h1>

A functional approach to validation in C#.

Validated provides a composable, functional validation framework for .NET applications.
It’s designed to make validation predictable, testable, and reusable — from simple property checks to validating complex object graphs, collections, and multi-tenant scenarios driven by configuration.

## Features

- Validated<T> result type (or applicative functor for those who care) for all validations (valid or invalid with failure details).

- MemberValidator<T> delegate as the core building block — create custom or use built-in validators.

- Fluent ValidationBuilder<TEntity> for static/manual composition of validators.

- TenantValidationBuilder<TEntity> for configuration-driven, multi-tenant, multi-culture validation.

- Combine validators for objects, nested objects, collections, and recursive graphs.

- Built-in validators for common use cases (regex, length, comparisons, etc.).

- Fully async to support your own async validators.



. .  nuget release after this readme is done as V1 code is done.