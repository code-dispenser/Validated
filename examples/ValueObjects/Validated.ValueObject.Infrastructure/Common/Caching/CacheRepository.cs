using System.Collections.Immutable;
using Validated.Core.Common.Constants;
using Validated.Core.Types;
using Validated.Core.Validators;
using Validated.ValueObject.Application.SeedWork;

namespace Validated.ValueObject.Infrastructure.Common.Caching;

public class CacheRepository(CacheProvider cacheProvider) : ICacheRepository
{
    private readonly CacheProvider _cacheProvider = cacheProvider;
    public async Task<ImmutableList<ValidationRuleConfig>> GetRuleConfigurations()

        => await _cacheProvider.GetOrCreate<ImmutableList<ValidationRuleConfig>>(getData: () => BuildConfigurations(), "RuleConfigs", 60);


    /*
        * Data like this can be retrieved your database and converted into a list of ValidationRuleConfig see the docs for all of the property definitions
        * 
        * The full type name is used to avoid any conflicts.
        * 
        * I have used the bare minimum properties for this demo, lots are optional, so we are just using the fallback data that you should have that can be used for All tenants. 
        * You would then create additional items for your tenant specific needs, and if the rule for the property in not found it the fallback information is used.
        * 
        * I have also defined the same rules as the static validators in the CommonValidators class. Other than the predicate validator you can do pretty much the same dynamically, that we can do statically.
        * 
        * Again, I have split the FamilyName to use two validators. If the type name, property name is listed more than once they get combined in to a single validator for that property.
        * 
        * The string constants RuleType_Regex and RuleType_StringLength are used to determine which validators should be be created which then use information from the other fields, such as the regex patterns and min max length properties.
        * 
        * ValidatedConstants (using Validated.Core.Common.Constants;) holds all of these strings, see the docs for details
    */
    internal static Task<ImmutableList<ValidationRuleConfig>> BuildConfigurations()

         => Task.FromResult(ImmutableList.Create<ValidationRuleConfig>
               (
                   new("Validated.ValueObject.Domain.ValueObjects.FullName", "GivenName", "First name", "RuleType_Regex", "MinMaxToValueType_String", @"^(?=.{2,50}$)[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]+$", "Must start with a capital letter, no double spaces, dashes or apostrophes, and be between 2 and 50 characters in length", 2, 50),
                   new("Validated.ValueObject.Domain.ValueObjects.FullName", "FamilyName", "Surname", "RuleType_Regex", "MinMaxToValueType_String", @"^[A-Z]+['\- ]?[A-Za-z]*['\- ]?[A-Za-z]*$", "Must start with a capital letter, no double spaces, dashes or apostrophes", 2, 50),
                   new("Validated.ValueObject.Domain.ValueObjects.FullName", "FamilyName", "Surname", "RuleType_StringLength", "MinMaxToValueType_String", "", "Must be between 2 and 50 characters in length", 2, 50),

                   new("Validated.ValueObject.Domain.ValueObjects.DateRange", "StartDate", "Start date", "RuleType_VOComparison", "", "", "Must be before the end date value of: {CompareToValue} but found {ValidatedValue}", 0, 0, "", "", "", "", "CompareType_LessThan"),
                   new("Validated.ValueObject.Domain.ValueObjects.DateRange", "EndDate", "EndDate", "RuleType_CompareTo", "MinMaxToValueType_DateOnly", "", "Must be after {CompareToValue} but found {ValidatedValue}", 0, 0, "", "", "2025-08-01", "", "CompareType_GreaterThan")
               ));// If you want to see the exception logging, just remove the y from MinMaxToValueType_DateOnly or the the o from RuleType_CompareTo. No exceptions raised just logged and a failed validation with the cause of failure in the entry. 


    /*
        * For the DateRange Value Object I am showing the two ways that can compare values used via the configurations.
        * 
        * The "RuleType_VOComparison" means that you will provide both of the values for the comparison, with rules dictating the comparison as seen for the StartDate.
        * 
        * The "RuleType_CompareTo" means that you will provide the left hand value which is compared to the the value defined in the config in this instance "2025-8-01"
        * 
        * When ever doing comparison to values in the config (MinValue, MaxValue or CompareValue) you must specify the data type for the comparison in this instance "MinMaxToValueType_DateOnly"
     */
}
