<h1>
<img src="https://raw.github.com/code-dispenser/Validated/main/assets/logo-64.png" align="center" alt="Conditionals icon" /> Validated
</h1>


## Working with Value Objects

This standalone solution using the Validated.Core NuGet package demonstrates one approach to validating the values used to create your value objects using validators that can be shared for similar inputs elsewhere in your application.

It is important to note that Value Objects, unlike Data Transfer Objects (DTOs) or entities that may be created in an invalid state, should never be created in an invalid state. This makes the validation process different from that of a DTO where you might validate the entire object after creation.

The sample shows one way of using a service to keep your Domain and Value Objects isolated and free from pollution, while still allowing them to use the validator framework for both static and dynamic validation via configuration.

I use the terms "dynamic" and "semi-dynamic" interchangeably when discussing multi-tenant/configuration-based validation, as the validators are created from configuration data without using reflection.

This was a deliberate design decision when creating the library - to avoid reflection so it's clearer when looking at the code what is happening, but with the trade-off of requiring a few more lines of code versus less code but reduced visibility into what's happening.

### Key Concepts


- Value Objects: Immutable objects that represent a descriptive aspect of the domain with no conceptual identity
- Configuration-driven Validation: Validators created from configuration data rather than hardcoded rules
- Shared Validators: Reusable validation logic that can be applied across different parts of your application
- Domain Isolation: Keeping input validation concerns separate from your core domain log



