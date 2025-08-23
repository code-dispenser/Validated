using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using Validated.Core.Builders;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Tests.SharedDataFixtures.Common.Data;
using Validated.Core.Tests.SharedDataFixtures.Common.Loggers;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Tests.SharedDataFixtures.Common.ValidatorFactory;
using Validated.Core.Tests.SharedDataFixtures.Fixtures;
using Validated.Core.Types;

namespace Validated.Core.Tests.Integration.Scenarios;

public class With_Dependency_Injection_Tests 
{
    public static (IValidatorFactoryProvider, InMemoryLoggerFactory) SetupDependencyInjection()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ILoggerFactory, InMemoryLoggerFactory>();
        services.AddSingleton<IValidatorFactoryProvider, ValidatorFactoryProvider>();

        var serviceProvider = services.BuildServiceProvider();

        return (serviceProvider.GetRequiredService<IValidatorFactoryProvider>(), (InMemoryLoggerFactory)serviceProvider.GetRequiredService<ILoggerFactory>());

    }


    [Fact]
    public async Task Overall_result_should_be_an_invalid_validated_due_to_a_bad_postcode_but_one_error_Log_should_be_written_for_missing_nullable_age_rule()
    {
        
        var(validatorFactoryProvider, loggerFactory) = SetupDependencyInjection();//Keeping separate instead of a fixture in case of multiple tests checking log entries
        
        var contactData = StaticData.CreateContactObjectGraph();
        var ruleConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();

        var addressValidator = TenantValidationBuilder<AddressDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                    .ForMember(a => a.AddressLine)
                                        .ForMember(a => a.TownCity)
                                            .ForMember(a => a.County)
                                                .ForNullableStringMember(a => a.Postcode).Build();

        var contactMethodValidator = TenantValidationBuilder<ContactMethodDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                        .ForMember(m => m.MethodType)
                                            .ForMember(m => m.MethodValue)
                                                .Build();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                    .ForNullableNestedMember(c => c.Address, addressValidator)
                                        .ForEachCollectionMember(c => c.ContactMethods, contactMethodValidator)
                                            .ForMember(c => c.GivenName)
                                            .ForMember(c => c.FamilyName)
                                            .ForMember(c => c.Age)
                                            .ForNullableMember(c => c.NullableAge)
                                                .Build();

        var validated = await contactValidator(contactData, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count ==1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path =="ContactDto.Address.Postcode" && i.PropertyName == "Postcode" && i.DisplayName == "Postcode"
                                                           && i.FailureMessage =="Must be a valid UK formatted postcode.");


            loggerFactory.GetTestLogger(typeof(ValidatorFactoryProvider).FullName!)!.LogEntries[0].Message.Should().Be("No rules found for member: Validated.Core.Tests.SharedDataFixtures.Common.Models.ContactDto.NullableAge");
        }

    }
    [Fact]
    public async Task Overall_result_should_be_an_invalid_validated_due_to_a_bad_postcode_without_any_error_log_entries()
    {

        var (validatorFactoryProvider, loggerFactory) = SetupDependencyInjection();//Keeping separate instead of a fixture in case of multiple tests checking log entries

        var contactData = StaticData.CreateContactObjectGraph();
        var baseConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();

        ImmutableList<ValidationRuleConfig> ruleConfigs = baseConfigs.Add(new("Validated.Core.Tests.SharedDataFixtures.Common.Models.ContactDto", "NullableAge", "Nullable Age", "RuleType_Range", "MinMaxToValueType_Int32", "", "Must be between @MINVALUE and @MAXVALUE", 1, 3, "10", "50"));                           

        var addressValidator = TenantValidationBuilder<AddressDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                    .ForMember(a => a.AddressLine)
                                        .ForMember(a => a.TownCity)
                                            .ForMember(a => a.County)
                                                .ForNullableStringMember(a => a.Postcode).Build();

        var contactMethodValidator = TenantValidationBuilder<ContactMethodDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                        .ForMember(m => m.MethodType)
                                            .ForMember(m => m.MethodValue)
                                                .Build();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                    .ForNullableNestedMember(c => c.Address, addressValidator)
                                        .ForEachCollectionMember(c => c.ContactMethods, contactMethodValidator)
                                            .ForMember(c => c.GivenName)
                                            .ForMember(c => c.FamilyName)
                                            .ForMember(c => c.Age)
                                            .ForNullableMember(c => c.NullableAge)
                                                .Build();

        var validated = await contactValidator(contactData, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count ==1);
            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path =="ContactDto.Address.Postcode" && i.PropertyName == "Postcode" && i.DisplayName == "Postcode"
                                                           && i.FailureMessage =="Must be a valid UK formatted postcode.");

            loggerFactory.GetTestLogger(typeof(ValidatorFactoryProvider).FullName!)!.LogEntries.Should().BeEmpty();
        }

    }

    [Fact]
    public async Task Overall_result_should_be_a_valid_validated_without_any_failures_or_error_log_entries_and_without_any_nullable_age_value()
    {

        var (validatorFactoryProvider, loggerFactory) = SetupDependencyInjection();//Keeping separate instead of a fixture in case of multiple tests checking log entries

        var contactData = StaticData.CreateContactObjectGraph();
        var baseConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();

        var ruleConfigs = baseConfigs.Add(new("Validated.Core.Tests.SharedDataFixtures.Common.Models.ContactDto", "NullableAge", "Nullable Age", "RuleType_Range", "MinMaxToValueType_Int32", "", "Must be between @MINVALUE and @MAXVALUE", 1, 3, "10", "50"));

        contactData.NullableAge = null;
        contactData.Address!.Postcode = "SW1A 1AA";

        var addressValidator = TenantValidationBuilder<AddressDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                    .ForMember(a => a.AddressLine)
                                        .ForMember(a => a.TownCity)
                                            .ForMember(a => a.County)
                                                .ForNullableStringMember(a => a.Postcode).Build();

        var contactMethodValidator = TenantValidationBuilder<ContactMethodDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                        .ForMember(m => m.MethodType)
                                            .ForMember(m => m.MethodValue)
                                                .Build();

        var contactValidator = TenantValidationBuilder<ContactDto>.Create(ruleConfigs, validatorFactoryProvider, "TenantOne")
                                    .ForNullableNestedMember(c => c.Address, addressValidator)
                                        .ForEachCollectionMember(c => c.ContactMethods, contactMethodValidator)
                                            .ForMember(c => c.GivenName)
                                            .ForMember(c => c.FamilyName)
                                            .ForMember(c => c.Age)
                                            .ForNullableMember(c => c.NullableAge)
                                                .Build();

        var validated = await contactValidator(contactData, nameof(ContactDto));

        using (new AssertionScope())
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
            loggerFactory.GetTestLogger(typeof(ValidatorFactoryProvider).FullName!)!.LogEntries.Should().BeEmpty();
        }

    }

    [Fact]
    public async Task Should_be_able_to_validate_with_both_a_collection_level_validator_and_a_foreach_primitive_validator()
    {
        var (validatorFactoryProvider, loggerFactory) = SetupDependencyInjection();//Keeping separate instead of a fixture in case of multiple tests checking log entries

        var contactData = StaticData.CreateContactObjectGraph();
        var baseConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();

        contactData.Entries = ["StringOne", "StringTwo"];

        var validator = TenantValidationBuilder<ContactDto>.Create(baseConfigs, validatorFactoryProvider)
                                .ForEachPrimitiveItem(c => c.Entries) // rule is length 1 - 10
                                    .ForCollection(c => c.Entries)    // rule is items 1 - 5
                                        .Build();

        var validated = await validator(contactData);


        validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
    
    }

    [Theory]
    [InlineData(42, true)]
    [InlineData(9, false)]
    public async Task Should_work_with_user_provided_tenant_validators(int age, bool shouldPass)
    {
        var (validatorFactoryProvider, loggerFactory) = SetupDependencyInjection();//Keeping separate instead of a fixture in case of multiple tests checking log entries

        var contactData = StaticData.CreateContactObjectGraph();
        var baseConfigs = StaticData.ValidationRuleConfigsForTenantValidationBuilder();
        var ruleType    = "Custom_IntValidator";
        var tenantID    = "TenantTwo";

        contactData.Age = age;

        var configs = baseConfigs.Add(new(typeof(ContactDto).FullName!, nameof(ContactDto.Age), nameof(ContactDto.Age), ruleType, "", "", "Must be 42",2, 2, "", "", "", "", "", ValidatedConstants.TargetType_Item, tenantID));

        validatorFactoryProvider.AddOrUpdateFactory(ruleType, new CustomTenantIntValidatorFactory(loggerFactory.CreateLogger<CustomTenantIntValidatorFactory>()));

        var validator = TenantValidationBuilder<ContactDto>.Create(configs,validatorFactoryProvider,tenantID)
                            .ForMember(c => c.Age)
                                .Build();

        var validated = await validator(contactData);

        if (true == shouldPass)
        {
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
            return;
        }

        using(new AssertionScope())
        {
            /*
                * Has age rule to check that age is between 10 and 50, so the 9 will fail that and as the custom validator that wants the age of 42 there 
                * will should be two failures.
                * 
                * Custom validator rule comes after the normal age rule so should be second in this test.
            */
            validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);

            validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Age)}" && i.PropertyName == nameof(ContactDto.Age) && i.DisplayName == nameof(ContactDto.Age)
                                                            && i.FailureMessage == "Must be between 10 and 50");

            validated.Failures[1].Should().Match<InvalidEntry>(i => i.Path == $"{nameof(ContactDto)}.{nameof(ContactDto.Age)}" && i.PropertyName == nameof(ContactDto.Age) && i.DisplayName == nameof(ContactDto.Age)
                                                && i.FailureMessage == "Must be 42");
        }

    }
}
