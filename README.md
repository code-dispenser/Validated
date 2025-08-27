[![.NET](https://github.com/code-dispenser/Validated/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/code-dispenser/Validated/actions/workflows/dotnet.yml) 
[![Coverage Status](https://coveralls.io/repos/github/code-dispenser/Validated/badge.svg?branch=main)](https://coveralls.io/github/code-dispenser/Validated?branch=main)
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

## 1. Getting started

Add the Validator.Core nuget package to your project using Nuget Package Manager or the dotnet CLI:

```csharp
dotnet add package Validator.Core
```

## 2. The basics - Validated&lt;T&gt; Type, used for all returns (the seed that sprouted to form this library).

Every validator returns a Validated<T>, which is either Valid (with the value) or Invalid (with a list of one or more `InvalidEntry` objects). But its just a type, so it can be returned from any method.
```csharp
private static Validated<string> Hello(string input)  
     => input == "World"
            ? Validated<string>.Valid(input + " is correct")
                : Validated<string>.Invalid(new InvalidEntry("Expected 'World' to be entered"));
```

## 3. MemberValidator&lt;T&gt; deletage (the building block for all validators)
The core building block is MemberValidator&lt;T&gt;, which is simply a delegate (function) that takes a value (also has optional paramaters, more on those later) and returns a Validated<T>. You can implement your own validators by writing any function that matches this signature:
```csharp

    public static MemberValidator<string> CreateHelloWorldValidator(string failureMessage)

        => (valueToValidate, path, _, _) => // the delegate needs a value, but we can discard optional params if not needed (path, compareTo, cancellationToken)  
        {
            /*
                * The delegate MemberValidator returns a Task<Validated<T>> so as we have no async stuff in here to await we just use Task.FromResult 
            */ 
            return (valueToValidate == "World") ? Task.FromResult(Validated<string>.Valid(valueToValidate)) 
                                                    : Task.FromResult(Validated<string>.Invalid(new InvalidEntry(failureMessage,path)));
                                                      //path is good to add as its populated when validating entities
        };   
```



## 4. Built-In Validators
The library provides a static `MemberValidators` class with common, pre-built validator factories for scenarios like regex, string length, ranges, and more.

You can compose validators together using the `.AndThen()` extension method. This chains validators together, and crucially, all validators are executed to accumulate every failure.

**Note:** It can sometimes be beneficial to create separate validators even if one can do the job.  
For example regex patterns can include lengths, but it may be better to have two separate validators to gain two failure messages rather than trying to squeeze all the information into a long failure message, when in reality its only one part of the data that was errant like the length

```csharp
// Create a validator for a name that must start with a capital, not have double spaces,apostrophes or double dashes 
// and be between 2 and 50 characters.

var namePattern = @"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$";

// Chain two validators together
var nameValidator = MemberValidators.CreateStringRegexValidator(
                        namePattern, "FirstName", "First name", "Must start with a capital and not contain double spaces, apostrophes or dashes"
                        )
                    .AndThen(MemberValidators.CreateStringLengthValidator(
                            2, 50,"FirstName", "First name","Must be between 2 and 50 characters in length")
                        );

// --- Usage ---
var validatedName = await nameValidator("S"); // This will fail both checks

// The result contains two failures
Console.WriteLine(validatedName.Failures.Count); // Outputs: 2
validatedName.Failures.ToList().ForEach(f => Console.WriteLine(f.FailureMessage));
// Outputs:
// Must start with a capital and not contain double spaces, apostrophes or dashes
// Must be between 2 and 50 characters in length

```
## 5. Using the ValidationBuilder&lt;T&gt; 
For validating complex objects, the `ValidationBuilder<T>` provides a fluent and discoverable API.

```csharp

var contactValidator = ValidationBuilder<ContactDto>.Create()
    .ForMember(c => c.Title,
        MemberValidators.CreateStringRegexValidator("^(Mr|Mrs|Ms|Dr|Prof)$", "Title", "Title", "Invalid title"))
    .ForMember(c => c.GivenName,
        MemberValidators.CreateStringRegexValidator(@"^[A-Z][a-z]+$", "GivenName", "First name", "Invalid name"))
    .Build();

var contact   = new ContactDto { Title = "Mr", GivenName = "John" };
var validated = await contactValidator(contact);

```
**Note:** Reuse with shared validators.  
Because `MemberValidator<T>` is just a delegate, you can (and should) place common ones in shared static classes to avoid duplication.

For example, you could put your frequently reused field validators in a GeneralFieldValidators class:
```csharp
public static class GeneralFieldValidators
{
    public static MemberValidator<string> TitleValidator() 
        
        => MemberValidators.CreateStringRegexValidator("^(Mr|Mrs|Ms|Dr|Prof)$", "Title", "Title", "Must be a valid title");

    public static MemberValidator<string> GivenNameValidator() 
        
        => MemberValidators.CreateStringRegexValidator(@"^(?=.{2,50}$)[A-Z][a-z]+$", "GivenName", "First name", "Must start with a capital and be 2–50 characters");

    public static MemberValidator<string> FamilyNameValidator() 
    
        => MemberValidators.CreateStringRegexValidator(@"^[A-Z][a-z]+$", "FamilyName", "Surname", "Must start with a capital letter")
            .AndThen(MemberValidators.CreateStringLengthValidator(2, 50, "FamilyName", "Surname", "Must be between 2 and 50 characters"));
}

//these are just returning functions with the provided details baked in, ready for the value to validate.
```
Now a ValidationBuilder can reuse the appropriate validator for any matching fields:

```csharp
var contactValidator = ValidationBuilder<ContactDto>.Create()
                        .ForMember(c => c.Title, GeneralFieldValidators.TitleValidator())
                            .ForMember(c => c.GivenName, GeneralFieldValidators.GivenNameValidator())
                                .ForMember(c => c.FamilyName, GeneralFieldValidators.FamilyNameValidator())
                                    .Build();

var validated = await contactValidator(contact)
```
### Handling Nested Objects and Nullable Properties

The builder makes it easy to handle complex object graphs.

- Use `ForNestedMember` to validate a property that is itself a complex object. You can reuse another builder for the nested type.

- Use `ForNullableMember` for optional properties. Validation is only triggered if the property is not null.

- Use `ForNullableNestedMember` for optional complex object properties.


```csharp
// First, create a validator for the nested AddressDto
var addressValidator = ValidationBuilder<AddressDto>.Create()
                        .ForMember(a => a.AddressLine, GeneralFieldValidators.AddressLineValidator())
                            .ForMember(a => a.TownCity, GeneralFieldValidators.TownCityValidator())
                                .ForMember(a => a.County, GeneralFieldValidators.CountyValidator())
                                    .ForNullableStringMember(a => a.Postcode, GeneralFieldValidators.UKPostcodeValidator()) // Nullable primitive
                                        .Build();

// Now, use it in the parent ContactDto validator
var contactValidator = ValidationBuilder<ContactDto>.Create()
                        .ForMember(c => c.GivenName, GeneralFieldValidators.GivenNameValidator())
                            .ForMember(c => c.FamilyName, GeneralFieldValidators.FamilyNameValidator())
                                .ForNullableMember(c => c.NullableAge, GeneralFieldValidators.NullableAgeValidator()) // Nullable value type
                                 .ForNestedMember(c => c.Address, addressValidator) // Required nested object
                                    .ForNullableNestedMember(c => c.NullableAddress, addressValidator) // Optional nested object
                                        .Build();

var validated = await contactValidator(contact)
```
### Validating Collections
The builder has specific methods for validating collections:


- `ForEachCollectionMember`: Validates each item in a collection of complex types.


- `ForEachPrimitiveItem`: Validates each item in a collection of primitive types.


- `ForCollection`: Validates the collection itself (e.g., its size).

```csharp
// Validator for items in the ContactMethods collection
var contactMethodValidator = ValidationBuilder<ContactMethodDto>.Create()
                                .ForMember(c => c.MethodType, GeneralFieldValidators.MethodTypeValidator())
                                    .ForMember(c => c.MethodValue, GeneralFieldValidators.MethodValueValidator())
                                        .Build();

var contactValidator = ValidationBuilder<ContactDto>.Create()
                        // Validate each string in the 'Entries' list
                        .ForEachPrimitiveItem(c => c.Entries, GeneralFieldValidators.EntryValidator())
                            // Validate the 'Entries' list itself (e.g., must have 1-3 items)
                            .ForCollection(c => c.Entries, GeneralFieldValidators.EntryCountValidator())
                                // Validate each complex object in the 'ContactMethods' list
                                .ForEachCollectionMember(c => c.ContactMethods, contactMethodValidator)
                                    .Build();

var validated = await contactValidator(contact)
```

## 6. Advanced Usage: Dynamic Validation with TenantValidationBuilder&lt;T&gt;
For multi-tenant applications or scenarios where validation rules need to be dynamic, the library provides the `TenantValidationBuilder<T>`.

Instead of providing validator instances directly, this builder creates them at runtime from a list of `ValidationRuleConfig` objects. This configuration data can be loaded from a database, a JSON file, or any other source, and can be cached and periodically refreshed. 
It supports tenant- and culture-specific rule resolution. But just like the rest of the library everything is comprised of the `MemberValidator<T>` delegate, so just functions all the way until your `Validated<T>` return (all without reflection or source generators).  

More detailed standalone solutions showing more advanced usage of the library will be placed in the main examples folder in the repo as they are completed.

It is also possible to validate Value Object both statically and dynamically from configuration data in such a way as not to violate (IMHO) your protected core domain project.

## 7. Demo Project
A Console Demo project is included within the solution containing the project for the Nuget so you can step into to source code if necessary.

Just uncomment the section you want to run in `Program.cs`.

## 8. Roadmap
Full documentation (working on).

Advanced scenario solutions in to placed in the /examples folder (working on) showing:

- Multi-tenant configuration-driven validators (TenantValidationBuilder)

- Validating domain value objects without polluting your domain model (Done)

- Advanced collection and recursive scenarios
