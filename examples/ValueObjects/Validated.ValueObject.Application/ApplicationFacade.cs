using Validated.ValueObject.Application.DomainServices;
using Validated.ValueObject.Domain.SeedWork;

namespace Validated.ValueObject.Application;

public class ApplicationFacade(ValueObjectServiceBase valueObjectService)
{
    private readonly ValueObjectServiceBase _valueObjectService = valueObjectService; 
    public async Task<string> StaticallyCreateFullName(string givenName, string familyName)

        => (await _valueObjectService.CreateFullName(givenName, familyName))
                                .Match(failure => String.Join(Environment.NewLine, failure), success => success.ToString());


    public async Task<string> DynamicallyCreateFullName(string givenName, string familyName)

        => (await _valueObjectService.CreateFullNameUsingConfig(givenName, familyName))
                                .Match(failure => String.Join(Environment.NewLine, failure), success => success.ToString());

    public async Task<string> StaticallyCreateDateRangeWithCompareTo(DateOnly startDate,  DateOnly endDate)

        => (await _valueObjectService.CreateDateRangeWithCompareTo(startDate, endDate))
                        .Match(failure => String.Join(Environment.NewLine, failure), success => success.ToString());

    public async Task<string> DynamicallyCreateDateRangeWithCompareTo(DateOnly startDate, DateOnly endDate)

        => (await _valueObjectService.CreateDateRangeWithConfigCompareTo(startDate, endDate))
                        .Match(failure => String.Join(Environment.NewLine, failure), success => success.ToString());
}
