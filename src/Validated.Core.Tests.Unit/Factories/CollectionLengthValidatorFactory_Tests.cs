using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Factories;

public class CollectionLengthValidatorFactory_Tests

{
    [Fact]
    public async Task Create_from_configuration_should_return_a_valid_validated_if_it_passes_validation()
    {
        var contact    = StaticData.CreateContactObjectGraph();
        var ruleConfig = StaticData.ValidationRuleConfigForCollectionLengthValidator(typeof(ContactDto).FullName!, nameof(ContactDto.ContactMethods), nameof(ContactDto.ContactMethods), 1, 10);
        var logger     = new InMemoryLoggerFactory().CreateLogger<CollectionLengthValidatorFactory>();
        var validator  = new CollectionLengthValidatorFactory(logger).CreateFromConfiguration<List<ContactMethodDto>>(ruleConfig);

        var validated = await validator(contact.ContactMethods, nameof(ContactDto));

        validated.Should().Match<Validated<List<ContactMethodDto>>>(v => v.IsValid == true && v.Failures.Count == 0);

    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_if_it_fails_the_length_check()
    {
        var contact     = StaticData.CreateContactObjectGraph();
        var ruleConfig  = StaticData.ValidationRuleConfigForCollectionLengthValidator(typeof(ContactDto).FullName!, nameof(ContactDto.ContactMethods), nameof(ContactDto.ContactMethods), 3, 10);
        var logger      = new InMemoryLoggerFactory().CreateLogger<CollectionLengthValidatorFactory>();
        var validator   = new CollectionLengthValidatorFactory(logger).CreateFromConfiguration<List<ContactMethodDto>>(ruleConfig);

        var validated = await validator(contact.ContactMethods, nameof(ContactDto));

        using(new AssertionScope())
        {
            validated.Should().Match<Validated<List<ContactMethodDto>>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.ContactMethods)
                                                           && i.FailureMessage == "Must have at least 3 item(s) but no more than 10 items");
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_and_log_an_error_if_the_type_is_not_a_collection_or_is_a_string()
    {
        var contact     = StaticData.CreateContactObjectGraph();
        var ruleConfig  = StaticData.ValidationRuleConfigForCollectionLengthValidator(typeof(ContactDto).FullName!, nameof(ContactDto.ContactMethods), nameof(ContactDto.ContactMethods), 3, 10);
        var logger      = new InMemoryLoggerFactory().CreateLogger<CollectionLengthValidatorFactory>();
        var validator  = new CollectionLengthValidatorFactory(logger).CreateFromConfiguration<string>(ruleConfig);

        var validated = await validator(contact.GivenName, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<string>>(v => v.IsValid == false && v.Failures.Count == 1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == nameof(ContactDto) && i.PropertyName == nameof(ContactDto.ContactMethods)
                                                           && i.FailureMessage == "Must have at least 3 item(s) but no more than 10 items");

            ((InMemoryLogger<CollectionLengthValidatorFactory>)logger).LogEntries[0]
             .Should().Match<LogEntry>(l => l.Category == typeof(CollectionLengthValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
        }
    }

    [Fact]
    public async Task Create_from_configuration_should_return_a_valid_validated_for_a_hash_set_if_its_length_is_valid()
    {
        var ruleConfig  = StaticData.ValidationRuleConfigForCollectionLengthValidator(typeof(ContactDto).FullName!, nameof(ContactDto.ContactMethods), nameof(ContactDto.ContactMethods), 1, 10);
        var logger      = new InMemoryLoggerFactory().CreateLogger<CollectionLengthValidatorFactory>();
        var validator   = new CollectionLengthValidatorFactory(logger).CreateFromConfiguration<HashSet<string>>(ruleConfig);

        HashSet<string> hash = ["StringOne", "StringTwo"];

        var validated = await validator(hash, "Path");

        validated.Should().Match<Validated<HashSet<string>>>(v => v.IsValid == true && v.Failures.Count == 0);
    }

    [Fact]
    public async Task Create_from_configuration_should_return_an_invalid_validated_and_log_an_error_if_an_error_is_encountered()
    {
        var contact     = StaticData.CreateContactObjectGraph();
        var ruleConfig  = StaticData.ValidationRuleConfigForCollectionLengthValidator(typeof(ContactDto).FullName!, nameof(ContactDto.ContactMethods), nameof(ContactDto.ContactMethods), 3, 10);
        var logger      = new InMemoryLoggerFactory().CreateLogger<CollectionLengthValidatorFactory>();
        var validator   = new CollectionLengthValidatorFactory(logger).CreateFromConfiguration<List<ContactMethodDto>>(null!);

        var validated = await validator(contact.ContactMethods, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<List<ContactMethodDto>>>(v => v.IsValid == false && v.Failures.Count == 1);

            ((InMemoryLogger<CollectionLengthValidatorFactory>)logger).LogEntries[0]
             .Should().Match<LogEntry>(l => l.Category == typeof(CollectionLengthValidatorFactory).FullName && l.Exception != null && l.Message.StartsWith("Configuration error"));
        }
    }
}
