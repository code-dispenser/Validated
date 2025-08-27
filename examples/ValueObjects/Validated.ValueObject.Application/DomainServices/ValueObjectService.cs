using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;
using Validated.Core.Validators;
using Validated.ValueObject.Application.SeedWork;
using Validated.ValueObject.Domain.SeedWork;
using Validated.ValueObject.Domain.ValueObjects;
using Validated.ValueObject.Shared.Validators;

namespace Validated.ValueObject.Application.DomainServices;

internal class ValueObjectService(ICacheRepository cacheRepository, IValidatorFactoryProvider validatorFactoryProvider) : ValueObjectServiceBase
{
    private readonly IValidatorFactoryProvider _factoryProvider = validatorFactoryProvider;//This is what is used to create the validators, you can also add your own validators to this.
    private readonly ICacheRepository          _cacheRepository = cacheRepository;


    public override async Task<Validated<FullName>> CreateFullNameUsingConfig(string givenName, string familyName)
    {
        var ruleConfigs = await _cacheRepository.GetRuleConfigurations();

        var givenNameValidator  = _factoryProvider.CreateValidator<string>(typeof(FullName).FullName!, nameof(FullName.GivenName), ruleConfigs);
        var familyNameValidator = _factoryProvider.CreateValidator<string>(typeof(FullName).FullName!, nameof(FullName.FamilyName), ruleConfigs);

        var validatedGivenName  = await givenNameValidator(givenName);
        var validatedFamilyName = await familyNameValidator(familyName);

        return base.CreateFullName(validatedGivenName, validatedFamilyName);
    }

    public override async Task<Validated<FullName>> CreateFullName(string givenName, string familyName)
    {
        /*
            * I'm just using the validator that is good for all given name fields. But you could create another in the shared project 
            * or just call the MemberValidators static class here and create it here so yu can see the rule, failure messages etc
        */

        var validatedGivenName = await (CommonValidators.GivenNameValidator())(givenName);//I am just getting and then executing/awaiting the validator function on the same line.
        /*
            * Or separated calls 
        */ 
        var familyNameValidator = CommonValidators.FamilyNameValidator();
        var validatedFamilyName = await familyNameValidator(familyName);

        /*
            * Now return the result a Validated<FullName> which could be valid or invalid - there are no thrown exceptions see docs on the libraries approach to exceptions.
        */

        return base.CreateFullName(validatedGivenName, validatedFamilyName);

    }

    public override async Task<Validated<DateRange>> CreateDateRangeWithCompareTo(DateOnly startDate, DateOnly endDate)
    {
        /*
            * Just doing everything here so you can see it all. Using replacement tokens to get some values for the failure message, I could have just output that from the params here 
        */ 
        var startDateValidator = MemberValidators.CreateCompareToValidator<DateOnly>(endDate,CompareType.LessThan, nameof(DateRange.StartDate), 
                                                                                      "Start date", $"Must be before the end date value of: {FailureMessageTokens.COMPARE_TO_VALUE} but found: {FailureMessageTokens.VALIDATED_VALUE}"
                                                                                    );

        var endDateValidator   = MemberValidators.CreateCompareToValidator<DateOnly>(DateOnly.FromDateTime(new DateTime(2025,8,1)), CompareType.GreaterThan, nameof(DateRange.EndDate), 
                                                                                       "End date", $"Must be after {FailureMessageTokens.COMPARE_TO_VALUE} but found {FailureMessageTokens.VALIDATED_VALUE}"
                                                                                     );

        var validatedStartDate = await startDateValidator(startDate);
        var validatedEndDate   = await endDateValidator(endDate);

        return base.CreateDateRange(validatedStartDate, validatedEndDate);
    }

    public override async Task<Validated<DateRange>> CreateDateRangeWithConfigCompareTo(DateOnly startDate, DateOnly endDate)
    {
        /*
            * I created the same rules as of the static method in the validation rule config including the message token replacements. 
        */ 
        var ruleConfigs = await _cacheRepository.GetRuleConfigurations();

        var startDateValidator  = _factoryProvider.CreateValidator<DateOnly>(typeof(DateRange).FullName!, nameof(DateRange.StartDate), ruleConfigs);
        var endDateValidator    = _factoryProvider.CreateValidator<DateOnly>(typeof(DateRange).FullName!, nameof(DateRange.EndDate), ruleConfigs);


        var validatedStartDate = await startDateValidator(startDate,"",endDate);//This is special you are comparing the left hand side value against the right VO values 
        var validatedEndDate   = await endDateValidator(endDate);// This is just like a normal CompareTo except the right hand side of the comparison is in the config data.

        return base.CreateDateRange(validatedStartDate, validatedEndDate);
    }
}
