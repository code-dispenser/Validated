using Validated.Core.Types;
using Validated.ValueObject.Domain.ValueObjects;

namespace Validated.ValueObject.Domain.SeedWork;

public abstract class ValueObjectServiceBase
{
    /*
        * Just include the value objects that you want to create using the external validators/rules that will come from the application project. 
    */ 
    protected Validated<FullName> CreateFullName(Validated<string> validatedGivenName, Validated<string> validatedFamilyName)

        => FullName.Create(validatedGivenName, validatedFamilyName);


    public abstract Task<Validated<FullName>> CreateFullName(string givenName, string familyName);
    /*
        * Your application will more than likely either take the multi-tenant/dynamic approach or the static approach. 
        * It makes no difference to your ValueObjects they have no knowledge of any of this.
    */
    public abstract Task<Validated<FullName>> CreateFullNameUsingConfig(string givenName, string familyName);


    protected Validated<DateRange> CreateDateRange(Validated<DateOnly> startDate,  Validated<DateOnly> endDate)

        => DateRange.Create(startDate, endDate);


    public abstract Task<Validated<DateRange>> CreateDateRangeWithCompareTo(DateOnly startDate, DateOnly endDate);

    public abstract Task<Validated<DateRange>> CreateDateRangeWithConfigCompareTo(DateOnly startDate, DateOnly endDate);


}
